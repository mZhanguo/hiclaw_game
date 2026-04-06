// ============================================================================
// 官途浮沉 - 行动力系统
// ActionPointSystem.cs — 管理6点行动力的分配与执行
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Core;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;
using GuantuFucheng.Data;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// 行动力系统
    /// 
    /// 核心机制：
    /// - 每回合6点行动力（基础值，可通过Meta升级和官职加成增加）
    /// - 每点行动力投入到一个行动方向，投入越多效果越强
    /// - 行动结果 = 基础效果 × 属性加成 × 随机因子
    /// 
    /// 行动类型与效果：
    /// - 政务(Politics)：+声望，+影响力，可能触发治理事件
    /// - 交际(Social)：+NPC好感，可能获得情报/支持
    /// - 情报(Intelligence)：揭示NPC真实性格，发现威胁/机会
    /// - 修身(SelfImprove)：+才学/武略/体魄，长期投资
    /// - 谋略(Scheme)：执行暗中布局，高风险高回报
    /// - 休息(Rest)：+体魄恢复，但可能错过事件
    /// </summary>
    public class ActionPointSystem : Singleton<ActionPointSystem>
    {
        // ======================== 配置 ========================

        [Header("行动效果基础值")]
        [SerializeField] private int _basePoliticsReputation = 3;
        [SerializeField] private int _baseSocialFavor = 5;
        [SerializeField] private int _baseIntelligenceRevealChance = 20;  // 百分比
        [SerializeField] private int _baseSelfImproveGain = 2;
        [SerializeField] private int _baseSchemeSuccessChance = 40;       // 百分比
        [SerializeField] private int _baseRestHeal = 10;

        // ======================== 公开方法 ========================

        /// <summary>
        /// 执行某类行动
        /// 由TurnManager在执行阶段调用
        /// </summary>
        /// <param name="type">行动类型</param>
        /// <param name="points">投入的行动力点数</param>
        public void ExecuteAction(ActionType type, int points)
        {
            var player = GameManager.Instance.CurrentRun.Player;

            switch (type)
            {
                case ActionType.Politics:
                    ExecutePolitics(player, points);
                    break;
                case ActionType.Social:
                    ExecuteSocial(player, points);
                    break;
                case ActionType.Intelligence:
                    ExecuteIntelligence(player, points);
                    break;
                case ActionType.SelfImprove:
                    ExecuteSelfImprove(player, points);
                    break;
                case ActionType.Scheme:
                    ExecuteScheme(player, points);
                    break;
                case ActionType.Rest:
                    ExecuteRest(player, points);
                    break;
            }
        }

        /// <summary>
        /// 预览行动的期望效果（供UI展示）
        /// </summary>
        public string GetActionPreview(ActionType type, int points)
        {
            var player = GameManager.Instance.CurrentRun.Player;

            return type switch
            {
                ActionType.Politics =>
                    $"预计：声望+{CalculateReputationGain(player, points)}，影响力+{points}",
                ActionType.Social =>
                    $"预计：好感度变化约+{points * _baseSocialFavor}",
                ActionType.Intelligence =>
                    $"探查成功率：{CalculateIntelSuccessRate(player, points)}%",
                ActionType.SelfImprove =>
                    $"预计：随机属性+{points * _baseSelfImproveGain}",
                ActionType.Scheme =>
                    $"谋略成功率：{CalculateSchemeSuccessRate(player, points)}%",
                ActionType.Rest =>
                    $"预计：体魄恢复+{points * _baseRestHeal}",
                _ => "未知行动"
            };
        }

        // ======================== 各行动执行逻辑 ========================

        private void ExecutePolitics(Models.PlayerState player, int points)
        {
            int repGain = CalculateReputationGain(player, points);
            player.ModifyAttribute(PlayerAttribute.Reputation, repGain);
            player.Influence += points;

            GameEvents.AttributeChanged(PlayerAttribute.Reputation, repGain, player.Reputation);
            Debug.Log($"[ActionPoint] 政务：声望+{repGain}，影响力+{points}");

            // 概率触发政务相关卡牌
            if (Random.value < 0.3f * points)
            {
                // TODO: CardSystem.DrawCardOfType(CardType.Event, ActionType.Politics)
                Debug.Log("[ActionPoint] 政务触发了一个事件！");
            }
        }

        private void ExecuteSocial(Models.PlayerState player, int points)
        {
            // 选择一个NPC进行社交（TODO: 由UI选择目标NPC）
            // 这里先实现基础逻辑框架
            int favorGain = points * _baseSocialFavor;
            int charismaBonus = player.Charisma / 10;
            favorGain += charismaBonus;

            Debug.Log($"[ActionPoint] 交际：好感度预计+{favorGain}（含人望加成{charismaBonus}）");

            // TODO: NPCRelationshipGraph.ModifyFavor(targetNpcId, favorGain)
        }

        private void ExecuteIntelligence(Models.PlayerState player, int points)
        {
            int successRate = CalculateIntelSuccessRate(player, points);
            bool success = Random.Range(0, 100) < successRate;

            if (success)
            {
                Debug.Log("[ActionPoint] 情报收集成功！");
                // TODO: 揭示随机NPC的真实性格，或发现一张情报卡
            }
            else
            {
                Debug.Log("[ActionPoint] 情报收集未获成果");
                // 失败有一定概率被发现
                if (Random.value < 0.15f)
                {
                    Debug.Log("[ActionPoint] 糟糕！刺探被目标NPC察觉！");
                    // TODO: 降低目标NPC信任度
                }
            }
        }

        private void ExecuteSelfImprove(Models.PlayerState player, int points)
        {
            int totalGain = points * _baseSelfImproveGain;

            // 随机分配到才学、武略中
            for (int i = 0; i < totalGain; i++)
            {
                PlayerAttribute attr = Random.value > 0.5f
                    ? PlayerAttribute.Intellect
                    : PlayerAttribute.Martial;

                int delta = player.ModifyAttribute(attr, 1);
                GameEvents.AttributeChanged(attr, delta, player.GetAttribute(attr));
            }

            Debug.Log($"[ActionPoint] 修身：属性共提升{totalGain}点");
        }

        private void ExecuteScheme(Models.PlayerState player, int points)
        {
            int successRate = CalculateSchemeSuccessRate(player, points);
            bool success = Random.Range(0, 100) < successRate;

            if (success)
            {
                Debug.Log("[ActionPoint] 谋略执行成功！");
                player.Influence += points * 2;
                // TODO: 触发谋略成功事件卡
            }
            else
            {
                Debug.Log("[ActionPoint] 谋略失败！");
                int repLoss = -points * 2;
                player.ModifyAttribute(PlayerAttribute.Reputation, repLoss);
                GameEvents.AttributeChanged(PlayerAttribute.Reputation, repLoss, player.Reputation);
                // 谋略失败可能暴露
            }
        }

        private void ExecuteRest(Models.PlayerState player, int points)
        {
            int heal = points * _baseRestHeal;
            int actual = player.ModifyAttribute(PlayerAttribute.Health, heal);
            GameEvents.AttributeChanged(PlayerAttribute.Health, actual, player.Health);
            Debug.Log($"[ActionPoint] 休息：体魄恢复+{actual}");
        }

        // ======================== 辅助计算 ========================

        private int CalculateReputationGain(Models.PlayerState player, int points)
        {
            return _basePoliticsReputation * points + player.Intellect / 10;
        }

        private int CalculateIntelSuccessRate(Models.PlayerState player, int points)
        {
            return Mathf.Clamp(
                _baseIntelligenceRevealChance * points + player.Scheming,
                0, 95
            );
        }

        private int CalculateSchemeSuccessRate(Models.PlayerState player, int points)
        {
            return Mathf.Clamp(
                _baseSchemeSuccessChance + points * 10 + player.Scheming - 10,
                5, 90
            );
        }
    }
}
