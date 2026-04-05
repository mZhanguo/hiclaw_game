// ============================================================================
// 官途浮沉 - 吏部考评面板
// EvaluationPanel.cs — 展示吏部考评结果（升迁/平调/贬官）
// ============================================================================
// 视觉设计：
//   模拟圣旨/敕令样式：
//   - 顶部：黄绢底色 + "敕" 字大印（朱砂色）
//   - 中央：评语正文（楷书竖排）
//   - 底部：考评等级（大写印章风格：甲/乙/丙/丁）
//   - 升迁时：金色粒子+官印盖章音效
//   - 贬官时：画面微微震动+墨色加深
//   作为弹窗覆盖在当前面板上方
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
    /// 吏部考评面板 — 展示官职变动结果
    /// 
    /// 触发时机：
    /// - 每N回合的定期考评
    /// - 达成特殊条件时的破格提拔
    /// - 触发负面事件时的贬官处分
    /// </summary>
    public class EvaluationPanel : UIPanel
    {
        public override string PanelName => "吏部考评";

        [Header("=== 圣旨样式 ===")]
        [Tooltip("顶部装饰（黄绢 + 敕字印）")]
        [SerializeField] private Image headerDecoration;

        [Tooltip("考评标题")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("=== 考评内容 ===")]
        [Tooltip("评语正文")]
        [SerializeField] private TextMeshProUGUI evaluationText;

        [Tooltip("旧官职")]
        [SerializeField] private TextMeshProUGUI oldRankText;

        [Tooltip("箭头/过渡标志")]
        [SerializeField] private TextMeshProUGUI arrowText;

        [Tooltip("新官职")]
        [SerializeField] private TextMeshProUGUI newRankText;

        [Header("=== 考评等级 ===")]
        [Tooltip("等级印章（甲/乙/丙/丁）")]
        [SerializeField] private TextMeshProUGUI gradeStampText;
        [SerializeField] private Image gradeStampBg;

        [Header("=== 操作 ===")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;

        // ======================== 数据 ========================

        /// <summary>升迁/贬官的旧官职</summary>
        private OfficialRank _oldRank;
        /// <summary>新官职</summary>
        private OfficialRank _newRank;

        protected override void Awake()
        {
            base.Awake();
            confirmButton?.onClick.AddListener(OnConfirmClicked);
        }

        private void OnEnable()
        {
            GameEvents.OnRankChanged += OnRankChangedReceived;
        }

        private void OnDisable()
        {
            GameEvents.OnRankChanged -= OnRankChangedReceived;
        }

        /// <summary>接收官职变动事件</summary>
        private void OnRankChangedReceived(OfficialRank oldRank, OfficialRank newRank)
        {
            _oldRank = oldRank;
            _newRank = newRank;
        }

        protected override void OnShow()
        {
            bool isPromotion = _newRank > _oldRank;
            bool isDemotion = _newRank < _oldRank;

            // 标题
            if (titleText != null)
            {
                titleText.text = isPromotion ? "敕 · 擢升" : isDemotion ? "敕 · 贬谪" : "敕 · 平调";
                titleText.color = UIConfig.CinnabarRed; // 圣旨用朱砂色
                titleText.fontSize = UIConfig.FontSizeTitle + 8;
            }

            // 评语
            if (evaluationText != null)
            {
                evaluationText.text = GenerateEvaluation(isPromotion, isDemotion);
                evaluationText.color = UIConfig.InkBlack;
                evaluationText.fontSize = UIConfig.FontSizeBody;
            }

            // 官职变动
            string oldName = GetRankDisplayName(_oldRank);
            string newName = GetRankDisplayName(_newRank);

            if (oldRankText != null)
            {
                oldRankText.text = oldName;
                oldRankText.color = UIConfig.InkMedium;
                oldRankText.fontSize = UIConfig.FontSizeSubtitle;
            }
            if (arrowText != null)
            {
                arrowText.text = isPromotion ? "▲" : isDemotion ? "▼" : "→";
                arrowText.color = isPromotion ? UIConfig.OchreGold
                    : isDemotion ? UIConfig.CinnabarRed : UIConfig.InkMedium;
                arrowText.fontSize = UIConfig.FontSizeTitle;
            }
            if (newRankText != null)
            {
                newRankText.text = newName;
                newRankText.color = isPromotion ? UIConfig.OchreGold : UIConfig.InkBlack;
                newRankText.fontSize = UIConfig.FontSizeSubtitle + 4;
            }

            // 考评等级印章
            if (gradeStampText != null)
            {
                string grade = isPromotion ? "甲" : isDemotion ? "丁" : "乙";
                gradeStampText.text = grade;
                gradeStampText.color = UIConfig.CinnabarRed;
                gradeStampText.fontSize = 72;
            }
            if (gradeStampBg != null)
            {
                gradeStampBg.color = new Color(UIConfig.CinnabarRed.r, UIConfig.CinnabarRed.g,
                    UIConfig.CinnabarRed.b, 0.15f); // 半透明朱砂底
            }

            // 按钮
            if (confirmButtonText != null)
            {
                confirmButtonText.text = "领旨谢恩";
            }
        }

        // ======================== 评语生成 ========================

        /// <summary>生成考评评语（模拟吏部公文风格）</summary>
        private string GenerateEvaluation(bool isPromotion, bool isDemotion)
        {
            if (isPromotion)
            {
                return "吏部奏称：该员任职以来，勤勉奉公，政绩卓著，" +
                       "上合天心，下顺民意。今特擢升一级，望再接再厉，" +
                       "不负朝廷恩遇。";
            }
            else if (isDemotion)
            {
                return "吏部奏称：该员任职期间，或有懈怠疏忽之处，" +
                       "致使政务有失。今降一级以示惩戒，望其深自反省，" +
                       "痛改前非，以图后效。";
            }
            else
            {
                return "吏部奏称：该员任职中规中矩，无大功亦无大过。" +
                       "今维持原职，望其勉力精进，争取来年佳评。";
            }
        }

        // ======================== 按钮回调 ========================

        private void OnConfirmClicked()
        {
            Debug.Log("[Evaluation] 玩家确认考评结果");
            UIManager.Instance.CloseTopPopup();
        }

        // ======================== 工具方法 ========================

        private string GetRankDisplayName(OfficialRank rank)
        {
            return rank switch
            {
                OfficialRank.Candidate => "候补",
                OfficialRank.CountyMagistrate => "县令（正七品）",
                OfficialRank.Prefect => "州刺史（从四品）",
                OfficialRank.ViceMinister => "侍郎（正四品）",
                OfficialRank.Minister => "尚书（正三品）",
                OfficialRank.MilitaryGovernor => "节度使（从二品）",
                OfficialRank.GrandCouncilor => "宰相（正二品）",
                _ => "未知"
            };
        }
    }
}
