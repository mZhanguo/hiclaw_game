// ============================================================================
// 官途浮沉 - 卡牌系统
// CardSystem.cs — 决策卡牌：卡池管理、抽取、选项分支、数值结算
// ============================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GuantuFucheng.Core;
using GuantuFucheng.Data;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// 卡牌系统 — 游戏核心交互机制
    /// 
    /// 设计思路：
    /// 1. 卡池(Deck)：所有可用卡牌的集合，根据条件动态筛选
    /// 2. 抽取(Draw)：每回合从卡池中抽取N张卡，作为本回合事件
    /// 3. 选择(Choice)：玩家从2-4个选项中选择应对方式
    /// 4. 结算(Resolve)：根据选择计算属性/关系/资源变化
    /// 
    /// 卡牌来源：
    /// - 基础卡池：游戏初始就有的通用卡
    /// - 官职卡池：特定官职解锁的卡
    /// - NPC卡池：特定NPC关系达到阈值触发的卡
    /// - Meta卡池：通过永久进度解锁的卡
    /// </summary>
    public class CardSystem : Singleton<CardSystem>
    {
        // ======================== 数据 ========================

        [Header("卡牌数据库")]
        [Tooltip("所有卡牌数据（在Inspector中拖入）")]
        [SerializeField] private List<CardData> _allCards = new List<CardData>();

        [Header("配置")]
        [SerializeField] private int _cardsPerTurn = 3;     // 每回合抽取卡牌数
        [SerializeField] private int _maxHandSize = 5;       // 手牌上限

        /// <summary>当前手牌</summary>
        private List<CardData> _hand = new List<CardData>();

        /// <summary>已使用/弃置的卡牌ID（本局内不再出现）</summary>
        private HashSet<string> _usedOneTimeCards = new HashSet<string>();

        // ======================== 公开属性 ========================

        /// <summary>当前手牌</summary>
        public IReadOnlyList<CardData> Hand => _hand.AsReadOnly();

        // ======================== 生命周期 ========================

        private void OnEnable()
        {
            GameEvents.OnNewGameStarted += OnNewGame;
            GameEvents.OnTurnStarted += OnTurnStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnNewGameStarted -= OnNewGame;
            GameEvents.OnTurnStarted -= OnTurnStarted;
        }

        private void OnNewGame()
        {
            _hand.Clear();
            _usedOneTimeCards.Clear();
            Debug.Log("[CardSystem] 新局开始，卡池已重置");
        }

        private void OnTurnStarted(int turnNumber)
        {
            DrawCards(_cardsPerTurn);
        }

        // ======================== 核心方法 ========================

        /// <summary>
        /// 从卡池中抽取卡牌
        /// </summary>
        /// <param name="count">抽取数量</param>
        public void DrawCards(int count)
        {
            var run = GameManager.Instance.CurrentRun;
            var availableCards = GetAvailableCards(run);

            int actualDraw = Mathf.Min(count, _maxHandSize - _hand.Count, availableCards.Count);

            for (int i = 0; i < actualDraw; i++)
            {
                // 加权随机抽取（稀有度影响权重）
                var card = WeightedRandomPick(availableCards);
                if (card != null)
                {
                    _hand.Add(card);
                    availableCards.Remove(card);
                    run.HandCards.Add(card.CardId);

                    GameEvents.CardDrawn(card.CardId);
                    Debug.Log($"[CardSystem] 抽到卡牌：{card.Title}（{card.Rarity}）");
                }
            }
        }

        /// <summary>
        /// 玩家选择卡牌的某个选项
        /// </summary>
        /// <param name="cardIndex">手牌中的卡牌索引</param>
        /// <param name="choiceIndex">选项索引</param>
        public void MakeChoice(int cardIndex, int choiceIndex)
        {
            if (cardIndex < 0 || cardIndex >= _hand.Count)
            {
                Debug.LogError($"[CardSystem] 无效卡牌索引：{cardIndex}");
                return;
            }

            var card = _hand[cardIndex];
            if (choiceIndex < 0 || choiceIndex >= card.Choices.Count)
            {
                Debug.LogError($"[CardSystem] 无效选项索引：{choiceIndex}");
                return;
            }

            var choice = card.Choices[choiceIndex];
            var run = GameManager.Instance.CurrentRun;
            var player = run.Player;

            Debug.Log($"[CardSystem] 选择了 [{card.Title}] 的选项：{choice.ChoiceText}");

            // 1. 检查属性要求
            if (choice.RequiredValue > 0)
            {
                int playerAttr = player.GetAttribute(choice.RequiredAttribute);
                if (playerAttr < choice.RequiredValue)
                {
                    Debug.LogWarning($"[CardSystem] 属性不足：需要{choice.RequiredAttribute}>={choice.RequiredValue}");
                    // 可以选择：禁止选择 / 降低成功率 / 允许但有惩罚
                    return;
                }
            }

            // 2. 应用属性修改
            foreach (var mod in choice.Modifiers)
            {
                int actual = player.ModifyAttribute(mod.Attribute, mod.Value);
                GameEvents.AttributeChanged(mod.Attribute, actual, player.GetAttribute(mod.Attribute));
            }

            // 3. 应用NPC关系变化
            foreach (var relMod in choice.RelationshipEffects)
            {
                NPCRelationshipGraph.Instance.ModifyRelationship(
                    relMod.NpcId, relMod.FavorDelta, relMod.TrustDelta
                );
            }

            // 4. 设置剧情标记
            foreach (var flag in choice.SetFlags)
            {
                if (!run.ActiveFlags.Contains(flag))
                    run.ActiveFlags.Add(flag);
            }

            // 5. 处理后续卡牌
            if (!string.IsNullOrEmpty(choice.FollowUpCardId))
            {
                var followUp = _allCards.Find(c => c.CardId == choice.FollowUpCardId);
                if (followUp != null)
                {
                    _hand.Add(followUp);
                    Debug.Log($"[CardSystem] 后续卡牌触发：{followUp.Title}");
                }
            }

            // 6. 移除已使用的卡牌
            _hand.RemoveAt(cardIndex);
            run.HandCards.Remove(card.CardId);
            run.UsedCards.Add(card.CardId);
            _usedOneTimeCards.Add(card.CardId);

            GameEvents.CardChoiceMade(card.CardId, choiceIndex);
        }

        /// <summary>弃置手牌中的卡牌（不选择，直接跳过）</summary>
        public void DiscardCard(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _hand.Count) return;

            var card = _hand[cardIndex];
            var run = GameManager.Instance.CurrentRun;

            _hand.RemoveAt(cardIndex);
            run.HandCards.Remove(card.CardId);

            // 危机卡不能弃置（强制选择），此处简化处理
            if (card.Type == CardType.Crisis)
            {
                Debug.LogWarning("[CardSystem] 危机卡不能弃置！");
                _hand.Insert(cardIndex, card);
                return;
            }

            Debug.Log($"[CardSystem] 弃置卡牌：{card.Title}");
        }

        // ======================== 内部方法 ========================

        /// <summary>获取当前可用的卡池</summary>
        private List<CardData> GetAvailableCards(Models.RunState run)
        {
            return _allCards.Where(card =>
            {
                // 排除已使用的一次性卡
                if (_usedOneTimeCards.Contains(card.CardId)) return false;

                // 排除已在手牌中的
                if (_hand.Any(h => h.CardId == card.CardId)) return false;

                // 检查出场条件
                if (!card.CanAppear(run.Player.CurrentTurn, run.Player.CurrentRank, run.ActiveFlags))
                    return false;

                // 检查Meta解锁
                if (card.RequiresMetaUnlock)
                {
                    var meta = GameManager.Instance.MetaProgress;
                    if (!meta.UnlockedCards.Contains(card.MetaUnlockId))
                        return false;
                }

                return true;
            }).ToList();
        }

        /// <summary>加权随机抽取（稀有卡概率更低）</summary>
        private CardData WeightedRandomPick(List<CardData> pool)
        {
            if (pool.Count == 0) return null;

            // 稀有度权重：Common=50, Uncommon=30, Rare=15, Epic=4, Legendary=1
            float totalWeight = 0;
            var weights = new float[pool.Count];

            for (int i = 0; i < pool.Count; i++)
            {
                weights[i] = pool[i].Rarity switch
                {
                    CardRarity.Common => 50f,
                    CardRarity.Uncommon => 30f,
                    CardRarity.Rare => 15f,
                    CardRarity.Epic => 4f,
                    CardRarity.Legendary => 1f,
                    _ => 50f
                };
                totalWeight += weights[i];
            }

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0;

            for (int i = 0; i < pool.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return pool[i];
            }

            return pool[pool.Count - 1];
        }
    }
}
