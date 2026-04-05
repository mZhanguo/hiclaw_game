using TangMo;
// ============================================================
// Steps/IWeeklyStep.cs — 周步骤接口
// ============================================================
namespace TangMo.Steps
{
    /// <summary>
    /// 周流水线中的一个步骤。
    /// 每个步骤接收城市和世界状态，执行即时修改。
    /// </summary>
    public interface IWeeklyStep
    {
        /// <summary>步骤名称（用于日志）</summary>
        string Name { get; }

        /// <summary>执行此步骤</summary>
        void Execute(City city, World world);
    }
}
