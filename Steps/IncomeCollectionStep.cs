using TangMo;
// ============================================================
// Steps/IncomeCollectionStep.cs — Step 1: 收入结算
// ============================================================
using System;

namespace TangMo.Steps
{
    /// <summary>
    /// 收入结算：
    /// - 田赋 = AgriculturalYield × TaxRate × 履行率 × 季节乘数
    /// - 盐铁/商税 = MarketActivity × 基础值 × SaltTaxRate × 季节乘数
    /// </summary>
    public class IncomeCollectionStep : IWeeklyStep
    {
        public string Name => "收入结算";

        public void Execute(City city, World world)
        {
            // 履行率：PeopleBurden 越高，百姓越不堪重负，实际缴纳率越低
            double complianceRate = Math.Max(0.2, 1.0 - city.PeopleBurden * 0.8);

            // 田赋收入
            double landTax = city.AgriculturalYield
                           * city.TaxRate
                           * complianceRate
                           * world.SeasonYieldMultiplier;

            // 盐铁/商税收入
            double baseTrade = 40.0; // 基础贸易收入
            double tradeTax = city.MarketActivity
                            * baseTrade
                            * city.SaltTaxRate
                            * world.SeasonMarketMultiplier;

            // 私盐盛行则盐税打折
            if (city.HasFlag(CityFlags.PRIVATE_SALT_RAMPANT))
                tradeTax *= 0.5;

            double totalIncome = landTax + tradeTax;
            city.Treasury += totalIncome;
            city.GranaryGrain += landTax * 0.3; // 部分收入转化为粮食

            world.Log($"{city.Name} 收入：田赋={landTax:F1} 商税={tradeTax:F1} (合规率={complianceRate:F2})");
        }
    }
}
