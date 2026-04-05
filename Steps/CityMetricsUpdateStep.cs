using TangMo;
// ============================================================
// Steps/CityMetricsUpdateStep.cs — Step 4: 更新城市宏观指标
// ============================================================
using System;

namespace TangMo.Steps
{
    /// <summary>
    /// 更新城市宏观指标：
    /// - PeopleBurden = f(TaxRate, 徭役, 兵役)
    /// - MarketActivity 自然波动
    /// - Prosperity 受收入/支出影响
    /// </summary>
    public class CityMetricsUpdateStep : IWeeklyStep
    {
        public string Name => "城市指标更新";

        public void Execute(City city, World world)
        {
            // ── PeopleBurden 重新计算 ──
            // 基础负担 = 税率 + 兵力占比带来的徭役
            double corveeBurden = Math.Min(0.5, city.GarrisonStrength / 300.0); // 兵多徭重
            double taxBurden = city.TaxRate;
            double targetBurden = (taxBurden * 0.6 + corveeBurden * 0.4);
            // 平滑过渡（每帧变化不超过 0.05）
            city.PeopleBurden += (targetBurden - city.PeopleBurden) * 0.2;
            city.PeopleBurden = Math.Clamp(city.PeopleBurden, 0, 1);

            // ── MarketActivity 自然波动 ──
            double marketTarget = 0.3 + city.Prosperity * 0.5;
            city.MarketActivity += (marketTarget - city.MarketActivity) * 0.1;
            city.MarketActivity *= world.SeasonMarketMultiplier / 1.0; // 季节影响
            city.MarketActivity = Math.Clamp(city.MarketActivity, 0.05, 1.0);

            // ── Prosperity ──
            // 财政健康 → 繁荣上升；长期赤字 → 繁荣下降
            if (city.Treasury > 100)
                city.Prosperity = Math.Min(1.0, city.Prosperity + 0.005);
            else if (city.Treasury < 30)
                city.Prosperity = Math.Max(0, city.Prosperity - 0.01);

            // 恢复期：指标自然回升
            if (city.HasFlag(CityFlags.RECOVERY_MODE) && city.RecoveryWeeksLeft > 0)
            {
                city.Prosperity = Math.Min(1.0, city.Prosperity + 0.01);
                city.MarketActivity = Math.Min(1.0, city.MarketActivity + 0.008);
                city.RecoveryWeeksLeft--;
                if (city.RecoveryWeeksLeft <= 0)
                {
                    city.RemoveFlag(CityFlags.RECOVERY_MODE);
                    world.Log($"✓ {city.Name} 恢复期结束");
                }
            }

            // ── InstitutionPower 自然变化 ──
            double instTarget = world.InstitutionGain * 0.5 + city.InstitutionPower * 0.5;
            city.InstitutionPower += (instTarget - city.InstitutionPower) * 0.05;
            city.InstitutionPower = Math.Clamp(city.InstitutionPower, 0, 1);
        }
    }
}
