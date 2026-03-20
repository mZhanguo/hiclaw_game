// ============================================================
// UI/AnsiHelper.cs — ANSI 颜色和格式化工具
// ============================================================
using System;

namespace TangMo.UI
{
    /// <summary>
    /// ANSI escape code 工具类。
    /// 提供终端颜色、粗体、下划线等格式化能力。
    /// 兼容 Windows Terminal 和主流 Linux 终端。
    /// </summary>
    public static class AnsiHelper
    {
        // ── 颜色码 ──
        private const string RESET   = "\x1b[0m";
        private const string BOLD    = "\x1b[1m";
        private const string DIM     = "\x1b[2m";
        private const string UNDER   = "\x1b[4m";

        // 前景色
        private const string FG_RED     = "\x1b[31m";
        private const string FG_GREEN   = "\x1b[32m";
        private const string FG_YELLOW  = "\x1b[33m";
        private const string FG_BLUE    = "\x1b[34m";
        private const string FG_MAGENTA = "\x1b[35m";
        private const string FG_CYAN    = "\x1b[36m";
        private const string FG_WHITE   = "\x1b[37m";
        private const string FG_DEFAULT = "\x1b[39m";

        // 亮前景色
        private const string FG_BRIGHT_RED     = "\x1b[91m";
        private const string FG_BRIGHT_GREEN   = "\x1b[92m";
        private const string FG_BRIGHT_YELLOW  = "\x1b[93m";
        private const string FG_BRIGHT_CYAN    = "\x1b[96m";

        // 背景色
        private const string BG_RED    = "\x1b[41m";
        private const string BG_GREEN  = "\x1b[42m";
        private const string BG_YELLOW = "\x1b[43m";
        private const string BG_BLUE   = "\x1b[44m";
        private const string BG_CYAN   = "\x1b[46m";

        /// <summary>根据 HealthStatus 返回颜色字符串</summary>
        public static string HealthColor(HealthStatus status) => status switch
        {
            HealthStatus.Green  => FG_GREEN,
            HealthStatus.Yellow => FG_YELLOW,
            HealthStatus.Red    => FG_RED,
            _ => FG_DEFAULT
        };

        /// <summary>根据 HealthStatus 返回 emoji 圆点</summary>
        public static string HealthDot(HealthStatus status) => status switch
        {
            HealthStatus.Green  => "🟢",
            HealthStatus.Yellow => "🟡",
            HealthStatus.Red    => "🔴",
            _ => "⚪"
        };

        /// <summary>根据 HealthStatus 返回带颜色的标签文本</summary>
        public static string HealthLabel(string label, HealthStatus status)
        {
            return $"{HealthColor(status)}{label}{RESET}";
        }

        /// <summary>红黄绿着色数字（基于阈值自动判断）</summary>
        public static string ColorValue(double value, double greenMax, double yellowMax)
        {
            if (value <= greenMax) return $"{FG_GREEN}{value:F2}{RESET}";
            if (value <= yellowMax) return $"{FG_YELLOW}{value:F2}{RESET}";
            return $"{FG_RED}{value:F2}{RESET}";
        }

        /// <summary>反向红黄绿（越高越好时使用）</summary>
        public static string ColorValueInverse(double value, double greenMin, double yellowMin)
        {
            if (value >= greenMin) return $"{FG_GREEN}{value:F2}{RESET}";
            if (value >= yellowMin) return $"{FG_YELLOW}{value:F2}{RESET}";
            return $"{FG_RED}{value:F2}{RESET}";
        }

        // ── 格式化方法 ──
        public static string Bold(string text)     => $"{BOLD}{text}{RESET}";
        public static string Dim(string text)      => $"{DIM}{text}{RESET}";
        public static string Red(string text)      => $"{FG_RED}{text}{RESET}";
        public static string Green(string text)    => $"{FG_GREEN}{text}{RESET}";
        public static string Yellow(string text)   => $"{FG_YELLOW}{text}{RESET}";
        public static string Blue(string text)     => $"{FG_BLUE}{text}{RESET}";
        public static string Cyan(string text)     => $"{FG_CYAN}{text}{RESET}";
        public static string Magenta(string text)  => $"{FG_MAGENTA}{text}{RESET}";
        public static string White(string text)    => $"{FG_WHITE}{text}{RESET}";

        public static string BoldRed(string text)    => $"{BOLD}{FG_RED}{text}{RESET}";
        public static string BoldGreen(string text)  => $"{BOLD}{FG_GREEN}{text}{RESET}";
        public static string BoldYellow(string text) => $"{BOLD}{FG_YELLOW}{text}{RESET}";
        public static string BoldCyan(string text)   => $"{BOLD}{FG_CYAN}{text}{RESET}";

        /// <summary>百分比进度条（终端用）</summary>
        public static string ProgressBar(double ratio, int width = 20)
        {
            ratio = Math.Clamp(ratio, 0, 1);
            int filled = (int)(ratio * width);
            int empty = width - filled;

            string color = ratio < 0.3 ? FG_GREEN :
                           ratio < 0.7 ? FG_YELLOW : FG_RED;

            return $"{color}[{new string('█', filled)}{new string('░', empty)}]{RESET}";
        }

        /// <summary>正向进度条（越高越好时用绿色表示高值）</summary>
        public static string ProgressBarInverse(double ratio, int width = 20)
        {
            ratio = Math.Clamp(ratio, 0, 1);
            int filled = (int)(ratio * width);
            int empty = width - filled;

            string color = ratio > 0.7 ? FG_GREEN :
                           ratio > 0.3 ? FG_YELLOW : FG_RED;

            return $"{color}[{new string('█', filled)}{new string('░', empty)}]{RESET}";
        }

        /// <summary>居中文本（指定总宽度）</summary>
        public static string Center(string text, int width)
        {
            int textLen = StripAnsi(text).Length;
            if (textLen >= width) return text;
            int padLeft = (width - textLen) / 2;
            int padRight = width - textLen - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        /// <summary>去除 ANSI 转义序列，返回纯文本长度</summary>
        public static string StripAnsi(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // 简易去除 \x1b[...m 模式
            var result = new System.Text.StringBuilder();
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '\x1b' && i + 1 < text.Length && text[i + 1] == '[')
                {
                    // 跳到 'm'
                    while (i < text.Length && text[i] != 'm') i++;
                    i++; // skip 'm'
                }
                else
                {
                    result.Append(text[i]);
                    i++;
                }
            }
            return result.ToString();
        }

        /// <summary>计算 ANSI 文本的显示宽度</summary>
        public static int DisplayWidth(string text) => StripAnsi(text).Length;

        /// <summary>右对齐文本</summary>
        public static string RightAlign(string text, int width)
        {
            int textLen = DisplayWidth(text);
            if (textLen >= width) return text;
            return new string(' ', width - textLen) + text;
        }
    }
}
