using TangMo;
// ============================================================
// Steps/RebellionGateCheckStep.cs — Step 7: 叛乱阈值判定
// ============================================================
using System;
using System.Linq;

namespace TangMo.Steps
{
    /// <summary>
    /// 叛乱阈值判定：
    /// - 计算 Unrest
    /// - 若 Unrest >= 0.70 * RevoltThreshold，触发叛乱日实例
    /// - 恢复期内阈值降低 20%
    /// </summary>
    public class RebellionGateCheckStep : IWeeklyStep
    {
        public string Name => "叛乱判定";

        public void Execute(City city, World world)
        {
            if (!city.Families.Any()) return;

            // ── 计算 Unrest ──
            // 取各家族最高 Unrest 作为城市 Unrest
            double maxUnrest = 0;

            foreach (var family in city.Families)
            {
                double ed = family.GetEffectiveDominance(city.InstitutionPower);

                // shock 量化
                double shock = CalculateShock(city);

                double unrest =
                    0.45 * (1 - family.LoyaltyToLord)
                  + 0.35 * family.Grievance
                  + 0.25 * ed
                  + 0.20 * family.ExternalSupport
                  - 0.50 * city.InstitutionPower
                  + shock;

                unrest = Math.Clamp(unrest, 0, 1);
                maxUnrest = Math.Max(maxUnrest, unrest);
            }

            city.Unrest = maxUnrest;

            // ── 预警机制（Unrest >= 0.55 黄灯）──
            if (maxUnrest >= 0.55 && maxUnrest < 0.70)
            {
                var worstFamily = city.Families
                    .OrderByDescending(f =>
                        0.45 * (1 - f.LoyaltyToLord)
                      + 0.35 * f.Grievance
                      + 0.25 * f.GetEffectiveDominance(city.InstitutionPower))
                    .First();

                string warning = maxUnrest >= 0.65
                    ? $"⚠️⚠️ {city.Name} 局势紧张！{worstFamily.Name}不满加剧 (Unrest={maxUnrest:F2})，叛乱近在咫尺"
                    : $"⚠️ {city.Name} 暗流涌动，{worstFamily.Name}心怀异志 (Unrest={maxUnrest:F2})";

                world.Log(warning);
            }

            // ── 叛乱触发判定 ──
            double threshold = 0.70 * world.RevoltThreshold;

            // 恢复期内阈值降低 20%
            if (city.HasFlag(CityFlags.RECOVERY_MODE))
                threshold *= 0.80;

            // 日实例期间禁止嵌套触发
            if (city.HasFlag(CityFlags.REBELLION_ACTIVE))
                return;

            // 其他城市已在叛乱中则延迟（嵌套防护）
            if (world.PlayerCities.Any(c => c != city && c.HasFlag(CityFlags.REBELLION_ACTIVE)))
                return;

            if (city.Unrest >= threshold)
            {
                // 找到最不满的家族作为叛乱领袖
                var leader = city.Families
                    .OrderByDescending(f =>
                        0.45 * (1 - f.LoyaltyToLord)
                      + 0.35 * f.Grievance
                      + 0.25 * f.GetEffectiveDominance(city.InstitutionPower))
                    .First();

                world.Log($"🔥🔥🔥 {city.Name} 叛乱触发！领袖家族：{leader.Name} " +
                          $"(Unrest={city.Unrest:F2}, 阈值={threshold:F2})");

                // 标记叛乱（Phase 1 极简：直接进入 Resolution）
                city.AddFlag(CityFlags.REBELLION_ACTIVE);

                // 创建极简叛乱实例并立即解决
                var episode = new Rebellion.RebellionEpisode();
                episode.Resolve(city, world, leader);
            }
        }

        /// <summary>
        /// shock 量化：根据城市 Flags 计算 shock 值，∈ [0, 0.35]
        /// </summary>
        private double CalculateShock(City city)
        {
            double shock = 0;

            // 欠饷 shock
            if (city.HasFlag(CityFlags.ARMY_PAY_ISSUE))
                shock += 0.05 * Math.Min(city.ArmyPayArrearsWeeks, 3);

            // 盐税 shock
            if (city.HasFlag(CityFlags.SALT_TAX_ISSUE))
                shock += 0.07; // 简化：固定中间值

            // 官署瘫痪 shock
            if (city.HasFlag(CityFlags.OFFICE_PARALYZED_RISK))
                shock += 0.10;

            return Math.Min(shock, 0.35);
        }
    }
}
