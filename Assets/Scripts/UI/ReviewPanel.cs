// ============================================================================
// 官途浮沉 - 复盘面板
// ReviewPanel.cs — 回合结束时的总结与下回合预告
// ============================================================================
// 视觉设计：
//   仿"起居注"风格 — 竖排文字从右向左展开
//   三个区块：
//   1. 属性变化：用水墨数字动画展示增减（绿升红降）
//   2. 关系变动：NPC头像 + 好感度变化条
//   3. 下回合预告：一行提示语，暗示下回合可能发生的事
//   底部："安歇"按钮 → 结束回合
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GuantuFucheng.Core;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;
using GuantuFucheng.Models;

namespace GuantuFucheng.UI
{
    /// <summary>
    /// 复盘面板 — 回合结束的全面总结
    /// 
    /// 展示内容：
    /// 1. 本回合所有属性的变化量
    /// 2. NPC关系的变动（好感/信任增减）
    /// 3. 是否触发升迁/贬官
    /// 4. 下回合的事件预告（制造期待感）
    /// </summary>
    public class ReviewPanel : UIPanel
    {
        public override string PanelName => "复盘";

        [Header("=== 标题 ===")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI turnSummaryText;

        [Header("=== 属性变化区 ===")]
        [Tooltip("属性变化列表容器")]
        [SerializeField] private Transform attributeChangesContainer;
        [Tooltip("属性变化条目预制体")]
        [SerializeField] private GameObject attributeChangeItemPrefab;

        [Header("=== 关系变动区 ===")]
        [Tooltip("关系变动列表容器")]
        [SerializeField] private Transform relationChangesContainer;
        [Tooltip("关系变动条目预制体")]
        [SerializeField] private GameObject relationChangeItemPrefab;

        [Header("=== 下回合预告 ===")]
        [SerializeField] private TextMeshProUGUI previewText;

        [Header("=== 操作按钮 ===")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private TextMeshProUGUI endTurnButtonText;
        [SerializeField] private Button viewRelationsButton;

        // ======================== 运行时 ========================

        /// <summary>本回合记录的属性变化</summary>
        private Dictionary<PlayerAttribute, int> _attributeDeltas = new Dictionary<PlayerAttribute, int>();

        /// <summary>本回合记录的NPC关系变化</summary>
        private List<(string npcId, string npcName, int favorDelta, int trustDelta)> _relationDeltas
            = new List<(string, string, int, int)>();

        protected override void Awake()
        {
            base.Awake();
            endTurnButton?.onClick.AddListener(OnEndTurnClicked);
            viewRelationsButton?.onClick.AddListener(OnViewRelationsClicked);
        }

        private void OnEnable()
        {
            // 监听属性变化和关系变化，用于记录本回合的变动
            GameEvents.OnAttributeChanged += RecordAttributeChange;
            GameEvents.OnFavorChanged += RecordFavorChange;
        }

        private void OnDisable()
        {
            GameEvents.OnAttributeChanged -= RecordAttributeChange;
            GameEvents.OnFavorChanged -= RecordFavorChange;
        }

        protected override void OnShow()
        {
            var player = GameManager.Instance.CurrentRun.Player;
            int turn = TurnManager.Instance.CurrentTurnNumber;

            // 标题
            if (headerText != null)
            {
                headerText.text = "起居注 · 复盘";
                headerText.color = UIConfig.InkBlack;
                headerText.fontSize = UIConfig.FontSizeTitle;
            }

            if (turnSummaryText != null)
            {
                turnSummaryText.text = $"—— 第 {turn} 回合总结 ——";
                turnSummaryText.color = UIConfig.InkMedium;
                turnSummaryText.fontSize = UIConfig.FontSizeSubtitle;
            }

            // 展示属性变化
            RefreshAttributeChanges(player);

            // 展示关系变动
            RefreshRelationChanges();

            // 下回合预告
            RefreshNextTurnPreview(turn);

            // 按钮
            if (endTurnButtonText != null)
            {
                endTurnButtonText.text = "安歇";
            }
        }

        protected override void OnHide()
        {
            // 清除本回合记录
            _attributeDeltas.Clear();
            _relationDeltas.Clear();
        }

        // ======================== 数据记录 ========================

        /// <summary>记录属性变化（由GameEvents驱动）</summary>
        private void RecordAttributeChange(PlayerAttribute attr, int delta, int current)
        {
            if (!_attributeDeltas.ContainsKey(attr))
                _attributeDeltas[attr] = 0;
            _attributeDeltas[attr] += delta;
        }

        /// <summary>记录好感度变化</summary>
        private void RecordFavorChange(string npcId, int delta, int current)
        {
            // TODO: 从NPCData获取NPC名字
            _relationDeltas.Add((npcId, npcId, delta, 0));
        }

        // ======================== 界面刷新 ========================

        /// <summary>刷新属性变化列表</summary>
        private void RefreshAttributeChanges(PlayerState player)
        {
            if (attributeChangesContainer == null) return;

            // 清除旧条目
            foreach (Transform child in attributeChangesContainer)
                Destroy(child.gameObject);

            // 遍历所有属性维度
            var attributes = new (PlayerAttribute attr, string name)[]
            {
                (PlayerAttribute.Intellect, "才学"),
                (PlayerAttribute.Charisma, "人望"),
                (PlayerAttribute.Scheming, "权谋"),
                (PlayerAttribute.Martial, "武略"),
                (PlayerAttribute.Health, "体魄"),
                (PlayerAttribute.Reputation, "声望"),
            };

            foreach (var (attr, name) in attributes)
            {
                int delta = _attributeDeltas.ContainsKey(attr) ? _attributeDeltas[attr] : 0;
                int current = player.GetAttribute(attr);

                if (attributeChangeItemPrefab != null)
                {
                    var item = Instantiate(attributeChangeItemPrefab, attributeChangesContainer);
                    var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

                    if (texts.Length > 0)
                    {
                        texts[0].text = name;
                        texts[0].color = UIConfig.InkBlack;
                    }
                    if (texts.Length > 1)
                    {
                        texts[1].text = current.ToString();
                        texts[1].color = UIConfig.InkBlack;
                    }
                    if (texts.Length > 2)
                    {
                        texts[2].text = UIConfig.FormatDelta(delta);
                        // Rich Text处理颜色
                    }
                }
            }
        }

        /// <summary>刷新NPC关系变动列表</summary>
        private void RefreshRelationChanges()
        {
            if (relationChangesContainer == null) return;

            foreach (Transform child in relationChangesContainer)
                Destroy(child.gameObject);

            if (_relationDeltas.Count == 0)
            {
                // 无变动时显示提示
                if (relationChangeItemPrefab != null)
                {
                    var item = Instantiate(relationChangeItemPrefab, relationChangesContainer);
                    var text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = "本回合无关系变动";
                        text.color = UIConfig.InkLight;
                    }
                }
                return;
            }

            foreach (var (npcId, npcName, favorDelta, trustDelta) in _relationDeltas)
            {
                if (relationChangeItemPrefab != null)
                {
                    var item = Instantiate(relationChangeItemPrefab, relationChangesContainer);
                    var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

                    if (texts.Length > 0)
                    {
                        texts[0].text = npcName;
                        texts[0].color = UIConfig.InkBlack;
                    }
                    if (texts.Length > 1)
                    {
                        texts[1].text = $"好感 {UIConfig.FormatDelta(favorDelta)}";
                    }
                }
            }
        }

        /// <summary>生成下回合预告文字</summary>
        private void RefreshNextTurnPreview(int currentTurn)
        {
            if (previewText == null) return;

            // TODO: 从EventSystem获取下回合预告
            string preview = (currentTurn % 4) switch
            {
                0 => "下回合：吏部考评将至，宜早做准备。",
                1 => "下回合：听闻有贵客将至京城，或许是结交的好机会。",
                2 => "下回合：边关传来战报，朝堂上恐有一番争论。",
                _ => "下回合：风平浪静，正是修身养性的好时机。"
            };

            previewText.text = preview;
            previewText.color = UIConfig.InkMedium;
            previewText.fontSize = UIConfig.FontSizeBody;
            previewText.fontStyle = FontStyles.Italic;
        }

        // ======================== 按钮回调 ========================

        /// <summary>点击"安歇" → 结束本回合</summary>
        private void OnEndTurnClicked()
        {
            Debug.Log("[Review] 玩家确认安歇，结束回合");
            TurnManager.Instance.AdvancePhase();
        }

        /// <summary>查看NPC关系图谱</summary>
        private void OnViewRelationsClicked()
        {
            UIManager.Instance.ShowNPCRelationship();
        }
    }
}
