// ============================================================
// City.cs — 城市数据模型
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace TangMo
{
    /// <summary>
    /// 城市：藩镇治理的基本地理单位。
    /// 包含资源、军事、治理指标，以及驻扎的家族列表。
    /// </summary>
    public class City
    {
        // ── 标识 ──
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public CityType Type { get; set; } = CityType.None;
        public CityController Controller { get; set; } = CityController.Player;
        public HashSet<KeyFacility> Facilities { get; set; } = new();

        // ── 资源 ──
        public double GranaryGrain  { get; set; } = 500.0;  // 粮
        public double Treasury      { get; set; } = 300.0;  // 钱

        // ── 发展 ──
        public double AgriculturalYield { get; set; } = 50.0;   // 田赋产出/周
        public double Prosperity        { get; set; } = 0.50;   // 繁荣度 ∈ [0,1]
        public double MarketActivity    { get; set; } = 0.40;   // 市场活跃度 ∈ [0,1]

        // ── 军事 ──
        public double GarrisonStrength  { get; set; } = 100.0;  // 驻军兵力
        public double GarrisonMorale    { get; set; } = 0.70;   // 士气 ∈ [0,1]
        public int    ArmyPayArrearsWeeks { get; set; } = 0;    // 欠饷周数

        // ── 治理 ──
        public double PeopleBurden      { get; set; } = 0.30;   // 民负 ∈ [0,1]
        public double InstitutionPower  { get; set; } = 0.50;   // 制度力 ∈ [0,1]
        public double Unrest            { get; set; } = 0.0;    // 不安定度 ∈ [0,1]

        // ── 政策 ──
        public double TaxRate           { get; set; } = 0.30;   // 税率 ∈ [0,1]
        public double SaltTaxRate       { get; set; } = 0.20;   // 盐税率 ∈ [0,1]
        public double MilitaryPriority  { get; set; } = 0.50;   // 军费优先比例 ∈ [0,1]

        // ── 标记 ──
        public CityFlags Flags { get; set; } = CityFlags.None;

        // ── 家族 ──
        public List<Family> Families { get; set; } = new();

        // ── 恢复期 ──
        public int RecoveryWeeksLeft { get; set; } = 0;

        // ── 计算属性 ──

        /// <summary>是否为玩家控制</summary>
        public bool IsPlayerControlled => Controller == CityController.Player;

        /// <summary>是否有某个特定 Flag</summary>
        public bool HasFlag(CityFlags flag) => (Flags & flag) != 0;

        /// <summary>添加 Flag</summary>
        public void AddFlag(CityFlags flag) => Flags |= flag;

        /// <summary>移除 Flag</summary>
        public void RemoveFlag(CityFlags flag) => Flags &= ~flag;

        /// <summary>计算每周军饷需求</summary>
        public double WeeklyArmyPayNeeded => GarrisonStrength * 0.5;

        /// <summary>获取主导家族</summary>
        public Family? GetDominantFamily() => Families.FirstOrDefault(f => f.Dominance >= 0.60);

        /// <summary>健康度评估（用于第一层 UI）</summary>
        public HealthStatus EconomicHealth =>
            Treasury > 100 ? HealthStatus.Green :
            Treasury > 0   ? HealthStatus.Yellow : HealthStatus.Red;

        public HealthStatus MilitaryHealth =>
            ArmyPayArrearsWeeks == 0 ? HealthStatus.Green :
            ArmyPayArrearsWeeks <= 2 ? HealthStatus.Yellow : HealthStatus.Red;

        public HealthStatus PopulaceHealth =>
            Unrest < 0.30 ? HealthStatus.Green :
            Unrest < 0.55 ? HealthStatus.Yellow : HealthStatus.Red;

        public HealthStatus FamilyHealth
        {
            get
            {
                if (!Families.Any()) return HealthStatus.Green;
                var maxDom = Families.Max(f => f.Dominance);
                if (maxDom >= 0.60) return HealthStatus.Red;
                if (Families.Any(f => f.LoyaltyToLord < 0.30)) return HealthStatus.Yellow;
                return HealthStatus.Green;
            }
        }

        public override string ToString() =>
            $"[{Id}] {Name} (粮={GranaryGrain:F0}, 钱={Treasury:F0}, " +
            $"兵力={GarrisonStrength:F0}, Unrest={Unrest:F2})";
    }

    public enum HealthStatus { Green, Yellow, Red }
}
