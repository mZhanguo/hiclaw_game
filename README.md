# 唐末藩镇家族治理与叛乱系统

唐末藩镇割据背景的单机策略/模拟游戏。

## 项目结构

```
├── Program.cs              # 入口，演示周循环模拟
├── TangMo.csproj           # .NET 项目文件
├── Models/                 # 数据模型
│   ├── City.cs             # 城市
│   ├── Family.cs           # 家族
│   ├── World.cs            # 世界状态
│   ├── Enums.cs            # 枚举定义
│   └── CityPolicy.cs       # 城市政策
├── Simulation/
│   └── WeeklyPipeline.cs   # 周循环流水线
├── Steps/                  # 周循环步骤
│   ├── IncomeCollectionStep.cs
│   ├── ExpensePaymentStep.cs
│   ├── PolicyExecutionStep.cs
│   ├── CityMetricsUpdateStep.cs
│   ├── FamilyShareUpdateStep.cs
│   ├── DominanceRecalcStep.cs
│   └── RebellionGateCheckStep.cs
├── Rebellion/
│   └── RebellionEpisode.cs # 叛乱日实例状态机
└── DESIGN_GDD_v1.1_MODIFIED.md  # 游戏设计文档
```

## 运行

```bash
dotnet run
```

## 设计文档

详见 `DESIGN_GDD_v1.1_MODIFIED.md`

## 当前进度

- [x] Phase 0: 基础数据结构 + 周循环模拟
- [ ] Phase 1: 叛乱日实例玩家交互
- [ ] Phase 2: 危机链 JSON 事件池
- [ ] Phase 3: 多城市联动
