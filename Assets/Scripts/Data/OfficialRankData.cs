// ============================================================================
// 官途浮沉 - 官职数据定义
// OfficialRankData.cs — ScriptableObject，定义每个官职的属性和升迁条件
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Enums;

namespace GuantuFucheng.Data
{
    /// <summary>
    /// 升迁所需条件
    /// </summary>
    [Serializable]
    public class PromotionRequirement
    {
        public PlayerAttribute Attribute;
        public int MinValue;
    }

    /// <summary>
    /// 官职数据 ScriptableObject
    /// 
    /// 对应唐代官制简化版：
    /// 候补 → 县令 → 州刺史 → 侍郎 → 尚书/节度使 → 宰相
    /// </summary>
    [CreateAssetMenu(fileName = "NewRank", menuName = "GuantuFucheng/Official Rank Data")]
    public class OfficialRankData : ScriptableObject
    {
        [Header("基础信息")]
        public OfficialRank Rank;
        public string DisplayName;          // 如"正七品 县令"
        public string Description;          // 官职描述

        [Header("品级")]
        [Range(1, 9)]
        public int Grade = 7;               // 品级（1最高，9最低）

        public bool IsFromGrade = false;    // true=从X品，false=正X品

        [Header("能力加成")]
        public int BonusActionPoints = 0;   // 额外行动力
        public int BonusGoldPerTurn = 10;   // 每回合俸禄
        public int BonusInfluence = 0;      // 额外影响力

        [Header("升迁条件")]
        [Tooltip("升迁到下一级所需的最低属性")]
        public List<PromotionRequirement> PromotionRequirements = new List<PromotionRequirement>();

        [Tooltip("升迁所需的最低声望")]
        public int RequiredReputation = 0;

        [Tooltip("升迁所需的最低影响力")]
        public int RequiredInfluence = 0;

        [Tooltip("在此官职至少待满多少回合才能升迁")]
        public int MinTurnsAtRank = 3;

        [Header("风险")]
        [Tooltip("每回合被弹劾的基础概率（声望越低越高）")]
        [Range(0f, 1f)]
        public float ImpeachmentBaseChance = 0.05f;

        [Tooltip("此官职特有的可触发事件卡ID")]
        public List<string> RankSpecificCards = new List<string>();

        [Header("分支路线")]
        [Tooltip("从此官职可以升迁到哪些官职（通常1-2个）")]
        public List<OfficialRank> PromotionPaths = new List<OfficialRank>();
    }
}
