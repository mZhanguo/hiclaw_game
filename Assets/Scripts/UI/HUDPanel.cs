// ============================================================================
// 官途浮沉 - HUD面板
// HUDPanel.cs — 常驻界面，显示核心信息概览
// ============================================================================
// 视觉设计：
//   水墨风半透明条带，常驻屏幕顶部
//   左侧：回合数（毛笔数字风格）+ 当前阶段名
//   中央：官职名称（带品级印章小图标）
//   右侧：六维属性迷你条（细墨线风格，颜色编码）
//   整体高度约60-80px，不遮挡主要游戏内容
//   宣纸底色半透明(0.85)，底部有淡墨水渍装饰线
// ============================================================================

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
    /// HUD面板 — 常驻顶部的信息概览条
    /// 
    /// 实时显示：
    /// - 回合数 & 当前阶段
    /// - 官职名称
    /// - 六维属性条（才学/人望/权谋/武略/体魄/声望）
    /// - 金银 & 影响力
    /// - 行动力剩余（仅在行动分配阶段显示）
    /// 
    /// 所有数值变化带有平滑过渡动画
    /// </summary>
    public class HUDPanel : UIPanel
    {
        public override string PanelName => "HUD";

        [Header("=== 回合信息 ===")]
        [SerializeField] private TextMeshProUGUI turnNumberText;
        [SerializeField] private TextMeshProUGUI phaseNameText;

        [Header("=== 官职 ===")]
        [SerializeField] private TextMeshProUGUI rankText;

        [Header("=== 六维属性条 ===")]
        [SerializeField] private Slider intellectBar;
        [SerializeField] private TextMeshProUGUI intellectValueText;

        [SerializeField] private Slider charismaBar;
        [SerializeField] private TextMeshProUGUI charismaValueText;

        [SerializeField] private Slider schemingBar;
        [SerializeField] private TextMeshProUGUI schemingValueText;

        [SerializeField] private Slider martialBar;
        [SerializeField] private TextMeshProUGUI martialValueText;

        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthValueText;

        [SerializeField] private Slider reputationBar;
        [SerializeField] private TextMeshProUGUI reputationValueText;

        [Header("=== 资源 ===")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI influenceText;

        [Header("=== 行动力（分配阶段可见） ===")]
        [SerializeField] private GameObject actionPointsGroup;
        [SerializeField] private TextMeshProUGUI actionPointsText;

        // ======================== 动画目标值 ========================

        // 用于属性条平滑过渡的目标值
        private float _targetIntellect, _targetCharisma, _targetScheming;
        private float _targetMartial, _targetHealth, _targetReputation;

        // ======================== 生命周期 ========================

        private void OnEnable()
        {
            // 订阅核心事件
            GameEvents.OnTurnStarted += OnTurnStarted;
            GameEvents.OnPhaseChanged += OnPhaseChanged;
            GameEvents.OnAttributeChanged += OnAttributeChanged;
            GameEvents.OnRankChanged += OnRankChanged;
            GameEvents.OnActionPointAllocated += OnActionPointAllocated;
        }

        private void OnDisable()
        {
            GameEvents.OnTurnStarted -= OnTurnStarted;
            GameEvents.OnPhaseChanged -= OnPhaseChanged;
            GameEvents.OnAttributeChanged -= OnAttributeChanged;
            GameEvents.OnRankChanged -= OnRankChanged;
            GameEvents.OnActionPointAllocated -= OnActionPointAllocated;
        }

        protected override void OnShow()
        {
            RefreshAll();
        }

        private void Update()
        {
            if (!IsVisible) return;

            // 平滑过渡属性条
            float speed = 1f / UIConfig.HUDBarTransitionDuration;
            SmoothBar(intellectBar, _targetIntellect, speed);
            SmoothBar(charismaBar, _targetCharisma, speed);
            SmoothBar(schemingBar, _targetScheming, speed);
            SmoothBar(martialBar, _targetMartial, speed);
            SmoothBar(healthBar, _targetHealth, speed);
            SmoothBar(reputationBar, _targetReputation, speed);
        }

        // ======================== 全量刷新 ========================

        /// <summary>刷新所有HUD元素</summary>
        public void RefreshAll()
        {
            if (GameManager.Instance?.CurrentRun == null) return;

            var player = GameManager.Instance.CurrentRun.Player;
            var turnManager = TurnManager.Instance;

            // 回合数
            if (turnNumberText != null)
            {
                turnNumberText.text = $"第 {turnManager.CurrentTurnNumber} 回合";
                turnNumberText.color = UIConfig.InkBlack;
                turnNumberText.fontSize = UIConfig.FontSizeHUD;
            }

            // 阶段名
            UpdatePhaseName(turnManager.CurrentPhase);

            // 官职
            UpdateRankDisplay(player.CurrentRank);

            // 属性条
            UpdateAttributeBars(player);

            // 资源
            if (goldText != null)
            {
                goldText.text = $"金银 {player.Gold}";
                goldText.color = UIConfig.OchreGold;
                goldText.fontSize = UIConfig.FontSizeHUD;
            }
            if (influenceText != null)
            {
                influenceText.text = $"影响 {player.Influence}";
                influenceText.color = UIConfig.IndigoBlue;
                influenceText.fontSize = UIConfig.FontSizeHUD;
            }

            // 行动力
            UpdateActionPoints(player);
        }

        // ======================== 增量更新 ========================

        private void OnTurnStarted(int turnNumber)
        {
            if (turnNumberText != null)
                turnNumberText.text = $"第 {turnNumber} 回合";
            RefreshAll();
        }

        private void OnPhaseChanged(TurnPhase phase)
        {
            UpdatePhaseName(phase);

            // 行动力显示仅在分配阶段可见
            if (actionPointsGroup != null)
                actionPointsGroup.SetActive(phase == TurnPhase.ActionAllocation);
        }

        private void OnAttributeChanged(PlayerAttribute attr, int delta, int current)
        {
            // 更新目标值（Update中平滑过渡）
            switch (attr)
            {
                case PlayerAttribute.Intellect:
                    _targetIntellect = current;
                    UpdateValueText(intellectValueText, current);
                    break;
                case PlayerAttribute.Charisma:
                    _targetCharisma = current;
                    UpdateValueText(charismaValueText, current);
                    break;
                case PlayerAttribute.Scheming:
                    _targetScheming = current;
                    UpdateValueText(schemingValueText, current);
                    break;
                case PlayerAttribute.Martial:
                    _targetMartial = current;
                    UpdateValueText(martialValueText, current);
                    break;
                case PlayerAttribute.Health:
                    _targetHealth = current;
                    UpdateValueText(healthValueText, current);
                    break;
                case PlayerAttribute.Reputation:
                    _targetReputation = current;
                    UpdateValueText(reputationValueText, current);
                    break;
            }
        }

        private void OnRankChanged(OfficialRank oldRank, OfficialRank newRank)
        {
            UpdateRankDisplay(newRank);
        }

        private void OnActionPointAllocated(ActionType type, int points)
        {
            if (GameManager.Instance?.CurrentRun == null) return;
            UpdateActionPoints(GameManager.Instance.CurrentRun.Player);
        }

        // ======================== 显示更新方法 ========================

        private void UpdatePhaseName(TurnPhase phase)
        {
            if (phaseNameText == null) return;

            phaseNameText.text = phase switch
            {
                TurnPhase.MorningBriefing => "早朝简报",
                TurnPhase.ActionAllocation => "行动分配",
                TurnPhase.Execution => "执行结算",
                TurnPhase.Review => "复盘总结",
                _ => ""
            };
            phaseNameText.color = UIConfig.InkMedium;
            phaseNameText.fontSize = UIConfig.FontSizeHUD;
        }

        private void UpdateRankDisplay(OfficialRank rank)
        {
            if (rankText == null) return;

            string name = rank switch
            {
                OfficialRank.Candidate => "候补",
                OfficialRank.CountyMagistrate => "县令",
                OfficialRank.Prefect => "州刺史",
                OfficialRank.ViceMinister => "侍郎",
                OfficialRank.Minister => "尚书",
                OfficialRank.MilitaryGovernor => "节度使",
                OfficialRank.GrandCouncilor => "宰相",
                _ => "未知"
            };

            rankText.text = name;
            rankText.color = UIConfig.CinnabarRed;
            rankText.fontSize = UIConfig.FontSizeHUD + 2;
        }

        private void UpdateAttributeBars(PlayerState player)
        {
            // 属性最大值基准（用于归一化条长度）
            const float maxAttr = 50f; // 普通属性理论上限
            const float maxHealth = 100f;
            const float maxReputation = 100f;

            // 设置目标值
            _targetIntellect = player.Intellect / maxAttr;
            _targetCharisma = player.Charisma / maxAttr;
            _targetScheming = player.Scheming / maxAttr;
            _targetMartial = player.Martial / maxAttr;
            _targetHealth = player.Health / maxHealth;
            _targetReputation = (player.Reputation + 100f) / (maxReputation * 2f); // 声望可负，归一化到0-1

            // 设置初始值（避免首次显示跳变）
            SetBarImmediate(intellectBar, _targetIntellect);
            SetBarImmediate(charismaBar, _targetCharisma);
            SetBarImmediate(schemingBar, _targetScheming);
            SetBarImmediate(martialBar, _targetMartial);
            SetBarImmediate(healthBar, _targetHealth);
            SetBarImmediate(reputationBar, _targetReputation);

            // 数值文字
            UpdateValueText(intellectValueText, player.Intellect);
            UpdateValueText(charismaValueText, player.Charisma);
            UpdateValueText(schemingValueText, player.Scheming);
            UpdateValueText(martialValueText, player.Martial);
            UpdateValueText(healthValueText, player.Health);
            UpdateValueText(reputationValueText, player.Reputation);
        }

        private void UpdateActionPoints(PlayerState player)
        {
            if (actionPointsText == null) return;
            actionPointsText.text = $"行动力 {player.CurrentActionPoints}/{player.MaxActionPoints}";
            actionPointsText.color = player.CurrentActionPoints > 0 ? UIConfig.IndigoBlue : UIConfig.InkLight;
            actionPointsText.fontSize = UIConfig.FontSizeHUD;
        }

        // ======================== 工具方法 ========================

        /// <summary>平滑过渡Slider值</summary>
        private void SmoothBar(Slider bar, float target, float speed)
        {
            if (bar == null) return;
            bar.value = Mathf.MoveTowards(bar.value, target, speed * Time.deltaTime);
        }

        /// <summary>立即设置Slider值（无动画）</summary>
        private void SetBarImmediate(Slider bar, float value)
        {
            if (bar == null) return;
            bar.value = value;
        }

        /// <summary>更新数值文字</summary>
        private void UpdateValueText(TextMeshProUGUI text, int value)
        {
            if (text == null) return;
            text.text = value.ToString();
            text.color = UIConfig.InkBlack;
            text.fontSize = UIConfig.FontSizeHUD;
        }
    }
}
