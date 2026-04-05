// ============================================================================
// 官途浮沉 - 回合管理器
// TurnManager.cs — 控制回合制流程：早朝简报→行动分配→执行→复盘
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;
using GuantuFucheng.Models;

namespace GuantuFucheng.Core
{
    /// <summary>
    /// 回合管理器 — 游戏核心循环的指挥官
    /// 
    /// 每回合四个阶段，严格顺序执行：
    /// 
    /// 1. 早朝简报 (MorningBriefing)
    ///    - 展示当前回合数、官职、局势
    ///    - 触发随机事件
    ///    - NPC主动行为（可能来找你、弹劾你、结盟邀请等）
    ///    - 生成本回合可用的决策卡牌
    /// 
    /// 2. 行动分配 (ActionAllocation)
    ///    - 玩家拥有6点行动力（可通过Meta升级增加）
    ///    - 将行动力分配到：政务/交际/情报/修身/谋略/休息
    ///    - 预览每项行动的期望收益
    ///    - 确认后进入执行
    /// 
    /// 3. 执行结算 (Execution)
    ///    - 依次执行已分配的行动
    ///    - 每项行动可能触发对应的卡牌事件
    ///    - 计算成功率（基于属性+随机因子）
    ///    - NPC关系变化
    ///    - 属性变化
    /// 
    /// 4. 复盘总结 (Review)
    ///    - 展示本回合所有变化（属性增减、关系变动）
    ///    - 检查升迁/贬官条件
    ///    - 检查游戏结束条件
    ///    - 自动保存
    /// </summary>
    public class TurnManager : Singleton<TurnManager>
    {
        // ======================== 状态 ========================

        /// <summary>当前回合数</summary>
        public int CurrentTurnNumber { get; private set; } = 0;

        /// <summary>当前阶段</summary>
        public TurnPhase CurrentPhase { get; private set; }

        /// <summary>本回合的行动力分配方案</summary>
        public Dictionary<ActionType, int> ActionAllocation { get; private set; }
            = new Dictionary<ActionType, int>();

        /// <summary>本回合触发的事件卡牌ID列表</summary>
        public List<string> TurnCards { get; private set; } = new List<string>();

        // ======================== 公开方法 ========================

        /// <summary>开始新的一回合</summary>
        public void StartNewTurn()
        {
            CurrentTurnNumber++;
            var run = GameManager.Instance.CurrentRun;
            run.Player.CurrentTurn = CurrentTurnNumber;

            Debug.Log($"[TurnManager] ===== 第 {CurrentTurnNumber} 回合开始 =====");

            // 重置行动力
            run.Player.CurrentActionPoints = run.Player.MaxActionPoints;
            ActionAllocation.Clear();
            TurnCards.Clear();

            GameEvents.TurnStarted(CurrentTurnNumber);

            // 进入早朝简报阶段
            EnterPhase(TurnPhase.MorningBriefing);
        }

        /// <summary>
        /// 推进到下一个阶段
        /// UI层在玩家确认后调用此方法
        /// </summary>
        public void AdvancePhase()
        {
            switch (CurrentPhase)
            {
                case TurnPhase.MorningBriefing:
                    EnterPhase(TurnPhase.ActionAllocation);
                    break;

                case TurnPhase.ActionAllocation:
                    if (ValidateAllocation())
                    {
                        EnterPhase(TurnPhase.Execution);
                    }
                    else
                    {
                        Debug.LogWarning("[TurnManager] 行动力未分配完毕");
                    }
                    break;

                case TurnPhase.Execution:
                    EnterPhase(TurnPhase.Review);
                    break;

                case TurnPhase.Review:
                    // 检查游戏结束条件
                    var endReason = CheckGameOverConditions();
                    if (endReason.HasValue)
                    {
                        GameEvents.GameOver(endReason.Value);
                    }
                    else
                    {
                        // 自动保存后开始下一回合
                        GameManager.Instance.SaveGame();
                        StartNewTurn();
                    }
                    break;
            }
        }

        /// <summary>
        /// 分配行动力到指定行动类型
        /// </summary>
        /// <param name="type">行动类型</param>
        /// <param name="points">分配的点数</param>
        /// <returns>是否分配成功</returns>
        public bool AllocateAction(ActionType type, int points)
        {
            if (CurrentPhase != TurnPhase.ActionAllocation)
            {
                Debug.LogWarning("[TurnManager] 当前不在行动分配阶段");
                return false;
            }

            var player = GameManager.Instance.CurrentRun.Player;
            int alreadyAllocated = GetTotalAllocated();
            int remaining = player.MaxActionPoints - alreadyAllocated;

            if (points > remaining)
            {
                Debug.LogWarning($"[TurnManager] 行动力不足：需要{points}，剩余{remaining}");
                return false;
            }

            if (!ActionAllocation.ContainsKey(type))
                ActionAllocation[type] = 0;

            ActionAllocation[type] += points;
            player.CurrentActionPoints = player.MaxActionPoints - GetTotalAllocated();

            Debug.Log($"[TurnManager] 分配 {points} 点到 {type}，剩余 {player.CurrentActionPoints}");
            GameEvents.ActionPointAllocated(type, ActionAllocation[type]);

            return true;
        }

        /// <summary>重置行动力分配</summary>
        public void ResetAllocation()
        {
            ActionAllocation.Clear();
            var player = GameManager.Instance.CurrentRun.Player;
            player.CurrentActionPoints = player.MaxActionPoints;
            Debug.Log("[TurnManager] 行动分配已重置");
        }

        // ======================== 内部方法 ========================

        private void EnterPhase(TurnPhase phase)
        {
            CurrentPhase = phase;
            Debug.Log($"[TurnManager] 进入阶段：{phase}");
            GameEvents.PhaseChanged(phase);

            switch (phase)
            {
                case TurnPhase.MorningBriefing:
                    ExecuteMorningBriefing();
                    break;
                case TurnPhase.Execution:
                    ExecuteActions();
                    break;
                case TurnPhase.Review:
                    ExecuteReview();
                    break;
            }
        }

        /// <summary>早朝简报阶段逻辑</summary>
        private void ExecuteMorningBriefing()
        {
            var run = GameManager.Instance.CurrentRun;

            // 1. 每回合基础收入（俸禄）
            // TODO: 从OfficialRankData获取具体数值
            run.Player.Gold += 10;

            // 2. 触发随机事件
            // TODO: EventSystem检查并触发事件

            // 3. 卡牌系统生成本回合卡牌
            // TODO: CardSystem.DrawCards()

            // 4. NPC行为AI
            // TODO: NPCRelationshipGraph.ProcessNPCActions()

            Debug.Log("[TurnManager] 早朝简报完成，等待玩家确认");
            // UI层通过 GameEvents.OnPhaseChanged 自动收到通知
            // UIManager.HandlePhaseChanged → ShowMorningBriefing()
            // MorningBriefingPanel 展示简报内容
            // 玩家点击"上朝"后，MorningBriefingPanel 调用 AdvancePhase()
        }

        /// <summary>执行已分配的行动</summary>
        private void ExecuteActions()
        {
            var run = GameManager.Instance.CurrentRun;

            foreach (var kvp in ActionAllocation)
            {
                ActionType actionType = kvp.Key;
                int points = kvp.Value;

                if (points <= 0) continue;

                Debug.Log($"[TurnManager] 执行行动：{actionType} × {points}");

                // 将行动交给ActionPointSystem处理
                Systems.ActionPointSystem.Instance.ExecuteAction(actionType, points);
            }

            Debug.Log("[TurnManager] 所有行动执行完毕");
        }

        /// <summary>回合复盘逻辑</summary>
        private void ExecuteReview()
        {
            var run = GameManager.Instance.CurrentRun;

            // 1. 检查升迁条件
            Systems.OfficialRankSystem.Instance.CheckPromotion();

            // 2. NPC关系衰减/增长
            // TODO: 自然衰减机制

            // 3. 生成回合总结数据供UI展示
            Debug.Log($"[TurnManager] 第{CurrentTurnNumber}回合复盘完成");

            GameEvents.TurnEnded(CurrentTurnNumber);
        }

        /// <summary>验证行动力是否全部分配</summary>
        private bool ValidateAllocation()
        {
            int total = GetTotalAllocated();
            int max = GameManager.Instance.CurrentRun.Player.MaxActionPoints;
            // 允许不用完（但至少分配1点）
            return total >= 1;
        }

        /// <summary>获取已分配的总行动力</summary>
        private int GetTotalAllocated()
        {
            int total = 0;
            foreach (var kvp in ActionAllocation)
                total += kvp.Value;
            return total;
        }

        /// <summary>检查游戏结束条件</summary>
        private GameOverReason? CheckGameOverConditions()
        {
            var player = GameManager.Instance.CurrentRun.Player;

            // 体魄归零 → 暴毙
            if (player.Health <= 0)
                return GameOverReason.Executed;

            // 声望过低 → 被弹劾
            if (player.Reputation <= -50)
                return GameOverReason.Impeached;

            // 达到宰相 → 通关
            if (player.CurrentRank == OfficialRank.GrandCouncilor)
                return GameOverReason.Victory;

            return null;
        }
    }
}
