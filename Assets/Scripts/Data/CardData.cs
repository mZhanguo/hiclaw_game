// ============================================================================
// 官途浮沉 - 卡牌数据定义
// CardData.cs — ScriptableObject，策划在Unity编辑器中配置每张卡牌
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Enums;

namespace GuantuFucheng.Data
{
    /// <summary>
    /// 卡牌选项 — 每张决策卡可以有2-4个选项分支
    /// </summary>
    [Serializable]
    public class CardChoice
    {
        [Tooltip("选项文本，如：'严查到底'")]
        public string ChoiceText;

        [Tooltip("选项描述/提示，如：'可能得罪权贵'")]
        public string Tooltip;

        [Tooltip("选择此项的属性要求（如需要权谋>=15才能选）")]
        public PlayerAttribute RequiredAttribute;
        public int RequiredValue;

        [Tooltip("选择后的属性变动列表")]
        public List<AttributeModifier> Modifiers = new List<AttributeModifier>();

        [Tooltip("选择后触发的后续卡牌ID（空则无后续）")]
        public string FollowUpCardId;

        [Tooltip("选择后设置的剧情标记")]
        public List<string> SetFlags = new List<string>();

        [Tooltip("选择后影响的NPC关系")]
        public List<RelationshipModifier> RelationshipEffects = new List<RelationshipModifier>();
    }

    /// <summary>
    /// 属性修改器
    /// </summary>
    [Serializable]
    public class AttributeModifier
    {
        public PlayerAttribute Attribute;
        public int Value;   // 正数增加，负数减少
    }

    /// <summary>
    /// NPC关系修改器
    /// </summary>
    [Serializable]
    public class RelationshipModifier
    {
        public string NpcId;
        public int FavorDelta;
        public int TrustDelta;
    }

    /// <summary>
    /// 卡牌数据 ScriptableObject
    /// 每张卡代表一个事件/决策场景
    /// 
    /// 使用方式：在Unity中 Create → GuantuFucheng → Card Data 创建
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "GuantuFucheng/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("卡牌唯一ID，如 'card_tax_corruption_001'")]
        public string CardId;

        [Tooltip("卡牌标题")]
        public string Title;

        [TextArea(3, 8)]
        [Tooltip("卡牌描述/剧情文本")]
        public string Description;

        [Tooltip("卡牌类型")]
        public CardType Type;

        [Tooltip("稀有度")]
        public CardRarity Rarity;

        [Header("触发条件")]
        [Tooltip("最低出现回合")]
        public int MinTurn = 1;

        [Tooltip("最高出现回合（0=无上限）")]
        public int MaxTurn = 0;

        [Tooltip("需要的最低官职")]
        public OfficialRank MinRank = OfficialRank.Candidate;

        [Tooltip("需要的最高官职")]
        public OfficialRank MaxRank = OfficialRank.GrandCouncilor;

        [Tooltip("需要满足的前置剧情标记")]
        public List<string> RequiredFlags = new List<string>();

        [Tooltip("与此卡互斥的剧情标记（有此标记则不出现）")]
        public List<string> ExcludedFlags = new List<string>();

        [Header("选项分支")]
        [Tooltip("玩家可选的选项列表（2-4个）")]
        public List<CardChoice> Choices = new List<CardChoice>();

        [Header("Meta")]
        [Tooltip("是否需要通过Meta进度解锁才能进入卡池")]
        public bool RequiresMetaUnlock = false;

        [Tooltip("Meta解锁ID")]
        public string MetaUnlockId;

        /// <summary>检查此卡是否满足当前局面的出场条件</summary>
        public bool CanAppear(int currentTurn, OfficialRank currentRank, List<string> activeFlags)
        {
            if (currentTurn < MinTurn) return false;
            if (MaxTurn > 0 && currentTurn > MaxTurn) return false;
            if (currentRank < MinRank || currentRank > MaxRank) return false;

            // 检查前置标记
            foreach (var flag in RequiredFlags)
            {
                if (!activeFlags.Contains(flag)) return false;
            }

            // 检查互斥标记
            foreach (var flag in ExcludedFlags)
            {
                if (activeFlags.Contains(flag)) return false;
            }

            return true;
        }
    }
}
