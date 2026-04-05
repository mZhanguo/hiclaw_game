// ============================================================================
// 官途浮沉 - 全局UI管理器
// UIManager.cs — 面板生命周期管理、弹窗队列、过渡动画调度
// ============================================================================
// 职责：
//   1. 管理所有UI面板的显示/隐藏/切换
//   2. 维护面板栈（支持弹窗覆盖主面板）
//   3. 提供全局过渡动画接口（水墨晕染遮罩）
//   4. 监听GameEvents，自动在阶段切换时调度对应面板
// ============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuantuFucheng.Core;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;

namespace GuantuFucheng.UI
{
    /// <summary>
    /// UI管理器 — 全局单例，管理所有UI面板的生命周期
    /// 
    /// 面板层级设计（从低到高）：
    /// - Layer 0: HUD（常驻不隐藏）
    /// - Layer 1: 主面板（同时只有一个：主菜单/简报/分配/卡牌/复盘）
    /// - Layer 2: 弹窗（NPC关系图谱、考评结果等，可覆盖在主面板上）
    /// - Layer 3: 全局遮罩（场景过渡的水墨晕染效果）
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        // ======================== 面板引用 ========================

        [Header("=== 主面板 ===")]
        [Tooltip("主菜单面板")]
        [SerializeField] private MainMenuPanel mainMenuPanel;

        [Tooltip("早朝简报面板")]
        [SerializeField] private MorningBriefingPanel morningBriefingPanel;

        [Tooltip("行动分配面板")]
        [SerializeField] private ActionAllocationPanel actionAllocationPanel;

        [Tooltip("卡牌决策面板")]
        [SerializeField] private CardDecisionPanel cardDecisionPanel;

        [Tooltip("复盘面板")]
        [SerializeField] private ReviewPanel reviewPanel;

        [Header("=== 弹窗面板 ===")]
        [Tooltip("NPC关系图谱")]
        [SerializeField] private NPCRelationshipPanel npcRelationshipPanel;

        [Tooltip("吏部考评面板")]
        [SerializeField] private EvaluationPanel evaluationPanel;

        [Header("=== 常驻UI ===")]
        [Tooltip("HUD面板")]
        [SerializeField] private HUDPanel hudPanel;

        [Header("=== 过渡动画 ===")]
        [Tooltip("水墨晕染遮罩Image（全屏覆盖，用于场景过渡）")]
        [SerializeField] private Image transitionMask;

        // ======================== 内部状态 ========================

        /// <summary>当前显示的主面板</summary>
        private UIPanel _currentMainPanel;

        /// <summary>弹窗栈 — 支持多层弹窗叠加</summary>
        private readonly Stack<UIPanel> _popupStack = new Stack<UIPanel>();

        /// <summary>是否正在执行过渡动画</summary>
        private bool _isTransitioning = false;

        /// <summary>所有面板引用列表（方便批量操作）</summary>
        private readonly List<UIPanel> _allPanels = new List<UIPanel>();

        // ======================== 初始化 ========================

        protected override void OnSingletonAwake()
        {
            // 收集所有面板引用
            RegisterPanel(mainMenuPanel);
            RegisterPanel(morningBriefingPanel);
            RegisterPanel(actionAllocationPanel);
            RegisterPanel(cardDecisionPanel);
            RegisterPanel(reviewPanel);
            RegisterPanel(npcRelationshipPanel);
            RegisterPanel(evaluationPanel);
            RegisterPanel(hudPanel);

            // 确保遮罩初始不可见
            if (transitionMask != null)
            {
                transitionMask.color = new Color(0, 0, 0, 0);
                transitionMask.raycastTarget = false;
                transitionMask.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            // 订阅游戏事件 — 自动调度UI面板
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
            GameEvents.OnRankChanged += HandleRankChanged;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
            GameEvents.OnRankChanged -= HandleRankChanged;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        // ======================== 面板切换 ========================

        /// <summary>
        /// 切换主面板（带过渡动画）
        /// 同一时间只有一个主面板可见，切换时先淡出旧面板再淡入新面板
        /// </summary>
        /// <param name="panel">目标面板</param>
        /// <param name="withTransition">是否使用水墨过渡动画</param>
        public void ShowMainPanel(UIPanel panel, bool withTransition = true)
        {
            if (panel == null || panel == _currentMainPanel) return;
            if (_isTransitioning) return; // 防止动画重入

            if (withTransition && _currentMainPanel != null)
            {
                StartCoroutine(TransitionToPanel(panel));
            }
            else
            {
                // 无动画直接切换
                HideCurrentMainPanel();
                _currentMainPanel = panel;
                panel.Show();
            }
        }

        /// <summary>隐藏当前主面板</summary>
        private void HideCurrentMainPanel()
        {
            if (_currentMainPanel != null)
            {
                _currentMainPanel.Hide();
                _currentMainPanel = null;
            }
        }

        /// <summary>
        /// 显示弹窗面板（压入弹窗栈）
        /// 弹窗不会隐藏主面板，而是覆盖在其上方
        /// </summary>
        public void ShowPopup(UIPanel popup)
        {
            if (popup == null) return;
            _popupStack.Push(popup);
            popup.Show();
        }

        /// <summary>
        /// 关闭最顶层弹窗
        /// </summary>
        public void CloseTopPopup()
        {
            if (_popupStack.Count > 0)
            {
                var top = _popupStack.Pop();
                top.Hide();
            }
        }

        /// <summary>关闭所有弹窗</summary>
        public void CloseAllPopups()
        {
            while (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                popup.Hide();
            }
        }

        /// <summary>隐藏所有面板（用于重置/切场景）</summary>
        public void HideAll()
        {
            foreach (var panel in _allPanels)
            {
                if (panel != null)
                    panel.Hide();
            }
            _currentMainPanel = null;
            _popupStack.Clear();
        }

        // ======================== 快捷访问方法 ========================

        /// <summary>显示主菜单</summary>
        public void ShowMainMenu() => ShowMainPanel(mainMenuPanel, false);

        /// <summary>显示早朝简报</summary>
        public void ShowMorningBriefing() => ShowMainPanel(morningBriefingPanel);

        /// <summary>显示行动分配界面</summary>
        public void ShowActionAllocation() => ShowMainPanel(actionAllocationPanel);

        /// <summary>显示卡牌决策界面</summary>
        public void ShowCardDecision() => ShowMainPanel(cardDecisionPanel);

        /// <summary>显示复盘界面</summary>
        public void ShowReview() => ShowMainPanel(reviewPanel);

        /// <summary>弹出NPC关系图谱</summary>
        public void ShowNPCRelationship() => ShowPopup(npcRelationshipPanel);

        /// <summary>弹出考评结果</summary>
        public void ShowEvaluation() => ShowPopup(evaluationPanel);

        /// <summary>显示/隐藏HUD</summary>
        public void SetHUDVisible(bool visible)
        {
            if (visible) hudPanel?.Show();
            else hudPanel?.Hide();
        }

        // ======================== 过渡动画 ========================

        /// <summary>
        /// 水墨晕染过渡 — 模拟墨水晕开又收回的视觉效果
        /// 过程：墨色渐浓(遮罩淡入) → 切换面板 → 墨色渐淡(遮罩淡出)
        /// </summary>
        private IEnumerator TransitionToPanel(UIPanel targetPanel)
        {
            _isTransitioning = true;

            if (transitionMask != null)
            {
                transitionMask.gameObject.SetActive(true);
                transitionMask.raycastTarget = true; // 阻止穿透点击

                // Phase 1: 墨色渐浓（遮罩淡入）
                float halfDuration = UIConfig.SceneTransitionDuration * 0.5f;
                yield return StartCoroutine(FadeMask(0f, 1f, halfDuration));

                // 在遮罩全黑时切换面板
                HideCurrentMainPanel();
                _currentMainPanel = targetPanel;
                targetPanel.Show();

                // Phase 2: 墨色渐淡（遮罩淡出）
                yield return StartCoroutine(FadeMask(1f, 0f, halfDuration));

                transitionMask.raycastTarget = false;
                transitionMask.gameObject.SetActive(false);
            }
            else
            {
                // 无遮罩时直接切换
                HideCurrentMainPanel();
                _currentMainPanel = targetPanel;
                targetPanel.Show();
            }

            _isTransitioning = false;
        }

        /// <summary>遮罩Alpha渐变协程</summary>
        private IEnumerator FadeMask(float from, float to, float duration)
        {
            float elapsed = 0f;
            Color maskColor = UIConfig.OverlayDim;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration); // 使用SmoothStep实现缓入缓出
                maskColor.a = Mathf.Lerp(from, to, t);
                transitionMask.color = maskColor;
                yield return null;
            }

            maskColor.a = to;
            transitionMask.color = maskColor;
        }

        /// <summary>
        /// 播放全屏过渡动画（供外部调用，如场景切换）
        /// </summary>
        /// <param name="onMidpoint">在遮罩全黑时执行的回调</param>
        public void PlayTransition(Action onMidpoint = null)
        {
            StartCoroutine(PlayTransitionCoroutine(onMidpoint));
        }

        private IEnumerator PlayTransitionCoroutine(Action onMidpoint)
        {
            _isTransitioning = true;

            if (transitionMask != null)
            {
                transitionMask.gameObject.SetActive(true);
                transitionMask.raycastTarget = true;

                float halfDuration = UIConfig.SceneTransitionDuration * 0.5f;
                yield return StartCoroutine(FadeMask(0f, 1f, halfDuration));

                onMidpoint?.Invoke();

                yield return StartCoroutine(FadeMask(1f, 0f, halfDuration));

                transitionMask.raycastTarget = false;
                transitionMask.gameObject.SetActive(false);
            }
            else
            {
                onMidpoint?.Invoke();
            }

            _isTransitioning = false;
        }

        // ======================== 事件处理 ========================

        /// <summary>游戏状态变化 → 切换对应UI</summary>
        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    SetHUDVisible(false);
                    CloseAllPopups();
                    ShowMainMenu();
                    break;

                case GameState.InTurn:
                    SetHUDVisible(true);
                    break;

                case GameState.GameOver:
                case GameState.Victory:
                    // GameOver由HandleGameOver处理
                    break;
            }
        }

        /// <summary>
        /// 回合阶段变化 → 自动切换到对应面板
        /// 这是UI与TurnManager的核心连接点
        /// </summary>
        private void HandlePhaseChanged(TurnPhase phase)
        {
            Debug.Log($"[UIManager] 阶段切换至：{phase}");

            switch (phase)
            {
                case TurnPhase.MorningBriefing:
                    ShowMorningBriefing();
                    break;

                case TurnPhase.ActionAllocation:
                    ShowActionAllocation();
                    break;

                case TurnPhase.Execution:
                    // 执行阶段用卡牌决策界面展示（如有卡牌触发）
                    // 无卡牌时直接推进到Review
                    ShowCardDecision();
                    break;

                case TurnPhase.Review:
                    ShowReview();
                    break;
            }
        }

        /// <summary>官职变动 → 弹出考评面板</summary>
        private void HandleRankChanged(OfficialRank oldRank, OfficialRank newRank)
        {
            ShowEvaluation();
        }

        /// <summary>游戏结束处理</summary>
        private void HandleGameOver(GameOverReason reason)
        {
            CloseAllPopups();
            SetHUDVisible(false);
            // TODO: 显示GameOver专用面板（可复用ReviewPanel或新建）
            Debug.Log($"[UIManager] 游戏结束：{reason}");
        }

        // ======================== 工具方法 ========================

        private void RegisterPanel(UIPanel panel)
        {
            if (panel != null)
            {
                _allPanels.Add(panel);
            }
        }
    }
}
