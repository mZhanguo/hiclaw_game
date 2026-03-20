using TangMo;
// ============================================================
// Steps/FamilyShareUpdateStep.cs — Step 5: 更新家族状态与三份额
// ============================================================
using System;
using System.Linq;

namespace TangMo.Steps
{
    /// <summary>
    /// 更新家族状态：
    /// - 根据城市指标和家族类型，微调三份额
    /// - 更新 LoyaltyToLord / Grievance / Fear / Dependence
    /// </summary>
    public class FamilyShareUpdateStep : IWeeklyStep
    {
        public string Name => "家族份额更新";

        public void Execute(City city, World world)
        {
            if (!city.Families.Any()) return;

            foreach (var family in city.Families)
            {
                // ── 三份额自然漂移 ──
                // 军事家族：武力份额自然上升
                if (family.Type.HasFlag(FamilyType.MilitaryClan))
                {
                    family.Militia = Math.Min(1.0, family.Militia + 0.005);
                    // 欠饷时军事家族影响力上升更快
                    if (city.HasFlag(CityFlags.ARMY_PAY_ISSUE))
                        family.Militia = Math.Min(1.0, family.Militia + 0.008);
                }

                // 商贾家族：经济份额自然上升
                if (family.Type.HasFlag(FamilyType.MerchantClan))
                {
                    family.EconShare = Math.Min(1.0, family.EconShare + 0.006);
                    if (city.HasFlag(CityFlags.SALT_TAX_ISSUE))
                        family.EconShare = Math.Min(1.0, family.EconShare + 0.004);
                }

                // 士绅家族：民意份额上升
                if (family.Type.HasFlag(FamilyType.GentryClan))
                    family.Popularity = Math.Min(1.0, family.Popularity + 0.004);

                // 宗教家族：民意为主
                if (family.Type.HasFlag(FamilyType.ReligiousClan))
                    family.Popularity = Math.Min(1.0, family.Popularity + 0.006);

                // 乡老家族：民意微升
                if (family.Type.HasFlag(FamilyType.LocalElderClan))
                    family.Popularity = Math.Min(1.0, family.Popularity + 0.003);

                // 朝中关系家族：微弱全面加成
                if (family.Type.HasFlag(FamilyType.CourtTiedClan))
                {
                    family.EconShare = Math.Min(1.0, family.EconShare + 0.002);
                    family.Popularity = Math.Min(1.0, family.Popularity + 0.002);
                }

                // ── LoyaltyToLord 更新 ──
                double loyaltyDelta = 0;

                // 高民负 → 忠诚下降
                loyaltyDelta -= city.PeopleBurden * 0.02;

                // 欠饷 → 忠诚下降（军事家族更敏感）
                if (city.HasFlag(CityFlags.ARMY_PAY_ISSUE))
                {
                    double payHit = 0.03 * Math.Min(city.ArmyPayArrearsWeeks, 5);
                    if (family.Type.HasFlag(FamilyType.MilitaryClan))
                        payHit *= 1.5;
                    loyaltyDelta -= payHit;
                }

                // 高制度力 → 忠诚稳中有升
                loyaltyDelta += city.InstitutionPower * 0.01;

                // 朝中评价高 → 忠诚微升
                loyaltyDelta += world.CourtStanding * 0.005;

                // 恢复期：忠诚缓慢回升
                if (city.HasFlag(CityFlags.RECOVERY_MODE))
                    loyaltyDelta += 0.02;

                family.LoyaltyToLord = Math.Clamp(family.LoyaltyToLord + loyaltyDelta, 0, 1);

                // ── Grievance 更新 ──
                double grievanceDelta = 0;

                // 民负高 → 怨气上升
                grievanceDelta += city.PeopleBurden * 0.015;

                // 欠饷 → 怨气上升
                if (city.HasFlag(CityFlags.ARMY_PAY_ISSUE))
                    grievanceDelta += 0.02 * city.ArmyPayArrearsWeeks;

                // 高制度力 → 怨气消退更快
                grievanceDelta -= city.InstitutionPower * 0.01;

                // 恢复期：怨气自然消退
                if (city.HasFlag(CityFlags.RECOVERY_MODE))
                    grievanceDelta -= 0.025;

                family.Grievance = Math.Clamp(family.Grievance + grievanceDelta, 0, 1);

                // ── Fear 更新 ──
                // 高驻军 → 恐惧上升
                double fearTarget = Math.Min(0.8, city.GarrisonStrength / 200.0);
                family.Fear += (fearTarget - family.Fear) * 0.1;
                family.Fear = Math.Clamp(family.Fear, 0, 1);

                // ── Dependence 更新 ──
                // 高制度力 → 依赖度上升
                double depTarget = 0.3 + city.InstitutionPower * 0.5;
                family.Dependence += (depTarget - family.Dependence) * 0.05;
                family.Dependence = Math.Clamp(family.Dependence, 0, 1);
            }

            // ── 份额归一化（确保所有家族的每项份额之和 <= 1.0）──
            NormalizeShares(city);
        }

        private void NormalizeShares(City city)
        {
            var families = city.Families;
            if (!families.Any()) return;

            // EconShare 归一化
            double totalEcon = families.Sum(f => f.EconShare);
            if (totalEcon > 1.0)
            {
                double scale = 1.0 / totalEcon;
                foreach (var f in families) f.EconShare *= scale;
            }

            // Militia 归一化
            double totalMil = families.Sum(f => f.Militia);
            if (totalMil > 1.0)
            {
                double scale = 1.0 / totalMil;
                foreach (var f in families) f.Militia *= scale;
            }

            // Popularity 归一化
            double totalPop = families.Sum(f => f.Popularity);
            if (totalPop > 1.0)
            {
                double scale = 1.0 / totalPop;
                foreach (var f in families) f.Popularity *= scale;
            }
        }
    }
}
