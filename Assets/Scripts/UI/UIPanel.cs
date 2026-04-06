// ============================================================================
// 官途浮沉 - UI面板基类
// UIPanel.cs — 所有UI面板的抽象基类，定义生命周期与动画接口
// ============================================================================

using System.Collections;
using UnityEngine;

namespace GuantuFucheng.UI
{
    /// <summary>
    /// UI面板抽象基类
    /// 
    /// 所有面板继承此类，获得统一的显示/隐藏行为和动画支持。
    /// 子类通过重写OnShow/OnHide来实现各自的数据刷新逻辑。
    /// 
    /// 动画策略：
    /// - 默认使用CanvasGroup的Alpha淡入淡出
    /// - 子类可重写PlayShowAnimation/PlayHideAnimation实现自定义动画
    ///   （如面板从右侧滑入、卡牌翻转等）
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        /// <summary>面板标识名（调试用）</summary>
        public virtual string PanelName => GetType().Name;

        /// <summary>是否当前可见</summary>
        public bool IsVisible { get; private set; }

        /// <summary>缓存的CanvasGroup引用</summary>
        protected CanvasGroup CanvasGroup { get; private set; }

        /// <summary>当前动画协程（防止重复播放）</summary>
        private Coroutine _animationCoroutine;

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            // 初始隐藏
            SetVisibleImmediate(false);
        }

        // ======================== 显示/隐藏 ========================

        /// <summary>
        /// 显示面板（带动画）
        /// 子类重写OnShow()来填充数据
        /// </summary>
        public void Show()
        {
            if (IsVisible) return;

            gameObject.SetActive(true);
            IsVisible = true;

            // 先让子类刷新数据
            OnShow();

            // 播放显示动画
            StopCurrentAnimation();
            _animationCoroutine = StartCoroutine(PlayShowAnimation());

            Debug.Log($"[UI] {PanelName} 显示");
        }

        /// <summary>
        /// 隐藏面板（带动画）
        /// 子类重写OnHide()来清理状态
        /// </summary>
        public void Hide()
        {
            if (!IsVisible) return;

            IsVisible = false;
            OnHide();

            StopCurrentAnimation();
            _animationCoroutine = StartCoroutine(PlayHideAnimation());

            Debug.Log($"[UI] {PanelName} 隐藏");
        }

        /// <summary>立即设置可见状态（无动画，用于初始化）</summary>
        protected void SetVisibleImmediate(bool visible)
        {
            IsVisible = visible;
            gameObject.SetActive(visible);
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = visible ? 1f : 0f;
                CanvasGroup.interactable = visible;
                CanvasGroup.blocksRaycasts = visible;
            }
        }

        // ======================== 子类重写 ========================

        /// <summary>面板显示时调用 — 子类在此刷新数据</summary>
        protected virtual void OnShow() { }

        /// <summary>面板隐藏时调用 — 子类在此清理状态</summary>
        protected virtual void OnHide() { }

        // ======================== 动画 ========================

        /// <summary>
        /// 显示动画 — 默认Alpha淡入
        /// 子类可重写实现水墨卷轴展开、纸张飘入等效果
        /// </summary>
        protected virtual IEnumerator PlayShowAnimation()
        {
            float duration = UIConfig.PanelTransitionDuration;
            float elapsed = 0f;

            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = true;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                CanvasGroup.alpha = t;
                yield return null;
            }

            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
        }

        /// <summary>
        /// 隐藏动画 — 默认Alpha淡出
        /// </summary>
        protected virtual IEnumerator PlayHideAnimation()
        {
            float duration = UIConfig.PanelTransitionDuration;
            float elapsed = 0f;

            CanvasGroup.interactable = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                CanvasGroup.alpha = 1f - t;
                yield return null;
            }

            CanvasGroup.alpha = 0f;
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private void StopCurrentAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
        }
    }
}
