// ============================================================
// UI/RebellionDecisionUI.cs — 叛乱决策界面
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace TangMo.UI
{
    /// <summary>
    /// 叛乱决策界面 — GDD §7 叛乱日实例。
    /// 触发叛乱时展示：
    /// - 叛乱实力对比（领主 vs 叛军）
    /// - 参与家族详情
    /// - 可选行动列表
    /// - 玩家输入选择
    /// 
    /// 返回玩家选择的 EpisodeResolution。
    /// </summary>
    public static class RebellionDecisionUI
    {
        /// <summary>
        /// 展示叛乱决策界面，等待玩家选择。
        /// </summary>
        /// <param name="city">发生叛乱的城市</param>
        /// <param name="world">世界状态</param>
        /// <param name="leaderFamily">叛乱领袖家族</param>
        /// <returns>玩家选择的结局类型</returns>
        public static EpisodeResolution Show(City city, World world, Family leaderFamily)
        {
            // ── 计算双方实力 ──
            var powerData = CalculatePower(city, leaderFamily);
            var availableOptions = GetAvailableOptions(powerData.Ratio);

            // ── 渲染界面 ──
            Console.Clear();
            Console.WriteLine(RenderRebellionAlert(city, leaderFamily));
            Console.WriteLine();
            Console.WriteLine(RenderPowerComparison(powerData));
            Console.WriteLine();
            Console.WriteLine(RenderRebelFamilies(city, leaderFamily));
            Console.WriteLine();
            Console.WriteLine(RenderOptions(availableOptions, powerData.Ratio));
            Console.WriteLine();

            // ── 等待玩家输入 ──
            return PromptPlayerChoice(availableOptions);
        }

        /// <summary>
        /// 纯渲染版本（不阻塞等待输入，返回渲染字符串）。
        /// 用于在主循环中统一处理输入。
        /// </summary>
        public static string Render(City city, World world, Family leaderFamily)
        {
            var powerData = CalculatePower(city, leaderFamily);
            var availableOptions = GetAvailableOptions(powerData.Ratio);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(RenderRebellionAlert(city, leaderFamily));
            sb.AppendLine();
            sb.AppendLine(RenderPowerComparison(powerData));
            sb.AppendLine();
            sb.AppendLine(RenderRebelFamilies(city, leaderFamily));
            sb.AppendLine();
            sb.AppendLine(RenderOptions(availableOptions, powerData.Ratio));

            return sb.ToString();
        }

        // ── 实力计算 ──

        public struct PowerData
        {
            public double LordPower;
            public double RebelPower;
            public double Ratio; // LordPower / RebelPower
            public double LordLoyaltyAvg;
            public List<Family> RebelFamilies;
            public string RebelNames;
        }

        public static PowerData CalculatePower(City city, Family leaderFamily)
        {
            // 领主方实力
            double lordLoyaltyAvg = city.Families
                .Where(f => f != leaderFamily)
                .Select(f => f.LoyaltyToLord)
                .DefaultIfEmpty(0.5)
                .Average();

            double lordPower = city.GarrisonStrength * (1 + 0.5 * lordLoyaltyAvg);

            // 叛军方实力
            var rebelFamilies = city.Families
                .Where(f => f.LoyaltyToLord < 0.40 || f == leaderFamily)
                .ToList();

            double rebelPower = rebelFamilies.Sum(f => f.Militia * f.Dominance);
            double externalSupport = rebelFamilies.Average(f => f.ExternalSupport);
            rebelPower *= (1 + externalSupport);

            double ratio = lordPower / Math.Max(rebelPower, 0.01);

            return new PowerData
            {
                LordPower = lordPower,
                RebelPower = rebelPower,
                Ratio = ratio,
                LordLoyaltyAvg = lordLoyaltyAvg,
                RebelFamilies = rebelFamilies,
                RebelNames = string.Join("、", rebelFamilies.Select(f => f.Name))
            };
        }

        // ── 渲染方法 ──

        private static string RenderRebellionAlert(City city, Family leader)
        {
            var lines = new List<string>
            {
                "",
                AnsiHelper.BoldRed($"  ╔══════════════════════════════════════════╗"),
                AnsiHelper.BoldRed($"  ║     🔥🔥🔥 叛 乱 爆 发 🔥🔥🔥          ║"),
                AnsiHelper.BoldRed($"  ╚══════════════════════════════════════════╝"),
                "",
                $"  {AnsiHelper.Bold(leader.Name)} 率众于 {AnsiHelper.BoldRed(city.Name)} 起事！",
                $"  {AnsiHelper.Dim("日实例进入 — 周层暂停推进")}"
            };
            return string.Join("\n", lines);
        }

        private static string RenderPowerComparison(PowerData data)
        {
            string ratioColor;
            string ratioDesc;
            if (data.Ratio > 1.2)
            {
                ratioColor = AnsiHelper.Green($"领主优势 ({data.Ratio:F2}:1)");
                ratioDesc = "我军占优，可主动出击";
            }
            else if (data.Ratio >= 0.8)
            {
                ratioColor = AnsiHelper.Yellow($"势均力敌 ({data.Ratio:F2}:1)");
                ratioDesc = "胜负难料，需谨慎抉择";
            }
            else
            {
                ratioColor = AnsiHelper.Red($"叛军占优 ({data.Ratio:F2}:1)");
                ratioDesc = "敌强我弱，正面冲突代价极大";
            }

            var lines = new List<string>
            {
                $"  ┌─ 实力对比 ──────────────────────────────┐",
                $"  │                                        │",
                $"  │  {AnsiHelper.Bold("领主方")}                     {AnsiHelper.Bold("叛军方")}      │",
                $"  │  兵力: {city_GarrisonStr(data.LordPower),-10}    兵力: {data.RebelPower,-10:F0}   │",
                $"  │  忠诚均值: {data.LordLoyaltyAvg,-8:F2}    外援: 有       │",
                $"  │                                        │",
                $"  │  {AnsiHelper.Bold("态势:")} {ratioColor,-20}        │",
                $"  │  {AnsiHelper.Dim(ratioDesc),-40}│",
                $"  └────────────────────────────────────────┘"
            };

            // 这里用简化的渲染，不严格对齐列宽（终端宽度不可预知）
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"  ┌─ 实力对比 ──────────────────────────────┐");
            sb.AppendLine($"  │                                        │");
            sb.AppendLine($"  │  {AnsiHelper.Bold("领主方")}              vs        {AnsiHelper.Bold("叛军方")}  │");
            sb.AppendLine($"  │  兵力: {data.LordPower:F0}                  {data.RebelPower:F0}         │");
            sb.AppendLine($"  │  忠诚均值: {data.LordLoyaltyAvg:F2}              外援 有  │");
            sb.AppendLine($"  │                                        │");
            sb.AppendLine($"  │  {AnsiHelper.Bold("态势:")} {ratioColor}                │");
            sb.AppendLine($"  │  {AnsiHelper.Dim(ratioDesc)}                          │");
            sb.AppendLine($"  └────────────────────────────────────────┘");
            return sb.ToString();
        }

        private static string city_GarrisonStr(double power) => $"{power:F0}";

        private static string RenderRebelFamilies(City city, Family leader)
        {
            var rebelFamilies = city.Families
                .Where(f => f.LoyaltyToLord < 0.40 || f == leader)
                .OrderByDescending(f => f.Power)
                .ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"  📋 参与叛乱家族:");
            sb.AppendLine($"  {AnsiHelper.Dim("──────────────────────────────────────────")}");

            foreach (var f in rebelFamilies)
            {
                string leaderTag = f == leader ? AnsiHelper.BoldRed(" [领袖]") : "";
                string typeTag = FormatFamilyType(f.Type);

                sb.AppendLine($"    {AnsiHelper.Bold(f.Name)}{leaderTag} {AnsiHelper.Dim(typeTag)}");
                sb.AppendLine($"      武力份额 {f.Militia:F2}  经济份额 {f.EconShare:F2}  " +
                              $"民望 {f.Popularity:F2}  Dominance {f.Dominance:F2}");
                sb.AppendLine($"      忠诚 {AnsiHelper.ColorValueInverse(f.LoyaltyToLord, 0.5, 0.3)}  " +
                              $"怨气 {AnsiHelper.ColorValue(f.Grievance, 0.2, 0.5)}  " +
                              $"恐惧 {f.Fear:F2}");
            }

            // 忠诚家族一览
            var loyalFamilies = city.Families
                .Where(f => f.LoyaltyToLord >= 0.40 && f != leader)
                .OrderByDescending(f => f.LoyaltyToLord)
                .ToList();

            if (loyalFamilies.Any())
            {
                sb.AppendLine();
                sb.AppendLine($"  🛡️ 忠诚家族:");
                foreach (var f in loyalFamilies)
                {
                    sb.AppendLine($"    {AnsiHelper.Green("●")} {f.Name,-12} 忠诚 {f.LoyaltyToLord:F2}  " +
                                  $"武力 {f.Militia:F2}");
                }
            }

            return sb.ToString();
        }

        private static string RenderOptions(List<RebelOption> options, double ratio)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(AnsiHelper.Bold("  🎯 可选行动:"));
            sb.AppendLine($"  {AnsiHelper.Dim("──────────────────────────────────────────")}");

            for (int i = 0; i < options.Count; i++)
            {
                var opt = options[i];
                string key = AnsiHelper.BoldCyan($"[{i + 1}]");
                string costTag = opt.APCost > 0 ? AnsiHelper.Dim($" (AP {opt.APCost})") : "";

                sb.AppendLine($"    {key} {AnsiHelper.Bold(opt.Name)}{costTag}");

                // 描述
                sb.AppendLine($"        {AnsiHelper.Dim(opt.Description)}");

                // 预估效果
                sb.AppendLine($"        {AnsiHelper.Dim($"预估: {opt.EstimatedOutcome}")}");

                if (i < options.Count - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        // ── 玩家输入 ──

        private static EpisodeResolution PromptPlayerChoice(List<RebelOption> options)
        {
            while (true)
            {
                Console.Write($"  {AnsiHelper.Bold("请选择行动 [1-{0}]: ", options.Count)}");
                string? input = Console.ReadLine()?.Trim();

                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= options.Count)
                {
                    var selected = options[choice - 1];
                    Console.WriteLine();
                    Console.WriteLine($"  {AnsiHelper.Bold("已选择:")} {selected.Name}");
                    Console.WriteLine($"  {AnsiHelper.Dim("正在处理...")}");
                    return selected.Resolution;
                }

                Console.WriteLine($"  {AnsiHelper.Red("无效输入，请重新选择。")}");
            }
        }

        // ── 数据定义 ──

        public class RebelOption
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string EstimatedOutcome { get; set; } = "";
            public int APCost { get; set; } = 1;
            public EpisodeResolution Resolution { get; set; }
        }

        private static List<RebelOption> GetAvailableOptions(double ratio)
        {
            var options = new List<RebelOption>();

            if (ratio > 1.2)
            {
                // 领主优势
                options.Add(new RebelOption
                {
                    Name = "武力镇压",
                    Description = "发兵直取叛军，以优势兵力迅速平叛。",
                    EstimatedOutcome = "镇压成功，叛军忠诚大减，己方损耗 20-40%",
                    APCost = 1,
                    Resolution = EpisodeResolution.Suppress
                });
                options.Add(new RebelOption
                {
                    Name = "谈判让步",
                    Description = "以部分经济/军事利益换取叛军归顺。",
                    EstimatedOutcome = "妥协协议，让渡 10-20% 份额，短期稳定",
                    APCost = 1,
                    Resolution = EpisodeResolution.Autonomy
                });
            }
            else if (ratio >= 0.8)
            {
                // 势均力敌
                options.Add(new RebelOption
                {
                    Name = "强攻",
                    Description = "不顾伤亡，全力出击，赌一个惨胜。",
                    EstimatedOutcome = "惨胜，双方兵力损耗 40-60%，经济重创",
                    APCost = 1,
                    Resolution = EpisodeResolution.CostlySuppress
                });
                options.Add(new RebelOption
                {
                    Name = "谈判",
                    Description = "承认现状，以更大让步换取和平。",
                    EstimatedOutcome = "妥协协议，让步较大，但避免流血",
                    APCost = 1,
                    Resolution = EpisodeResolution.Autonomy
                });
            }
            else
            {
                // 叛军优势
                options.Add(new RebelOption
                {
                    Name = "让步改革",
                    Description = "赦免叛乱家族，给予重权，换取归顺。",
                    EstimatedOutcome = "忠诚飙升，但叛军 Dominance 大幅上升",
                    APCost = 1,
                    Resolution = EpisodeResolution.GrantReform
                });
                options.Add(new RebelOption
                {
                    Name = "弃城撤退",
                    Description = "放弃此城，保存实力，留待日后。",
                    EstimatedOutcome = "城市脱离控制，威望大损，边患增加",
                    APCost = 1,
                    Resolution = EpisodeResolution.CityFlipped
                });
            }

            return options;
        }

        private static string FormatFamilyType(FamilyType type)
        {
            var tags = new List<string>();
            if (type.HasFlag(FamilyType.MilitaryClan))   tags.Add("军");
            if (type.HasFlag(FamilyType.MerchantClan))   tags.Add("商");
            if (type.HasFlag(FamilyType.GentryClan))     tags.Add("士");
            if (type.HasFlag(FamilyType.ReligiousClan))  tags.Add("宗");
            if (type.HasFlag(FamilyType.LocalElderClan)) tags.Add("乡");
            if (type.HasFlag(FamilyType.CourtTiedClan))  tags.Add("朝");
            return tags.Any() ? $"[{string.Join("/", tags)}]" : "";
        }
    }
}
