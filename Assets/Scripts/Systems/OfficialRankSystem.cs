// ============================================================================
// 官途浮沉 - 官职升迁系统
// OfficialRankSystem.cs — 管理官职等级、升迁条件检查、路线分支
// ============================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GuantuFucheng.Core;
using GuantuFucheng.Data;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// 官职升迁系统
    /// 
    /// 升迁路线：
    /// 
    ///   候补 → 县令 → 州刺史 → 侍郎 ─┬→ 尚书（文官路线）──→ 宰相
    ///                                   └→ 节度使（武将路线）─→ 宰相
    /// 
    /// 升迁条件：
    /// 1. 在当前官职待满最少回合数
    /// 2. 满足目标官职的属性要求
    /// 3. 声望和影响力达标
    /// 4. 通过升迁考核事件（可能被NPC阻挠或支持）
    /// 
    /// 降职/罢官：
    /// - 声望过低触发弹劾
    /// - 得罪皇帝直接贬官
    /// - 卷入政治风波被问责
    /// </summary>
    public class OfficialRankSystem : Singleton<OfficialRankSystem>
    {
        // ======================== 数据 ========================

        [Header("官职数据")]
        [SerializeField] private List<OfficialRankData> _rankDataList = new List<OfficialRankData>();

        /// <summary>在当前官职已待的回合数</summary>
        private int _turnsAtCurrentRank = 0;

        // ======================== 生命周期 ========================

        private void OnEnable()
        {
            GameEvents.OnNewGameStarted += OnNewGame;
            GameEvents.OnTurnStarted += OnTurnStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnNewGameStarted -= OnNewGame;
            GameEvents.OnTurnStarted -= OnTurnStarted;
        }

        private void OnNewGame()
        {
            _turnsAtCurrentRank = 0;
        }

        private void OnTurnStarted(int turn)
        {
            _turnsAtCurrentRank++;
            ApplyRankBonuses();
        }

        // ======================== 公开方法 ========================

        /// <summary>获取指定官职的数据</summary>
        public OfficialRankData GetRankData(OfficialRank rank)
        {
            return _rankDataList.Find(r => r.Rank == rank);
        }

        /// <summary>获取当前官职数据</summary>
        public OfficialRankData GetCurrentRankData()
        {
            var player = GameManager.Instance.CurrentRun.Player;
            return GetRankData(player.CurrentRank);
        }

        /// <summary>
        /// 检查是否满足升迁条件（在回合复盘阶段调用）
        /// </summary>
        public void CheckPromotion()
        {
            var player = GameManager.Instance.CurrentRun.Player;
            var currentRankData = GetCurrentRankData();

            if (currentRankData == null)
            {
                Debug.LogWarning("[官职系统] 找不到当前官职数据");
                return;
            }

            // 已经是最高官职
            if (player.CurrentRank == OfficialRank.GrandCouncilor) return;

            // 检查在位时间
            if (_turnsAtCurrentRank < currentRankData.MinTurnsAtRank)
            {
                Debug.Log($"[官职系统] 在位时间不足：{_turnsAtCurrentRank}/{currentRankData.MinTurnsAtRank}");
                return;
            }

            // 检查声望
            if (player.Reputation < currentRankData.RequiredReputation)
            {
                Debug.Log($"[官职系统] 声望不足：{player.Reputation}/{currentRankData.RequiredReputation}");
                return;
            }

            // 检查影响力
            if (player.Influence < currentRankData.RequiredInfluence)
            {
                Debug.Log($"[官职系统] 影响力不足：{player.Influence}/{currentRankData.RequiredInfluence}");
                return;
            }

            // 检查属性要求
            foreach (var req in currentRankData.PromotionRequirements)
            {
                if (player.GetAttribute(req.Attribute) < req.MinValue)
                {
                    Debug.Log($"[官职系统] {req.Attribute}不足：{player.GetAttribute(req.Attribute)}/{req.MinValue}");
                    return;
                }
            }

            // 所有条件满足，触发升迁考核
            Debug.Log("[官职系统] ✨ 升迁条件满足！触发考核事件...");
            GameEvents.PromotionCheck();

            // 简化处理：直接升迁（后续应触发升迁卡牌事件）
            var paths = currentRankData.PromotionPaths;
            if (paths.Count > 0)
            {
                // 如果有分支路线，取第一个（后续应由玩家选择或根据属性决定）
                OfficialRank targetRank = paths[0];

                // 侍郎之后的分支：根据武略决定文武路线
                if (player.CurrentRank == OfficialRank.ViceMinister && paths.Count > 1)
                {
                    targetRank = player.Martial > player.Intellect
                        ? OfficialRank.MilitaryGovernor  // 武略高 → 节度使
                        : OfficialRank.Minister;          // 才学高 → 尚书
                }

                Promote(targetRank);
            }
        }

        /// <summary>
        /// 执行升迁
        /// </summary>
        public void Promote(OfficialRank newRank)
        {
            var player = GameManager.Instance.CurrentRun.Player;
            var oldRank = player.CurrentRank;

            if (newRank <= oldRank)
            {
                Debug.LogWarning($"[官职系统] 无法升迁到同级或更低官职：{oldRank} → {newRank}");
                return;
            }

            player.CurrentRank = newRank;
            _turnsAtCurrentRank = 0;

            Debug.Log($"[官职系统] 🎉 升迁！{oldRank} → {newRank}");
            GameEvents.RankChanged(oldRank, newRank);
        }

        /// <summary>
        /// 执行贬官
        /// </summary>
        public void Demote(OfficialRank newRank)
        {
            var player = GameManager.Instance.CurrentRun.Player;
            var oldRank = player.CurrentRank;

            if (newRank >= oldRank)
            {
                Debug.LogWarning("[官职系统] 贬官目标不能高于当前官职");
                return;
            }

            player.CurrentRank = newRank;
            _turnsAtCurrentRank = 0;

            Debug.Log($"[官职系统] 😰 被贬！{oldRank} → {newRank}");
            GameEvents.RankChanged(oldRank, newRank);
        }

        /// <summary>
        /// 检查弹劾风险（每回合早朝阶段调用）
        /// </summary>
        /// <returns>是否被弹劾</returns>
        public bool CheckImpeachment()
        {
            var player = GameManager.Instance.CurrentRun.Player;
            var rankData = GetCurrentRankData();
            if (rankData == null) return false;

            // 基础弹劾概率 + 声望影响
            float impeachChance = rankData.ImpeachmentBaseChance;
            if (player.Reputation < 0)
                impeachChance += Mathf.Abs(player.Reputation) * 0.005f;

            // 政敌数量增加弹劾概率
            int enemies = NPCRelationshipGraph.Instance.GetEnemies().Count;
            impeachChance += enemies * 0.03f;

            // 盟友减少弹劾概率
            int allies = NPCRelationshipGraph.Instance.GetAllies().Count;
            impeachChance -= allies * 0.02f;

            impeachChance = Mathf.Clamp(impeachChance, 0.01f, 0.8f);

            if (Random.value < impeachChance)
            {
                Debug.Log($"[官职系统] ⚠️ 遭到弹劾！概率为{impeachChance:P1}");
                // TODO: 触发弹劾事件卡，玩家有机会应对
                return true;
            }

            return false;
        }

        // ======================== 内部方法 ========================

        /// <summary>应用当前官职的每回合加成</summary>
        private void ApplyRankBonuses()
        {
            var player = GameManager.Instance.CurrentRun.Player;
            var rankData = GetCurrentRankData();
            if (rankData == null) return;

            player.Gold += rankData.BonusGoldPerTurn;
            player.Influence += rankData.BonusInfluence;

            // 官职带来的额外行动力
            player.MaxActionPoints = 6 + rankData.BonusActionPoints
                + GameManager.Instance.MetaProgress.GetUpgradeLevel("extra_ap");
        }
    }
}
