using TangMo;
// ============================================================
// Simulation/WeeklyPipeline.cs — 周流水线
// ============================================================
using System;
using System.Collections.Generic;
using TangMo.Steps;

namespace TangMo.Simulation
{
    /// <summary>
    /// 周流水线：按固定顺序执行 7 个步骤，完成一座城市的一周结算。
    /// </summary>
    public class WeeklyPipeline
    {
        private readonly List<IWeeklyStep> _steps = new()
        {
            new IncomeCollectionStep(),
            new ExpensePaymentStep(),
            new PolicyExecutionStep(),
            new CityMetricsUpdateStep(),
            new FamilyShareUpdateStep(),
            new DominanceRecalcStep(),
            new RebellionGateCheckStep(),
        };

        /// <summary>
        /// 执行一座城市的周结算流水线。
        /// </summary>
        public void Run(City city, World world)
        {
            // 跳过非玩家控制的城市
            if (!city.IsPlayerControlled) return;

            // 日实例期间冻结周结算
            if (city.HasFlag(CityFlags.REBELLION_ACTIVE)) return;

            foreach (var step in _steps)
            {
                step.Execute(city, world);
            }
        }

        /// <summary>
        /// 运行所有玩家城市的周结算。
        /// </summary>
        public void RunAll(World world)
        {
            foreach (var city in world.PlayerCities)
            {
                Run(city, world);
            }
        }
    }
}
