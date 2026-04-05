// ============================================================
// CityPolicy.cs — 城市周政策（占位，Phase 0 简化版）
// ============================================================
namespace TangMo
{
    /// <summary>
    /// 城市政策：玩家可以设置的周度策略方向。
    /// Phase 0 简化为枚举，后续扩展为 JSON 驱动的动作系统。
    /// </summary>
    public enum PolicyFocus
    {
        Balanced,           // 均衡发展
        EconomicGrowth,     // 重商（提升市场活动）
        MilitaryBuildup,    // 重兵（提升驻军）
        PeopleRelief,       // 赈济（降低民负）
        InstitutionStrengthen, // 强化制度
    }

    public class CityPolicy
    {
        public PolicyFocus Focus { get; set; } = PolicyFocus.Balanced;
    }
}
