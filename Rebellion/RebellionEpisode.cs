// ============================================================
// Rebellion/RebellionEpisode.cs — Phase 1 极简版叛乱状态机
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using TangMo;

namespace TangMo.Rebellion
{
    /// <summary>
    /// Phase 1 极简版叛乱实例：
    /// SPARK → 根据实力对比和简单规则自动进入 RESOLUTION
    /// 结局写回周层城市和家族状态。
    /// 
    /// Phase 2 将扩展为 SPARK → RUMOR → CABAL → REVOLT → AFTERMATH 五阶段。
    /// </summary>
    public class RebellionEpisode
    {
        public EpisodePhase Phase { get; private set; } = EpisodePhase.Spark;
        public EpisodeResolution Resolution { get; private set; } = EpisodeResolution.None;

        /// <summary>
        /// 极简叛乱解决：根据实力对比自动选择结局。
        /// Phase 1 不引入玩家选择，按规则自动裁决。
        /// </summary>
        public void Resolve(City city, World world, Family leaderFamily)
        {
            Phase = EpisodePhase.Spark;

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

            // ── 根据实力比裁决 ──
            if (ratio > 1.2)
            {
                // 优势：Phase 1 默认武力镇压
                Resolution = EpisodeResolution.Suppress;
                ApplySuppress(city, world, rebelFamilies, lordPower, rebelPower);
                world.Log($"⚔️ {city.Name} 叛乱镇压成功！叛军家族：{rebelNames}");
            }
            else if (ratio >= 0.8)
            {
                // 均势：惨胜
                Resolution = EpisodeResolution.CostlySuppress;
                ApplyCostlySuppress(city, world, rebelFamilies, lordPower, rebelPower);
                world.Log($"💀 {city.Name} 惨胜！双方损失惨重。叛军家族：{rebelNames}");
            }
            else
            {
                // 劣势：Phase 1 默认赦免改革
                Resolution = EpisodeResolution.GrantReform;
                ApplyGrantReform(city, world, rebelFamilies);
                world.Log($"😰 {city.Name} 被迫赦免改革。叛军家族：{rebelNames}");
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
                    $"{year}{season}，{leaderName(leaderFamily)}因积怨率众起事，节度使发兵镇之，{leaderName(leaderFamily)}伏诛，然军民俱疲。",
                EpisodeResolution.CostlySuppress =>
                    $"{year}{season}，{leaderName(leaderFamily)}举兵相抗，官军苦战方克，城中大损，民不聊生。",
                EpisodeResolution.GrantReform =>
                    $"{year}{season}，{leaderName(leaderFamily)}势大难制，节度使许以重权，暂安其心，然藩镇格局已变。",
                _ => $"{year}{season}，{city.Name}发生叛乱，结局未知。"
            };
            world.Log($"📜 节度使回忆录：{narrative}");
        }

        private string leaderName(Family f) => f.Name;

        /// <summary>武力镇压结局</summary>
        private void ApplySuppress(City city, World world, 
            System.Collections.Generic.List<Family> rebels, double lordPower, double rebelPower)
        {
            // 叛乱家族：忠诚暴跌、武力大减
            foreach (var f in rebels)
            {
                f.LoyaltyToLord = Math.Max(0.05, f.LoyaltyToLord - 0.30);
                f.Militia = Math.Max(0.05, f.Militia * 0.4);
                f.Grievance = Math.Min(1.0, f.Grievance + 0.20);
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

        /// <summary>惨胜结局</summary>
        private void ApplyCostlySuppress(City city, World world,
            System.Collections.Generic.List<Family> rebels, double lordPower, double rebelPower)
        {
            // 双方大幅损耗 40%-60%
            foreach (var f in rebels)
            {
                f.LoyaltyToLord = Math.Max(0.05, f.LoyaltyToLord - 0.25);
                f.Militia = Math.Max(0.02, f.Militia * 0.3);
                f.Grievance = Math.Min(1.0, f.Grievance + 0.15);
            }

            city.GarrisonStrength *= 0.5; // 损失 50%
            city.GarrisonMorale = Math.Max(0.1, city.GarrisonMorale - 0.20);
            city.Treasury *= 0.4;
            city.Prosperity = Math.Max(0, city.Prosperity - 0.20);
            city.PeopleBurden = Math.Min(1.0, city.PeopleBurden + 0.15);
        }

        /// <summary>赦免改革结局</summary>
        private void ApplyGrantReform(City city, World world,
            System.Collections.Generic.List<Family> rebels)
        {
            // 叛乱家族：忠诚短期飙升，但 Dominance 上升
            foreach (var f in rebels)
            {
                f.LoyaltyToLord = Math.Min(0.90, f.LoyaltyToLord + 0.40);
                f.EconShare = Math.Min(1.0, f.EconShare + 0.10);
                f.Militia = Math.Min(1.0, f.Militia + 0.15);
                f.Grievance = Math.Max(0, f.Grievance - 0.30);
            }

            // 忠诚家族忠诚下降
            foreach (var f in city.Families.Except(rebels))
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
