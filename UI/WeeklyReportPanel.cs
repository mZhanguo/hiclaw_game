// ============================================================
// UI/WeeklyReportPanel.cs — 周推进报告面板
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace TangMo.UI
{
    /// <summary>
    /// 周推进报告 — 每周结束后的总结面板。
    /// 展示收入/支出/家族变化/关键事件。
    /// 支持记录周内变化并生成对比报告。
    /// </summary>
    public static class WeeklyReportPanel
    {
        /// <summary>
        /// 周变化快照：记录一周开始和结束的状态用于对比。
        /// </summary>
        public class WeekSnapshot
        {
            public int Week;
            public string Season = "";
            public int Year;

            // 城市快照
            public double GranaryGrain;
            public double Treasury;
            public double GarrisonStrength;
            public double GarrisonMorale;
            public int ArmyPayArrearsWeeks;
            public double Prosperity;
            public double MarketActivity;
            public double PeopleBurden;
            public double Unrest;

            // 收支明细（需外部填充）
            public double IncomeGrain;
            public double IncomeMoney;
            public double ExpenseArmy;
            public double ExpenseOffice;
            public List<string> Events = new();

            // 家族快照（ID → 快照）
            public Dictionary<string, FamilySnapshot> FamilySnapshots = new();

            /// <summary>从世界状态创建快照</summary>
            public static WeekSnapshot Capture(World world, City city)
            {
                var snap = new WeekSnapshot
                {
                    Week = world.Week,
                    Year = world.CurrentYear,
                    Season = FormatSeason(world.CurrentSeason),
                    GranaryGrain = city.GranaryGrain,
                    Treasury = city.Treasury,
                    GarrisonStrength = city.GarrisonStrength,
                    GarrisonMorale = city.GarrisonMorale,
                    ArmyPayArrearsWeeks = city.ArmyPayArrearsWeeks,
                    Prosperity = city.Prosperity,
                    MarketActivity = city.MarketActivity,
                    PeopleBurden = city.PeopleBurden,
                    Unrest = city.Unrest,
                };

                foreach (var f in city.Families)
                {
                    snap.FamilySnapshots[f.Id] = new FamilySnapshot
                    {
                        Name = f.Name,
                        Power = f.Power,
                        Dominance = f.Dominance,
                        Loyalty = f.LoyaltyToLord,
                        Grievance = f.Grievance,
                        EconShare = f.EconShare,
                        Militia = f.Militia,
                        Popularity = f.Popularity,
                    };
                }

                return snap;
            }
        }

        public class FamilySnapshot
        {
            public string Name = "";
            public double Power;
            public double Dominance;
            public double Loyalty;
            public double Grievance;
            public double EconShare;
            public double Militia;
            public double Popularity;
        }

        /// <summary>
        /// 渲染周推进报告。
        /// </summary>
        /// <param name="world">当前世界状态</param>
        /// <param name="city">目标城市</param>
        /// <param name="prevWeek">上周快照（第一周可为 null）</param>
        /// <returns>渲染的报告字符串</returns>
        public static string Render(World world, City city, WeekSnapshot? prevWeek)
        {
            var currSnap = WeekSnapshot.Capture(world, city);
            var sb = new System.Text.StringBuilder();

            // ── 标题 ──
            sb.AppendLine(RenderHeader(currSnap));
            sb.AppendLine();

            // ── 收支摘要 ──
            if (prevWeek != null)
            {
                sb.AppendLine(RenderFinance(currSnap, prevWeek));
                sb.AppendLine();
            }

            // ── 城市指标变化 ──
            if (prevWeek != null)
            {
                sb.AppendLine(RenderCityChanges(city, prevWeek));
                sb.AppendLine();
            }

            // ── 家族变化 ──
            if (prevWeek != null)
            {
                sb.AppendLine(RenderFamilyChanges(city, prevWeek));
                sb.AppendLine();
            }

            // ── 事件日志 ──
            var recentEvents = GetRecentEvents(world, currSnap.Week);
            if (recentEvents.Any())
            {
                sb.AppendLine(RenderEvents(recentEvents));
                sb.AppendLine();
            }

            // ── 底部提示 ──
            sb.AppendLine(AnsiHelper.Dim("  按任意键继续..."));

            return sb.ToString();
        }

        /// <summary>
        /// 简单渲染：只显示当前状态，不做对比。
        /// </summary>
        public static string RenderSimple(World world, City city)
        {
            var snap = WeekSnapshot.Capture(world, city);
            var sb = new System.Text.StringBuilder();

            sb.AppendLine(RenderHeader(snap));
            sb.AppendLine();
            sb.AppendLine(RenderCurrentFinance(city));
            sb.AppendLine();

            var recentEvents = GetRecentEvents(world, snap.Week);
            if (recentEvents.Any())
                sb.AppendLine(RenderEvents(recentEvents));

            return sb.ToString();
        }

        // ── 内部渲染方法 ──

        private static string RenderHeader(WeekSnapshot snap)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"  ╔══════════════════════════════════════════╗");
            sb.AppendLine($"  ║  {AnsiHelper.Bold($"第 {snap.Week} 周")}  {snap.Year}年 {snap.Season}" +
                          $"{new string(' ', 20 - snap.Season.Length * 2 - 5)}║");
            sb.AppendLine($"  ║  {AnsiHelper.Dim("周推进报告")}{new string(' ', 26)}║");
            sb.AppendLine($"  ╚══════════════════════════════════════════╝");
            return sb.ToString();
        }

        private static string RenderFinance(WeekSnapshot curr, WeekSnapshot prev)
        {
            double grainDelta = curr.GranaryGrain - prev.GranaryGrain;
            double moneyDelta = curr.Treasury - prev.Treasury;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(AnsiHelper.Bold("  💰 财政结算:"));

            // 收入
            sb.AppendLine($"    收入:");
            if (curr.IncomeGrain > 0 || curr.IncomeMoney > 0)
            {
                sb.AppendLine($"      田赋: +{curr.IncomeGrain:F0} 粮  +{curr.IncomeMoney:F0} 钱");
            }
            else
            {
                // 从变化反推（简化显示）
                sb.AppendLine($"      (明细未记录)");
            }

            // 支出
            sb.AppendLine($"    支出:");
            if (curr.ExpenseArmy > 0)
                sb.AppendLine($"      军饷: -{curr.ExpenseArmy:F0} 钱");
            if (curr.ExpenseOffice > 0)
                sb.AppendLine($"      官俸: -{curr.ExpenseOffice:F0} 钱");

            // 净变化
            string grainArrow = grainDelta >= 0 ? "+" : "";
            string moneyArrow = moneyDelta >= 0 ? "+" : "";
            string grainColor = grainDelta >= 0 ? AnsiHelper.Green : (Func<string, string>)AnsiHelper.Red;
            string moneyColor = moneyDelta >= 0 ? AnsiHelper.Green : (Func<string, string>)AnsiHelper.Red;

            sb.AppendLine($"    ─────────────────────────────");
            sb.AppendLine($"    {AnsiHelper.Bold("净变化:")}");
            sb.AppendLine($"      粮: {grainColor($"{grainArrow}{grainDelta:F0}")}  →  现有 {curr.GranaryGrain:F0}");
            sb.AppendLine($"      钱: {moneyColor($"{moneyArrow}{moneyDelta:F0}")}  →  现有 {curr.Treasury:F0}");

            return sb.ToString();
        }

        private static string RenderCurrentFinance(City city)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(AnsiHelper.Bold("  💰 财政状态:"));
            sb.AppendLine($"    粮: {city.GranaryGrain:F0}   钱: {city.Treasury:F0}");
            sb.AppendLine($"    税率: {city.TaxRate:P0}   盐税: {city.SaltTaxRate:P0}");
            sb.AppendLine($"    每周军饷需求: {city.WeeklyArmyPayNeeded:F0}");
            return sb.ToString();
        }

        private static string RenderCityChanges(City city, WeekSnapshot prev)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(AnsiHelper.Bold("  📊 城市变化:"));

            // 各指标的变化
            var changes = new List<(string label, double curr, double prev, bool inverse)>
            {
                ("繁荣度", city.Prosperity, prev.Prosperity, true),
                ("市场活跃", city.MarketActivity, prev.MarketActivity, true),
                ("民负", city.PeopleBurden, prev.PeopleBurden, false),
                ("Unrest", city.Unrest, prev.Unrest, false),
            };

            foreach (var (label, curr, prevVal, inverse) in changes)
            {
                double delta = curr - prevVal;
                if (Math.Abs(delta) < 0.001) continue; // 无变化跳过

                string arrow = delta > 0 ? "↑" : "↓";
                bool isGood = inverse ? delta > 0 : delta < 0;
                string colorFn = isGood ? AnsiHelper.Green : AnsiHelper.Red;
                sb.AppendLine($"    {label,-8} {colorFn($"{arrow} {Math.Abs(delta):F2}")} " +
                              $"({prevVal:F2} → {curr:F2})");
            }

            // 兵力变化
            double strDelta = city.GarrisonStrength - prev.GarrisonStrength;
            if (Math.Abs(strDelta) > 0.1)
            {
                string arrow = strDelta > 0 ? "↑" : "↓";
                string color = strDelta > 0 ? AnsiHelper.Green : AnsiHelper.Red;
                sb.AppendLine($"    兵力   {color($"{arrow} {Math.Abs(strDelta):F0}")} " +
                              $"({prev.GarrisonStrength:F0} → {city.GarrisonStrength:F0})");
            }

            // 欠饷变化
            if (city.ArmyPayArrearsWeeks != prev.ArmyPayArrearsWeeks)
            {
                int payDelta = city.ArmyPayArrearsWeeks - prev.ArmyPayArrearsWeeks;
                string payColor = payDelta > 0 ? AnsiHelper.Red : AnsiHelper.Green;
                sb.AppendLine($"    欠饷   {payColor($"{(payDelta > 0 ? "+" : "")}{payDelta} 周")} " +
                              $"(当前 {city.ArmyPayArrearsWeeks} 周)");
            }

            return sb.ToString();
        }

        private static string RenderFamilyChanges(City city, WeekSnapshot prev)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(AnsiHelper.Bold("  🏯 家族变化:"));

            bool anyChange = false;
            foreach (var f in city.Families.OrderByDescending(f => f.Power))
            {
                if (!prev.FamilySnapshots.TryGetValue(f.Id, out var prevSnap))
                    continue;

                double powerDelta = f.Power - prevSnap.Power;
                double loyaltyDelta = f.LoyaltyToLord - prevSnap.Loyalty;
                double grievanceDelta = f.Grievance - prevSnap.Grievance;

                if (Math.Abs(powerDelta) < 0.001 && Math.Abs(loyaltyDelta) < 0.001
                    && Math.Abs(grievanceDelta) < 0.001)
                    continue;

                anyChange = true;
                string domStar = f.IsDominant ? " ⭐" : "";
                sb.AppendLine($"    {AnsiHelper.Bold(f.Name)}{domStar}");

                if (Math.Abs(powerDelta) >= 0.001)
                {
                    string arrow = powerDelta > 0 ? "↑" : "↓";
                    string color = powerDelta > 0 ? AnsiHelper.Green : AnsiHelper.Red;
                    sb.AppendLine($"      Power:    {color($"{arrow} {Math.Abs(powerDelta):F3}")}");
                }
                if (Math.Abs(loyaltyDelta) >= 0.001)
                {
                    string arrow = loyaltyDelta > 0 ? "↑" : "↓";
                    string color = loyaltyDelta > 0 ? AnsiHelper.Green : AnsiHelper.Red;
                    sb.AppendLine($"      忠诚:     {color($"{arrow} {Math.Abs(loyaltyDelta):F3}")}");
                }
                if (Math.Abs(grievanceDelta) >= 0.001)
                {
                    string arrow = grievanceDelta > 0 ? "↑" : "↓";
                    string color = grievanceDelta < 0 ? AnsiHelper.Green : AnsiHelper.Red;
                    sb.AppendLine($"      怨气:     {color($"{arrow} {Math.Abs(grievanceDelta):F3}")}");
                }
            }

            if (!anyChange)
                sb.AppendLine("    (本周无显著变化)");

            return sb.ToString();
        }

        private static string RenderEvents(List<string> events)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(AnsiHelper.Bold("  📜 本周大事:"));
            foreach (var evt in events)
            {
                sb.AppendLine($"    • {evt}");
            }
            return sb.ToString();
        }

        private static List<string> GetRecentEvents(World world, int currentWeek)
        {
            // 获取本周的叙事日志条目
            return world.Chronicle
                .Where(e => e.Contains($"第{currentWeek}周"))
                .ToList();
        }

        private static string FormatSeason(Season s) => s switch
        {
            Season.Spring => "春",
            Season.Summer => "夏",
            Season.Autumn => "秋",
            Season.Winter => "冬",
            _ => ""
        };
    }
}
