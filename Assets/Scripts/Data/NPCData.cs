// ============================================================================
// 官途浮沉 - NPC数据定义
// NPCData.cs — ScriptableObject，定义每个NPC的基础属性和行为模式
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Enums;

namespace GuantuFucheng.Data
{
    /// <summary>
    /// NPC对特定行动的态度反应
    /// </summary>
    [Serializable]
    public class ActionReaction
    {
        public ActionType ActionType;
        [Range(-10, 10)]
        public int FavorReaction;       // 对该行动的好感反应
        [Range(-10, 10)]
        public int TrustReaction;       // 对该行动的信任反应
    }

    /// <summary>
    /// NPC数据 ScriptableObject
    /// 
    /// 设计说明：
    /// - 好感度：NPC对玩家的私人感情，影响社交互动
    /// - 信任度：NPC对玩家的政治信任，影响关键时刻是否支持你
    /// - 两个维度独立变化：可能好感高但不信任（酒肉朋友），也可能好感低但信任（政敌但尊重）
    /// </summary>
    [CreateAssetMenu(fileName = "NewNPC", menuName = "GuantuFucheng/NPC Data")]
    public class NPCData : ScriptableObject
    {
        [Header("基础信息")]
        public string NpcId;
        public string DisplayName;

        [TextArea(2, 5)]
        public string Biography;         // 人物背景

        public Faction Faction;           // 所属阵营
        public OfficialRank Rank;         // 官职

        [Header("性格")]
        public List<PersonalityTrait> Traits = new List<PersonalityTrait>();

        [Header("初始关系")]
        [Range(-100, 100)]
        public int InitialFavor = 0;      // 初始好感度

        [Range(-100, 100)]
        public int InitialTrust = 0;      // 初始信任度

        [Header("行为模式")]
        [Tooltip("NPC对各种行动类型的天然反应")]
        public List<ActionReaction> Reactions = new List<ActionReaction>();

        [Header("特殊")]
        [Tooltip("是否在本局初始就出场")]
        public bool StartsActive = true;

        [Tooltip("出场的最低回合")]
        public int AppearTurn = 1;

        [Tooltip("出场需要的最低官职")]
        public OfficialRank AppearMinRank = OfficialRank.Candidate;

        [Tooltip("关联的专属事件卡ID列表")]
        public List<string> PersonalCardIds = new List<string>();

        [Header("AI行为权重")]
        [Range(0f, 1f)]
        [Tooltip("主动找玩家麻烦的概率")]
        public float AggressionWeight = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("愿意结盟的概率基数")]
        public float AllianceWeight = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("背叛概率基数")]
        public float BetrayalWeight = 0.1f;

        /// <summary>获取NPC对特定行动的好感反应值</summary>
        public int GetFavorReaction(ActionType actionType)
        {
            var reaction = Reactions.Find(r => r.ActionType == actionType);
            return reaction?.FavorReaction ?? 0;
        }

        /// <summary>获取NPC对特定行动的信任反应值</summary>
        public int GetTrustReaction(ActionType actionType)
        {
            var reaction = Reactions.Find(r => r.ActionType == actionType);
            return reaction?.TrustReaction ?? 0;
        }
    }
}
