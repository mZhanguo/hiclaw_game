using TangMo;
// ============================================================
// Steps/DominanceRecalcStep.cs — Step 6: 计算权力与 Dominance
// ============================================================
using System;
using System.Linq;

namespace TangMo.Steps
{
    /// <summary>
    /// 重新计算所有家族的 Power 和 Dominance。
    /// - Power = 0.35*Econ + 0.45*Militia + 0.20*Popularity
    /// - Dominance = Power / sum(Power_all)
    /// - EffectiveDominance = min(Dominance, 1.0 - 0.6*InstitutionPower)
    /// </summary>
    public class DominanceRecalcStep : IWeeklyStep
    {
        public string Name => "Dominance 重算";

        public void Execute(City city, World world)
        {
            if (!city.Families.Any()) return;

            // Power 已经是计算属性，这里只需要算 Dominance
            double totalPower = city.Families.Sum(f => f.Power);

            if (totalPower <= 0)
            {
                // 兜底：均分
                double share = 1.0 / city.Families.Count;
                foreach (var f in city.Families)
                    f.Dominance = share;
            }
            else
            {
                foreach (var f in city.Families)
                    f.Dominance = f.Power / totalPower;
            }

            // 检查主导家族
            var dominant = city.GetDominantFamily();
            if (dominant != null)
            {
                bool isMonopoly = city.Families.Count == 1 || dominant.Dominance >= 0.90;
                string status = isMonopoly ? "单家族占有态" : "主导";
                // 仅在变化时记日志（减少噪音）
                if (dominant.Dominance >= 0.60)
                {
                    // 静默记录，不每帧刷屏
                }
            }
        }
    }
}
