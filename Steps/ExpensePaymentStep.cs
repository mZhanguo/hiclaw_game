using TangMo;
// ============================================================
// Steps/ExpensePaymentStep.cs — Step 2: 支出结算
// ============================================================
using System;

namespace TangMo.Steps
{
    /// <summary>
    /// 支出结算：
    /// - 军饷：按 MilitaryPriority 优先支付，不足则欠饷递增
    /// - 官俸与维护：固定开支，不足则标记官署瘫痪风险
    /// </summary>
    public class ExpensePaymentStep : IWeeklyStep
    {
        public string Name => "支出结算";

        public void Execute(City city, World world)
        {
            // ── 军饷 ──
            double payNeeded = city.WeeklyArmyPayNeeded;
            double payBudget = city.Treasury * city.MilitaryPriority;
            double payActual = Math.Min(payNeeded, payBudget);

            city.Treasury -= payActual;

            if (payActual < payNeeded)
            {
                // 欠饷
                city.ArmyPayArrearsWeeks++;
                city.GarrisonMorale = Math.Max(0, city.GarrisonMorale - 0.05);
                city.AddFlag(CityFlags.ARMY_PAY_ISSUE);
                world.Log($"⚠️ {city.Name} 军饷不足！应发{payNeeded:F0} 实发{payActual:F0} " +
                          $"欠饷{city.ArmyPayArrearsWeeks}周 士气={city.GarrisonMorale:F2}");
            }
            else
            {
                // 军饷充足 → 欠饷递减
                if (city.ArmyPayArrearsWeeks > 0)
                {
                    city.ArmyPayArrearsWeeks = Math.Max(0, city.ArmyPayArrearsWeeks - 1);
                    if (city.ArmyPayArrearsWeeks == 0)
                    {
                        city.RemoveFlag(CityFlags.ARMY_PAY_ISSUE);
                        world.Log($"✓ {city.Name} 欠饷清偿");
                    }
                }
                // 士气恢复
                city.GarrisonMorale = Math.Min(1.0, city.GarrisonMorale + 0.02);
            }

            // ── 官俸与维护 ──
            double adminCost = 20.0; // 基础官俸
            double maintenanceCost = 10.0; // 维护费

            if (city.Treasury >= adminCost + maintenanceCost)
            {
                city.Treasury -= (adminCost + maintenanceCost);
                city.RemoveFlag(CityFlags.OFFICE_PARALYZED_RISK);
            }
            else
            {
                city.Treasury = Math.Max(0, city.Treasury);
                city.AddFlag(CityFlags.OFFICE_PARALYZED_RISK);
                world.Log($"⚠️ {city.Name} 官俸不足，官署面临瘫痪风险");
            }

            // ── 军粮消耗 ──
            double grainConsumption = city.GarrisonStrength * 0.2;
            city.GranaryGrain -= grainConsumption;
            if (city.GranaryGrain < 0)
            {
                city.GranaryGrain = 0;
                city.AddFlag(CityFlags.FAMINE_RISK);
                city.GarrisonMorale = Math.Max(0, city.GarrisonMorale - 0.10);
                world.Log($"🔥 {city.Name} 军粮告罄！饥荒风险");
            }
        }
    }
}
