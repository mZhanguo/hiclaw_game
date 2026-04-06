// ============================================================================
// 官途浮沉 - 运行时数据模型
// GameState.cs — 单局运行时状态（非持久化用，持久化见SaveData）
// ============================================================================

using System;
using System.Collections.Generic;
using GuantuFucheng.Enums;

namespace GuantuFucheng.Models
{
    /// <summary>
    /// 玩家角色运行时数据
    /// </summary>
    [Serializable]
    public class PlayerState
    {
        public string PlayerName = "无名氏";
        public OfficialRank CurrentRank = OfficialRank.Candidate;
        public int CurrentTurn = 1;

        // 六维属性
        public int Intellect = 10;      // 才学
        public int Charisma = 10;       // 人望
        public int Scheming = 10;       // 权谋
        public int Martial = 5;         // 武略
        public int Health = 100;        // 体魄
        public int Reputation = 0;      // 声望（可为负）

        // 资源
        public int Gold = 100;          // 金银
        public int Influence = 0;       // 影响力（用于升迁）

        // 行动力
        public int MaxActionPoints = 6;
        public int CurrentActionPoints = 6;

        /// <summary>获取指定属性值</summary>
        public int GetAttribute(PlayerAttribute attr)
        {
            return attr switch
            {
                PlayerAttribute.Intellect => Intellect,
                PlayerAttribute.Charisma => Charisma,
                PlayerAttribute.Scheming => Scheming,
                PlayerAttribute.Martial => Martial,
                PlayerAttribute.Health => Health,
                PlayerAttribute.Reputation => Reputation,
                _ => 0
            };
        }

        /// <summary>修改指定属性值，返回实际变化量</summary>
        public int ModifyAttribute(PlayerAttribute attr, int delta)
        {
            switch (attr)
            {
                case PlayerAttribute.Intellect:
                    Intellect = Math.Max(0, Intellect + delta);
                    return delta;
                case PlayerAttribute.Charisma:
                    Charisma = Math.Max(0, Charisma + delta);
                    return delta;
                case PlayerAttribute.Scheming:
                    Scheming = Math.Max(0, Scheming + delta);
                    return delta;
                case PlayerAttribute.Martial:
                    Martial = Math.Max(0, Martial + delta);
                    return delta;
                case PlayerAttribute.Health:
                    int oldHealth = Health;
                    Health = Math.Clamp(Health + delta, 0, 100);
                    return Health - oldHealth;
                case PlayerAttribute.Reputation:
                    Reputation += delta;
                    return delta;
                default:
                    return 0;
            }
        }
    }

    /// <summary>
    /// NPC关系运行时数据
    /// </summary>
    [Serializable]
    public class NPCRelationshipState
    {
        public string NpcId;
        public int Favor;       // 好感度 [-100, 100]
        public int Trust;       // 信任度 [-100, 100]
        public bool IsRevealed; // 是否已探明此NPC的真实性格
        public List<string> TriggeredEvents = new List<string>(); // 已触发的关系事件

        /// <summary>好感度等级描述</summary>
        public string FavorLevel => Favor switch
        {
            >= 80 => "莫逆之交",
            >= 50 => "至交好友",
            >= 20 => "相熟",
            >= 0 => "泛泛之交",
            >= -30 => "面和心不和",
            >= -60 => "政敌",
            _ => "死敌"
        };

        /// <summary>信任度等级描述</summary>
        public string TrustLevel => Trust switch
        {
            >= 80 => "肝胆相照",
            >= 50 => "深信不疑",
            >= 20 => "略有信任",
            >= 0 => "将信将疑",
            >= -30 => "心存疑虑",
            _ => "完全不信任"
        };
    }

    /// <summary>
    /// 单局游戏运行时状态（包含所有需要存档的数据）
    /// </summary>
    [Serializable]
    public class RunState
    {
        public PlayerState Player = new PlayerState();
        public List<NPCRelationshipState> Relationships = new List<NPCRelationshipState>();
        public List<string> HandCards = new List<string>();      // 手牌中的卡牌ID
        public List<string> UsedCards = new List<string>();      // 已使用/弃置的卡牌
        public List<string> ActiveFlags = new List<string>();    // 剧情标记（影响分支）
        public int Seed;                                         // 随机种子（重现性）

        /// <summary>查找指定NPC的关系数据</summary>
        public NPCRelationshipState GetRelationship(string npcId)
        {
            return Relationships.Find(r => r.NpcId == npcId);
        }

        /// <summary>获取或创建NPC关系</summary>
        public NPCRelationshipState GetOrCreateRelationship(string npcId)
        {
            var rel = GetRelationship(npcId);
            if (rel == null)
            {
                rel = new NPCRelationshipState { NpcId = npcId, Favor = 0, Trust = 0 };
                Relationships.Add(rel);
            }
            return rel;
        }
    }
}
