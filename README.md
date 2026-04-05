# 🏛️ 官途浮沉 (GuanTu FuCheng)

> **一款融合Roguelite元素的回合制古代官场策略卡牌游戏**
> 
> 从一介进士，到位极人臣。每一次选择，都是权谋与道义的博弈。

---

## 📋 项目概览

- **引擎：** Unity 2022 LTS+
- **语言：** C# (.NET Standard 2.1)
- **类型：** 回合制策略 × 卡牌 × Roguelite
- **平台：** PC / 移动端
- **状态：** 🏗️ 架构搭建阶段

---

## 🎮 核心玩法

```
每局游戏 = 一次仕途人生

开局：刚中进士，候补等官
目标：官至宰相，位极人臣
失败：被贬、被杀、流放……但积累"官运"，永久变强
```

### 回合循环（每回合 = 一个季度）

```
┌─────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  早朝简报    │ ──→ │  行动分配     │ ──→ │  执行结算     │ ──→ │  复盘总结     │
│ 局势/事件/NPC│     │ 6点行动力     │     │ 触发卡牌/事件 │     │ 升迁检查     │
└─────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
                            │
                    ┌───────┼───────┐
                    ▼       ▼       ▼
                  政务    交际    谋略 ...
```

### 双维度NPC关系

```
              高信任
                │
    敬而远之    │    铁杆盟友
                │
  低好感 ──────┼────── 高好感
                │
    宿敌        │    酒肉朋友
                │
              低信任
```

### 升迁路线

```
候补 → 县令(正七品) → 州刺史(从四品) → 侍郎(正四品)
                                            │
                                    ┌───────┴───────┐
                                    ▼               ▼
                              尚书(正三品)     节度使(从二品)
                               [文官路线]      [武将路线]
                                    └───────┬───────┘
                                            ▼
                                      宰相(正二品) 🏆
```

---

## 📁 项目结构

```
Assets/
├── Scripts/
│   ├── Core/                       # 核心框架
│   │   ├── Singleton.cs            # 泛型单例基类
│   │   ├── GameManager.cs          # 游戏主管理器（生命周期、状态机）
│   │   └── TurnManager.cs          # 回合管理器（四阶段流程控制）
│   │
│   ├── Systems/                    # 游戏子系统
│   │   ├── ActionPointSystem.cs    # 行动力系统（6点分配与执行）
│   │   ├── CardSystem.cs           # 卡牌系统（卡池、抽取、选项、结算）
│   │   ├── NPCRelationshipGraph.cs # NPC关系图谱（好感/信任双维度）
│   │   ├── OfficialRankSystem.cs   # 官职升迁系统（晋升路线与条件）
│   │   ├── RogueliteMetaSystem.cs  # Roguelite Meta系统（跨局永久进度）
│   │   └── SaveSystem.cs           # 存档系统（本地JSON持久化）
│   │
│   ├── Data/                       # ScriptableObject数据定义
│   │   ├── CardData.cs             # 卡牌数据模板
│   │   ├── NPCData.cs              # NPC数据模板
│   │   ├── OfficialRankData.cs     # 官职数据模板
│   │   └── EventData.cs            # 事件数据模板
│   │
│   ├── Models/                     # 运行时数据模型
│   │   ├── GameStateModel.cs       # 单局状态（PlayerState、RunState）
│   │   └── MetaProgress.cs         # Meta永久进度数据
│   │
│   ├── Enums/                      # 枚举定义
│   │   └── GameEnums.cs            # 全局枚举（状态、阶段、类型等）
│   │
│   ├── Events/                     # 事件系统
│   │   └── GameEvents.cs           # 全局事件总线（观察者模式）
│   │
│   └── Utils/                      # 工具类（待扩展）
│
├── ScriptableObjects/              # 策划配置的数据资产
│   ├── Cards/                      # 卡牌配置
│   ├── NPCs/                       # NPC配置
│   ├── Ranks/                      # 官职配置
│   └── Events/                     # 事件配置
│
├── Prefabs/                        # 预制体
│   ├── UI/
│   ├── Characters/
│   └── Effects/
│
├── Scenes/                         # 场景文件
├── Art/                            # 美术资源
│   ├── Sprites/
│   ├── UI/
│   └── Backgrounds/
├── Audio/                          # 音频资源
│   ├── BGM/
│   └── SFX/
├── UI/                             # UI资源
│   ├── Fonts/
│   └── Atlas/
└── Resources/                      # 运行时加载资源
    ├── Config/
    └── Localization/
```

---

## 🏗️ 技术架构

### 设计原则

1. **单例 + 事件总线**：各系统通过 `Singleton<T>` 确保唯一实例，通过 `GameEvents` 事件解耦通信
2. **数据驱动**：所有游戏内容（卡牌、NPC、官职）通过 ScriptableObject 配置，策划无需改代码
3. **状态分离**：静态数据（ScriptableObject）与运行时状态（Model类）严格分离
4. **Roguelite双层存档**：单局存档（死亡清除）+ Meta进度（永久保存）

### 系统依赖关系

```
GameManager（顶层调度）
    ├── TurnManager（回合流程）
    │       ├── ActionPointSystem（行动执行）
    │       ├── CardSystem（卡牌触发）
    │       └── OfficialRankSystem（升迁检查）
    ├── NPCRelationshipGraph（NPC关系）
    ├── RogueliteMetaSystem（永久进度）
    └── SaveSystem（持久化）

通信方式：GameEvents 事件总线（单向依赖，系统间不直接引用）
```

### 数据流

```
ScriptableObject（策划配置）
        │
        ▼
System 读取配置 → 生成运行时 Model → 修改 Model → 触发 Event
        │                                              │
        ▼                                              ▼
    SaveSystem 序列化 Model                    UI 监听 Event 更新显示
```

---

## 🔑 核心系统说明

### GameManager
- 游戏生命周期管理（菜单→游戏→暂停→结算）
- 持有 `RunState`（当前局数据）和 `MetaProgressData`（永久进度）
- 协调子系统初始化，不处理具体逻辑

### TurnManager
- 严格的四阶段回合制：早朝→分配→执行→复盘
- 每阶段由 UI 确认后推进，保证玩家操作节奏
- 回合结束检查胜负条件

### ActionPointSystem
- 6点行动力（可通过升级和官职增加）
- 6种行动方向，投入越多效果越强
- 行动结果 = 基础效果 × 属性加成 × 随机因子

### CardSystem
- 加权随机抽牌（稀有度影响概率）
- 条件卡池（根据回合、官职、剧情标记筛选）
- 选项分支 → 属性变化 + NPC关系变化 + 剧情标记 + 后续卡牌链

### NPCRelationshipGraph
- 好感度 × 信任度 双维度独立系统
- 关系里程碑（自动触发特殊事件）
- NPC间关系网络（阵营政治）
- 好感自然衰减（需要持续经营）

### OfficialRankSystem
- 升迁条件：在位时间 + 属性 + 声望 + 影响力
- 侍郎之后分叉：文官（尚书）vs 武将（节度使）
- 弹劾机制：声望低 + 政敌多 = 被弹劾风险高

### RogueliteMetaSystem
- "官运"点数：每局结算获得，永久货币
- 8种永久升级：属性加成、行动力、卡池扩展等
- 成就系统：达成条件解锁额外奖励

### SaveSystem
- 单局存档：JSON序列化，每回合自动保存，死亡删除（防SL）
- Meta存档：独立文件，永久保存
- 备份/导出/导入功能

---

## 🗺️ 开发路线图

### Phase 1 — 架构搭建 ✅（当前）
- [x] 项目目录结构
- [x] 核心系统代码框架
- [x] 数据模型定义
- [x] 事件总线
- [x] 技术文档

### Phase 2 — 原型验证
- [ ] 在Unity编辑器中创建基础场景
- [ ] 实现最小可玩回合循环（纯文本UI）
- [ ] 配置3-5张测试卡牌
- [ ] 配置2-3个测试NPC
- [ ] 实现存档读写

### Phase 3 — 核心玩法
- [ ] UI框架搭建（Unity UI Toolkit / UGUI）
- [ ] 完善卡牌系统（20+张卡牌）
- [ ] 完善NPC系统（8+个NPC）
- [ ] 升迁全流程打通
- [ ] Meta永久升级商店

### Phase 4 — 内容填充
- [ ] 50+张卡牌
- [ ] 完整NPC关系网
- [ ] 音效/BGM
- [ ] 美术资源接入
- [ ] 多结局系统

### Phase 5 — 打磨发布
- [ ] 平衡性调优
- [ ] 本地化
- [ ] 移动端适配
- [ ] 测试 & Bug修复
- [ ] 发布

---

## 🛠️ 开发环境

- **Unity：** 2022.3 LTS 或更新版本
- **IDE：** Visual Studio / Rider
- **版本控制：** Git（建议使用 Git LFS 管理大文件）

### 在Unity中打开

1. 安装 Unity 2022.3 LTS
2. 打开 Unity Hub → Add → 选择本项目根目录
3. 等待导入完成
4. 打开 `Assets/Scenes/` 中的场景文件开始开发

---

*官途浮沉 — SCDC LLC © 2026*
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
