// ============================================================================
// 官途浮沉 - 主菜单面板
// MainMenuPanel.cs — 游戏入口界面
// ============================================================================
// 视觉设计：
//   水墨山水画作为全屏背景，远山、流云、松柏
//   标题"官途浮沉"使用大号楷书/行书字体，墨色带微光
//   四个按钮纵向排列，宣纸底色 + 墨色边框，悬停时朱砂红描边
//   整体意境：一卷徐徐展开的山水长卷
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GuantuFucheng.Core;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;

namespace GuantuFucheng.UI
{
    /// <summary>
    /// 主菜单面板 — 游戏首屏
    /// 
    /// 功能按钮：
    /// - 新游戏：开始新的一局（确认覆盖存档）
    /// - 继续游戏：加载上次存档（无存档时置灰）
    /// - 设置：音量、画质、操作偏好
    /// - 退出：退出游戏
    /// </summary>
    public class MainMenuPanel : UIPanel
    {
        public override string PanelName => "主菜单";

        [Header("=== 标题 ===")]
        [Tooltip("游戏标题文字")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("副标题/版本号")]
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("=== 按钮 ===")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("=== 存档提示 ===")]
        [Tooltip("无存档时的提示文字")]
        [SerializeField] private TextMeshProUGUI noSaveHint;

        protected override void Awake()
        {
            base.Awake();

            // 绑定按钮事件
            newGameButton?.onClick.AddListener(OnNewGameClicked);
            continueButton?.onClick.AddListener(OnContinueClicked);
            settingsButton?.onClick.AddListener(OnSettingsClicked);
            quitButton?.onClick.AddListener(OnQuitClicked);
        }

        protected override void OnShow()
        {
            // 设置标题样式
            if (titleText != null)
            {
                titleText.text = "官途浮沉";
                titleText.color = UIConfig.InkBlack;
                titleText.fontSize = 64; // 主标题特殊大号
            }

            if (subtitleText != null)
            {
                subtitleText.text = "宦海沉浮，一步一生";
                subtitleText.color = UIConfig.InkLight;
                subtitleText.fontSize = UIConfig.FontSizeSubtitle;
            }

            // 检查存档状态
            RefreshContinueButton();
        }

        /// <summary>刷新"继续游戏"按钮状态</summary>
        private void RefreshContinueButton()
        {
            bool hasSave = GameManager.Instance.HasSaveData();

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;
            }

            if (noSaveHint != null)
            {
                noSaveHint.gameObject.SetActive(!hasSave);
                noSaveHint.text = "暂无存档";
                noSaveHint.color = UIConfig.InkLight;
            }
        }

        // ======================== 按钮回调 ========================

        private void OnNewGameClicked()
        {
            Debug.Log("[MainMenu] 新游戏");
            // TODO: 若已有存档，弹出确认覆盖对话框
            GameManager.Instance.StartNewGame();
        }

        private void OnContinueClicked()
        {
            Debug.Log("[MainMenu] 继续游戏");
            GameManager.Instance.LoadGame();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] 设置");
            // TODO: 打开设置面板（音量/画质/操作偏好）
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenu] 退出游戏");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
