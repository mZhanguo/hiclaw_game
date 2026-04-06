// ============================================================================
// 官途浮沉 - 事件数据定义
// EventData.cs — ScriptableObject，定义随机事件和剧情事件
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Enums;

namespace GuantuFucheng.Data
{
    /// <summary>
    /// 事件结果
    /// </summary>
    [Serializable]
    public class EventOutcome
    {
        [TextArea(2, 4)]
        public string NarrativeText;        // 结果描述文本
        public List<AttributeModifier> AttributeChanges = new List<AttributeModifier>();
        public List<RelationshipModifier> RelationshipChanges = new List<RelationshipModifier>();
        public List<string> SetFlags = new List<string>();
        public List<string> TriggerCardIds = new List<string>();    // 后续触发的卡牌
        public int GoldChange = 0;
        public int InfluenceChange = 0;
    }

    /// <summary>
    /// 事件数据 ScriptableObject
    /// 与CardData的区别：Event是系统自动触发的环境事件
    /// Card是玩家交互的决策卡
    /// 
    /// 例如：
    /// - "黄河决堤" → 全局事件，影响所有玩家属性
    /// - "皇帝驾崩" → 剧情事件，改变政治格局
    /// - "科举开考" → 周期事件，每N回合触发
    /// </summary>
    [CreateAssetMenu(fileName = "NewEvent", menuName = "GuantuFucheng/Event Data")]
    public class EventData : ScriptableObject
    {
        [Header("基础信息")]
        public string EventId;
        public string Title;

        [TextArea(3, 8)]
        public string Description;

        [Header("触发条件")]
        [Tooltip("触发概率（每回合检查）")]
        [Range(0f, 1f)]
        public float TriggerChance = 0.1f;

        [Tooltip("是否为一次性事件")]
        public bool OneTimeOnly = true;

        [Tooltip("周期触发间隔（0=非周期事件）")]
        public int CycleTurns = 0;

        public int MinTurn = 1;
        public int MaxTurn = 0;
        public OfficialRank MinRank = OfficialRank.Candidate;
        public List<string> RequiredFlags = new List<string>();
        public List<string> ExcludedFlags = new List<string>();

        [Header("事件效果")]
        [Tooltip("事件触发后的结果")]
        public EventOutcome Outcome;

        [Header("关联")]
        [Tooltip("事件触发后弹出的决策卡（让玩家应对此事件）")]
        public string ResponseCardId;

        /// <summary>检查事件是否可以触发</summary>
        public bool CanTrigger(int currentTurn, OfficialRank rank, List<string> flags)
        {
            if (currentTurn < MinTurn) return false;
            if (MaxTurn > 0 && currentTurn > MaxTurn) return false;
            if (rank < MinRank) return false;

            foreach (var f in RequiredFlags)
                if (!flags.Contains(f)) return false;
            foreach (var f in ExcludedFlags)
                if (flags.Contains(f)) return false;

            return true;
        }
    }
}
