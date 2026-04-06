// ============================================================
// Family.cs — 家族数据模型
// ============================================================
using System;

namespace TangMo
{
    /// <summary>
    /// 家族：游戏中最基本的权力单位。
    /// 由族长代表对外，在城市中争夺经济、武力、民意三份额。
    /// </summary>
    public class Family
    {
        // ── 标识 ──
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public FamilyType Type { get; set; } = FamilyType.None;

        // ── 忠诚与情绪（均 ∈ [0,1]）──
        public double LoyaltyToLord { get; set; } = 0.5;  // 对领主忠诚
        public double Grievance     { get; set; } = 0.0;  // 怨气
        public double Fear          { get; set; } = 0.0;  // 恐惧
        public double Dependence    { get; set; } = 0.5;  // 依赖度

        // ── 三份额（均 ∈ [0,1] 归一化值）──
        public double EconShare   { get; set; } = 0.0;  // 经济份额
        public double Militia     { get; set; } = 0.0;  // 武力份额
        public double Popularity  { get; set; } = 0.0;  // 民意份额

        // ── 计算属性 ──

        /// <summary>
        /// Power = 0.35 * Econ + 0.45 * Militia + 0.20 * Popularity
        /// </summary>
        public double Power =>
            0.35 * EconShare + 0.45 * Militia + 0.20 * Popularity;

        /// <summary>
        /// Dominance = Power / sum(Power_allFamilies)，∈ [0,1]
        /// 由外部计算后写入（需要知道所有家族的 Power 总和）。
        /// </summary>
        public double Dominance { get; set; } = 0.0;

        /// <summary>
        /// EffectiveDominance = min(Dominance, cap)
        /// cap = 1.0 - 0.6 * InstitutionPower
        /// </summary>
        public double GetEffectiveDominance(double institutionPower)
        {
            double cap = 1.0 - 0.6 * institutionPower;
            return Math.Min(Dominance, cap);
        }

        /// <summary>是否为主导家族（Dominance >= 0.60）</summary>
        public bool IsDominant => Dominance >= 0.60;

        /// <summary>外部支持（∈ [0,1]）— 用于 Unrest 计算</summary>
        public double ExternalSupport { get; set; } = 0.0;

        // ── 辅助方法 ──

        public Family Clone() => new()
        {
            Id = Id, Name = Name, Type = Type,
            LoyaltyToLord = LoyaltyToLord,
            Grievance = Grievance,
            Fear = Fear,
            Dependence = Dependence,
            EconShare = EconShare,
            Militia = Militia,
            Popularity = Popularity,
            Dominance = Dominance,
            ExternalSupport = ExternalSupport,
        };

        public override string ToString() =>
            $"[{Id}] {Name} (Power={Power:F2}, Dom={Dominance:F2}, Loyalty={LoyaltyToLord:F2})";
    }
}
