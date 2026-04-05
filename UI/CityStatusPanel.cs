// ============================================================
// UI/CityStatusPanel.cs — 城市状态面板（红黄绿健康度四维）
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace TangMo.UI
{
    /// <summary>
    /// 城市状态面板 — GDD §3.1 第一层信息展示。
    /// 使用终端 Unicode 框线绘制城市概览，包含：
    /// - 红黄绿健康度四维（经济/军事/民心/家族）
    /// - 资源概览（粮/钱/兵力）
    /// - 关键风险提示
    /// </summary>
    public static class CityStatusPanel
    {
        /// <summary>
        /// 渲染城市状态面板（第一层：红黄绿健康度）。
        /// </summary>
        public static string Render(City city)
        {
            var sections = new List<List<string>>();

            // ── 区段 1: 健康度总览 ──
            sections.Add(BuildHealthOverview(city));

            // ── 区段 2: 关键资源 ──
            sections.Add(BuildResourceSummary(city));

            // ── 区段 3: 关键风险提示（如有）──
            var alerts = BuildAlerts(city);
            if (alerts.Any())
                sections.Add(alerts);

            return BuildPanel(city.Name, city, sections);
        }

        /// <summary>
        /// 渲染第二层（展开详情）：显示关键数值。
        /// </summary>
        public static string RenderDetail(City city)
        {
            var sections = new List<List<string>>();

            // 健康度
            sections.Add(BuildHealthOverview(city));

            // 资源
            sections.Add(BuildResourceSummary(city));

            // 详细数值
            sections.Add(BuildDetailNumbers(city));

            // 家族概览
            sections.Add(BuildFamilyOverview(city));

            // 风险
            var alerts = BuildAlerts(city);
            if (alerts.Any())
                sections.Add(alerts);

            return BuildPanel(city.Name, city, sections);
        }

        // ── 内部渲染方法 ──

        private static List<string> BuildHealthOverview(City city)
        {
            var lines = new List<string>();
            lines.Add(AnsiHelper.Bold("┌─ 健康度 ─────────────────────────┐"));

            // 四维健康度
            string eco = FormatHealth("💰 经济", city.EconomicHealth);
            string mil = FormatHealth("⚔️  军事", city.MilitaryHealth);
            string pop = FormatHealth("👥 民心", city.PopulaceHealth);
            string fam = FormatHealth("🏯 家族", city.FamilyHealth);

            lines.Add($"  {eco}  {mil}");
            lines.Add($"  {pop}  {fam}");

            // 综合健康度
            var overall = GetOverallHealth(city);
            string overallLabel = overall switch
            {
                HealthStatus.Green  => AnsiHelper.Green("安居乐业"),
                HealthStatus.Yellow => AnsiHelper.Yellow("暗流涌动"),
                HealthStatus.Red    => AnsiHelper.BoldRed("危机四伏"),
                _ => "未知"
            };
            lines.Add($"  ──── 综合: {overallLabel}");
            lines.Add(AnsiHelper.Dim("└───────────────────────────────────┘"));

            return lines;
        }

        private static List<string> BuildResourceSummary(City city)
        {
            var lines = new List<string>();

            // 粮食
            string grainBar = city.GranaryGrain > 500
                ? AnsiHelper.ProgressBarInverse(city.GranaryGrain / 1000.0, 12)
                : AnsiHelper.ProgressBar(city.GranaryGrain / 500.0, 12);

            // 钱
            string treasuryBar = city.Treasury > 200
                ? AnsiHelper.ProgressBarInverse(city.Treasury / 500.0, 12)
                : AnsiHelper.ProgressBar(city.Treasury / 200.0, 12);

            lines.Add(AnsiHelper.Bold("  💰 财政"));
            lines.Add($"    粮  {grainBar} {city.GranaryGrain:F0}");
            lines.Add($"    钱  {treasuryBar} {city.Treasury:F0}");

            // 军事
            string moraleBar = AnsiHelper.ProgressBarInverse(city.GarrisonMorale, 12);
            lines.Add(AnsiHelper.Bold("  ⚔️  军事"));
            lines.Add($"    兵力  {city.GarrisonStrength:F0}");
            lines.Add($"    士气  {moraleBar} {city.GarrisonMorale:P0}");

            if (city.ArmyPayArrearsWeeks > 0)
            {
                string payColor = city.ArmyPayArrearsWeeks <= 1 ? AnsiHelper.Yellow("⚠") :
                                  city.ArmyPayArrearsWeeks <= 2 ? AnsiHelper.Red("⚠") :
                                  AnsiHelper.BoldRed("🔥");
                lines.Add($"    欠饷  {payColor} {city.ArmyPayArrearsWeeks} 周");
            }

            return lines;
        }

        private static List<string> BuildDetailNumbers(City city)
        {
            var lines = new List<string>
            {
                AnsiHelper.Bold("  📊 详细数值"),
                $"    农业产出  {city.AgriculturalYield:F1}/周",
                $"    繁荣度    {AnsiHelper.ColorValueInverse(city.Prosperity, 0.6, 0.3)}",
                $"    市场活跃  {AnsiHelper.ColorValueInverse(city.MarketActivity, 0.5, 0.3)}",
                $"    民负      {AnsiHelper.ColorValue(city.PeopleBurden, 0.3, 0.6)}",
                $"    制度力    {AnsiHelper.ColorValueInverse(city.InstitutionPower, 0.5, 0.3)}",
                $"    Unrest    {AnsiHelper.ColorValue(city.Unrest, 0.25, 0.50)}",
                $"    税率      {city.TaxRate:P0}  盐税 {city.SaltTaxRate:P0}  军费优先 {city.MilitaryPriority:P0}"
            };
            return lines;
        }

        private static List<string> BuildFamilyOverview(City city)
        {
            var lines = new List<string>
            {
                AnsiHelper.Bold("  🏯 家族概览")
            };

            if (!city.Families.Any())
            {
                lines.Add("    (无家族)");
                return lines;
            }

            var sorted = city.Families.OrderByDescending(f => f.Power).ToList();
            foreach (var f in sorted)
            {
                string loyaltyDot = f.LoyaltyToLord >= 0.6 ? "🟢" :
                                    f.LoyaltyToLord >= 0.3 ? "🟡" : "🔴";
                string domStar = f.IsDominant ? " ⭐" : "";
                string name = f.Name.Length > 8 ? f.Name.Substring(0, 8) : f.Name;

                lines.Add($"    {loyaltyDot} {name,-8} 力{f.Power:F2} 主{f.Dominance:F2}{domStar}");
            }

            return lines;
        }

        private static List<string> BuildAlerts(City city)
        {
            var alerts = new List<string>();
            alerts.Add(AnsiHelper.Bold("  ⚡ 风险提示"));

            if (city.HasFlag(CityFlags.ARMY_PAY_ISSUE))
                alerts.Add($"    {AnsiHelper.Red("▸")} 欠饷积压 {city.ArmyPayArrearsWeeks} 周 — 兵变风险");

            if (city.HasFlag(CityFlags.SALT_TAX_ISSUE))
                alerts.Add($"    {AnsiHelper.Red("▸")} 盐税问题 — 商帮不满");

            if (city.HasFlag(CityFlags.OFFICE_PARALYZED_RISK))
                alerts.Add($"    {AnsiHelper.Red("▸")} 官署有瘫痪风险");

            if (city.HasFlag(CityFlags.PRIVATE_SALT_RAMPANT))
                alerts.Add($"    {AnsiHelper.Yellow("▸")} 私盐盛行 — 税收流失");

            if (city.HasFlag(CityFlags.PORT_BLOCKED))
                alerts.Add($"    {AnsiHelper.Red("▸")} 河港封锁 — 贸易中断");

            if (city.HasFlag(CityFlags.FAMINE_RISK))
                alerts.Add($"    {AnsiHelper.BoldRed("▸")} 饥荒风险！");

            if (city.HasFlag(CityFlags.REBELLION_ACTIVE))
                alerts.Add($"    {AnsiHelper.BoldRed("▸▸▸ 叛乱进行中 ◂◂◂")}");

            if (city.HasFlag(CityFlags.RECOVERY_MODE))
                alerts.Add($"    {AnsiHelper.Yellow("▸")} 恢复期 剩余 {city.RecoveryWeeksLeft} 周");

            // 检查主导家族
            var dominant = city.GetDominantFamily();
            if (dominant != null)
                alerts.Add($"    {AnsiHelper.Yellow("▸")} {dominant.Name} 为当前主导家族 (Dom={dominant.Dominance:F2})");

            // 只有一条"风险提示"标题且无实际风险时不显示
            if (alerts.Count == 1)
                return new List<string>();

            return alerts;
        }

        // ── 辅助方法 ──

        private static string FormatHealth(string label, HealthStatus status)
        {
            string dot = AnsiHelper.HealthDot(status);
            string statusText = status switch
            {
                HealthStatus.Green  => AnsiHelper.Green("正常"),
                HealthStatus.Yellow => AnsiHelper.Yellow("警惕"),
                HealthStatus.Red    => AnsiHelper.Red("危险"),
                _ => "???"
            };
            return $"{dot} {label} {statusText}";
        }

        private static HealthStatus GetOverallHealth(City city)
        {
            var statuses = new[] { city.EconomicHealth, city.MilitaryHealth,
                                   city.PopulaceHealth, city.FamilyHealth };
            if (statuses.Any(s => s == HealthStatus.Red)) return HealthStatus.Red;
            if (statuses.Any(s => s == HealthStatus.Yellow)) return HealthStatus.Yellow;
            return HealthStatus.Green;
        }

        private static string BuildPanel(string cityName, City city, List<List<string>> sections)
        {
            var allLines = new List<string>();

            // 城市类型标签
            string typeTag = BuildTypeTag(city);
            allLines.Add(AnsiHelper.Dim(typeTag));

            for (int i = 0; i < sections.Count; i++)
            {
                if (i > 0)
                    allLines.Add(AnsiHelper.Dim("  · · · · · · · · · · · · · · · · · · ·"));
                allLines.AddRange(sections[i]);
            }

            return BoxRenderer.Box(cityName, allLines, BoxRenderer.BoxStyle.Rounded);
        }

        private static string BuildTypeTag(City city)
        {
            var tags = new List<string>();
            if (city.Type.HasFlag(CityType.ProvincialCapital))    tags.Add("州府");
            if (city.Type.HasFlag(CityType.MilitaryGarrisonTown)) tags.Add("军镇");
            if (city.Type.HasFlag(CityType.TradeHubSalt))         tags.Add("盐铁");
            if (city.Type.HasFlag(CityType.TradeHubTeaHorse))     tags.Add("茶马");
            if (city.Type.HasFlag(CityType.RiverPortCity))        tags.Add("河港");
            if (city.Type.HasFlag(CityType.AgriculturalCountyTown)) tags.Add("县城");
            if (city.Type.HasFlag(CityType.ReligiousCenter))      tags.Add("宗教");
            return string.Join(" · ", tags);
        }
    }
}
