// ============================================================================
// 官途浮沉 - 场景一键初始化
// SceneSetup.cs — 挂到空物体上，自动搭建完整运行环境
// ============================================================================
// 使用方式：
//   1. 创建空场景
//   2. 创建空GameObject，命名为 "[SceneSetup]"
//   3. 挂载此脚本
//   4. 点击Play — 自动创建Canvas/EventSystem/所有Manager/所有UI面板
//
// 也可通过菜单栏 "官途浮沉/一键搭建场景" 自动完成上述步骤。
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GuantuFucheng.UI;
using GuantuFucheng.Systems;

namespace GuantuFucheng.Core
{
    /// <summary>
    /// 场景一键初始化 — 自动创建所有运行时必需的GameObject和组件
    /// 确保从空场景出发也能完整运行游戏
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("场景配置")]
        [Tooltip("是否在Awake时自动搭建（false则需手动调用Setup）")]
        [SerializeField] private bool _autoSetup = true;

        [Tooltip("Canvas缩放参考分辨率")]
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920, 1080);

        [Header("运行时引用（自动填充，无需手动赋值）")]
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private GameObject _eventSystem;
        [SerializeField] private GameObject _managersRoot;
        [SerializeField] private GameObject _uiRoot;

        // ======================== 生命周期 ========================

        private void Awake()
        {
            if (_autoSetup)
            {
                Setup();
            }
        }

        // ======================== 公开方法 ========================

        /// <summary>
        /// 一键搭建完整场景
        /// 幂等 — 重复调用不会创建重复对象
        /// </summary>
        public void Setup()
        {
            Debug.Log("╔══════════════════════════════════════════╗");
            Debug.Log("║     《官途浮沉》场景初始化开始...         ║");
            Debug.Log("╚══════════════════════════════════════════╝");

            EnsureEventSystem();
            EnsureCanvas();
            EnsureManagers();
            EnsureUIPanels();
            WireUIManager();
            EnsureGameBootstrap();

            Debug.Log("╔══════════════════════════════════════════╗");
            Debug.Log("║     ✓ 场景初始化完成！可以运行游戏       ║");
            Debug.Log("╚══════════════════════════════════════════╝");
        }

        // ======================== EventSystem ========================

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                _eventSystem = FindObjectOfType<EventSystem>().gameObject;
                Debug.Log("[SceneSetup] EventSystem 已存在，跳过创建");
                return;
            }

            _eventSystem = new GameObject("[EventSystem]");
            _eventSystem.AddComponent<EventSystem>();
            _eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("[SceneSetup] ✓ 创建 EventSystem");
        }

        // ======================== Canvas ========================

        private void EnsureCanvas()
        {
            if (_mainCanvas == null)
                _mainCanvas = FindObjectOfType<Canvas>();

            if (_mainCanvas != null)
            {
                Debug.Log("[SceneSetup] Canvas 已存在，跳过创建");
                return;
            }

            var canvasObj = new GameObject("[MainCanvas]");
            _mainCanvas = canvasObj.AddComponent<Canvas>();
            _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _mainCanvas.sortingOrder = 0;

            // CanvasScaler — 按宽度适配
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // GraphicRaycaster — UI交互必需
            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("[SceneSetup] ✓ 创建 Canvas（ScreenSpaceOverlay, 1920×1080）");
        }

        // ======================== Manager单例 ========================

        private void EnsureManagers()
        {
            _managersRoot = GameObject.Find("[Managers]");
            if (_managersRoot == null)
            {
                _managersRoot = new GameObject("[Managers]");
                DontDestroyOnLoad(_managersRoot);
            }

            // 触发所有Singleton的懒加载创建
            // Singleton<T>基类会自动创建独立的GameObject并DontDestroyOnLoad
            Debug.Log("[SceneSetup] 初始化核心管理器...");

            var gm = GameManager.Instance;
            Debug.Log($"[SceneSetup]   ✓ GameManager");

            var tm = TurnManager.Instance;
            Debug.Log($"[SceneSetup]   ✓ TurnManager");

            var cs = CardSystem.Instance;
            Debug.Log($"[SceneSetup]   ✓ CardSystem");

            var npc = NPCRelationshipGraph.Instance;
            Debug.Log($"[SceneSetup]   ✓ NPCRelationshipGraph");

            var ap = ActionPointSystem.Instance;
            Debug.Log($"[SceneSetup]   ✓ ActionPointSystem");

            var ors = OfficialRankSystem.Instance;
            Debug.Log($"[SceneSetup]   ✓ OfficialRankSystem");

            var rms = RogueliteMetaSystem.Instance;
            Debug.Log($"[SceneSetup]   ✓ RogueliteMetaSystem");

            var ss = SaveSystem.Instance;
            Debug.Log($"[SceneSetup]   ✓ SaveSystem");

            Debug.Log("[SceneSetup] ✓ 所有Manager初始化完成（8个单例）");
        }

        // ======================== UI面板 ========================

        private void EnsureUIPanels()
        {
            if (_mainCanvas == null)
            {
                Debug.LogError("[SceneSetup] Canvas不存在，无法创建UI面板！");
                return;
            }

            Transform canvasTransform = _mainCanvas.transform;

            // UI层级容器
            _uiRoot = FindOrCreateChild(canvasTransform, "[UI_Panels]");

            Debug.Log("[SceneSetup] 创建UI面板...");

            // Layer 0: HUD（常驻）
            CreatePanel<HUDPanel>(_uiRoot.transform, "HUD_Panel", 0);

            // Layer 1: 主面板（同时只显示一个）
            CreatePanel<MainMenuPanel>(_uiRoot.transform, "MainMenu_Panel", 1);
            CreatePanel<MorningBriefingPanel>(_uiRoot.transform, "MorningBriefing_Panel", 2);
            CreatePanel<ActionAllocationPanel>(_uiRoot.transform, "ActionAllocation_Panel", 3);
            CreatePanel<CardDecisionPanel>(_uiRoot.transform, "CardDecision_Panel", 4);
            CreatePanel<ReviewPanel>(_uiRoot.transform, "Review_Panel", 5);

            // Layer 2: 弹窗
            CreatePanel<NPCRelationshipPanel>(_uiRoot.transform, "NPCRelationship_Panel", 6);
            CreatePanel<EvaluationPanel>(_uiRoot.transform, "Evaluation_Panel", 7);

            // Layer 3: 过渡遮罩
            CreateTransitionMask(canvasTransform);

            Debug.Log("[SceneSetup] ✓ 所有UI面板创建完成（8个面板 + 过渡遮罩）");
        }

        /// <summary>
        /// 创建单个UI面板 — 带RectTransform全屏拉伸 + CanvasGroup
        /// </summary>
        private T CreatePanel<T>(Transform parent, string name, int siblingIndex) where T : UIPanel
        {
            // 检查是否已存在
            var existing = parent.Find(name);
            if (existing != null)
            {
                var existingPanel = existing.GetComponent<T>();
                if (existingPanel != null)
                {
                    Debug.Log($"[SceneSetup]   {name} 已存在，跳过");
                    return existingPanel;
                }
            }

            var panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);

            // 全屏RectTransform
            var rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // CanvasGroup（UIPanel基类需要）
            var cg = panelObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // 宣纸底色背景Image
            var bg = panelObj.AddComponent<Image>();
            bg.color = UIConfig.PaperColor;
            bg.raycastTarget = true;

            // 挂载面板脚本
            var panel = panelObj.AddComponent<T>();

            panelObj.transform.SetSiblingIndex(siblingIndex);

            Debug.Log($"[SceneSetup]   ✓ {name} ({typeof(T).Name})");
            return panel;
        }

        /// <summary>
        /// 创建水墨晕染过渡遮罩
        /// </summary>
        private void CreateTransitionMask(Transform canvasTransform)
        {
            string name = "TransitionMask";
            var existing = canvasTransform.Find(name);
            if (existing != null) return;

            var maskObj = new GameObject(name);
            maskObj.transform.SetParent(canvasTransform, false);

            var rect = maskObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = maskObj.AddComponent<Image>();
            img.color = new Color(0.05f, 0.04f, 0.03f, 0f); // 初始透明
            img.raycastTarget = false;

            var cg = maskObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;

            // 确保在最顶层
            maskObj.transform.SetAsLastSibling();

            Debug.Log($"[SceneSetup]   ✓ 过渡遮罩（TransitionMask）");
        }

        // ======================== 连线UIManager ========================

        /// <summary>
        /// 通过反射将创建好的面板引用注入UIManager的SerializeField字段
        /// </summary>
        private void WireUIManager()
        {
            Debug.Log("[SceneSetup] 连线 UIManager...");

            // 确保UIManager存在
            var uiMgr = UIManager.Instance;
            if (uiMgr == null)
            {
                Debug.LogError("[SceneSetup] UIManager实例创建失败！");
                return;
            }

            var uiRoot = _uiRoot.transform;

            // 通过反射注入面板引用到UIManager的私有SerializeField
            InjectField(uiMgr, "mainMenuPanel", uiRoot.GetComponentInChildren<MainMenuPanel>(true));
            InjectField(uiMgr, "morningBriefingPanel", uiRoot.GetComponentInChildren<MorningBriefingPanel>(true));
            InjectField(uiMgr, "actionAllocationPanel", uiRoot.GetComponentInChildren<ActionAllocationPanel>(true));
            InjectField(uiMgr, "cardDecisionPanel", uiRoot.GetComponentInChildren<CardDecisionPanel>(true));
            InjectField(uiMgr, "reviewPanel", uiRoot.GetComponentInChildren<ReviewPanel>(true));
            InjectField(uiMgr, "npcRelationshipPanel", uiRoot.GetComponentInChildren<NPCRelationshipPanel>(true));
            InjectField(uiMgr, "evaluationPanel", uiRoot.GetComponentInChildren<EvaluationPanel>(true));
            InjectField(uiMgr, "hudPanel", uiRoot.GetComponentInChildren<HUDPanel>(true));

            // 注入过渡遮罩
            var mask = _mainCanvas.transform.Find("TransitionMask");
            if (mask != null)
            {
                InjectField(uiMgr, "transitionMask", mask.GetComponent<Image>());
            }

            Debug.Log("[SceneSetup] ✓ UIManager连线完成");
        }

        // ======================== GameBootstrap ========================

        private void EnsureGameBootstrap()
        {
            if (FindObjectOfType<GameBootstrap>() != null)
            {
                Debug.Log("[SceneSetup] GameBootstrap 已存在，跳过");
                return;
            }

            // 在自身GameObject上添加GameBootstrap
            var bootstrap = gameObject.AddComponent<GameBootstrap>();
            Debug.Log("[SceneSetup] ✓ 添加 GameBootstrap（将在Start中加载数据并启动游戏）");
        }

        // ======================== 工具方法 ========================

        private GameObject FindOrCreateChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var child = new GameObject(name);
            child.transform.SetParent(parent, false);

            var rect = child.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return child;
        }

        private void InjectField<T>(object target, string fieldName, T value)
        {
            if (value == null)
            {
                Debug.LogWarning($"[SceneSetup] 注入失败：{fieldName} 值为null");
                return;
            }

            var type = target.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(target, value);
                Debug.Log($"[SceneSetup]   → {fieldName} ✓");
            }
            else
            {
                Debug.LogWarning($"[SceneSetup]   找不到字段 {type.Name}.{fieldName}");
            }
        }
    }
}
