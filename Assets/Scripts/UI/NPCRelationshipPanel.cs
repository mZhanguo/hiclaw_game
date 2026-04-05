// ============================================================================
// 官途浮沉 - NPC关系图谱面板
// NPCRelationshipPanel.cs — 以图谱形式展示所有NPC关系
// ============================================================================
// 视觉设计：
//   中央：玩家角色头像（水墨画风圆形头像）
//   周围：已认识的NPC按关系远近排列
//   连线：墨色粗细表示关系强度，颜色表示好恶
//     - 粗金线：莫逆之交
//     - 蓝线：至交
//     - 灰线：泛泛之交
//     - 红虚线：政敌/死敌
//   NPC卡片：小型竖版卡片，显示头像、名字、官职、阵营图标
//   点击NPC卡片展开详情（好感度/信任度数值条、性格标签、历史互动）
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
    /// NPC关系图谱面板 — 可视化玩家的人际关系网络
    /// 
    /// 功能：
    /// 1. 展示所有已结识NPC的关系状态
    /// 2. 按阵营/好感度分组或排序
    /// 3. 点击查看NPC详情
    /// 4. 高亮显示本回合发生变化的关系
    /// 
    /// 作为弹窗面板使用，覆盖在主面板之上
    /// </summary>
    public class NPCRelationshipPanel : UIPanel
    {
        public override string PanelName => "NPC关系图谱";

        [Header("=== 标题 ===")]
        [SerializeField] private TextMeshProUGUI headerText;

        [Header("=== 图谱视图 ===")]
        [Tooltip("NPC卡片的父容器（可用GridLayout或自定义布局）")]
        [SerializeField] private Transform npcCardsContainer;
        [Tooltip("NPC卡片预制体")]
        [SerializeField] private GameObject npcCardPrefab;

        [Header("=== NPC详情区 ===")]
        [Tooltip("选中NPC的详情面板")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailFactionText;
        [SerializeField] private Slider favorSlider;
        [SerializeField] private TextMeshProUGUI favorLabelText;
        [SerializeField] private Slider trustSlider;
        [SerializeField] private TextMeshProUGUI trustLabelText;
        [SerializeField] private TextMeshProUGUI personalityText;
        [SerializeField] private TextMeshProUGUI historyText;

        [Header("=== 筛选/排序 ===")]
        [SerializeField] private Button sortByFavorButton;
        [SerializeField] private Button sortByFactionButton;

        [Header("=== 关闭按钮 ===")]
        [SerializeField] private Button closeButton;

        // ======================== 运行时 ========================

        /// <summary>当前选中的NPC ID</summary>
        private string _selectedNpcId;

        protected override void Awake()
        {
            base.Awake();
            closeButton?.onClick.AddListener(OnCloseClicked);
            sortByFavorButton?.onClick.AddListener(() => RefreshNPCList(SortMode.ByFavor));
            sortByFactionButton?.onClick.AddListener(() => RefreshNPCList(SortMode.ByFaction));
        }

        protected override void OnShow()
        {
            if (headerText != null)
            {
                headerText.text = "关系图谱";
                headerText.color = UIConfig.InkBlack;
                headerText.fontSize = UIConfig.FontSizeTitle;
            }

            // 默认按好感度排序
            RefreshNPCList(SortMode.ByFavor);

            // 隐藏详情
            detailPanel?.SetActive(false);
        }

        // ======================== NPC列表 ========================

        private enum SortMode { ByFavor, ByFaction }

        /// <summary>刷新NPC卡片列表</summary>
        private void RefreshNPCList(SortMode sortMode)
        {
            if (npcCardsContainer == null) return;

            // 清除旧卡片
            foreach (Transform child in npcCardsContainer)
                Destroy(child.gameObject);

            var relationships = GameManager.Instance.CurrentRun.Relationships;

            // 排序
            var sorted = new List<NPCRelationshipState>(relationships);
            switch (sortMode)
            {
                case SortMode.ByFavor:
                    sorted.Sort((a, b) => b.Favor.CompareTo(a.Favor)); // 好感高的排前面
                    break;
                case SortMode.ByFaction:
                    // TODO: 按阵营分组排序
                    break;
            }

            // 创建NPC卡片
            foreach (var rel in sorted)
            {
                CreateNPCCard(rel);
            }
        }

        /// <summary>创建单个NPC卡片</summary>
        private void CreateNPCCard(NPCRelationshipState rel)
        {
            if (npcCardPrefab == null) return;

            var cardObj = Instantiate(npcCardPrefab, npcCardsContainer);

            // 设置卡片颜色（根据好感度）
            var bgImage = cardObj.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = UIConfig.PaperColor;
            }

            // 设置NPC名字和好感度
            var texts = cardObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
            {
                // TODO: 从NPCData获取NPC显示名
                texts[0].text = rel.NpcId;
                texts[0].color = UIConfig.InkBlack;
                texts[0].fontSize = UIConfig.FontSizeBody;
            }
            if (texts.Length > 1)
            {
                texts[1].text = rel.FavorLevel;
                texts[1].color = UIConfig.GetFavorColor(rel.Favor);
                texts[1].fontSize = UIConfig.FontSizeCaption;
            }

            // 左边框颜色指示关系等级
            // 通过Outline或额外Image实现
            var outlines = cardObj.GetComponentsInChildren<Outline>();
            if (outlines.Length > 0)
            {
                outlines[0].effectColor = UIConfig.GetFavorColor(rel.Favor);
            }

            // 点击卡片显示详情
            var button = cardObj.GetComponent<Button>();
            if (button == null)
                button = cardObj.AddComponent<Button>();

            string npcId = rel.NpcId;
            button.onClick.AddListener(() => ShowNPCDetail(npcId));
        }

        // ======================== NPC详情 ========================

        /// <summary>展示选中NPC的详细信息</summary>
        private void ShowNPCDetail(string npcId)
        {
            _selectedNpcId = npcId;
            var rel = GameManager.Instance.CurrentRun.GetRelationship(npcId);
            if (rel == null) return;

            detailPanel?.SetActive(true);

            // NPC名称
            if (detailNameText != null)
            {
                detailNameText.text = npcId; // TODO: 从NPCData获取
                detailNameText.color = UIConfig.InkBlack;
                detailNameText.fontSize = UIConfig.FontSizeSubtitle;
            }

            // 官职
            if (detailTitleText != null)
            {
                detailTitleText.text = "（官职待接入）";
                detailTitleText.color = UIConfig.InkMedium;
            }

            // 阵营
            if (detailFactionText != null)
            {
                detailFactionText.text = "阵营：待接入";
                detailFactionText.color = UIConfig.InkMedium;
            }

            // 好感度条
            if (favorSlider != null)
            {
                favorSlider.minValue = -100;
                favorSlider.maxValue = 100;
                favorSlider.value = rel.Favor;
                favorSlider.interactable = false; // 只读
            }
            if (favorLabelText != null)
            {
                favorLabelText.text = $"好感：{rel.Favor}（{rel.FavorLevel}）";
                favorLabelText.color = UIConfig.GetFavorColor(rel.Favor);
            }

            // 信任度条
            if (trustSlider != null)
            {
                trustSlider.minValue = -100;
                trustSlider.maxValue = 100;
                trustSlider.value = rel.Trust;
                trustSlider.interactable = false;
            }
            if (trustLabelText != null)
            {
                trustLabelText.text = $"信任：{rel.Trust}（{rel.TrustLevel}）";
                trustLabelText.color = UIConfig.IndigoBlue;
            }

            // 性格标签
            if (personalityText != null)
            {
                personalityText.text = rel.IsRevealed ? "性格：待接入" : "性格：未探明（需投入情报点）";
                personalityText.color = rel.IsRevealed ? UIConfig.InkBlack : UIConfig.InkLight;
            }

            // 互动历史
            if (historyText != null)
            {
                if (rel.TriggeredEvents.Count > 0)
                {
                    historyText.text = $"共有 {rel.TriggeredEvents.Count} 次重要互动";
                }
                else
                {
                    historyText.text = "尚无重要互动记录";
                }
                historyText.color = UIConfig.InkLight;
            }
        }

        // ======================== 按钮回调 ========================

        private void OnCloseClicked()
        {
            UIManager.Instance.CloseTopPopup();
        }
    }
}
