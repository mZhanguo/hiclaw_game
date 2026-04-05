// ============================================================
// Rebellion/RebellionEpisode.cs — Phase 1 叛乱状态机（含玩家选择接口）
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using TangMo;

namespace TangMo.Rebellion
{
    /// <summary>
    /// 玩家在叛乱中的选择。
    /// Phase 1 由 AI 自动裁决，但预留接口供 Phase 2 接入 UI。
    /// </summary>
    public enum PlayerChoice
    {
        Auto,           // 自动裁决（Phase 1 默认）
        Suppress,       // 武力镇压（优势时可选）
        Negotiate,      // 谈判让步（优势/均势时可选）
        CostlyAttack,   // 强攻（均势时可选）
        Yield,          // 让步改革（劣势时可选）
        Abandon,        // 弃城（劣势时可选）— Phase 2 TODO
    }

    /// <summary>
    /// Phase 1 叛乱实例：
    /// SPARK → 根据实力对比和玩家选择进入 RESOLUTION
    /// 结局写回周层城市和家族状态。
    /// 
    /// Phase 2 将扩展为 SPARK → RUMOR → CABAL → REVOLT → AFTERMATH 五阶段。
    /// </summary>
    public class RebellionEpisode
    {
        public EpisodePhase Phase { get; private set; } = EpisodePhase.Spark;
        public EpisodeResolution Resolution { get; private set; } = EpisodeResolution.None;
        public PlayerChoice ChosenAction { get; private set; } = PlayerChoice.Auto;

        /// <summary>
        /// 叛乱解决：根据实力对比和玩家选择确定结局。
        /// playerChoice = Auto 时按规则自动裁决。
        /// </summary>
        public void Resolve(City city, World world, Family leaderFamily,
                            PlayerChoice playerChoice = PlayerChoice.Auto)
        {
            Phase = EpisodePhase.Spark;
            ChosenAction = playerChoice;

            // 计算双方实力
            double lordLoyaltyAvg = city.Families
                .Where(f => f != leaderFamily)
                .Select(f => f.LoyaltyToLord)
                .DefaultIfEmpty(0.5)
                .Average();

            double lordPower = city.GarrisonStrength * (1 + 0.5 * lordLoyaltyAvg);

            var rebelFamilies = city.Families
                .Where(f => f.LoyaltyToLord < 0.40 || f == leaderFamily)
                .ToList();

            double rebelPower = rebelFamilies.Sum(f => f.Militia * f.Dominance);
            double externalSupport = rebelFamilies.Average(f => f.ExternalSupport);
            rebelPower *= (1 + externalSupport);

            double ratio = lordPower / Math.Max(rebelPower, 0.01);

            string rebelNames = string.Join("、", rebelFamilies.Select(f => f.Name));

            // 非叛乱家族（忠诚家族）
            var loyalFamilies = city.Families.Except(rebelFamilies).ToList();

            // ── 根据实力比 + 玩家选择裁决 ──
            if (ratio > 1.2)
            {
                // 我方优势
                if (playerChoice == PlayerChoice.Negotiate)
                {
                    Resolution = EpisodeResolution.Autonomy;
                    ApplyAutonomy(city, world, rebelFamilies, loyalFamilies);
                    world.Log($"🤝 {city.Name} 优势谈判，叛军家族：{rebelNames}");
                }
                else
                {
                    // Auto 或 Suppress → 武力镇压
                    Resolution = EpisodeResolution.Suppress;
                    ChosenAction = PlayerChoice.Suppress;
                    ApplySuppress(city, world, rebelFamilies, loyalFamilies, lordPower, rebelPower);
                    world.Log($"⚔️ {city.Name} 叛乱镇压成功！叛军家族：{rebelNames}");
                }
            }
            else if (ratio >= 0.8)
            {
                // 均势
                if (playerChoice == PlayerChoice.Negotiate)
                {
                    Resolution = EpisodeResolution.Autonomy;
                    ApplyAutonomy(city, world, rebelFamilies, loyalFamilies);
                    world.Log($"🤝 {city.Name} 均势谈判，让步较多。叛军家族：{rebelNames}");
                }
                else
                {
                    // Auto 或 CostlyAttack → 惨胜
                    Resolution = EpisodeResolution.CostlySuppress;
                    ChosenAction = PlayerChoice.CostlyAttack;
                    ApplyCostlySuppress(city, world, rebelFamilies, loyalFamilies, lordPower, rebelPower);
                    world.Log($"💀 {city.Name} 惨胜！双方损失惨重。叛军家族：{rebelNames}");
                }
            }
            else
            {
                // 我方劣势
                if (playerChoice == PlayerChoice.Abandon)
                {
                    // Phase 2 TODO: 弃城逻辑
                    // Phase 1 兜底为赦免改革
                    Resolution = EpisodeResolution.GrantReform;
                    ChosenAction = PlayerChoice.Yield;
                    ApplyGrantReform(city, world, rebelFamilies, loyalFamilies);
                    world.Log($"😰 {city.Name} 被迫赦免改革（弃城 Phase 2 待实现）。叛军家族：{rebelNames}");
                }
                else
                {
                    // Auto 或 Yield → 赦免改革
                    Resolution = EpisodeResolution.GrantReform;
                    ChosenAction = PlayerChoice.Yield;
                    ApplyGrantReform(city, world, rebelFamilies, loyalFamilies);
                    world.Log($"😰 {city.Name} 被迫赦免改革。叛军家族：{rebelNames}");
                }
            }

            Phase = EpisodePhase.Resolution;

            // 解除叛乱标记，进入恢复期
            city.RemoveFlag(CityFlags.REBELLION_ACTIVE);
            city.AddFlag(CityFlags.RECOVERY_MODE);
            city.RecoveryWeeksLeft = 3; // 3 周恢复期

            // 添加叙事记录
            string year = $"{world.CurrentYear}年";
            string season = world.CurrentSeason.ToString() switch
            {
                "Spring" => "春", "Summer" => "夏",
                "Autumn" => "秋", "Winter" => "冬", _ => ""
            };
            string narrative = Resolution switch
            {
                EpisodeResolution.Suppress =>
                    $"{year}{season}，{leaderFamily.Name}因积怨率众起事，节度使发兵镇之，{leaderFamily.Name}伏诛，然军民俱疲。",
                EpisodeResolution.CostlySuppress =>
                    $"{year}{season}，{leaderFamily.Name}举兵相抗，官军苦战方克，城中大损，民不聊生。",
                EpisodeResolution.Autonomy =>
                    $"{year}{season}，{leaderFamily.Name}势起，节度使权衡利弊，许以自治，{city.Name}权力格局生变。",
                EpisodeResolution.GrantReform =>
                    $"{year}{season}，{leaderFamily.Name}势大难制，节度使许以重权，暂安其心，然藩镇格局已变。",
                EpisodeResolution.CityFlipped =>
                    $"{year}{season}，{city.Name}失守，节度使弃城而去，威望扫地。",
                _ => $"{year}{season}，{city.Name}发生叛乱，结局未知。"
            };
            world.Log($"📜 节度使回忆录：{narrative}");
        }

        // ═══════════════════════════════════════
        //  结局效果
        // ═══════════════════════════════════════

        /// <summary>武力镇压结局（含忠诚家族波及）</summary>
        private void ApplySuppress(City city, World world,
            List<Family> rebels, List<Family> loyals,
            double lordPower, double rebelPower)
        {
            // 叛乱家族：忠诚暴跌、武力大减
            foreach (var f in rebels)
            {
                f.LoyaltyToLord = Math.Max(0.05, f.LoyaltyToLord - 0.30);
                f.Militia = Math.Max(0.05, f.Militia * 0.4);
                f.Grievance = Math.Min(1.0, f.Grievance + 0.20);
            }

            // 忠诚家族波及：见血腥镇压，忠诚微降、恐惧上升
            foreach (var f in loyals)
            {
                f.LoyaltyToLord = Math.Max(0.10, f.LoyaltyToLord - 0.05);
                f.Fear = Math.Min(1.0, f.Fear + 0.10);
            }

            // 己方兵力损耗 20%-40%
            double lossRate = 0.20 + (1.0 - (lordPower / (lordPower + rebelPower))) * 0.20;
            city.GarrisonStrength *= (1 - lossRate);

            // 士气恢复（打赢了）
            city.GarrisonMorale = Math.Min(1.0, city.GarrisonMorale + 0.15);

            // 经济消耗
            city.Treasury *= 0.7;
            city.Prosperity = Math.Max(0, city.Prosperity - 0.10);
        }

        /// <summary>惨胜结局（含忠诚家族波及）</summary>
        private void ApplyCostlySuppress(City city, World world,
            List<Family> rebels, List<Family> loyals,
            double lordPower, double rebelPower)
        {
            // 叛乱家族大幅损耗
            foreach (var f in rebels)
            {
                f.LoyaltyToLord = Math.Max(0.05, f.LoyaltyToLord - 0.25);
                f.Militia = Math.Max(0.02, f.Militia * 0.3);
                f.Grievance = Math.Min(1.0, f.Grievance + 0.15);
            }

            // 忠诚家族波及：惨胜人人自危，忠诚明显下降
            foreach (var f in loyals)
            {
                f.LoyaltyToLord = Math.Max(0.10, f.LoyaltyToLord - 0.10);
                f.Fear = Math.Min(1.0, f.Fear + 0.15);
                f.Grievance = Math.Min(1.0, f.Grievance + 0.05);
            }

            city.GarrisonStrength *= 0.5; // 损失 50%
            city.GarrisonMorale = Math.Max(0.1, city.GarrisonMorale - 0.20);
            city.Treasury *= 0.4;
            city.Prosperity = Math.Max(0, city.Prosperity - 0.20);
            city.PeopleBurden = Math.Min(1.0, city.PeopleBurden + 0.15);
        }

        /// <summary>谈判让步结局（Autonomy）— Phase 2 TODO: 完善效果</summary>
        private void ApplyAutonomy(City city, World world,
            List<Family> rebels, List<Family> loyals)
        {
            // 叛乱家族获得份额让渡
            foreach (var f in rebels)
            {
                f.LoyaltyToLord = Math.Min(0.70, f.LoyaltyToLord + 0.15);
                f.EconShare = Math.Min(1.0, f.EconShare + 0.10);
                f.Militia = Math.Min(1.0, f.Militia + 0.10);
                f.Grievance = Math.Max(0, f.Grievance - 0.15);
            }

            // 忠诚家族不满：凭什么反叛还能拿好处？
            foreach (var f in loyals)
            {
                f.LoyaltyToLord = Math.Max(0.10, f.LoyaltyToLord - 0.08);
                f.Grievance = Math.Min(1.0, f.Grievance + 0.05);
            }

            city.InstitutionPower = Math.Max(0, city.InstitutionPower - 0.05);
            world.Prestige = Math.Max(0, world.Prestige - 0.05);
        }

        /// <summary>赦免改革结局（含忠诚家族波及）</summary>
        private void ApplyGrantReform(City city, World world,
            List<Family> rebels, List<Family> loyals)
        {
            // 叛乱家族：忠诚短期飙升，但 Dominance 上升
            foreach (var f in rebels)
            {
                f.LoyaltyToLord = Math.Min(0.90, f.LoyaltyToLord + 0.40);
                f.EconShare = Math.Min(1.0, f.EconShare + 0.10);
                f.Militia = Math.Min(1.0, f.Militia + 0.15);
                f.Grievance = Math.Max(0, f.Grievance - 0.30);
            }

            // 忠诚家族忠诚下降：领主软弱，靠不住
            foreach (var f in loyals)
            {
                f.LoyaltyToLord = Math.Max(0.1, f.LoyaltyToLord - 0.15);
            }

            // 制度力下降
            city.InstitutionPower = Math.Max(0, city.InstitutionPower - 0.10);

            // 威望受损
            world.Prestige = Math.Max(0, world.Prestige - 0.10);
        }
    }
}
