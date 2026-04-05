using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// 美术资源管理器 - 管理立绘、卡牌插图、场景背景等Sprite资源
    /// 单例模式，统一资源加载入口
    /// </summary>
    public class ArtManager : MonoBehaviour
    {
        public static ArtManager Instance { get; private set; }

        // 资源缓存
        private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private ArtConfig artConfig;

        #region Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadArtConfig();
        }

        private void LoadArtConfig()
        {
            var configText = Resources.Load<TextAsset>("Art/ArtConfig");
            if (configText != null)
            {
                artConfig = JsonUtility.FromJson<ArtConfig>(configText.text);
                Debug.Log($"[ArtManager] Loaded config: {artConfig.NPCPortraits.Count} NPCs, " +
                          $"{artConfig.CardIllustrations.Count} card types, {artConfig.Backgrounds.Count} backgrounds.");
            }
            else
            {
                Debug.LogWarning("[ArtManager] ArtConfig.json not found in Resources/Art/");
                artConfig = new ArtConfig();
            }
        }

        #endregion

        #region NPC Portraits (NPC立绘)

        /// <summary>
        /// 根据NPC ID加载立绘
        /// </summary>
        /// <param name="npcId">NPC ID，如 "npc_qinhu"</param>
        /// <param name="expression">表情变体，如 "normal", "angry", "happy"</param>
        /// <returns>立绘Sprite，未找到返回默认占位</returns>
        public Sprite GetNPCPortrait(string npcId, string expression = "normal")
        {
            string cacheKey = $"npc_{npcId}_{expression}";
            if (spriteCache.TryGetValue(cacheKey, out Sprite cached))
                return cached;

            var entry = artConfig.NPCPortraits.Find(e => e.NpcId == npcId);
            if (entry == null)
            {
                Debug.LogWarning($"[ArtManager] NPC portrait config not found: {npcId}");
                return GetPlaceholder("npc");
            }

            // 尝试加载带表情变体的立绘
            string path = $"{entry.BasePath}_{expression}";
            Sprite sprite = LoadSprite(path);

            // 回退到默认表情
            if (sprite == null && expression != "normal")
            {
                path = $"{entry.BasePath}_normal";
                sprite = LoadSprite(path);
            }

            // 回退到无后缀
            if (sprite == null)
            {
                sprite = LoadSprite(entry.BasePath);
            }

            if (sprite != null)
            {
                spriteCache[cacheKey] = sprite;
                return sprite;
            }

            Debug.LogWarning($"[ArtManager] NPC portrait not found: {path}");
            return GetPlaceholder("npc");
        }

        /// <summary>
        /// 获取NPC的所有可用表情列表
        /// </summary>
        public List<string> GetNPCExpressions(string npcId)
        {
            var entry = artConfig.NPCPortraits.Find(e => e.NpcId == npcId);
            return entry?.Expressions ?? new List<string> { "normal" };
        }

        #endregion

        #region Card Illustrations (卡牌插图)

        /// <summary>
        /// 根据卡牌类型加载插图
        /// </summary>
        /// <param name="cardType">卡牌类型，如 "Policy", "Crisis", "Relationship"</param>
        /// <param name="variant">变体（可选），用于同类型不同插图</param>
        public Sprite GetCardIllustration(string cardType, string variant = "default")
        {
            string cacheKey = $"card_{cardType}_{variant}";
            if (spriteCache.TryGetValue(cacheKey, out Sprite cached))
                return cached;

            var entry = artConfig.CardIllustrations.Find(e => e.CardType == cardType);
            if (entry == null)
            {
                Debug.LogWarning($"[ArtManager] Card illustration config not found: {cardType}");
                return GetPlaceholder("card");
            }

            string path = variant != "default"
                ? $"{entry.BasePath}_{variant}"
                : entry.BasePath;

            Sprite sprite = LoadSprite(path) ?? LoadSprite(entry.BasePath);
            if (sprite != null)
            {
                spriteCache[cacheKey] = sprite;
                return sprite;
            }

            return GetPlaceholder("card");
        }

        /// <summary>
        /// 获取卡牌背面图
        /// </summary>
        public Sprite GetCardBack()
        {
            return LoadSpriteOrPlaceholder("Art/Cards/card_back", "card");
        }

        /// <summary>
        /// 获取卡牌边框（按稀有度）
        /// </summary>
        public Sprite GetCardFrame(string rarity)
        {
            string path = $"Art/Cards/Frames/frame_{rarity.ToLower()}";
            return LoadSpriteOrPlaceholder(path, "card");
        }

        #endregion

        #region Scene Backgrounds (场景背景)

        /// <summary>
        /// 根据官职阶段加载场景背景
        /// </summary>
        /// <param name="rank">官职阶段，如 "CountyMagistrate", "Prefect", "GrandCouncilor"</param>
        /// <param name="scene">场景类型，如 "office", "court", "street"</param>
        public Sprite GetBackground(string rank, string scene = "office")
        {
            string cacheKey = $"bg_{rank}_{scene}";
            if (spriteCache.TryGetValue(cacheKey, out Sprite cached))
                return cached;

            var entry = artConfig.Backgrounds.Find(e => e.RankStage == rank);
            if (entry == null)
            {
                Debug.LogWarning($"[ArtManager] Background config not found for rank: {rank}");
                return GetPlaceholder("background");
            }

            string path = $"{entry.BasePath}_{scene}";
            Sprite sprite = LoadSprite(path) ?? LoadSprite(entry.BasePath);

            if (sprite != null)
            {
                spriteCache[cacheKey] = sprite;
                return sprite;
            }

            return GetPlaceholder("background");
        }

        /// <summary>
        /// 根据官职阶段获取对应场景名（县衙/州府/朝堂）
        /// </summary>
        public string GetSceneName(string rank)
        {
            return rank switch
            {
                "CountyMagistrate" or "CountyAssistant" => "county",    // 县衙
                "Prefect" or "VicePrefect" => "prefecture",              // 州府
                "Censor" or "Minister" or "GrandCouncilor" => "court",   // 朝堂
                _ => "county"
            };
        }

        #endregion

        #region UI Icons (UI图标)

        /// <summary>
        /// 加载属性图标
        /// </summary>
        public Sprite GetAttributeIcon(string attribute)
        {
            string path = $"Art/Icons/Attributes/icon_{attribute.ToLower()}";
            return LoadSpriteOrPlaceholder(path, "icon");
        }

        /// <summary>
        /// 加载阵营图标
        /// </summary>
        public Sprite GetFactionIcon(string faction)
        {
            string path = $"Art/Icons/Factions/icon_{faction.ToLower()}";
            return LoadSpriteOrPlaceholder(path, "icon");
        }

        /// <summary>
        /// 加载官职图标
        /// </summary>
        public Sprite GetRankIcon(string rank)
        {
            string path = $"Art/Icons/Ranks/icon_{rank.ToLower()}";
            return LoadSpriteOrPlaceholder(path, "icon");
        }

        #endregion

        #region Utilities

        private Sprite LoadSprite(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return null;
            return Resources.Load<Sprite>(resourcePath);
        }

        private Sprite LoadSpriteOrPlaceholder(string path, string placeholderType)
        {
            Sprite sprite = LoadSprite(path);
            return sprite != null ? sprite : GetPlaceholder(placeholderType);
        }

        /// <summary>
        /// 获取占位图
        /// </summary>
        private Sprite GetPlaceholder(string type)
        {
            string path = $"Art/Placeholders/placeholder_{type}";
            if (spriteCache.TryGetValue(path, out Sprite cached))
                return cached;

            Sprite placeholder = Resources.Load<Sprite>(path);
            if (placeholder != null)
                spriteCache[path] = placeholder;

            return placeholder;
        }

        /// <summary>
        /// 预加载指定NPC的立绘
        /// </summary>
        public void PreloadNPCPortraits(params string[] npcIds)
        {
            foreach (var npcId in npcIds)
            {
                GetNPCPortrait(npcId, "normal");
            }
        }

        /// <summary>
        /// 预加载当前官职阶段的所有背景
        /// </summary>
        public void PreloadBackgrounds(string rank)
        {
            string[] scenes = { "office", "court", "street", "garden" };
            foreach (var scene in scenes)
            {
                GetBackground(rank, scene);
            }
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void ClearCache()
        {
            spriteCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 清理指定前缀的缓存
        /// </summary>
        public void ClearCacheByPrefix(string prefix)
        {
            var keysToRemove = new List<string>();
            foreach (var key in spriteCache.Keys)
            {
                if (key.StartsWith(prefix))
                    keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
                spriteCache.Remove(key);
        }

        #endregion
    }

    #region Data Models

    [Serializable]
    public class ArtConfig
    {
        public List<NPCPortraitEntry> NPCPortraits = new List<NPCPortraitEntry>();
        public List<CardIllustrationEntry> CardIllustrations = new List<CardIllustrationEntry>();
        public List<BackgroundEntry> Backgrounds = new List<BackgroundEntry>();
        public List<IconEntry> Icons = new List<IconEntry>();
    }

    [Serializable]
    public class NPCPortraitEntry
    {
        public string NpcId;
        public string DisplayName;
        public string BasePath;
        public List<string> Expressions = new List<string> { "normal" };
    }

    [Serializable]
    public class CardIllustrationEntry
    {
        public string CardType;
        public string BasePath;
        public string Description;
    }

    [Serializable]
    public class BackgroundEntry
    {
        public string RankStage;
        public string BasePath;
        public string Description;
        public List<string> Scenes = new List<string>();
    }

    [Serializable]
    public class IconEntry
    {
        public string Category;
        public string Key;
        public string Path;
    }

    #endregion
}
