// ============================================================================
// 官途浮沉 - 行动力分配面板
// ActionAllocationPanel.cs — 玩家将6点行动力分配到各行动池
// ============================================================================
// 视觉设计：
//   六个行动池排列为两行三列的"六芒格局"
//   每个池子是一个墨色圆形容器，中央显示已分配点数
//   池子图标风格：水墨简笔画（毛笔书卷=政务、酒杯=交际、暗影=情报...）
//   未分配的行动力在顶部显示为一排墨点
//   拖拽或点击+/-分配行动力
//   已分配的池子墨色加深，发出微光
//   底部：确认按钮（确认后不可更改）+ 重置按钮
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
    /// 行动力分配面板 — 游戏核心交互界面之一
    /// 
    /// 玩家拥有6点行动力（可通过Meta升级增加），
    /// 需要将行动力分配到六个行动方向：
    /// 政务 / 交际 / 情报 / 修身 / 谋略 / 休息
    /// 
    /// 交互方式：
    /// - 点击 +/- 按钮增减分配
    /// - 或直接拖拽行动力墨点到对应池子
    /// - 确认后进入执行阶段
    /// </summary>
    public class ActionAllocationPanel : UIPanel
    {
        public override string PanelName => "行动力分配";

        [Header("=== 标题 ===")]
        [SerializeField] private TextMeshProUGUI headerText;

        [Header("=== 剩余行动力 ===")]
        [SerializeField] private TextMeshProUGUI remainingPointsText;

        [Header("=== 行动力池容器 ===")]
        [Tooltip("六个ActionSlot的父容器")]
        [SerializeField] private Transform slotsContainer;

        [Header("=== 操作按钮 ===")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private Button resetButton;
        [SerializeField] private TextMeshProUGUI resetButtonText;

        [Header("=== 预览区 ===")]
        [Tooltip("选中行动的预期效果预览")]
        [SerializeField] private TextMeshProUGUI previewText;

        // ======================== 运行时数据 ========================

        /// <summary>各行动类型的分配点数</summary>
        private Dictionary<ActionType, int> _allocation = new Dictionary<ActionType, int>();

        /// <summary>总可用行动力</summary>
        private int _maxPoints;

        /// <summary>行动类型的中文名称与描述</summary>
        private static readonly Dictionary<ActionType, (string Name, string Desc, string Icon)> ActionInfo
            = new Dictionary<ActionType, (string, string, string)>
        {
            { ActionType.Politics,     ("政务", "处理公文、推行政策、治理地方。\n提升声望与影响力。", "📜") },
            { ActionType.Social,       ("交际", "拜访官员、宴请结交、维护关系。\n提升NPC好感度。", "🍶") },
            { ActionType.Intelligence, ("情报", "打探消息、收集把柄、监视对手。\n获取情报与预警。", "👁") },
            { ActionType.SelfImprove,  ("修身", "读书练武、养生调息、提升自我。\n增加属性点。", "📖") },
            { ActionType.Scheme,       ("谋略", "设局布阵、暗中操作、推波助澜。\n影响事件走向。", "♟") },
            { ActionType.Rest,         ("休息", "告假休沐、养精蓄锐。\n恢复体魄，但可能错过机会。", "🏠") },
        };

        protected override void Awake()
        {
            base.Awake();
            confirmButton?.onClick.AddListener(OnConfirmClicked);
            resetButton?.onClick.AddListener(OnResetClicked);
        }

        protected override void OnShow()
        {
            var player = GameManager.Instance.CurrentRun.Player;
            _maxPoints = player.MaxActionPoints;

            // 重置分配
            _allocation.Clear();
            foreach (ActionType type in System.Enum.GetValues(typeof(ActionType)))
            {
                _allocation[type] = 0;
            }

            // 标题
            if (headerText != null)
            {
                headerText.text = "行动力分配";
                headerText.color = UIConfig.InkBlack;
                headerText.fontSize = UIConfig.FontSizeTitle;
            }

            // 按钮文字
            if (confirmButtonText != null)
            {
                confirmButtonText.text = "确认分配";
            }
            if (resetButtonText != null)
            {
                resetButtonText.text = "重置";
            }

            // 清空预览
            if (previewText != null)
            {
                previewText.text = "点击行动池查看预期效果";
                previewText.color = UIConfig.InkLight;
            }

            RefreshDisplay();
        }

        // ======================== 分配逻辑 ========================

        /// <summary>
        /// 增加指定行动类型的分配点数
        /// 供ActionSlot的+按钮调用
        /// </summary>
        public void AddPoint(ActionType type)
        {
            int remaining = GetRemainingPoints();
            if (remaining <= 0)
            {
                Debug.Log("[ActionAllocation] 行动力已全部分配");
                return;
            }

            _allocation[type]++;
            RefreshDisplay();
            ShowActionPreview(type);
        }

        /// <summary>
        /// 减少指定行动类型的分配点数
        /// </summary>
        public void RemovePoint(ActionType type)
        {
            if (_allocation[type] <= 0) return;

            _allocation[type]--;
            RefreshDisplay();
        }

        /// <summary>获取剩余未分配行动力</summary>
        public int GetRemainingPoints()
        {
            int used = 0;
            foreach (var kvp in _allocation)
                used += kvp.Value;
            return _maxPoints - used;
        }

        /// <summary>获取指定行动的已分配点数</summary>
        public int GetAllocated(ActionType type)
        {
            return _allocation.ContainsKey(type) ? _allocation[type] : 0;
        }

        // ======================== 界面刷新 ========================

        /// <summary>刷新所有显示元素</summary>
        private void RefreshDisplay()
        {
            int remaining = GetRemainingPoints();

            // 剩余行动力
            if (remainingPointsText != null)
            {
                remainingPointsText.text = $"剩余行动力：{remaining} / {_maxPoints}";
                remainingPointsText.color = remaining > 0 ? UIConfig.InkBlack : UIConfig.CinnabarRed;
                remainingPointsText.fontSize = UIConfig.FontSizeSubtitle;
            }

            // 确认按钮状态（至少分配1点才能确认）
            int totalAllocated = _maxPoints - remaining;
            if (confirmButton != null)
            {
                confirmButton.interactable = totalAllocated >= 1;
            }

            // 通知各ActionSlot刷新（通过事件或直接引用）
            // 子对象会通过GetComponentInParent获取此面板来查询分配状态
        }

        /// <summary>显示行动类型的预期效果预览</summary>
        private void ShowActionPreview(ActionType type)
        {
            if (previewText == null) return;

            if (ActionInfo.TryGetValue(type, out var info))
            {
                int points = _allocation[type];
                previewText.text = $"{info.Icon} {info.Name}（已分配 {points} 点）\n{info.Desc}";
                previewText.color = UIConfig.InkBlack;
            }
        }

        // ======================== 按钮回调 ========================

        /// <summary>确认分配 → 提交到TurnManager并推进阶段</summary>
        private void OnConfirmClicked()
        {
            Debug.Log("[ActionAllocation] 玩家确认行动分配");

            // 将分配方案提交给TurnManager
            TurnManager.Instance.ResetAllocation();
            foreach (var kvp in _allocation)
            {
                if (kvp.Value > 0)
                {
                    TurnManager.Instance.AllocateAction(kvp.Key, kvp.Value);
                }
            }

            // 推进到执行阶段
            TurnManager.Instance.AdvancePhase();
        }

        /// <summary>重置所有分配</summary>
        private void OnResetClicked()
        {
            Debug.Log("[ActionAllocation] 重置行动分配");
            foreach (ActionType type in System.Enum.GetValues(typeof(ActionType)))
            {
                _allocation[type] = 0;
            }

            if (previewText != null)
            {
                previewText.text = "点击行动池查看预期效果";
                previewText.color = UIConfig.InkLight;
            }

            RefreshDisplay();
        }

        // ======================== 公开查询（供子组件使用） ========================

        /// <summary>获取行动类型的显示信息</summary>
        public static (string Name, string Desc, string Icon) GetActionInfo(ActionType type)
        {
            return ActionInfo.TryGetValue(type, out var info) ? info : ("未知", "", "?");
        }
    }
}
