// ============================================================
// UI/BoxRenderer.cs — Unicode 框线绘制器
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace TangMo.UI
{
    /// <summary>
    /// Unicode Box-drawing 字符绘制工具。
    /// 支持单线/双线框、标题行、自动宽度计算。
    /// 使用 Unicode box-drawing characters (U+2500 series)。
    /// </summary>
    public static class BoxRenderer
    {
        // ── 单线框字符 ──
        private const string TL = "┌";  // top-left
        private const string TR = "┐";  // top-right
        private const string BL = "└";  // bottom-left
        private const string BR = "┘";  // bottom-right
        private const string H  = "─";  // horizontal
        private const string V  = "│";  // vertical
        private const string TJ = "┬";  // top junction
        private const string BJ = "┴";  // bottom junction
        private const string LJ = "├";  // left junction
        private const string RJ = "┤";  // right junction
        private const string CJ = "┼";  // center junction

        // ── 双线框字符 ──
        private const string DTL = "╔";
        private const string DTR = "╗";
        private const string DBL = "╚";
        private const string DBR = "╝";
        private const string DH  = "═";
        private const string DV  = "║";

        // ── 圆角框 ──
        private const string RTL = "╭";
        private const string RTR = "╮";
        private const string RBL = "╰";
        private const string RBR = "╯";

        /// <summary>框线样式</summary>
        public enum BoxStyle { Single, Double, Rounded }

        /// <summary>
        /// 绘制一个带标题的框。
        /// </summary>
        /// <param name="title">标题（居中显示在顶边）</param>
        /// <param name="lines">内容行（ANSI 着色文本）</param>
        /// <param name="style">框线样式</param>
        /// <param name="padding">左右内边距（字符数）</param>
        public static string Box(string title, IEnumerable<string> lines,
                                  BoxStyle style = BoxStyle.Single, int padding = 1)
        {
            var lineList = lines.ToList();
            int maxContentWidth = lineList.Any()
                ? lineList.Max(l => AnsiHelper.DisplayWidth(l))
                : 20;
            int innerWidth = maxContentWidth + padding * 2;

            // 标题需要的宽度
            int titleWidth = string.IsNullOrEmpty(title) ? 0 : AnsiHelper.DisplayWidth(title) + 4;
            innerWidth = Math.Max(innerWidth, titleWidth);

            // 选择框字符
            string tl, tr, bl, br, h, v;
            GetChars(style, out tl, out tr, out bl, out br, out h, out v);

            var sb = new System.Text.StringBuilder();
            string pad = new string(' ', padding);

            // 顶部边框（带标题）
            if (!string.IsNullOrEmpty(title))
            {
                string titleText = $" {title} ";
                int titleLen = AnsiHelper.DisplayWidth(titleText);
                int leftDash = (innerWidth - titleLen) / 2;
                int rightDash = innerWidth - titleLen - leftDash;
                sb.AppendLine($"{tl}{h}{new string(h[0], leftDash)}{titleText}{new string(h[0], rightDash)}{h}{tr}");
            }
            else
            {
                sb.AppendLine($"{tl}{new string(h[0], innerWidth + 2)}{tr}");
            }

            // 内容行
            foreach (var line in lineList)
            {
                int lineLen = AnsiHelper.DisplayWidth(line);
                int rightPad = innerWidth - lineLen - padding;
                if (rightPad < 0) rightPad = 0;
                sb.AppendLine($"{v}{pad}{line}{new string(' ', rightPad)}{pad}{v}");
            }

            // 底部边框
            sb.Append($"{bl}{new string(h[0], innerWidth + 2)}{br}");

            return sb.ToString();
        }

        /// <summary>
        /// 绘制带分隔线的多段框（如面板各部分之间有 ──── 分隔）。
        /// </summary>
        public static string BoxWithSections(string title, List<List<string>> sections,
                                              BoxStyle style = BoxStyle.Single, int padding = 1)
        {
            var allLines = new List<string>();
            for (int i = 0; i < sections.Count; i++)
            {
                if (i > 0)
                {
                    // 分隔线（需要特殊处理，这里用空行代替，外部可自定义）
                    allLines.Add("");
                }
                allLines.AddRange(sections[i]);
            }
            return Box(title, allLines, style, padding);
        }

        /// <summary>
        /// 绘制简单的水平分隔线。
        /// </summary>
        public static string Separator(int width, BoxStyle style = BoxStyle.Single)
        {
            string h = style == BoxStyle.Double ? DH : H;
            return $"{LJ}{new string(h[0], width)}{RJ}";
        }

        /// <summary>
        /// 创建两列数据面板（标签: 值）。
        /// </summary>
        public static string KeyValueBox(string title, Dictionary<string, string> data,
                                          BoxStyle style = BoxStyle.Single, int labelWidth = 0)
        {
            if (labelWidth == 0)
                labelWidth = data.Keys.Max(k => k.Length);

            var lines = new List<string>();
            foreach (var kvp in data)
            {
                string label = kvp.Key.PadRight(labelWidth);
                lines.Add($"{AnsiHelper.Dim(label)} {kvp.Value}");
            }
            return Box(title, lines, style);
        }

        /// <summary>
        /// 绘制表格框（多列对齐）。
        /// </summary>
        public static string TableBox(string title, List<string> headers, List<List<string>> rows,
                                       BoxStyle style = BoxStyle.Single, int padding = 1)
        {
            // 计算每列最大宽度
            int colCount = headers.Count;
            var colWidths = new int[colCount];
            for (int c = 0; c < colCount; c++)
            {
                colWidths[c] = AnsiHelper.DisplayWidth(headers[c]);
                foreach (var row in rows)
                {
                    if (c < row.Count)
                        colWidths[c] = Math.Max(colWidths[c], AnsiHelper.DisplayWidth(row[c]));
                }
            }

            // 选择框字符
            GetChars(style, out string tl, out string tr, out string bl, out string br, out string h, out string v);

            var sb = new System.Text.StringBuilder();
            int totalWidth = colWidths.Sum() + (colCount - 1) * 3 + padding * 2;

            // 顶部
            sb.AppendLine(BuildTopBorder(tl, tr, h, TJ, colWidths, padding));

            // 表头
            sb.Append(v);
            sb.Append(new string(' ', padding));
            for (int c = 0; c < colCount; c++)
            {
                string header = AnsiHelper.Bold(headers[c]);
                int headerLen = AnsiHelper.DisplayWidth(headers[c]);
                sb.Append(header);
                sb.Append(new string(' ', colWidths[c] - headerLen));
                if (c < colCount - 1) sb.Append($" {v} ");
            }
            sb.Append(new string(' ', padding));
            sb.AppendLine(v);

            // 表头分隔
            sb.AppendLine(BuildMidBorder(LJ, RJ, h, CJ, colWidths, padding));

            // 数据行
            foreach (var row in rows)
            {
                sb.Append(v);
                sb.Append(new string(' ', padding));
                for (int c = 0; c < colCount; c++)
                {
                    string cell = c < row.Count ? row[c] : "";
                    int cellLen = AnsiHelper.DisplayWidth(cell);
                    sb.Append(cell);
                    sb.Append(new string(' ', colWidths[c] - cellLen));
                    if (c < colCount - 1) sb.Append($" {v} ");
                }
                sb.Append(new string(' ', padding));
                sb.AppendLine(v);
            }

            // 底部
            sb.Append(BuildBottomBorder(bl, br, h, BJ, colWidths, padding));

            return sb.ToString();
        }

        // ── 内部辅助方法 ──

        private static void GetChars(BoxStyle style,
            out string tl, out string tr, out string bl, out string br, out string h, out string v)
        {
            switch (style)
            {
                case BoxStyle.Double:
                    tl = DTL; tr = DTR; bl = DBL; br = DBR; h = DH; v = DV;
                    break;
                case BoxStyle.Rounded:
                    tl = RTL; tr = RTR; bl = RBL; br = RBR; h = H; v = V;
                    break;
                default:
                    tl = TL; tr = TR; bl = BL; br = BR; h = H; v = V;
                    break;
            }
        }

        private static string BuildTopBorder(string tl, string tr, string h, string tj,
                                              int[] colWidths, int padding)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(tl);
            sb.Append(new string(h[0], padding));
            for (int c = 0; c < colWidths.Length; c++)
            {
                sb.Append(new string(h[0], colWidths[c]));
                if (c < colWidths.Length - 1)
                    sb.Append(h + tj + h);
            }
            sb.Append(new string(h[0], padding));
            sb.Append(tr);
            return sb.ToString();
        }

        private static string BuildMidBorder(string lj, string rj, string h, string cj,
                                              int[] colWidths, int padding)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(lj);
            sb.Append(new string(h[0], padding));
            for (int c = 0; c < colWidths.Length; c++)
            {
                sb.Append(new string(h[0], colWidths[c]));
                if (c < colWidths.Length - 1)
                    sb.Append(h + cj + h);
            }
            sb.Append(new string(h[0], padding));
            sb.Append(rj);
            return sb.ToString();
        }

        private static string BuildBottomBorder(string bl, string br, string h, string bj,
                                                 int[] colWidths, int padding)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(bl);
            sb.Append(new string(h[0], padding));
            for (int c = 0; c < colWidths.Length; c++)
            {
                sb.Append(new string(h[0], colWidths[c]));
                if (c < colWidths.Length - 1)
                    sb.Append(h + bj + h);
            }
            sb.Append(new string(h[0], padding));
            sb.Append(br);
            return sb.ToString();
        }
    }
}
