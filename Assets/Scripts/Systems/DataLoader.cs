// ============================================================================
// 官途浮沉 - JSON数据加载器
// DataLoader.cs — 从Resources/GameData/加载JSON配置，转换为运行时数据对象
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Data;
using GuantuFucheng.Enums;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// JSON数据加载工具类
    /// 负责从Resources/GameData/目录加载所有JSON配置文件，
    /// 并转换为游戏运行时可用的数据对象。
    /// 
    /// 使用方式：
    ///   var cards = DataLoader.LoadCards();
    ///   var npcs = DataLoader.LoadNPCs();
    ///   var ranks = DataLoader.LoadRanks();
    ///   var evalConfig = DataLoader.LoadEvaluationConfig();
    /// </summary>
    public static class DataLoader
    {
        private const string CardPath = "GameData/CardDatabase";
        private const string NPCPath = "GameData/NPCDatabase";
        private const string RankPath = "GameData/RankDatabase";
        private const string EvalPath = "GameData/EvaluationConfig";

        // ====================================================================
        // 公开API
        // ====================================================================

        /// <summary>加载所有卡牌数据</summary>
        public static List<CardData> LoadCards()
        {
            var json = LoadJsonText(CardPath);
            if (json == null) return new List<CardData>();

            var wrapper = JsonUtility.FromJson<CardDatabaseWrapper>(json);
            var results = new List<CardData>();

            foreach (var raw in wrapper.Cards)
            {
                var card = ScriptableObject.CreateInstance<CardData>();
                card.CardId = raw.CardId;
                card.Title = raw.Title;
                card.Description = raw.Description;
                card.Type = ParseEnum<CardType>(raw.Type);
                card.Rarity = ParseEnum<CardRarity>(raw.Rarity);
                card.MinTurn = raw.MinTurn;
                card.MaxTurn = raw.MaxTurn;
                card.MinRank = ParseEnum<OfficialRank>(raw.MinRank);
                card.MaxRank = ParseEnum<OfficialRank>(raw.MaxRank);
                card.RequiredFlags = raw.RequiredFlags ?? new List<string>();
                card.ExcludedFlags = raw.ExcludedFlags ?? new List<string>();
                card.RequiresMetaUnlock = raw.RequiresMetaUnlock;
                card.MetaUnlockId = raw.MetaUnlockId ?? "";

                card.Choices = new List<CardChoice>();
                if (raw.Choices != null)
                {
                    foreach (var rawChoice in raw.Choices)
                    {
                        var choice = new CardChoice
                        {
                            ChoiceText = rawChoice.ChoiceText,
                            Tooltip = rawChoice.Tooltip,
                            RequiredAttribute = ParseEnum<PlayerAttribute>(rawChoice.RequiredAttribute),
                            RequiredValue = rawChoice.RequiredValue,
                            FollowUpCardId = rawChoice.FollowUpCardId ?? "",
                            SetFlags = rawChoice.SetFlags ?? new List<string>(),
                            Modifiers = new List<AttributeModifier>(),
                            RelationshipEffects = new List<RelationshipModifier>()
                        };

                        if (rawChoice.Modifiers != null)
                        {
                            foreach (var rawMod in rawChoice.Modifiers)
                            {
                                choice.Modifiers.Add(new AttributeModifier
                                {
                                    Attribute = ParseEnum<PlayerAttribute>(rawMod.Attribute),
                                    Value = rawMod.Value
                                });
                            }
                        }

                        if (rawChoice.RelationshipEffects != null)
                        {
                            foreach (var rawRel in rawChoice.RelationshipEffects)
                            {
                                choice.RelationshipEffects.Add(new RelationshipModifier
                                {
                                    NpcId = rawRel.NpcId,
                                    FavorDelta = rawRel.FavorDelta,
                                    TrustDelta = rawRel.TrustDelta
                                });
                            }
                        }

                        card.Choices.Add(choice);
                    }
                }

                results.Add(card);
            }

            Debug.Log($"[DataLoader] 已加载 {results.Count} 张卡牌");
            return results;
        }

        /// <summary>加载所有NPC数据</summary>
        public static List<NPCData> LoadNPCs()
        {
            var json = LoadJsonText(NPCPath);
            if (json == null) return new List<NPCData>();

            var wrapper = JsonUtility.FromJson<NPCDatabaseWrapper>(json);
            var results = new List<NPCData>();

            foreach (var raw in wrapper.NPCs)
            {
                var npc = ScriptableObject.CreateInstance<NPCData>();
                npc.NpcId = raw.NpcId;
                npc.DisplayName = raw.DisplayName;
                npc.Biography = raw.Biography;
                npc.Faction = ParseEnum<Faction>(raw.Faction);
                npc.Rank = ParseEnum<OfficialRank>(raw.Rank);
                npc.InitialFavor = raw.InitialFavor;
                npc.InitialTrust = raw.InitialTrust;
                npc.StartsActive = raw.StartsActive;
                npc.AppearTurn = raw.AppearTurn;
                npc.AppearMinRank = ParseEnum<OfficialRank>(raw.AppearMinRank);
                npc.PersonalCardIds = raw.PersonalCardIds ?? new List<string>();
                npc.AggressionWeight = raw.AggressionWeight;
                npc.AllianceWeight = raw.AllianceWeight;
                npc.BetrayalWeight = raw.BetrayalWeight;

                npc.Traits = new List<PersonalityTrait>();
                if (raw.Traits != null)
                {
                    foreach (var t in raw.Traits)
                    {
                        npc.Traits.Add(ParseEnum<PersonalityTrait>(t));
                    }
                }

                npc.Reactions = new List<ActionReaction>();
                if (raw.Reactions != null)
                {
                    foreach (var rawR in raw.Reactions)
                    {
                        npc.Reactions.Add(new ActionReaction
                        {
                            ActionType = ParseEnum<ActionType>(rawR.ActionType),
                            FavorReaction = rawR.FavorReaction,
                            TrustReaction = rawR.TrustReaction
                        });
                    }
                }

                results.Add(npc);
            }

            Debug.Log($"[DataLoader] 已加载 {results.Count} 个NPC");
            return results;
        }

        /// <summary>加载官职升迁数据</summary>
        public static List<OfficialRankData> LoadRanks()
        {
            var json = LoadJsonText(RankPath);
            if (json == null) return new List<OfficialRankData>();

            var wrapper = JsonUtility.FromJson<RankDatabaseWrapper>(json);
            var results = new List<OfficialRankData>();

            foreach (var raw in wrapper.Ranks)
            {
                var rank = ScriptableObject.CreateInstance<OfficialRankData>();
                rank.Rank = ParseEnum<OfficialRank>(raw.Rank);
                rank.DisplayName = raw.DisplayName;
                rank.Description = raw.Description;
                rank.Grade = raw.Grade;
                rank.IsFromGrade = raw.IsFromGrade;
                rank.BonusActionPoints = raw.BonusActionPoints;
                rank.BonusGoldPerTurn = raw.BonusGoldPerTurn;
                rank.BonusInfluence = raw.BonusInfluence;
                rank.RequiredReputation = raw.RequiredReputation;
                rank.RequiredInfluence = raw.RequiredInfluence;
                rank.MinTurnsAtRank = raw.MinTurnsAtRank;
                rank.ImpeachmentBaseChance = raw.ImpeachmentBaseChance;
                rank.RankSpecificCards = raw.RankSpecificCards ?? new List<string>();

                rank.PromotionRequirements = new List<PromotionRequirement>();
                if (raw.PromotionRequirements != null)
                {
                    foreach (var rawReq in raw.PromotionRequirements)
                    {
                        rank.PromotionRequirements.Add(new PromotionRequirement
                        {
                            Attribute = ParseEnum<PlayerAttribute>(rawReq.Attribute),
                            MinValue = rawReq.Value
                        });
                    }
                }

                rank.PromotionPaths = new List<OfficialRank>();
                if (raw.PromotionPaths != null)
                {
                    foreach (var p in raw.PromotionPaths)
                    {
                        rank.PromotionPaths.Add(ParseEnum<OfficialRank>(p));
                    }
                }

                results.Add(rank);
            }

            Debug.Log($"[DataLoader] 已加载 {results.Count} 个官职配置");
            return results;
        }

        /// <summary>加载吏部考评配置</summary>
        public static EvaluationConfigData LoadEvaluationConfig()
        {
            var json = LoadJsonText(EvalPath);
            if (json == null) return null;

            var config = JsonUtility.FromJson<EvaluationConfigData>(json);
            Debug.Log($"[DataLoader] 已加载考评配置：{config.Dimensions?.Count ?? 0} 个维度");
            return config;
        }

        // ====================================================================
        // 内部辅助
        // ====================================================================

        private static string LoadJsonText(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[DataLoader] 找不到资源文件: Resources/{resourcePath}.json");
                return null;
            }
            return asset.text;
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            if (string.IsNullOrEmpty(value))
                return default(T);

            if (Enum.TryParse<T>(value, true, out T result))
                return result;

            Debug.LogWarning($"[DataLoader] 无法解析枚举 {typeof(T).Name}: '{value}'，使用默认值");
            return default(T);
        }

        // ====================================================================
        // JSON反序列化用的中间数据结构
        // Unity的JsonUtility不支持直接反序列化ScriptableObject，
        // 所以需要先反序列化到普通类，再手动转换。
        // ====================================================================

        #region JSON Wrapper Classes

        [Serializable]
        private class CardDatabaseWrapper
        {
            public List<RawCardData> Cards;
        }

        [Serializable]
        private class RawCardData
        {
            public string CardId;
            public string Title;
            public string Description;
            public string Type;
            public string Rarity;
            public int MinTurn;
            public int MaxTurn;
            public string MinRank;
            public string MaxRank;
            public List<string> RequiredFlags;
            public List<string> ExcludedFlags;
            public bool RequiresMetaUnlock;
            public string MetaUnlockId;
            public List<RawCardChoice> Choices;
        }

        [Serializable]
        private class RawCardChoice
        {
            public string ChoiceText;
            public string Tooltip;
            public string RequiredAttribute;
            public int RequiredValue;
            public List<RawAttributeModifier> Modifiers;
            public string FollowUpCardId;
            public List<string> SetFlags;
            public List<RawRelationshipModifier> RelationshipEffects;
        }

        [Serializable]
        private class RawAttributeModifier
        {
            public string Attribute;
            public int Value;
        }

        [Serializable]
        private class RawRelationshipModifier
        {
            public string NpcId;
            public int FavorDelta;
            public int TrustDelta;
        }

        [Serializable]
        private class NPCDatabaseWrapper
        {
            public List<RawNPCData> NPCs;
        }

        [Serializable]
        private class RawNPCData
        {
            public string NpcId;
            public string DisplayName;
            public string Biography;
            public string Faction;
            public string Rank;
            public List<string> Traits;
            public int InitialFavor;
            public int InitialTrust;
            public List<RawActionReaction> Reactions;
            public bool StartsActive;
            public int AppearTurn;
            public string AppearMinRank;
            public List<string> PersonalCardIds;
            public float AggressionWeight;
            public float AllianceWeight;
            public float BetrayalWeight;
        }

        [Serializable]
        private class RawActionReaction
        {
            public string ActionType;
            public int FavorReaction;
            public int TrustReaction;
        }

        [Serializable]
        private class RankDatabaseWrapper
        {
            public List<RawRankData> Ranks;
        }

        [Serializable]
        private class RawRankData
        {
            public string Rank;
            public string DisplayName;
            public string Description;
            public int Grade;
            public bool IsFromGrade;
            public int BonusActionPoints;
            public int BonusGoldPerTurn;
            public int BonusInfluence;
            public List<RawPromotionRequirement> PromotionRequirements;
            public int RequiredReputation;
            public int RequiredInfluence;
            public int MinTurnsAtRank;
            public float ImpeachmentBaseChance;
            public List<string> RankSpecificCards;
            public List<string> PromotionPaths;
        }

        [Serializable]
        private class RawPromotionRequirement
        {
            public string Attribute;
            public int Value;
        }

        #endregion
    }

    // ========================================================================
    // 吏部考评配置数据结构（非ScriptableObject，直接从JSON反序列化）
    // ========================================================================

    [Serializable]
    public class EvaluationConfigData
    {
        public string EvaluationName;
        public string Description;
        public int EvaluationInterval;
        public List<EvaluationDimension> Dimensions;
        public OverallRatingConfig OverallRating;
        public string ScoreFormula;
        public List<SpecialRule> SpecialRules;

        /// <summary>
        /// 计算某个维度的得分
        /// </summary>
        public float CalculateDimensionScore(string dimensionId, float attributeValue)
        {
            var dim = Dimensions?.Find(d => d.DimensionId == dimensionId);
            if (dim == null) return 0f;
            return attributeValue * dim.Weight;
        }

        /// <summary>
        /// 根据总分获取考评等级
        /// </summary>
        public string GetRating(float totalScore)
        {
            if (OverallRating == null) return "未知";
            if (totalScore >= OverallRating.Excellent.MinScore) return OverallRating.Excellent.Description;
            if (totalScore >= OverallRating.Good.MinScore) return OverallRating.Good.Description;
            if (totalScore >= OverallRating.Pass.MinScore) return OverallRating.Pass.Description;
            return OverallRating.Fail.Description;
        }
    }

    [Serializable]
    public class EvaluationDimension
    {
        public string DimensionId;
        public string DisplayName;
        public string Description;
        public float Weight;
        public string SourceAttribute;
        public int PassLine;
        public int ExcellentLine;
        public string FailPenalty;
        public string ExcellentReward;
    }

    [Serializable]
    public class OverallRatingConfig
    {
        public RatingLevel Excellent;
        public RatingLevel Good;
        public RatingLevel Pass;
        public RatingLevel Fail;
    }

    [Serializable]
    public class RatingLevel
    {
        public int MinScore;
        public string Description;
        public string Effect;
    }

    [Serializable]
    public class SpecialRule
    {
        public string RuleId;
        public string Description;
    }
}
