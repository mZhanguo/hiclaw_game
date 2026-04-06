// ============================================================================
// 官途浮沉 - UI配置与水墨风样式定义
// UIConfig.cs — 全局UI配色、字体、动画参数
// ============================================================================
// 设计理念：
//   唐代水墨风（Ink-Wash Style）—— 以宣纸底色为基调，墨色层次分明，
//   朱砂红作为强调色（官印、重要信息），藏青/靛蓝作为辅助色。
//   整体追求"素雅而不素淡，庄重而不沉闷"的视觉效果。
//
//   配色灵感来源：
//   - 宣纸：温暖的米白色，非纯白，带有微微泛黄的质感
//   - 浓墨/淡墨：从纯黑到灰色的渐变层次
//   - 朱砂：古代官印、批红所用的正红色
//   - 藏青/靛蓝：唐代官服常见颜色，沉稳高雅
//   - 赭石：温暖的黄褐色，用于次要装饰
// ============================================================================

using UnityEngine;

namespace GuantuFucheng.UI
{
    /// <summary>
    /// UI全局配置 — 水墨风视觉参数中心
    /// 所有UI面板应引用此类获取颜色/尺寸/动画参数，确保风格一致
    /// </summary>
    public static class UIConfig
    {
        // ======================== 水墨风配色方案 ========================

        /// <summary>宣纸底色 — 主背景色（温暖米白，非纯白）</summary>
        public static readonly Color PaperColor = new Color(0.96f, 0.93f, 0.87f, 1f); // #F5EDD9

        /// <summary>做旧宣纸 — 用于次要面板背景</summary>
        public static readonly Color AgedPaperColor = new Color(0.91f, 0.87f, 0.78f, 1f); // #E8DDC7

        /// <summary>浓墨 — 主要文字颜色</summary>
        public static readonly Color InkBlack = new Color(0.12f, 0.10f, 0.08f, 1f); // #1F1A14

        /// <summary>中墨 — 次要文字/边框</summary>
        public static readonly Color InkMedium = new Color(0.30f, 0.27f, 0.24f, 1f); // #4D453D

        /// <summary>淡墨 — 提示文字/禁用状态</summary>
        public static readonly Color InkLight = new Color(0.55f, 0.51f, 0.47f, 1f); // #8C8278

        /// <summary>朱砂红 — 强调色（重要信息、官印、警告）</summary>
        public static readonly Color CinnabarRed = new Color(0.80f, 0.15f, 0.10f, 1f); // #CC2619

        /// <summary>暗朱砂 — 按钮按下/悬停态</summary>
        public static readonly Color CinnabarDark = new Color(0.60f, 0.10f, 0.08f, 1f); // #991A14

        /// <summary>藏青 — 辅助强调色（NPC关系、行动分配）</summary>
        public static readonly Color IndigoBlue = new Color(0.16f, 0.25f, 0.40f, 1f); // #294066

        /// <summary>靛蓝浅色 — 选中/激活状态</summary>
        public static readonly Color IndigoLight = new Color(0.28f, 0.42f, 0.60f, 1f); // #476B99

        /// <summary>赭石黄 — 金钱/奖励/正面反馈</summary>
        public static readonly Color OchreGold = new Color(0.76f, 0.60f, 0.28f, 1f); // #C29947

        /// <summary>松石绿 — 恢复/治愈/正面属性变化</summary>
        public static readonly Color JadeGreen = new Color(0.20f, 0.55f, 0.40f, 1f); // #338C66

        /// <summary>半透明墨色遮罩 — 弹窗背景</summary>
        public static readonly Color OverlayDim = new Color(0.05f, 0.04f, 0.03f, 0.65f);

        // ======================== 属性变化色彩 ========================

        /// <summary>属性增长（正面变化）— 松石绿</summary>
        public static readonly Color PositiveChange = JadeGreen;

        /// <summary>属性下降（负面变化）— 朱砂红</summary>
        public static readonly Color NegativeChange = CinnabarRed;

        /// <summary>无变化 — 淡墨灰</summary>
        public static readonly Color NeutralChange = InkLight;

        // ======================== NPC关系色阶 ========================

        /// <summary>死敌/政敌 — 深朱砂</summary>
        public static readonly Color RelationHostile = CinnabarDark;

        /// <summary>泛泛之交 — 中墨灰</summary>
        public static readonly Color RelationNeutral = InkMedium;

        /// <summary>至交好友 — 藏青蓝</summary>
        public static readonly Color RelationFriendly = IndigoBlue;

        /// <summary>莫逆之交 — 赭石金</summary>
        public static readonly Color RelationAllied = OchreGold;

        // ======================== 字体大小（基于1080p基准） ========================

        /// <summary>大标题 — 面板标题（如"早朝简报"）</summary>
        public const int FontSizeTitle = 36;

        /// <summary>中标题 — 区块标题（如"属性变化"）</summary>
        public const int FontSizeSubtitle = 28;

        /// <summary>正文 — 主要内容文字</summary>
        public const int FontSizeBody = 22;

        /// <summary>注释/提示 — 次要信息</summary>
        public const int FontSizeCaption = 18;

        /// <summary>HUD数值 — 常驻界面的数字</summary>
        public const int FontSizeHUD = 20;

        // ======================== 动画时长（秒） ========================

        /// <summary>面板滑入/滑出时长 — 水墨晕染般的缓入缓出</summary>
        public const float PanelTransitionDuration = 0.45f;

        /// <summary>弹窗淡入淡出</summary>
        public const float PopupFadeDuration = 0.3f;

        /// <summary>属性数值跳动动画</summary>
        public const float NumberRollDuration = 0.6f;

        /// <summary>卡牌翻转动画</summary>
        public const float CardFlipDuration = 0.5f;

        /// <summary>HUD属性条变化过渡</summary>
        public const float HUDBarTransitionDuration = 0.4f;

        /// <summary>场景过渡（水墨晕染遮罩）</summary>
        public const float SceneTransitionDuration = 0.8f;

        // ======================== 布局常量 ========================

        /// <summary>面板边距（内边距）</summary>
        public const float PanelPadding = 24f;

        /// <summary>元素间距</summary>
        public const float ElementSpacing = 12f;

        /// <summary>卡牌宽度</summary>
        public const float CardWidth = 240f;

        /// <summary>卡牌高度</summary>
        public const float CardHeight = 360f;

        // ======================== 工具方法 ========================

        /// <summary>
        /// 根据好感度值获取对应颜色
        /// 用于NPC关系图谱中的颜色编码
        /// </summary>
        public static Color GetFavorColor(int favor)
        {
            if (favor >= 80) return RelationAllied;
            if (favor >= 20) return RelationFriendly;
            if (favor >= -30) return RelationNeutral;
            return RelationHostile;
        }

        /// <summary>
        /// 根据数值变化方向获取颜色
        /// 正值→绿，负值→红，零→灰
        /// </summary>
        public static Color GetDeltaColor(int delta)
        {
            if (delta > 0) return PositiveChange;
            if (delta < 0) return NegativeChange;
            return NeutralChange;
        }

        /// <summary>
        /// 格式化属性变化文本（带正负号和颜色标签）
        /// 用于Rich Text显示：<color=#338C66>+5</color>
        /// </summary>
        public static string FormatDelta(int delta)
        {
            if (delta == 0) return "<color=#8C8278>±0</color>";
            string hex = delta > 0 ? "338C66" : "CC2619";
            string sign = delta > 0 ? "+" : "";
            return $"<color=#{hex}>{sign}{delta}</color>";
        }
    }
}
