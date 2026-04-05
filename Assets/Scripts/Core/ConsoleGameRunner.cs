// ============================================================================
// 官途浮沉 - 纯文本控制台游戏运行器
// ConsoleGameRunner.cs — 用Debug.Log模拟完整游戏流程，不依赖Unity UI
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;
using GuantuFucheng.Models;
using GuantuFucheng.Systems;
using GuantuFucheng.Data;

namespace GuantuFucheng.Core
{
    /// <summary>
    /// 纯文本控制台游戏运行器
    /// 
    /// 不依赖任何UI，用Debug.Log输出所有游戏信息。
    /// 自动模拟玩家操作（随机分配行动力、随机选择卡牌选项）。
    /// 
    /// 运行方式：
    /// 1. GameBootstrap会自动添加此组件
    /// 2. 在Unity编辑器中按Play即可看到控制台输出
    /// 3. 每帧自动推进一个阶段
    /// </summary>
    public class ConsoleGameRunner : MonoBehaviour
    {
        [Header("模拟配置")]
        [SerializeField] private int _maxTurns = 20;
        [SerializeField] private float _phaseDelay = 0.5f; // 每阶段间隔（秒）
        [SerializeField] private int _evaluationInterval = 4; // 考评间隔

        private List<CardData> _allCards;
        private List<NPCData> _allNPCs;
        private List<OfficialRankData> _allRanks;
        private EvaluationConfigData _evalConfig;
        private bool _initialized = false;
        private int _completedTurns = 0;

        // ======================== 初始化 ========================

        public void Initialize(
            List<CardData> cards,
            List<NPCData> npcs,
            List<OfficialRankData> ranks,
            EvaluationConfigData evalConfig)
        {
            _allCards = cards;
            _allNPCs = npcs;
            _allRanks = ranks;
            _evalConfig = evalConfig;
            _initialized = true;

            // 订阅事件以输出日志
            SubscribeEvents();

            Debug.Log("[ConsoleRunner] ✓ 初始化完成，开始自动模拟游戏");
            StartCoroutine(RunGameLoop());
        }

        // ======================== 事件订阅（日志输出） ========================

        private void SubscribeEvents()
        {
            GameEvents.OnTurnStarted += turn =>
                LogSection($"═══ 第 {turn} 回合 ═══");

            GameEvents.OnPhaseChanged += phase =>
                Debug.Log($"  ◆ 阶段切换 → {GetPhaseName(phase)}");

            GameEvents.OnCardDrawn += cardId =>
            {
                var card = _allCards?.Find(c => c.CardId == cardId);
                Debug.Log($"  🃏 抽到卡牌：{card?.Title ?? cardId}（{card?.Rarity}）");
            };

            GameEvents.OnCardChoiceMade += (cardId, choiceIdx) =>
            {
                var card = _allCards?.Find(c => c.CardId == cardId);
                Debug.Log($"  📜 卡牌决策：{card?.Title} → 选项{choiceIdx + 1}");
            };

            GameEvents.OnFavorChanged += (npcId, delta, current) =>
            {
                string name = GetNPCName(npcId);
                string sign = delta >= 0 ? "+" : "";
                Debug.Log($"  💬 {name} 好感度 {sign}{delta} → {current}");
            };

            GameEvents.OnTrustChanged += (npcId, delta, current) =>
            {
                string name = GetNPCName(npcId);
                string sign = delta >= 0 ? "+" : "";
                Debug.Log($"  🔒 {name} 信任度 {sign}{delta} → {current}");
            };

            GameEvents.OnAttributeChanged += (attr, delta, current) =>
            {
                string sign = delta >= 0 ? "+" : "";
                Debug.Log($"  📊 {GetAttrName(attr)} {sign}{delta} → {current}");
            };

            GameEvents.OnRankChanged += (oldRank, newRank) =>
                Debug.Log($"  🎉 官职变动：{oldRank} → {newRank}");

            GameEvents.OnRelationshipMilestone += (npcId, milestone) =>
                Debug.Log($"  ⭐ 关系里程碑：{GetNPCName(npcId)} — {milestone}");

            GameEvents.OnGameOver += reason =>
                Debug.Log($"  💀 游戏结束：{reason}");
        }

        // ======================== 游戏主循环 ========================

        private IEnumerator RunGameLoop()
        {
            yield return new WaitForSeconds(0.5f); // 等待初始化完成

            while (_completedTurns < _maxTurns &&
                   GameManager.Instance.CurrentState != GameState.GameOver)
            {
                yield return StartCoroutine(RunSingleTurn());
                _completedTurns++;
            }

            PrintFinalReport();
        }

        /// <summary>
        /// 执行完整单回合：早朝简报→行动分配→卡牌执行→复盘
        /// </summary>
        private IEnumerator RunSingleTurn()
        {
            var run = GameManager.Instance.CurrentRun;
            var player = run.Player;
            int turnNum = TurnManager.Instance.CurrentTurnNumber;

            // ---- 阶段1：早朝简报 ----
            Debug.Log("");
            LogSection($"☀️ 早朝简报 — 第{turnNum}回合");
            PrintPlayerStatus(player);
            PrintNPCRelationships(run);

            yield return new WaitForSeconds(_phaseDelay);

            // 推进到行动分配阶段
            TurnManager.Instance.AdvancePhase();

            // ---- 阶段2：行动分配 ----
            Debug.Log("");
            LogSection("📋 行动分配");
            SimulateActionAllocation(player);

            yield return new WaitForSeconds(_phaseDelay);

            // 推进到执行阶段
            TurnManager.Instance.AdvancePhase();

            // ---- 阶段3：执行结算（含卡牌） ----
            Debug.Log("");
            LogSection("⚔️ 执行结算");
            SimulateCardChoices(run);

            yield return new WaitForSeconds(_phaseDelay);

            // 推进到复盘阶段
            TurnManager.Instance.AdvancePhase();

            // ---- 阶段4：复盘总结 ----
            Debug.Log("");
            LogSection("📖 回合复盘");

            // 吏部考评检查
            if (turnNum > 0 && turnNum % _evaluationInterval == 0)
            {
                RunEvaluation(player, run);
            }

            PrintTurnSummary(player);

            yield return new WaitForSeconds(_phaseDelay);

            // 推进（开始下一回合或结束游戏）
            TurnManager.Instance.AdvancePhase();
        }

        // ======================== 模拟玩家操作 ========================

        /// <summary>自动随机分配行动力</summary>
        private void SimulateActionAllocation(PlayerState player)
        {
            int totalAP = player.MaxActionPoints;
            var actions = new[] {
                ActionType.Politics, ActionType.Social, ActionType.Intelligence,
                ActionType.SelfImprove, ActionType.Scheme, ActionType.Rest
            };

            // 随机分配策略：每次随机选一个方向分配1-2点
            int remaining = totalAP;
            var allocation = new Dictionary<ActionType, int>();

            while (remaining > 0)
            {
                var type = actions[Random.Range(0, actions.Length)];
                int points = Mathf.Min(Random.Range(1, 3), remaining);

                if (!allocation.ContainsKey(type))
                    allocation[type] = 0;
                allocation[type] += points;

                TurnManager.Instance.AllocateAction(type, points);
                remaining -= points;
            }

            Debug.Log("  行动分配方案：");
            foreach (var kvp in allocation)
            {
                Debug.Log($"    • {GetActionName(kvp.Key)}：{kvp.Value}点");
            }
        }

        /// <summary>自动随机选择卡牌选项</summary>
        private void SimulateCardChoices(RunState run)
        {
            var hand = CardSystem.Instance.Hand;
            if (hand == null || hand.Count == 0)
            {
                Debug.Log("  （本回合无卡牌事件）");
                return;
            }

            // 处理所有手牌（从后往前以避免索引变动）
            int handCount = hand.Count;
            for (int i = handCount - 1; i >= 0; i--)
            {
                if (i >= hand.Count) continue; // 安全检查

                var card = hand[i];
                if (card.Choices.Count == 0) continue;

                Debug.Log($"\n  ┌─ 卡牌事件：{card.Title}（{card.Type}·{card.Rarity}）");
                Debug.Log($"  │  {card.Description.Substring(0, Mathf.Min(60, card.Description.Length))}...");

                for (int j = 0; j < card.Choices.Count; j++)
                {
                    var choice = card.Choices[j];
                    string reqStr = choice.RequiredValue > 0
                        ? $" [需{choice.RequiredAttribute}≥{choice.RequiredValue}]"
                        : "";
                    Debug.Log($"  │  {j + 1}. {choice.ChoiceText}{reqStr}");
                }

                // 随机选择一个满足条件的选项
                var validChoices = new List<int>();
                for (int j = 0; j < card.Choices.Count; j++)
                {
                    var choice = card.Choices[j];
                    if (choice.RequiredValue <= 0 ||
                        run.Player.GetAttribute(choice.RequiredAttribute) >= choice.RequiredValue)
                    {
                        validChoices.Add(j);
                    }
                }

                if (validChoices.Count > 0)
                {
                    int chosen = validChoices[Random.Range(0, validChoices.Count)];
                    Debug.Log($"  └─ 选择：{card.Choices[chosen].ChoiceText}");
                    CardSystem.Instance.MakeChoice(i, chosen);
                }
                else
                {
                    Debug.Log($"  └─ 无可用选项，弃置");
                    CardSystem.Instance.DiscardCard(i);
                }
            }
        }

        // ======================== 考评系统 ========================

        private void RunEvaluation(PlayerState player, RunState run)
        {
            if (_evalConfig == null) return;

            LogSection("📜 吏部考评");
            float totalScore = 0;

            foreach (var dim in _evalConfig.Dimensions)
            {
                int attrValue = 0;
                switch (dim.SourceAttribute)
                {
                    case "Intellect": attrValue = player.Intellect; break;
                    case "Reputation": attrValue = player.Reputation; break;
                    case "Charisma": attrValue = player.Charisma; break;
                    case "Scheming": attrValue = player.Scheming; break;
                    case "Health": attrValue = player.Health; break;
                }

                float dimScore = attrValue * dim.Weight;
                totalScore += dimScore;

                string status = attrValue >= dim.ExcellentLine ? "卓异★" :
                               attrValue >= dim.PassLine ? "称职✓" : "不称✗";
                Debug.Log($"  {dim.DisplayName}：{attrValue}（权重{dim.Weight}） = {dimScore:F1}  [{status}]");
            }

            // 特殊规则检查
            bool hasCorruption = run.ActiveFlags.Exists(f =>
                f.Contains("corrupt") || f == "tax_corrupt" || f == "judge_corrupt");
            if (hasCorruption)
            {
                Debug.Log("  ⚠ 贪腐标记检出，忠诚维度减半！");
                totalScore *= 0.9f; // 简化处理
            }

            // NPC推荐加分
            var govRel = run.GetRelationship("npc_governor");
            if (govRel != null && govRel.Favor >= 30 && govRel.Trust >= 20)
            {
                totalScore += 10;
                Debug.Log("  ★ 刺史推荐，总分+10");
            }

            string rating = _evalConfig.GetRating(totalScore);
            Debug.Log($"\n  ═══ 考评总分：{totalScore:F1} — {rating} ═══");

            // 应用考评效果
            if (totalScore >= 80)
            {
                player.ModifyAttribute(PlayerAttribute.Reputation, 20);
                Debug.Log("  效果：声望+20，获得升迁资格");
            }
            else if (totalScore >= 60)
            {
                player.ModifyAttribute(PlayerAttribute.Reputation, 5);
                Debug.Log("  效果：声望+5，留任");
            }
            else if (totalScore >= 40)
            {
                player.ModifyAttribute(PlayerAttribute.Reputation, -5);
                Debug.Log("  效果：声望-5，收到警告");
            }
            else
            {
                player.ModifyAttribute(PlayerAttribute.Reputation, -20);
                Debug.Log("  效果：声望-20，降职警告！");
            }
        }

        // ======================== 状态输出 ========================

        private void PrintPlayerStatus(PlayerState player)
        {
            var rankData = _allRanks?.Find(r => r.Rank == player.CurrentRank);
            string rankName = rankData?.DisplayName ?? player.CurrentRank.ToString();

            Debug.Log($"  👤 {player.PlayerName} — {rankName}");
            Debug.Log($"  ├ 才学:{player.Intellect} 人望:{player.Charisma} " +
                      $"权谋:{player.Scheming} 武略:{player.Martial}");
            Debug.Log($"  ├ 体魄:{player.Health} 声望:{player.Reputation} " +
                      $"金银:{player.Gold} 影响力:{player.Influence}");
            Debug.Log($"  └ 行动力:{player.CurrentActionPoints}/{player.MaxActionPoints}");
        }

        private void PrintNPCRelationships(RunState run)
        {
            if (run.Relationships.Count == 0) return;

            Debug.Log("  NPC关系：");
            foreach (var rel in run.Relationships)
            {
                string name = GetNPCName(rel.NpcId);
                string favorIcon = rel.Favor >= 20 ? "😊" : rel.Favor <= -20 ? "😠" : "😐";
                Debug.Log($"    {favorIcon} {name}：好感{rel.Favor}({rel.FavorLevel}) " +
                          $"信任{rel.Trust}({rel.TrustLevel})");
            }
        }

        private void PrintTurnSummary(PlayerState player)
        {
            Debug.Log($"  回合结束状态：");
            Debug.Log($"    才学:{player.Intellect} 人望:{player.Charisma} " +
                      $"权谋:{player.Scheming} 武略:{player.Martial}");
            Debug.Log($"    体魄:{player.Health} 声望:{player.Reputation} " +
                      $"金银:{player.Gold} 影响力:{player.Influence}");
        }

        private void PrintFinalReport()
        {
            var run = GameManager.Instance.CurrentRun;
            if (run == null) return;

            var player = run.Player;
            var rankData = _allRanks?.Find(r => r.Rank == player.CurrentRank);

            Debug.Log("\n");
            Debug.Log("╔══════════════════════════════════════════════════╗");
            Debug.Log("║            《官途浮沉》— 模拟结束报告            ║");
            Debug.Log("╠══════════════════════════════════════════════════╣");
            Debug.Log($"║  玩家：{player.PlayerName}");
            Debug.Log($"║  最终官职：{rankData?.DisplayName ?? player.CurrentRank.ToString()}");
            Debug.Log($"║  存活回合：{player.CurrentTurn}");
            Debug.Log($"║  才学:{player.Intellect} 人望:{player.Charisma} " +
                      $"权谋:{player.Scheming} 武略:{player.Martial}");
            Debug.Log($"║  体魄:{player.Health} 声望:{player.Reputation}");
            Debug.Log($"║  金银:{player.Gold} 影响力:{player.Influence}");
            Debug.Log("║");
            Debug.Log("║  NPC最终关系：");

            foreach (var rel in run.Relationships)
            {
                string name = GetNPCName(rel.NpcId);
                Debug.Log($"║    {name}：好感{rel.Favor} 信任{rel.Trust}");
            }

            Debug.Log("║");
            Debug.Log($"║  活跃标记：{string.Join(", ", run.ActiveFlags)}");
            Debug.Log("╚══════════════════════════════════════════════════╝");
        }

        // ======================== 辅助方法 ========================

        private void LogSection(string title)
        {
            Debug.Log($"━━━━ {title} ━━━━");
        }

        private string GetNPCName(string npcId)
        {
            var npc = _allNPCs?.Find(n => n.NpcId == npcId);
            return npc?.DisplayName ?? npcId;
        }

        private string GetPhaseName(TurnPhase phase)
        {
            return phase switch
            {
                TurnPhase.MorningBriefing => "☀️ 早朝简报",
                TurnPhase.ActionAllocation => "📋 行动分配",
                TurnPhase.Execution => "⚔️ 执行结算",
                TurnPhase.Review => "📖 复盘总结",
                _ => phase.ToString()
            };
        }

        private string GetActionName(ActionType type)
        {
            return type switch
            {
                ActionType.Politics => "政务",
                ActionType.Social => "交际",
                ActionType.Intelligence => "情报",
                ActionType.SelfImprove => "修身",
                ActionType.Scheme => "谋略",
                ActionType.Rest => "休息",
                _ => type.ToString()
            };
        }

        private string GetAttrName(PlayerAttribute attr)
        {
            return attr switch
            {
                PlayerAttribute.Intellect => "才学",
                PlayerAttribute.Charisma => "人望",
                PlayerAttribute.Scheming => "权谋",
                PlayerAttribute.Martial => "武略",
                PlayerAttribute.Health => "体魄",
                PlayerAttribute.Reputation => "声望",
                _ => attr.ToString()
            };
        }
    }
}
