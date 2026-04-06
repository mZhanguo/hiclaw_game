// ============================================================================
// 官途浮沉 - 卡牌决策面板
// CardDecisionPanel.cs — 展示卡牌事件，呈现玩家选项
// ============================================================================
// 视觉设计：
//   卡牌以竖版卷轴形式展开在画面中央
//   卡牌背面：水墨云纹 + 品级印章（白/绿/蓝/紫/金边框）
//   卡牌正面：
//     - 顶部：事件名称（楷书大字）
//     - 中部：事件描述（配场景水墨插画区域）
//     - 底部：选项按钮（2-4个，横排或竖排）
//   翻牌动画：卡牌从背面翻转到正面，伴随"砚墨"飞溅粒子特效
//   选择后：卡牌燃尽效果（墨色火焰从边缘蔓延）
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GuantuFucheng.Core;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;

namespace GuantuFucheng.UI
{
    /// <summary>
    /// 卡牌决策面板 — 展示决策卡牌并等待玩家选择
    /// 
    /// 工作流程：
    /// 1. 执行阶段触发卡牌事件时，显示此面板
    /// 2. 展示卡牌描述和背景故事
    /// 3. 列出2-4个选项，每个选项显示预期倾向（不显示精确数值）
    /// 4. 玩家选择后，通过事件总线通知CardSystem结算
    /// 5. 显示简短结果后自动关闭或展示下一张卡
    /// </summary>
    public class CardDecisionPanel : UIPanel
    {
        public override string PanelName => "卡牌决策";

        [Header("=== 卡牌展示区 ===")]
        [Tooltip("卡牌名称")]
        [SerializeField] private TextMeshProUGUI cardNameText;

        [Tooltip("卡牌稀有度/类型标签")]
        [SerializeField] private TextMeshProUGUI cardTagText;

        [Tooltip("卡牌描述（事件叙述）")]
        [SerializeField] private TextMeshProUGUI cardDescText;

        [Tooltip("卡牌边框Image（用于设置稀有度颜色）")]
        [SerializeField] private Image cardBorderImage;

        [Tooltip("卡牌插画区域")]
        [SerializeField] private Image cardArtwork;

        [Header("=== 选项按钮 ===")]
        [Tooltip("选项按钮的父容器")]
        [SerializeField] private Transform choicesContainer;

        [Tooltip("选项按钮预制体")]
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("=== 结果展示 ===")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button continueButton;

        [Header("=== 无卡牌提示 ===")]
        [SerializeField] private TextMeshProUGUI noCardText;
        [SerializeField] private Button skipButton;

        // ======================== 运行时状态 ========================

        /// <summary>当前展示的卡牌ID</summary>
        private string _currentCardId;

        /// <summary>待处理的卡牌队列</summary>
        private Queue<string> _cardQueue = new Queue<string>();

        /// <summary>是否正在等待玩家选择</summary>
        private bool _waitingForChoice = false;

        protected override void Awake()
        {
            base.Awake();
            continueButton?.onClick.AddListener(OnContinueClicked);
            skipButton?.onClick.AddListener(OnSkipClicked);
        }

        private void OnEnable()
        {
            GameEvents.OnCardDrawn += OnCardDrawn;
        }

        private void OnDisable()
        {
            GameEvents.OnCardDrawn -= OnCardDrawn;
        }

        protected override void OnShow()
        {
            // 获取本回合的卡牌队列
            _cardQueue.Clear();
            var turnCards = TurnManager.Instance.TurnCards;

            if (turnCards != null && turnCards.Count > 0)
            {
                foreach (var cardId in turnCards)
                    _cardQueue.Enqueue(cardId);

                ShowNextCard();
            }
            else
            {
                // 本回合无卡牌事件
                ShowNoCardState();
            }
        }

        protected override void OnHide()
        {
            _currentCardId = null;
            _waitingForChoice = false;
            _cardQueue.Clear();
        }

        // ======================== 卡牌展示 ========================

        /// <summary>展示下一张卡牌</summary>
        private void ShowNextCard()
        {
            if (_cardQueue.Count == 0)
            {
                // 所有卡牌处理完毕，推进到下一阶段
                OnAllCardsProcessed();
                return;
            }

            _currentCardId = _cardQueue.Dequeue();
            _waitingForChoice = true;

            // 隐藏结果区，显示卡牌区
            SetCardVisible(true);
            SetResultVisible(false);
            SetNoCardVisible(false);

            // TODO: 从CardSystem获取卡牌数据
            // 暂时用占位数据演示
            DisplayCardData(_currentCardId);
        }

        /// <summary>用卡牌数据填充UI（TODO: 接入真实CardData）</summary>
        private void DisplayCardData(string cardId)
        {
            // 占位展示
            if (cardNameText != null)
            {
                cardNameText.text = "御史弹劾案";
                cardNameText.color = UIConfig.InkBlack;
                cardNameText.fontSize = UIConfig.FontSizeTitle;
            }

            if (cardTagText != null)
            {
                cardTagText.text = "【决策卡 · 蓝】";
                cardTagText.color = UIConfig.IndigoBlue;
                cardTagText.fontSize = UIConfig.FontSizeCaption;
            }

            if (cardDescText != null)
            {
                cardDescText.text = "御史中丞李崇德上疏弹劾户部侍郎贪墨案。此人与你有过数面之缘，" +
                    "曾在你初入官场时略有提携。如今他深陷漩涡，朝野瞩目。你将如何应对？";
                cardDescText.color = UIConfig.InkBlack;
                cardDescText.fontSize = UIConfig.FontSizeBody;
            }

            // 设置卡牌边框颜色（根据稀有度）
            if (cardBorderImage != null)
            {
                cardBorderImage.color = UIConfig.IndigoBlue; // 蓝卡
            }

            // 生成选项按钮
            GenerateChoiceButtons(new List<(string text, string hint)>
            {
                ("附议弹劾", "声望↑ 但可能失去一个盟友"),
                ("为其辩护", "好感↑ 但御史台可能记恨你"),
                ("沉默不语", "安全但可能被视为软弱"),
                ("暗中调查", "需要情报点，但可能发现真相")
            });
        }

        /// <summary>生成选项按钮</summary>
        private void GenerateChoiceButtons(List<(string text, string hint)> choices)
        {
            if (choicesContainer == null) return;

            // 清除旧按钮
            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < choices.Count; i++)
            {
                int choiceIndex = i; // 闭包捕获
                var choice = choices[i];

                if (choiceButtonPrefab != null)
                {
                    var buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
                    var button = buttonObj.GetComponent<Button>();
                    var texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();

                    // 设置选项文字
                    if (texts.Length > 0)
                    {
                        texts[0].text = choice.text;
                        texts[0].color = UIConfig.InkBlack;
                        texts[0].fontSize = UIConfig.FontSizeBody;
                    }
                    // 设置提示文字（预期倾向）
                    if (texts.Length > 1)
                    {
                        texts[1].text = choice.hint;
                        texts[1].color = UIConfig.InkLight;
                        texts[1].fontSize = UIConfig.FontSizeCaption;
                    }

                    button?.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
                }
            }
        }

        // ======================== 无卡牌状态 ========================

        private void ShowNoCardState()
        {
            SetCardVisible(false);
            SetResultVisible(false);
            SetNoCardVisible(true);

            if (noCardText != null)
            {
                noCardText.text = "本回合风平浪静，未有特别事件发生。";
                noCardText.color = UIConfig.InkLight;
                noCardText.fontSize = UIConfig.FontSizeBody;
            }
        }

        // ======================== 事件回调 ========================

        /// <summary>收到新抽卡通知</summary>
        private void OnCardDrawn(string cardId)
        {
            if (IsVisible)
            {
                _cardQueue.Enqueue(cardId);
            }
        }

        /// <summary>玩家选择了一个选项</summary>
        private void OnChoiceSelected(int choiceIndex)
        {
            if (!_waitingForChoice) return;
            _waitingForChoice = false;

            Debug.Log($"[CardDecision] 玩家选择了选项 {choiceIndex}，卡牌：{_currentCardId}");

            // 通过事件总线通知CardSystem
            GameEvents.CardChoiceMade(_currentCardId, choiceIndex);

            // 展示结果
            ShowResult(choiceIndex);
        }

        /// <summary>展示选择结果</summary>
        private void ShowResult(int choiceIndex)
        {
            SetCardVisible(false);
            SetResultVisible(true);

            if (resultText != null)
            {
                // TODO: 从CardSystem获取实际结算结果
                resultText.text = "你的选择将在本回合结算中产生影响...";
                resultText.color = UIConfig.InkMedium;
                resultText.fontSize = UIConfig.FontSizeBody;
            }
        }

        /// <summary>点击继续 → 展示下一张卡或推进阶段</summary>
        private void OnContinueClicked()
        {
            ShowNextCard();
        }

        /// <summary>跳过（无卡牌时直接推进）</summary>
        private void OnSkipClicked()
        {
            OnAllCardsProcessed();
        }

        /// <summary>所有卡牌处理完毕</summary>
        private void OnAllCardsProcessed()
        {
            Debug.Log("[CardDecision] 所有卡牌处理完毕，推进到复盘阶段");
            TurnManager.Instance.AdvancePhase();
        }

        // ======================== 显示控制 ========================

        private void SetCardVisible(bool visible)
        {
            cardNameText?.gameObject.SetActive(visible);
            cardTagText?.gameObject.SetActive(visible);
            cardDescText?.gameObject.SetActive(visible);
            cardBorderImage?.gameObject.SetActive(visible);
            cardArtwork?.gameObject.SetActive(visible);
            choicesContainer?.gameObject.SetActive(visible);
        }

        private void SetResultVisible(bool visible)
        {
            resultText?.gameObject.SetActive(visible);
            continueButton?.gameObject.SetActive(visible);
        }

        private void SetNoCardVisible(bool visible)
        {
            noCardText?.gameObject.SetActive(visible);
            skipButton?.gameObject.SetActive(visible);
        }

        // ======================== 工具方法 ========================

        /// <summary>根据卡牌稀有度获取边框颜色</summary>
        public static Color GetRarityColor(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => UIConfig.InkLight,        // 白卡：淡墨
                CardRarity.Uncommon => UIConfig.JadeGreen,     // 绿卡：松石绿
                CardRarity.Rare => UIConfig.IndigoBlue,        // 蓝卡：藏青
                CardRarity.Epic => new Color(0.55f, 0.25f, 0.60f), // 紫卡：紫藤色
                CardRarity.Legendary => UIConfig.OchreGold,    // 金卡：赭石金
                _ => UIConfig.InkMedium
            };
        }
    }
}
