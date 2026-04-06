using TangMo;
// ============================================================
// Steps/PolicyExecutionStep.cs — Step 3: 执行周政策
// ============================================================
using System;

namespace TangMo.Steps
{
    /// <summary>
    /// 执行周政策：
    /// 根据城市当前政策方向，微调各项指标。
    /// Phase 0 简化为固定效果，后续扩展为 JSON 驱动。
    /// </summary>
    public class PolicyExecutionStep : IWeeklyStep
    {
        public string Name => "政策执行";

        public void Execute(City city, World world)
        {
            var focus = PolicyFocus.Balanced; // Phase 0 固定均衡

            switch (focus)
            {
                case PolicyFocus.EconomicGrowth:
                    city.MarketActivity = Math.Min(1.0, city.MarketActivity + 0.02);
                    city.PeopleBurden = Math.Min(1.0, city.PeopleBurden + 0.01);
                    break;

                case PolicyFocus.MilitaryBuildup:
                    city.GarrisonStrength += 2;
                    city.Treasury -= 10;
                    break;

                case PolicyFocus.PeopleRelief:
                    city.PeopleBurden = Math.Max(0, city.PeopleBurden - 0.03);
                    city.Treasury -= 15;
                    break;

                case PolicyFocus.InstitutionStrengthen:
                    city.InstitutionPower = Math.Min(1.0, city.InstitutionPower + 0.01);
                    break;

                case PolicyFocus.Balanced:
                default:
                    // 均衡：小幅自然恢复
                    city.PeopleBurden = Math.Max(0, city.PeopleBurden - 0.005);
                    break;
            }
        }
    }
}
