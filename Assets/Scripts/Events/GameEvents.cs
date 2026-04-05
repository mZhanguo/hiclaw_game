// ============================================================================
// 官途浮沉 - 事件总线
// GameEvents.cs — 基于C# event的观察者模式，解耦各系统间通信
// ============================================================================

using System;
using System.Collections.Generic;
using GuantuFucheng.Enums;

namespace GuantuFucheng.Events
{
    /// <summary>
    /// 全局事件总线 — 所有系统间通信通过此类发布/订阅
    /// 设计原则：系统之间不直接引用，通过事件解耦
    /// </summary>
    public static class GameEvents
    {
        // ======================== 游戏生命周期 ========================

        /// <summary>新游戏开始</summary>
        public static event Action OnNewGameStarted;
        public static void NewGameStarted() => OnNewGameStarted?.Invoke();

        /// <summary>游戏状态切换</summary>
        public static event Action<GameState> OnGameStateChanged;
        public static void GameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);

        /// <summary>单局结束</summary>
        public static event Action<GameOverReason> OnGameOver;
        public static void GameOver(GameOverReason reason) => OnGameOver?.Invoke(reason);

        // ======================== 回合系统 ========================

        /// <summary>新回合开始，参数为回合数</summary>
        public static event Action<int> OnTurnStarted;
        public static void TurnStarted(int turnNumber) => OnTurnStarted?.Invoke(turnNumber);

        /// <summary>回合阶段切换</summary>
        public static event Action<TurnPhase> OnPhaseChanged;
        public static void PhaseChanged(TurnPhase phase) => OnPhaseChanged?.Invoke(phase);

        /// <summary>回合结束</summary>
        public static event Action<int> OnTurnEnded;
        public static void TurnEnded(int turnNumber) => OnTurnEnded?.Invoke(turnNumber);

        // ======================== 行动力系统 ========================

        /// <summary>行动力分配变更（行动类型，分配点数）</summary>
        public static event Action<ActionType, int> OnActionPointAllocated;
        public static void ActionPointAllocated(ActionType type, int points)
            => OnActionPointAllocated?.Invoke(type, points);

        /// <summary>行动力耗尽</summary>
        public static event Action OnActionPointsDepleted;
        public static void ActionPointsDepleted() => OnActionPointsDepleted?.Invoke();

        // ======================== 卡牌系统 ========================

        /// <summary>抽到新卡（卡牌ID）</summary>
        public static event Action<string> OnCardDrawn;
        public static void CardDrawn(string cardId) => OnCardDrawn?.Invoke(cardId);

        /// <summary>玩家选择了卡牌选项（卡牌ID，选项索引）</summary>
        public static event Action<string, int> OnCardChoiceMade;
        public static void CardChoiceMade(string cardId, int choiceIndex)
            => OnCardChoiceMade?.Invoke(cardId, choiceIndex);

        // ======================== NPC关系 ========================

        /// <summary>NPC好感度变化（NPC ID，变化量，当前值）</summary>
        public static event Action<string, int, int> OnFavorChanged;
        public static void FavorChanged(string npcId, int delta, int current)
            => OnFavorChanged?.Invoke(npcId, delta, current);

        /// <summary>NPC信任度变化（NPC ID，变化量，当前值）</summary>
        public static event Action<string, int, int> OnTrustChanged;
        public static void TrustChanged(string npcId, int delta, int current)
            => OnTrustChanged?.Invoke(npcId, delta, current);

        /// <summary>NPC关系质变（如：从陌生变为盟友）</summary>
        public static event Action<string, string> OnRelationshipMilestone;
        public static void RelationshipMilestone(string npcId, string milestone)
            => OnRelationshipMilestone?.Invoke(npcId, milestone);

        // ======================== 官职系统 ========================

        /// <summary>官职变动（旧官职，新官职）</summary>
        public static event Action<OfficialRank, OfficialRank> OnRankChanged;
        public static void RankChanged(OfficialRank oldRank, OfficialRank newRank)
            => OnRankChanged?.Invoke(oldRank, newRank);

        /// <summary>触发升迁考核</summary>
        public static event Action OnPromotionCheck;
        public static void PromotionCheck() => OnPromotionCheck?.Invoke();

        // ======================== 玩家属性 ========================

        /// <summary>属性变化（属性类型，变化量，当前值）</summary>
        public static event Action<PlayerAttribute, int, int> OnAttributeChanged;
        public static void AttributeChanged(PlayerAttribute attr, int delta, int current)
            => OnAttributeChanged?.Invoke(attr, delta, current);

        // ======================== 存档系统 ========================

        /// <summary>游戏已保存</summary>
        public static event Action OnGameSaved;
        public static void GameSaved() => OnGameSaved?.Invoke();

        /// <summary>游戏已加载</summary>
        public static event Action OnGameLoaded;
        public static void GameLoaded() => OnGameLoaded?.Invoke();

        // ======================== Meta进度 ========================

        /// <summary>解锁新永久升级</summary>
        public static event Action<string> OnMetaUpgradeUnlocked;
        public static void MetaUpgradeUnlocked(string upgradeId)
            => OnMetaUpgradeUnlocked?.Invoke(upgradeId);
    }
}
