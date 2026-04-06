// ============================================================================
// 官途浮沉 - NPC关系图谱系统
// NPCRelationshipGraph.cs — 管理所有NPC的好感度/信任度双维度关系
// ============================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GuantuFucheng.Core;
using GuantuFucheng.Data;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;
using GuantuFucheng.Models;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// NPC关系图谱 — 管理玩家与所有NPC的双维度关系
    /// 
    /// 核心设计：
    /// 好感度(Favor)与信任度(Trust)是两个独立维度：
    /// 
    /// ┌──────────────┬────────────┬────────────────┐
    /// │              │ 高信任      │ 低信任          │
    /// ├──────────────┼────────────┼────────────────┤
    /// │ 高好感       │ 铁杆盟友    │ 酒肉朋友        │
    /// │ 低好感       │ 敬而远之    │ 宿敌           │
    /// └──────────────┴────────────┴────────────────┘
    /// 
    /// 关系里程碑：当好感/信任达到特定阈值时，解锁特殊互动和事件
    /// NPC之间也有关系网络，影响阵营政治
    /// </summary>
    public class NPCRelationshipGraph : Singleton<NPCRelationshipGraph>
    {
        // ======================== 数据 ========================

        [Header("NPC数据库")]
        [SerializeField] private List<NPCData> _allNPCs = new List<NPCData>();

        /// <summary>NPC之间的关系（NPC A对NPC B的态度）</summary>
        private Dictionary<string, Dictionary<string, int>> _npcToNpcRelations
            = new Dictionary<string, Dictionary<string, int>>();

        // ======================== 关系阈值常量 ========================

        private const int FAVOR_ALLY_THRESHOLD = 60;        // 好感≥60：可结盟
        private const int FAVOR_ENEMY_THRESHOLD = -40;      // 好感≤-40：成为政敌
        private const int TRUST_CONFIDE_THRESHOLD = 50;     // 信任≥50：可以托付机密
        private const int TRUST_BETRAY_THRESHOLD = -30;     // 信任≤-30：可能背叛你

        // ======================== 生命周期 ========================

        private void OnEnable()
        {
            GameEvents.OnNewGameStarted += OnNewGame;
            GameEvents.OnTurnEnded += OnTurnEnded;
        }

        private void OnDisable()
        {
            GameEvents.OnNewGameStarted -= OnNewGame;
            GameEvents.OnTurnEnded -= OnTurnEnded;
        }

        private void OnNewGame()
        {
            InitializeRelationships();
        }

        private void OnTurnEnded(int turn)
        {
            // 每回合结束进行自然衰减和NPC自主行为
            ProcessPassiveChanges();
            ProcessNPCAutonomousActions();
        }

        // ======================== 公开方法 ========================

        /// <summary>
        /// 修改玩家与NPC的关系
        /// </summary>
        public void ModifyRelationship(string npcId, int favorDelta, int trustDelta)
        {
            var run = GameManager.Instance.CurrentRun;
            var rel = run.GetOrCreateRelationship(npcId);

            int oldFavor = rel.Favor;
            int oldTrust = rel.Trust;

            rel.Favor = Mathf.Clamp(rel.Favor + favorDelta, -100, 100);
            rel.Trust = Mathf.Clamp(rel.Trust + trustDelta, -100, 100);

            if (favorDelta != 0)
                GameEvents.FavorChanged(npcId, favorDelta, rel.Favor);
            if (trustDelta != 0)
                GameEvents.TrustChanged(npcId, trustDelta, rel.Trust);

            // 检查关系里程碑
            CheckMilestones(npcId, oldFavor, rel.Favor, oldTrust, rel.Trust);

            Debug.Log($"[NPC关系] {GetNPCName(npcId)}：好感{oldFavor}→{rel.Favor}，信任{oldTrust}→{rel.Trust}");
        }

        /// <summary>获取NPC的好感度</summary>
        public int GetFavor(string npcId)
        {
            var rel = GameManager.Instance.CurrentRun.GetRelationship(npcId);
            return rel?.Favor ?? 0;
        }

        /// <summary>获取NPC的信任度</summary>
        public int GetTrust(string npcId)
        {
            var rel = GameManager.Instance.CurrentRun.GetRelationship(npcId);
            return rel?.Trust ?? 0;
        }

        /// <summary>获取所有盟友（好感≥60）</summary>
        public List<string> GetAllies()
        {
            return GameManager.Instance.CurrentRun.Relationships
                .Where(r => r.Favor >= FAVOR_ALLY_THRESHOLD)
                .Select(r => r.NpcId)
                .ToList();
        }

        /// <summary>获取所有政敌（好感≤-40）</summary>
        public List<string> GetEnemies()
        {
            return GameManager.Instance.CurrentRun.Relationships
                .Where(r => r.Favor <= FAVOR_ENEMY_THRESHOLD)
                .Select(r => r.NpcId)
                .ToList();
        }

        /// <summary>获取NPC间的关系值</summary>
        public int GetNPCToNPCRelation(string npcA, string npcB)
        {
            if (_npcToNpcRelations.TryGetValue(npcA, out var relations))
            {
                if (relations.TryGetValue(npcB, out var value))
                    return value;
            }
            return 0;
        }

        /// <summary>获取指定NPC的数据</summary>
        public NPCData GetNPCData(string npcId)
        {
            return _allNPCs.Find(n => n.NpcId == npcId);
        }

        /// <summary>获取NPC显示名称</summary>
        public string GetNPCName(string npcId)
        {
            return GetNPCData(npcId)?.DisplayName ?? npcId;
        }

        /// <summary>揭示NPC的真实性格</summary>
        public void RevealNPC(string npcId)
        {
            var rel = GameManager.Instance.CurrentRun.GetOrCreateRelationship(npcId);
            if (!rel.IsRevealed)
            {
                rel.IsRevealed = true;
                Debug.Log($"[NPC关系] 已探明 {GetNPCName(npcId)} 的真实性格！");
            }
        }

        // ======================== 内部方法 ========================

        /// <summary>初始化所有NPC的关系</summary>
        private void InitializeRelationships()
        {
            var run = GameManager.Instance.CurrentRun;
            run.Relationships.Clear();

            foreach (var npc in _allNPCs)
            {
                if (!npc.StartsActive) continue;

                run.Relationships.Add(new NPCRelationshipState
                {
                    NpcId = npc.NpcId,
                    Favor = npc.InitialFavor,
                    Trust = npc.InitialTrust,
                    IsRevealed = false
                });
            }

            // 初始化NPC之间的关系（基于阵营）
            InitializeNPCToNPCRelations();

            Debug.Log($"[NPC关系] 初始化完成，共{run.Relationships.Count}个NPC");
        }

        /// <summary>根据阵营自动生成NPC间关系</summary>
        private void InitializeNPCToNPCRelations()
        {
            _npcToNpcRelations.Clear();

            for (int i = 0; i < _allNPCs.Count; i++)
            {
                for (int j = i + 1; j < _allNPCs.Count; j++)
                {
                    var a = _allNPCs[i];
                    var b = _allNPCs[j];

                    // 同阵营：正关系；不同阵营：负关系或中立
                    int relation = 0;
                    if (a.Faction == b.Faction && a.Faction != Faction.Neutral)
                        relation = Random.Range(20, 60);
                    else if (a.Faction != Faction.Neutral && b.Faction != Faction.Neutral)
                        relation = Random.Range(-40, 10);

                    SetNPCRelation(a.NpcId, b.NpcId, relation);
                    SetNPCRelation(b.NpcId, a.NpcId, relation);
                }
            }
        }

        private void SetNPCRelation(string from, string to, int value)
        {
            if (!_npcToNpcRelations.ContainsKey(from))
                _npcToNpcRelations[from] = new Dictionary<string, int>();
            _npcToNpcRelations[from][to] = value;
        }

        /// <summary>每回合自然衰减/增长</summary>
        private void ProcessPassiveChanges()
        {
            var run = GameManager.Instance.CurrentRun;

            foreach (var rel in run.Relationships)
            {
                // 好感度自然趋向0（每回合衰减1）
                if (rel.Favor > 0) rel.Favor = Mathf.Max(0, rel.Favor - 1);
                else if (rel.Favor < 0) rel.Favor = Mathf.Min(0, rel.Favor + 1);

                // 信任度更稳定，每3回合衰减1
                if (run.Player.CurrentTurn % 3 == 0)
                {
                    if (rel.Trust > 0) rel.Trust = Mathf.Max(0, rel.Trust - 1);
                    else if (rel.Trust < 0) rel.Trust = Mathf.Min(0, rel.Trust + 1);
                }
            }
        }

        /// <summary>NPC自主行为（每回合结束时）</summary>
        private void ProcessNPCAutonomousActions()
        {
            // TODO: NPC基于性格和关系值主动采取行动
            // 例如：政敌可能发起弹劾、盟友可能提供情报
        }

        /// <summary>检查关系里程碑</summary>
        private void CheckMilestones(string npcId, int oldFavor, int newFavor, int oldTrust, int newTrust)
        {
            // 好感突破盟友阈值
            if (oldFavor < FAVOR_ALLY_THRESHOLD && newFavor >= FAVOR_ALLY_THRESHOLD)
            {
                GameEvents.RelationshipMilestone(npcId, "成为盟友");
                Debug.Log($"[NPC关系] 🤝 {GetNPCName(npcId)} 已成为你的盟友！");
            }

            // 好感跌破政敌阈值
            if (oldFavor > FAVOR_ENEMY_THRESHOLD && newFavor <= FAVOR_ENEMY_THRESHOLD)
            {
                GameEvents.RelationshipMilestone(npcId, "成为政敌");
                Debug.Log($"[NPC关系] ⚔️ {GetNPCName(npcId)} 已成为你的政敌！");
            }

            // 信任突破托付阈值
            if (oldTrust < TRUST_CONFIDE_THRESHOLD && newTrust >= TRUST_CONFIDE_THRESHOLD)
            {
                GameEvents.RelationshipMilestone(npcId, "可以托付机密");
                Debug.Log($"[NPC关系] 🔑 {GetNPCName(npcId)} 对你信任有加，可以托付机密");
            }

            // 信任跌破背叛阈值
            if (oldTrust > TRUST_BETRAY_THRESHOLD && newTrust <= TRUST_BETRAY_THRESHOLD)
            {
                GameEvents.RelationshipMilestone(npcId, "可能背叛");
                Debug.Log($"[NPC关系] ⚠️ {GetNPCName(npcId)} 对你极度不信任，随时可能背叛！");
            }
        }
    }
}
