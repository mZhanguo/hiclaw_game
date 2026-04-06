// ============================================================================
// 官途浮沉 — 独立控制台模拟器
// GameSimulator.cs — 纯C#，不依赖UnityEngine，可用 dotnet run 直接运行
//
// 用法: dotnet run [回合数] [随机种子]
//   例: dotnet run 30 12345
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GuantuFucheng.Simulator
{
    #region Enums
    public enum GameState { MainMenu, NewGame, InTurn, Paused, GameOver, Victory }
    public enum TurnPhase { MorningBriefing, ActionAllocation, Execution, Review }
    public enum ActionType { Politics, Social, Intelligence, SelfImprove, Scheme, Rest }
    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum CardType { Event, Decision, Crisis, Opportunity, Character }
    public enum OfficialRank { Candidate, CountyMagistrate, Prefect, ViceMinister, Minister, MilitaryGovernor, GrandCouncilor }
    public enum Faction { Neutral, Reformist, Conservative, Eunuch, Military, Imperial }
    public enum PersonalityTrait { Loyal, Cunning, Greedy, Righteous, Ambitious, Timid, Generous, Vengeful }
    public enum GameOverReason { Impeached, Executed, Exiled, Retired, Assassinated, Victory }
    public enum PlayerAttribute { Intellect, Charisma, Scheming, Martial, Health, Reputation }
    #endregion

    #region Models
    public class PlayerState
    {
        public string PlayerName { get; set; } = "林青云";
        public OfficialRank CurrentRank { get; set; } = OfficialRank.Candidate;
        public int CurrentTurn { get; set; } = 1;
        public int Intellect { get; set; } = 10;
        public int Charisma { get; set; } = 10;
        public int Scheming { get; set; } = 10;
        public int Martial { get; set; } = 5;
        public int Health { get; set; } = 100;
        public int Reputation { get; set; } = 0;
        public int Gold { get; set; } = 100;
        public int Influence { get; set; } = 0;
        public int MaxActionPoints { get; set; } = 6;
        public int CurrentActionPoints { get; set; } = 6;

        public int GetAttribute(PlayerAttribute attr) => attr switch
        {
            PlayerAttribute.Intellect => Intellect,
            PlayerAttribute.Charisma => Charisma,
            PlayerAttribute.Scheming => Scheming,
            PlayerAttribute.Martial => Martial,
            PlayerAttribute.Health => Health,
            PlayerAttribute.Reputation => Reputation,
            _ => 0
        };

        public int ModifyAttribute(PlayerAttribute attr, int delta)
        {
            switch (attr)
            {
                case PlayerAttribute.Intellect: Intellect = Math.Max(0, Intellect + delta); return delta;
                case PlayerAttribute.Charisma: Charisma = Math.Max(0, Charisma + delta); return delta;
                case PlayerAttribute.Scheming: Scheming = Math.Max(0, Scheming + delta); return delta;
                case PlayerAttribute.Martial: Martial = Math.Max(0, Martial + delta); return delta;
                case PlayerAttribute.Health:
                    int old = Health; Health = Math.Clamp(Health + delta, 0, 100); return Health - old;
                case PlayerAttribute.Reputation: Reputation += delta; return delta;
                default: return 0;
            }
        }

        public PlayerState Snapshot() => new()
        {
            PlayerName = PlayerName, CurrentRank = CurrentRank, CurrentTurn = CurrentTurn,
            Intellect = Intellect, Charisma = Charisma, Scheming = Scheming, Martial = Martial,
            Health = Health, Reputation = Reputation, Gold = Gold, Influence = Influence,
            MaxActionPoints = MaxActionPoints, CurrentActionPoints = CurrentActionPoints
        };
    }

    public class NPCRelationshipState
    {
        public string NpcId { get; set; } = "";
        public int Favor { get; set; }
        public int Trust { get; set; }
        public bool IsRevealed { get; set; }

        public string FavorLevel => Favor switch
        {
            >= 80 => "莫逆之交", >= 50 => "至交好友", >= 20 => "相熟",
            >= 0 => "泛泛之交", >= -30 => "面和心不和", >= -60 => "政敌", _ => "死敌"
        };
        public string TrustLevel => Trust switch
        {
            >= 80 => "肝胆相照", >= 50 => "深信不疑", >= 20 => "略有信任",
            >= 0 => "将信将疑", >= -30 => "心存疑虑", _ => "完全不信任"
        };
    }

    public class RunState
    {
        public PlayerState Player { get; set; } = new();
        public List<NPCRelationshipState> Relationships { get; set; } = new();
        public List<string> ActiveFlags { get; set; } = new();
        public List<string> UsedCards { get; set; } = new();
        public int Seed { get; set; }

        public NPCRelationshipState? GetRelationship(string npcId)
            => Relationships.Find(r => r.NpcId == npcId);

        public NPCRelationshipState GetOrCreateRelationship(string npcId)
        {
            var rel = GetRelationship(npcId);
            if (rel == null) { rel = new NPCRelationshipState { NpcId = npcId }; Relationships.Add(rel); }
            return rel;
        }
    }
    #endregion

    #region JSON Data Structures
    public class CardDatabase { public List<JsonCard> Cards { get; set; } = new(); }
    public class JsonCard
    {
        public string CardId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "";
        public string Rarity { get; set; } = "";
        public bool Repeatable { get; set; } = false;
        public int MinTurn { get; set; }
        public int MaxTurn { get; set; }
        public string MinRank { get; set; } = "";
        public string MaxRank { get; set; } = "";
        public List<string> RequiredFlags { get; set; } = new();
        public List<string> ExcludedFlags { get; set; } = new();
        public bool RequiresMetaUnlock { get; set; }
        public string MetaUnlockId { get; set; } = "";
        public List<JsonChoice> Choices { get; set; } = new();
    }
    public class JsonChoice
    {
        public string ChoiceText { get; set; } = "";
        public string Tooltip { get; set; } = "";
        public string RequiredAttribute { get; set; } = "";
        public int RequiredValue { get; set; }
        public List<JsonModifier> Modifiers { get; set; } = new();
        public string FollowUpCardId { get; set; } = "";
        public List<string> SetFlags { get; set; } = new();
        public List<JsonRelEffect> RelationshipEffects { get; set; } = new();
    }
    public class JsonModifier { public string Attribute { get; set; } = ""; public int Value { get; set; } }
    public class JsonRelEffect { public string NpcId { get; set; } = ""; public int FavorDelta { get; set; } public int TrustDelta { get; set; } }

    public class NPCDatabase { public List<JsonNPC> NPCs { get; set; } = new(); }
    public class JsonNPC
    {
        public string NpcId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Biography { get; set; } = "";
        public string Faction { get; set; } = "";
        public string Rank { get; set; } = "";
        public List<string> Traits { get; set; } = new();
        public int InitialFavor { get; set; }
        public int InitialTrust { get; set; }
        public List<JsonReaction> Reactions { get; set; } = new();
        public bool StartsActive { get; set; }
        public int AppearTurn { get; set; }
        public string AppearMinRank { get; set; } = "";
        public float AggressionWeight { get; set; }
        public float AllianceWeight { get; set; }
        public float BetrayalWeight { get; set; }
    }
    public class JsonReaction { public string ActionType { get; set; } = ""; public int FavorReaction { get; set; } public int TrustReaction { get; set; } }

    public class RankDatabase { public List<JsonRank> Ranks { get; set; } = new(); }
    public class JsonRank
    {
        public string Rank { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public int Grade { get; set; }
        public bool IsFromGrade { get; set; }
        public int BonusActionPoints { get; set; }
        public int BonusGoldPerTurn { get; set; }
        public int BonusInfluence { get; set; }
        public List<JsonPromReq> PromotionRequirements { get; set; } = new();
        public int RequiredReputation { get; set; }
        public int RequiredInfluence { get; set; }
        public int MinTurnsAtRank { get; set; }
        public float ImpeachmentBaseChance { get; set; }
        public List<string> PromotionPaths { get; set; } = new();
    }
    public class JsonPromReq { public string Attribute { get; set; } = ""; public int Value { get; set; } }

    public class EvalConfig
    {
        public string EvaluationName { get; set; } = "";
        public int EvaluationInterval { get; set; }
        public List<EvalDimension> Dimensions { get; set; } = new();
        public OverallRating? OverallRating { get; set; }
        public List<SpecialRule> SpecialRules { get; set; } = new();
    }
    public class EvalDimension
    {
        public string DimensionId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public float Weight { get; set; }
        public string SourceAttribute { get; set; } = "";
        public int PassLine { get; set; }
        public int ExcellentLine { get; set; }
    }
    public class OverallRating
    {
        public RatingLevel? Excellent { get; set; }
        public RatingLevel? Good { get; set; }
        public RatingLevel? Pass { get; set; }
        public RatingLevel? Fail { get; set; }
    }
    public class RatingLevel { public int MinScore { get; set; } public string Description { get; set; } = ""; public string Effect { get; set; } = ""; }
    public class SpecialRule { public string RuleId { get; set; } = ""; public string Description { get; set; } = ""; }
    #endregion

    #region Turn Record
    public class TurnRecord
    {
        public int TurnNumber { get; set; }
        public OfficialRank RankAtStart { get; set; }
        public Dictionary<ActionType, int> Actions { get; set; } = new();
        public List<string> CardsPlayed { get; set; } = new();
        public List<string> ChoicesMade { get; set; } = new();
        public Dictionary<string, (int favor, int trust)> RelationSnapshots { get; set; } = new();
        public string? EvaluationResult { get; set; }
        public PlayerState? SnapshotBefore { get; set; }
        public PlayerState? SnapshotAfter { get; set; }
    }
    #endregion

    // ====================================================================
    // 核心模拟器引擎
    // ====================================================================

    public class SimulatorEngine
    {
        private readonly Random _rng;
        private readonly RunState _run;
        private readonly List<JsonCard> _cards;
        private readonly List<JsonNPC> _npcs;
        private readonly List<JsonRank> _ranks;
        private readonly EvalConfig? _eval;
        private readonly List<TurnRecord> _records = new();

        private int _turnsAtCurrentRank = 0;
        private readonly HashSet<string> _usedCards = new();
        private GameOverReason? _endReason;

        const int BASE_POLITICS_REP = 3, BASE_SOCIAL_FAVOR = 5, BASE_INTEL_CHANCE = 20;
        const int BASE_SELF_IMPROVE = 2, BASE_SCHEME_CHANCE = 40, BASE_REST_HEAL = 10;

        public RunState Run => _run;
        public List<TurnRecord> Records => _records;
        public GameOverReason? EndReason => _endReason;

        public SimulatorEngine(int seed, List<JsonCard> cards, List<JsonNPC> npcs,
                               List<JsonRank> ranks, EvalConfig? eval)
        {
            _rng = new Random(seed);
            _cards = cards; _npcs = npcs; _ranks = ranks; _eval = eval;

            _run = new RunState
            {
                Seed = seed,
                Player = new PlayerState
                {
                    PlayerName = "林青云",
                    CurrentRank = OfficialRank.CountyMagistrate,
                    CurrentTurn = 0,
                    MaxActionPoints = 6,
                    CurrentActionPoints = 6
                }
            };

            foreach (var npc in _npcs.Where(n => n.StartsActive))
                _run.Relationships.Add(new NPCRelationshipState
                    { NpcId = npc.NpcId, Favor = npc.InitialFavor, Trust = npc.InitialTrust });
        }

        // ======================== 主循环 ========================

        public void Simulate(int maxTurns)
        {
            W("╔══════════════════════════════════════════════════╗");
            W("║       《官途浮沉》— 独立模拟器 v1.0             ║");
            W($"║  种子：{_run.Seed,-8}  最大回合：{maxTurns,-4}                ║");
            W("╚══════════════════════════════════════════════════╝");
            W("");

            for (int t = 1; t <= maxTurns; t++)
            {
                if (_endReason.HasValue) break;
                RunTurn(t);
            }

            if (!_endReason.HasValue && _run.Player.CurrentRank == OfficialRank.GrandCouncilor)
                _endReason = GameOverReason.Victory;

            PrintFinalReport(maxTurns);
        }

        private void RunTurn(int turn)
        {
            _run.Player.CurrentTurn = turn;
            _turnsAtCurrentRank++;
            var before = _run.Player.Snapshot();

            var rec = new TurnRecord { TurnNumber = turn, RankAtStart = _run.Player.CurrentRank, SnapshotBefore = before };

            W($"\n══════════ 第 {turn} 回合 ══════════");
            W($"  [{RankName(_run.Player.CurrentRank)}] " +
              $"才{_run.Player.Intellect} 望{_run.Player.Charisma} " +
              $"谋{_run.Player.Scheming} 武{_run.Player.Martial} " +
              $"体{_run.Player.Health} 名{_run.Player.Reputation} " +
              $"金{_run.Player.Gold} 权{_run.Player.Influence}");

            // 早朝
            PhaseMorning(turn);
            CheckNewNPCs(turn);

            // 行动
            var alloc = AllocateActions();
            rec.Actions = alloc;
            ExecuteAllActions(alloc);

            // 卡牌
            PlayCards(turn, rec);

            // 考评
            if (_eval != null && turn % _eval.EvaluationInterval == 0)
                rec.EvaluationResult = DoEvaluation();

            // 升迁
            CheckPromotion();

            // NPC衰减
            DecayRelationships(turn);

            // 记录
            foreach (var r in _run.Relationships)
                rec.RelationSnapshots[r.NpcId] = (r.Favor, r.Trust);
            rec.SnapshotAfter = _run.Player.Snapshot();
            _records.Add(rec);

            // 游戏结束检查
            CheckGameOver();
        }

        // ======================== 阶段实现 ========================

        private void PhaseMorning(int turn)
        {
            var rd = _ranks.Find(r => PE<OfficialRank>(r.Rank) == _run.Player.CurrentRank);
            if (rd != null)
            {
                _run.Player.Gold += rd.BonusGoldPerTurn;
                _run.Player.Influence += rd.BonusInfluence;
                _run.Player.MaxActionPoints = 6 + rd.BonusActionPoints;
            }
            _run.Player.CurrentActionPoints = _run.Player.MaxActionPoints;
        }

        private void CheckNewNPCs(int turn)
        {
            foreach (var npc in _npcs)
            {
                if (npc.StartsActive || _run.GetRelationship(npc.NpcId) != null) continue;
                if (turn >= npc.AppearTurn && _run.Player.CurrentRank >= PE<OfficialRank>(npc.AppearMinRank))
                {
                    _run.Relationships.Add(new NPCRelationshipState
                        { NpcId = npc.NpcId, Favor = npc.InitialFavor, Trust = npc.InitialTrust });
                    W($"  🆕 {npc.DisplayName} 登场");
                }
            }
        }

        private Dictionary<ActionType, int> AllocateActions()
        {
            var types = Enum.GetValues<ActionType>();
            var alloc = new Dictionary<ActionType, int>();
            int rem = _run.Player.MaxActionPoints;
            while (rem > 0)
            {
                var t = types[_rng.Next(types.Length)];
                int pts = Math.Min(_rng.Next(1, 3), rem);
                alloc.TryAdd(t, 0); alloc[t] += pts; rem -= pts;
            }
            W("  行动：" + string.Join(" ", alloc.Select(a => $"{ActName(a.Key)}×{a.Value}")));
            return alloc;
        }

        private void ExecuteAllActions(Dictionary<ActionType, int> alloc)
        {
            foreach (var (type, points) in alloc)
            {
                if (points <= 0) continue;
                ExecAction(type, points);

                // NPC反应
                foreach (var rel in _run.Relationships)
                {
                    var npc = _npcs.Find(n => n.NpcId == rel.NpcId);
                    var rx = npc?.Reactions?.Find(r => r.ActionType == type.ToString());
                    if (rx != null)
                    {
                        int fd = rx.FavorReaction * points / 2;
                        int td = rx.TrustReaction * points / 2;
                        if (fd != 0 || td != 0) ModRel(rel, fd, td);
                    }
                }
            }
        }

        private void ExecAction(ActionType type, int pts)
        {
            var p = _run.Player;
            switch (type)
            {
                case ActionType.Politics:
                    int rep = BASE_POLITICS_REP * pts + p.Intellect / 10;
                    p.ModifyAttribute(PlayerAttribute.Reputation, rep);
                    p.Influence += pts;
                    break;
                case ActionType.Social:
                    if (_run.Relationships.Count > 0)
                    {
                        var t = _run.Relationships[_rng.Next(_run.Relationships.Count)];
                        int fg = pts * BASE_SOCIAL_FAVOR + p.Charisma / 10;
                        ModRel(t, fg, pts);
                        W($"    交际→{NpcName(t.NpcId)} 好感+{fg}");
                    }
                    break;
                case ActionType.Intelligence:
                    int ir = Math.Clamp(BASE_INTEL_CHANCE * pts + p.Scheming, 0, 95);
                    if (_rng.Next(100) < ir)
                    {
                        var hidden = _run.Relationships.Where(r => !r.IsRevealed).ToList();
                        if (hidden.Count > 0) { hidden[_rng.Next(hidden.Count)].IsRevealed = true; W("    情报成功：揭示一名NPC"); }
                    }
                    else if (_rng.Next(100) < 15) W("    情报失败，被察觉！");
                    break;
                case ActionType.SelfImprove:
                    for (int i = 0; i < pts * BASE_SELF_IMPROVE; i++)
                        p.ModifyAttribute(_rng.Next(2) == 0 ? PlayerAttribute.Intellect : PlayerAttribute.Martial, 1);
                    break;
                case ActionType.Scheme:
                    int sr = Math.Clamp(BASE_SCHEME_CHANCE + pts * 10 + p.Scheming - 10, 5, 90);
                    if (_rng.Next(100) < sr) { p.Influence += pts * 2; p.ModifyAttribute(PlayerAttribute.Scheming, 1); }
                    else p.ModifyAttribute(PlayerAttribute.Reputation, -pts * 2);
                    break;
                case ActionType.Rest:
                    p.ModifyAttribute(PlayerAttribute.Health, pts * BASE_REST_HEAL);
                    break;
            }
        }

        private void PlayCards(int turn, TurnRecord rec)
        {
            var avail = _cards.Where(c =>
            {
                if (_usedCards.Contains(c.CardId) && !c.Repeatable) return false;
                if (turn < c.MinTurn) return false;
                if (c.MaxTurn > 0 && turn > c.MaxTurn) return false;
                var min = PE<OfficialRank>(c.MinRank);
                var max = PE<OfficialRank>(c.MaxRank);
                if (_run.Player.CurrentRank < min || _run.Player.CurrentRank > max) return false;
                if (c.RequiredFlags?.Any(f => !_run.ActiveFlags.Contains(f)) == true) return false;
                if (c.ExcludedFlags?.Any(f => _run.ActiveFlags.Contains(f)) == true) return false;
                if (c.RequiresMetaUnlock) return false;
                return true;
            }).ToList();

            int draw = Math.Min(3, avail.Count);
            for (int i = 0; i < draw; i++)
            {
                if (avail.Count == 0) break;
                var card = WPick(avail);
                avail.Remove(card);
                ResolveCard(card, rec);
            }
        }

        private void ResolveCard(JsonCard card, TurnRecord rec)
        {
            if (card.Choices == null || card.Choices.Count == 0) return;
            var valid = new List<int>();
            for (int i = 0; i < card.Choices.Count; i++)
            {
                var c = card.Choices[i];
                if (c.RequiredValue <= 0 ||
                    _run.Player.GetAttribute(PE<PlayerAttribute>(c.RequiredAttribute)) >= c.RequiredValue)
                    valid.Add(i);
            }
            if (valid.Count == 0) return;

            int idx = valid[_rng.Next(valid.Count)];
            var ch = card.Choices[idx];
            W($"  🃏 {card.Title}（{card.Type}·{card.Rarity}）→ {ch.ChoiceText}");
            rec.CardsPlayed.Add(card.CardId);
            rec.ChoicesMade.Add($"{card.Title}:{ch.ChoiceText}");

            foreach (var m in ch.Modifiers ?? new())
            {
                var a = PE<PlayerAttribute>(m.Attribute);
                int d = _run.Player.ModifyAttribute(a, m.Value);
                if (d != 0) W($"      {AttrName(a)} {(d >= 0 ? "+" : "")}{d}");
            }
            foreach (var re in ch.RelationshipEffects ?? new())
            {
                var rel = _run.GetOrCreateRelationship(re.NpcId);
                ModRel(rel, re.FavorDelta, re.TrustDelta);
                if (re.FavorDelta != 0 || re.TrustDelta != 0)
                    W($"      {NpcName(re.NpcId)} 好感{S(re.FavorDelta)} 信任{S(re.TrustDelta)}");
            }
            foreach (var f in ch.SetFlags ?? new())
                if (!_run.ActiveFlags.Contains(f)) _run.ActiveFlags.Add(f);
            _usedCards.Add(card.CardId);
        }

        private string DoEvaluation()
        {
            if (_eval == null) return "";
            var p = _run.Player;
            W("\n  ═══ 吏部考评 ═══");
            float total = 0;

            foreach (var dim in _eval.Dimensions)
            {
                int v = dim.SourceAttribute switch
                {
                    "Intellect" => p.Intellect, "Reputation" => p.Reputation,
                    "Charisma" => p.Charisma, "Scheming" => p.Scheming,
                    "Health" => p.Health, _ => 0
                };
                float s = v * dim.Weight; total += s;
                string st = v >= dim.ExcellentLine ? "卓异★" : v >= dim.PassLine ? "称职✓" : "不称✗";
                W($"    {dim.DisplayName}：{v} ×{dim.Weight} = {s:F1}  [{st}]");
            }

            // 特殊规则
            if (_run.ActiveFlags.Any(f => f.Contains("corrupt"))) { total *= 0.9f; W("    ⚠ 贪腐标记，扣分！"); }
            var gov = _run.GetRelationship("npc_governor");
            if (gov != null && gov.Favor >= 30 && gov.Trust >= 20) { total += 10; W("    ★ 刺史推荐+10"); }

            string rating;
            if (_eval.OverallRating != null)
            {
                if (total >= (_eval.OverallRating.Excellent?.MinScore ?? 80))
                { rating = "考评卓异"; p.ModifyAttribute(PlayerAttribute.Reputation, 20); }
                else if (total >= (_eval.OverallRating.Good?.MinScore ?? 60))
                { rating = "考评称职"; p.ModifyAttribute(PlayerAttribute.Reputation, 5); }
                else if (total >= (_eval.OverallRating.Pass?.MinScore ?? 40))
                { rating = "考评平常"; p.ModifyAttribute(PlayerAttribute.Reputation, -5); }
                else
                { rating = "考评不称"; p.ModifyAttribute(PlayerAttribute.Reputation, -20); }
            }
            else rating = $"分数{total:F0}";

            W($"  考评总分：{total:F1} — {rating}");
            return $"{total:F1} ({rating})";
        }

        private void CheckPromotion()
        {
            var p = _run.Player;
            var rd = _ranks.Find(r => PE<OfficialRank>(r.Rank) == p.CurrentRank);
            if (rd == null || p.CurrentRank == OfficialRank.GrandCouncilor) return;
            if (_turnsAtCurrentRank < rd.MinTurnsAtRank) return;
            if (p.Reputation < rd.RequiredReputation || p.Influence < rd.RequiredInfluence) return;
            foreach (var req in rd.PromotionRequirements ?? new())
                if (p.GetAttribute(PE<PlayerAttribute>(req.Attribute)) < req.Value) return;

            if (rd.PromotionPaths != null && rd.PromotionPaths.Count > 0)
            {
                OfficialRank target;
                if (p.CurrentRank == OfficialRank.Prefect && rd.PromotionPaths.Count > 1)
                    target = p.Martial > p.Intellect ? OfficialRank.MilitaryGovernor : OfficialRank.ViceMinister;
                else
                    target = PE<OfficialRank>(rd.PromotionPaths[0]);

                var old = p.CurrentRank;
                p.CurrentRank = target;
                _turnsAtCurrentRank = 0;
                W($"  🎉 升迁！{RankName(old)} → {RankName(target)}");
            }
        }

        private void DecayRelationships(int turn)
        {
            foreach (var rel in _run.Relationships)
            {
                // 好感每2回合衰减1（原来每回合衰减1）
                if (turn % 2 == 0)
                {
                    if (rel.Favor > 0) rel.Favor = Math.Max(0, rel.Favor - 1);
                    else if (rel.Favor < 0) rel.Favor = Math.Min(0, rel.Favor + 1);
                }
                // 信任每5回合衰减1（原来每3回合衰减1）
                if (turn % 5 == 0)
                {
                    if (rel.Trust > 0) rel.Trust = Math.Max(0, rel.Trust - 1);
                    else if (rel.Trust < 0) rel.Trust = Math.Min(0, rel.Trust + 1);
                }
            }
        }

        private void CheckGameOver()
        {
            var p = _run.Player;
            if (p.Health <= 0) { _endReason = GameOverReason.Executed; W("  💀 体魄归零，暴毙！"); }
            else if (p.Reputation <= -50) { _endReason = GameOverReason.Impeached; W("  💀 声望过低，被弹劾罢官！"); }
            else if (p.CurrentRank == OfficialRank.GrandCouncilor) { _endReason = GameOverReason.Victory; W("  🏆 位极人臣，通关！"); }
        }

        // ======================== 辅助 ========================

        private void ModRel(NPCRelationshipState rel, int fDelta, int tDelta)
        {
            rel.Favor = Math.Clamp(rel.Favor + fDelta, -100, 100);
            rel.Trust = Math.Clamp(rel.Trust + tDelta, -100, 100);
        }

        private JsonCard WPick(List<JsonCard> pool)
        {
            if (pool.Count == 0) return pool[0]; // shouldn't happen
            float total = 0;
            var weights = pool.Select(c => c.Rarity switch
            {
                "Common" => 50f, "Uncommon" => 30f, "Rare" => 15f, "Epic" => 4f, "Legendary" => 1f, _ => 50f
            }).ToArray();
            foreach (var w in weights) total += w;
            float roll = (float)(_rng.NextDouble() * total);
            float cum = 0;
            for (int i = 0; i < pool.Count; i++) { cum += weights[i]; if (roll <= cum) return pool[i]; }
            return pool[^1];
        }

        private static T PE<T>(string? value) where T : struct
        {
            if (string.IsNullOrEmpty(value)) return default;
            return Enum.TryParse<T>(value, true, out var r) ? r : default;
        }

        private string NpcName(string id) => _npcs.Find(n => n.NpcId == id)?.DisplayName ?? id;
        private string RankName(OfficialRank r) => _ranks.Find(rd => PE<OfficialRank>(rd.Rank) == r)?.DisplayName ?? r.ToString();
        private static string S(int v) => (v >= 0 ? "+" : "") + v;
        private static string ActName(ActionType t) => t switch
        {
            ActionType.Politics => "政务", ActionType.Social => "交际", ActionType.Intelligence => "情报",
            ActionType.SelfImprove => "修身", ActionType.Scheme => "谋略", ActionType.Rest => "休息", _ => t.ToString()
        };
        private static string AttrName(PlayerAttribute a) => a switch
        {
            PlayerAttribute.Intellect => "才学", PlayerAttribute.Charisma => "人望", PlayerAttribute.Scheming => "权谋",
            PlayerAttribute.Martial => "武略", PlayerAttribute.Health => "体魄", PlayerAttribute.Reputation => "声望", _ => a.ToString()
        };

        private static void W(string s) => Console.WriteLine(s);

        // ======================== 最终报告 ========================

        private void PrintFinalReport(int maxTurns)
        {
            var p = _run.Player;
            W("\n");
            W("╔══════════════════════════════════════════════════════════╗");
            W("║              《官途浮沉》— 模拟终局报告                 ║");
            W("╠══════════════════════════════════════════════════════════╣");
            W($"  玩家：{p.PlayerName}");
            W($"  结局：{(_endReason.HasValue ? EndReasonText(_endReason.Value) : "回合耗尽")}");
            W($"  最终官职：{RankName(p.CurrentRank)}");
            W($"  存活回合：{p.CurrentTurn}/{maxTurns}");
            W($"  才学:{p.Intellect} 人望:{p.Charisma} 权谋:{p.Scheming} 武略:{p.Martial}");
            W($"  体魄:{p.Health} 声望:{p.Reputation} 金银:{p.Gold} 影响力:{p.Influence}");
            W("");

            // NPC关系
            W("  ─── NPC最终关系 ───");
            foreach (var rel in _run.Relationships)
                W($"    {NpcName(rel.NpcId),-20} 好感:{rel.Favor,4}({rel.FavorLevel})  信任:{rel.Trust,4}({rel.TrustLevel})");

            // 考评历史
            var evals = _records.Where(r => r.EvaluationResult != null).ToList();
            if (evals.Count > 0)
            {
                W("");
                W("  ─── 考评历史 ───");
                foreach (var e in evals)
                    W($"    第{e.TurnNumber}回合：{e.EvaluationResult}");
            }

            // 关键卡牌决策
            var allChoices = _records.SelectMany(r => r.ChoicesMade).ToList();
            if (allChoices.Count > 0)
            {
                W("");
                W("  ─── 重要决策 ───");
                foreach (var c in allChoices.Take(15))
                    W($"    • {c}");
                if (allChoices.Count > 15) W($"    ...共{allChoices.Count}个决策");
            }

            // 剧情标记
            if (_run.ActiveFlags.Count > 0)
            {
                W("");
                W($"  ─── 剧情标记（{_run.ActiveFlags.Count}）───");
                W($"    {string.Join(", ", _run.ActiveFlags)}");
            }

            // 属性变化趋势
            W("");
            W("  ─── 属性变化趋势 ───");
            W("  回合  官职                才学  人望  权谋  武略  体魄  声望");
            foreach (var r in _records)
            {
                var s = r.SnapshotAfter!;
                string rn = RankName(s.CurrentRank);
                if (rn.Length > 18) rn = rn[..18];
                W($"  {r.TurnNumber,4}  {rn,-18} {s.Intellect,4}  {s.Charisma,4}  {s.Scheming,4}  {s.Martial,4}  {s.Health,4}  {s.Reputation,4}");
            }

            // 分数
            int score = (int)p.CurrentRank * 100 + p.CurrentTurn * 10 + p.Reputation * 2
                       + p.Intellect + p.Charisma + p.Scheming;
            W("");
            W($"  ═══ 最终得分：{score} ═══");
            W("╚══════════════════════════════════════════════════════════╝");
        }

        private static string EndReasonText(GameOverReason r) => r switch
        {
            GameOverReason.Victory => "🏆 位极人臣（通关）",
            GameOverReason.Impeached => "被弹劾罢官",
            GameOverReason.Executed => "体魄归零，暴毙",
            GameOverReason.Exiled => "被流放",
            GameOverReason.Assassinated => "被暗杀",
            GameOverReason.Retired => "告老还乡",
            _ => r.ToString()
        };
    }

    // ====================================================================
    // 程序入口
    // ====================================================================

    public class Program
    {
        static void Main(string[] args)
        {
            int maxTurns = 30;
            int seed = 0;

            if (args.Length >= 1 && int.TryParse(args[0], out var t)) maxTurns = t;
            if (args.Length >= 2 && int.TryParse(args[1], out var s)) seed = s;
            if (seed == 0) seed = new Random().Next(1, int.MaxValue);

            // 查找JSON数据文件
            string dataDir = FindDataDir();
            if (dataDir == null)
            {
                Console.WriteLine("错误：找不到 GameData 目录！");
                Console.WriteLine("请确保在项目目录下运行，或 Assets/Resources/GameData/ 路径可达。");
                return;
            }

            Console.WriteLine($"数据目录：{dataDir}");

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var cards = JsonSerializer.Deserialize<CardDatabase>(
                File.ReadAllText(Path.Combine(dataDir, "CardDatabase.json")), opts)?.Cards ?? new();
            var npcs = JsonSerializer.Deserialize<NPCDatabase>(
                File.ReadAllText(Path.Combine(dataDir, "NPCDatabase.json")), opts)?.NPCs ?? new();
            var ranks = JsonSerializer.Deserialize<RankDatabase>(
                File.ReadAllText(Path.Combine(dataDir, "RankDatabase.json")), opts)?.Ranks ?? new();
            var eval = JsonSerializer.Deserialize<EvalConfig>(
                File.ReadAllText(Path.Combine(dataDir, "EvaluationConfig.json")), opts);

            Console.WriteLine($"已加载：{cards.Count}张卡牌, {npcs.Count}个NPC, {ranks.Count}个官职");
            Console.WriteLine("");

            var engine = new SimulatorEngine(seed, cards, npcs, ranks, eval);
            engine.Simulate(maxTurns);
        }

        static string? FindDataDir()
        {
            // 尝试多个可能的路径
            string[] candidates = {
                "Assets/Resources/GameData",
                "../Assets/Resources/GameData",
                "../../Assets/Resources/GameData",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Resources/GameData"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Assets/Resources/GameData"),
            };

            foreach (var dir in candidates)
            {
                string full = Path.GetFullPath(dir);
                if (Directory.Exists(full) && File.Exists(Path.Combine(full, "CardDatabase.json")))
                    return full;
            }

            return null;
        }
    }
}
