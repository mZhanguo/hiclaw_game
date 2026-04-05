// ============================================================
// UI/FamilyRelationsView.cs — 家族关系视图
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace TangMo.UI
{
    /// <summary>
    /// 家族关系视图 — GDD §4 家族系统 UI。
    /// 展示各家族的：
    /// - 三份额（经济/武力/民意）
    /// - 忠诚度 / 怨气 / 恐惧 / 依赖
    /// - Power / Dominance / EffectiveDominance
    /// - 关系网（主导家族、对立家族、联盟可能性）
    /// </summary>
    public static class FamilyRelationsView
    {
        /// <summary>
        /// 渲染完整家族关系视图。
        /// </summary>
        public static string Render(City city)
        {
            var sb = new System.Text.StringBuilder();
            var families = city.Families.OrderByDescending(f => f.Power).ToList();

            if (!families.Any())
            {
                sb.AppendLine(BoxRenderer.Box("家族关系", new[] { "  (此城无家族)" },
                    BoxRenderer.BoxStyle.Rounded));
                return sb.ToString();
            }

            // ── 总览面板 ──
            sb.AppendLine(RenderOverview(city, families));
            sb.AppendLine();

            // ── 份额对比表 ──
            sb.AppendLine(RenderShareComparison(families));
            sb.AppendLine();

            // ── 详细卡片 ──
            sb.AppendLine(RenderFamilyCards(families, city.InstitutionPower));
            sb.AppendLine();

            // ── 权力格局分析 ──
            sb.AppendLine(RenderPowerAnalysis(city, families));

            return sb.ToString();
        }

        // ── 总览 ──

        private static string RenderOverview(City city, List<Family> families)
        {
            var sb = new System.Text.StringBuilder();
            int familyCount = families.Count;
            var dominant = families.FirstOrDefault(f => f.IsDominant);
            double totalPower = families.Sum(f => f.Power);
            double avgLoyalty = families.Average(f => f.LoyaltyToLord);
            double maxGrievance = families.Max(f => f.Grievance);

            sb.AppendLine($"  ╭─ 🏯 家族关系 — {AnsiHelper.Bold(city.Name)} ────────────────╮");
            sb.AppendLine($"  │                                              │");
            sb.AppendLine($"  │  家族数量: {familyCount}  " +
                          $"制度力: {city.InstitutionPower:F2}  " +
                          $"总 Power: {totalPower:F2}    │");

            string domText = dominant != null
                ? $"{dominant.Name} (Dom={dominant.Dominance:F2})"
                : AnsiHelper.Dim("无主导家族");
            sb.AppendLine($"  │  主导家族: {domText,-33} │");

            sb.AppendLine($"  │  平均忠诚: {AnsiHelper.ColorValueInverse(avgLoyalty, 0.6, 0.3),-12} " +
                          $"最大怨气: {AnsiHelper.ColorValue(maxGrievance, 0.2, 0.5),-12} │");
            sb.AppendLine($"  │                                              │");
            sb.AppendLine($"  ╰──────────────────────────────────────────────╯");

            return sb.ToString();
        }

        // ── 份额对比表 ──

        private static string RenderShareComparison(List<Family> families)
        {
            var headers = new List<string> { "家族", "经济", "武力", "民望", "Power", "Dom" };
            var rows = new List<List<string>>();

            foreach (var f in families)
            {
                string domStar = f.IsDominant ? " ⭐" : "";
                string econBar = MiniBar(f.EconShare);
                string militiaBar = MiniBar(f.Militia);
                string popBar = MiniBar(f.Popularity);

                rows.Add(new List<string>
                {
                    $"{f.Name}{domStar}",
                    $"{econBar} {f.EconShare:F2}",
                    $"{militiaBar} {f.Militia:F2}",
                    $"{popBar} {f.Popularity:F2}",
                    $"{f.Power:F2}",
                    $"{f.Dominance:F2}"
                });
            }

            return BoxRenderer.TableBox("三份额对比", headers, rows,
                BoxRenderer.BoxStyle.Single, 1);
        }

        // ── 详细卡片 ──

        private static string RenderFamilyCards(List<Family> families, double institutionPower)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(AnsiHelper.Bold("  📋 家族详情:"));
            sb.AppendLine($"  {AnsiHelper.Dim("──────────────────────────────────────────")}");

            foreach (var f in families)
            {
                sb.AppendLine(RenderSingleCard(f, institutionPower));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string RenderSingleCard(Family f, double institutionPower)
        {
            var sb = new System.Text.StringBuilder();

            // 家族名 + 类型
            string typeTag = FormatFamilyType(f.Type);
            string domTag = f.IsDominant ? AnsiHelper.BoldRed(" ⭐主导") : "";
            string nameLine = $"  {AnsiHelper.Bold(f.Name)} {AnsiHelper.Dim(typeTag)}{domTag}";
            sb.AppendLine(nameLine);

            // 三份额条形图
            sb.AppendLine($"    {AnsiHelper.Dim("经济")} {AnsiHelper.ProgressBarInverse(f.EconShare, 10)} {f.EconShare:F2}");
            sb.AppendLine($"    {AnsiHelper.Dim("武力")} {AnsiHelper.ProgressBarInverse(f.Militia, 10)} {f.Militia:F2}");
            sb.AppendLine($"    {AnsiHelper.Dim("民望")} {AnsiHelper.ProgressBarInverse(f.Popularity, 10)} {f.Popularity:F2}");

            // 四情绪
            sb.AppendLine($"    {AnsiHelper.Dim("忠诚")} {AnsiHelper.ProgressBarInverse(f.LoyaltyToLord, 10)} {f.LoyaltyToLord:F2}  " +
                          $"{AnsiHelper.Dim("怨气")} {AnsiHelper.ProgressBar(f.Grievance, 10)} {f.Grievance:F2}");
            sb.AppendLine($"    {AnsiHelper.Dim("恐惧")} {AnsiHelper.ProgressBar(f.Fear, 10)} {f.Fear:F2}  " +
                          $"{AnsiHelper.Dim("依赖")} {AnsiHelper.ProgressBarInverse(f.Dependence, 10)} {f.Dependence:F2}");

            // Power / Dominance / EffectiveDominance
            double ed = f.GetEffectiveDominance(institutionPower);
            sb.AppendLine($"    {AnsiHelper.Dim("Power")} {AnsiHelper.Bold($"{f.Power:F3}")}  " +
                          $"{AnsiHelper.Dim("Dom")} {f.Dominance:F3}  " +
                          $"{AnsiHelper.Dim("EDom")} {ed:F3}");

            // 关系提示
            var hints = new List<string>();
            if (f.LoyaltyToLord < 0.30) hints.Add(AnsiHelper.Red("⚠ 忠诚极低"));
            if (f.Grievance > 0.60) hints.Add(AnsiHelper.Red("⚠ 怨气高涨"));
            if (f.IsDominant) hints.Add(AnsiHelper.Yellow("⚠ 主导家族"));
            if (f.Fear > 0.70) hints.Add(AnsiHelper.Yellow("⚠ 恐惧驱动"));
            if (f.Dependence < 0.30) hints.Add(AnsiHelper.Yellow("⚠ 低依赖，随时可能脱离"));

            if (hints.Any())
                sb.AppendLine($"    {string.Join("  ", hints)}");

            return sb.ToString();
        }

        // ── 权力格局分析 ──

        private static string RenderPowerAnalysis(City city, List<Family> families)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(AnsiHelper.Bold("  🔍 权力格局:"));
            sb.AppendLine($"  {AnsiHelper.Dim("──────────────────────────────────────────")}");

            var dominant = families.FirstOrDefault(f => f.IsDominant);
            if (dominant != null)
            {
                if (dominant.Dominance >= 0.90)
                {
                    sb.AppendLine($"    {AnsiHelper.BoldRed("⚠ 单家族占有态!")} {dominant.Name} " +
                                  $"掌控全局 (Dom={dominant.Dominance:F2})");
                    sb.AppendLine($"    {AnsiHelper.Dim("其他家族势力微弱，叛乱风险来自被压制家族的绝望反击。")}");
                }
                else
                {
                    sb.AppendLine($"    {AnsiHelper.BoldYellow("⚡ 主导态势")} {dominant.Name} " +
                                  $"主导 (Dom={dominant.Dominance:F2})");
                    var others = families.Where(f => f != dominant).ToList();
                    var threat = others.OrderByDescending(f => f.Grievance).FirstOrDefault();
                    if (threat != null)
                        sb.AppendLine($"    {AnsiHelper.Dim($"潜在威胁: {threat.Name} (怨气={threat.Grievance:F2})")}");
                }
            }
            else
            {
                sb.AppendLine($"    {AnsiHelper.Green("✓ 多元均势")} — 无单一家族主导");

                // 检查是否有两强对峙
                var top2 = families.Take(2).ToList();
                if (top2.Count == 2 && top2[1].Dominance > 0.35)
                {
                    sb.AppendLine($"    {AnsiHelper.Dim($"双强格局: {top2[0].Name} vs {top2[1].Name}")}");
                }
            }

            // 联盟可能性分析
            var alliances = DetectPotentialAlliances(families);
            if (alliances.Any())
            {
                sb.AppendLine($"    {AnsiHelper.Dim("可能的联盟:")}");
                foreach (var a in alliances)
                {
                    sb.AppendLine($"      {AnsiHelper.Cyan("↔")} {a}");
                }
            }

            return sb.ToString();
        }

        // ── 辅助方法 ──

        private static string MiniBar(double value)
        {
            value = Math.Clamp(value, 0, 1);
            int filled = (int)(value * 5);
            int empty = 5 - filled;
            string color = value > 0.6 ? AnsiHelper.Green("") :
                           value > 0.3 ? AnsiHelper.Yellow("") : AnsiHelper.Red("");
            // 使用简化条形图
            return $"{color}{new string('█', filled)}{new string('░', empty)}\x1b[0m";
        }

        private static string FormatFamilyType(FamilyType type)
        {
            var tags = new List<string>();
            if (type.HasFlag(FamilyType.MilitaryClan))   tags.Add("牙将");
            if (type.HasFlag(FamilyType.MerchantClan))   tags.Add("盐商");
            if (type.HasFlag(FamilyType.GentryClan))     tags.Add("士绅");
            if (type.HasFlag(FamilyType.ReligiousClan))  tags.Add("寺院");
            if (type.HasFlag(FamilyType.LocalElderClan)) tags.Add("乡老");
            if (type.HasFlag(FamilyType.CourtTiedClan))  tags.Add("朝中");
            return tags.Any() ? $"[{string.Join("·", tags)}]" : "";
        }

        private static List<string> DetectPotentialAlliances(List<Family> families)
        {
            var alliances = new List<string>();

            // 简单启发式：低忠诚 + 高怨气家族之间可能结盟
            var discontented = families
                .Where(f => f.LoyaltyToLord < 0.40 && f.Grievance > 0.30)
                .ToList();

            for (int i = 0; i < discontented.Count; i++)
            {
                for (int j = i + 1; j < discontented.Count; j++)
                {
                    var a = discontented[i];
                    var b = discontented[j];
                    double combinedDom = a.Dominance + b.Dominance;
                    if (combinedDom > 0.50)
                    {
                        alliances.Add($"{a.Name} + {b.Name} (联合 Dom={combinedDom:F2})");
                    }
                }
            }

            return alliances;
        }
    }
}
