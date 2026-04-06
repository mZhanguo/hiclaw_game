# 📋 GDD 修改版（v1.1）— 整合 Product + Tech 修改建议

> **修改说明**：本版本在原始 GDD 基础上整合了 Product（产品）和 Tech（技术）的修改建议。
> 所有 **【新增】** 和 **【修改】** 标记的内容是相对于原文的变更。
> **修改者**：小爱（Manager）统一整合
> **日期**：2026-03-20

---

## 游戏设计总览：唐末藩镇家族治理与叛乱系统

### 1. 基本设定
- **题材**：唐末藩镇割据背景的单机策略 / 模拟游戏。
- **玩家身份**：一方节度使（或观察使），实际统治一个州府及其周边郡县、军镇。
- **核心体验**：
  - 以**家族为单位**治理人口与权力结构（族长代表）。
  - 在吃饭（田赋/盐铁）、养兵（军费/兵心）、应付朝廷与邻镇之间做平衡。
  - 高自由度权谋：联姻、授职、授盐引、抄家、调防、练兵等操作影响城市控制与叛乱。

#### 【新增】1.1 设计哲学 — Product P0

本游戏不设传统胜负条件。玩家的乐趣来源于在唐末乱世中扮演节度使，体验权力博弈、家族兴衰与历史洪流中的个人抉择。每局游戏是一段独特的故事，不是一道待解的最优解。

**核心体验锚点**：每周打开游戏，玩家应该期待"这周又会出什么幺蛾子"，而不是"这周我该怎么 min-max"。

系统不评判玩家的"好坏"，只呈现决策的因果链。你可以做一个仁慈但软弱的节度使，也可以做一个残暴但稳固的节度使——游戏的乐趣在于看到这些选择如何塑造你的藩镇故事。

---

### 2. 时间结构
- **周推进（战略层）**：
  - 1 回合 = 1 周。
  - 用于结算资源（税收/军饷/徭役）、执行经济/军事政策、更新家族权力份额与忠诚、判定是否进入关键危机。
- **日推进（关键事件层）**：
  - 当触发叛乱等关键事件时，进入一个持续数日到数周的"日更实例"（RebellionEpisode）。
  - 玩家每天有若干行动点（AP），使用权谋与应对手段处理危机。
  - 结束后将结果写回周层世界状态。

---

### 3. 世界与城市结构
- **世界（World）关键数值**：
  - `Week`、`Season`（春夏秋冬循环）。
  - `Legitimacy`（名义合法性）、`Prestige`（威望）、`CourtStanding`（朝中评价）、`BorderThreat`（边患压力）。
  - 难度/环境参数：`InstitutionGain`、`ClanCoalitionPower`、`RevoltThreshold`、`RevoltEscalationRate`、`IntelClarity`、`PurgeBacklash`。

- **城市（City）类型标签**（可叠加）：
  - `ProvincialCapital`：州府 / 节度使治所。
  - `MilitaryGarrisonTown`：军镇 / 关隘城。
  - `TradeHubSalt`：盐铁城市。
  - `TradeHubTeaHorse`：茶马互市城。
  - `RiverPortCity`：漕运 / 河港城。
  - `AgriculturalCountyTown`：普通县城。
  - `ReligiousCenter`：名寺 / 名山 / 大观所在地。

- **关键设施（KeyFacility）**：
  - 粮与军：`ChangpingGranary`（常平仓）、`ArmyGrainDepot`（军粮库）、`Armory`（军械库）。
  - 税与市：`SaltIronOffice`（盐铁务）、`MarketEast/MarketWest`（东西市）、`RiverPortDock`（漕运码头）。
  - 权力中枢：`YamenGovernmentHall`（州府/县衙公堂）、`JiedushiResidence`（帅府）。

- **城市核心数值**：
  - 资源：`GranaryGrain`（粮）、`Treasury`（钱）。
  - 发展：`AgriculturalYield`、`Prosperity`、`MarketActivity`。
  - 军事：`GarrisonStrength`、`GarrisonMorale`、`ArmyPayArrearsWeeks`（欠饷周数）。
  - 治理：`PeopleBurden`（民负）、`InstitutionPower`（制度力）。
  - 标志：若干 `Flags` 表示事件状态（如 `ARMY_PAY_ISSUE`、`SALT_TAX_ISSUE` 等）。

#### 【新增】3.1 信息分层展示 — Product P2

**第一层（默认视图）**：红黄绿健康度
- 经济 💰：绿（盈余）/ 黄（持平）/ 红（赤字）
- 军事 ⚔️：绿（满饷满编）/ 黄（欠饷 1-2 周）/ 红（欠饷 3+ 周或兵变风险）
- 民心 👥：绿（安居）/ 黄（怨声）/ 红（民变风险）
- 家族 🏯：绿（忠诚稳定）/ 黄（有家族不满）/ 红（有家族 Dominance 过高）

**第二层（悬停/点击展开）**：关键数值
- 显示 GranaryGrain、Treasury、ArmyPayArrearsWeeks、最危险家族的 Loyalty/Dominance

**第三层（详细面板）**：完整数值表
- 所有家族的三份额、四情绪、Power、Dominance
- 所有城市指标的精确数值

---

### 4. 家族系统
- **基本单位**：`Family`（家族），由 `族长` 代表对外。
- **家族类型（FamilyType，可多选）**：
  - `MilitaryClan`：牙将家族 / 军镇将门。
  - `MerchantClan`：盐商 / 茶马 / 粮商。
  - `GentryClan`：士绅 / 幕僚 / 大族。
  - `ReligiousClan`：寺院 / 道观附属家族。
  - `LocalElderClan`：乡老 / 里正家族。
  - `CourtTiedClan`：与宦官 / 权臣 / 宗室有门生或姻亲关系。

- **家族关键属性**：
  - 忠诚与情绪：`LoyaltyToLord`、`Grievance`、`Fear`、`Dependence`。
  - 三份额（城市权力来源，**以下三项均为 [0,1] 归一化值**）：
    - `EconShare`：经济份额（产业 / 税契 / 商路）。
    - `Militia`：武力份额（私兵 / 团练 / 牙兵影响力）。
    - `Popularity`：民意份额（民望 / 香火 / 乡约影响）。

#### 【修改】4.1 计算属性 — Tech P1

- **Power**（以下三份额均为 [0,1] 归一化值）：
  - `Power = 0.35 * Econ + 0.45 * Militia + 0.20 * Popularity`
- **Dominance**：
  - `Dominance = Power / sum(Power_allFamilies)`，结果 ∈ [0,1]
- **EffectiveDominance**：
  - `EffectiveDominance = min(Dominance, cap)`
  - `cap = 1.0 - 0.6 * InstitutionPower`，其中 `InstitutionPower ∈ [0,1]`
  - 制度力为 0 时 cap=1.0（无压制），制度力为 1 时 cap=0.4（强力压制主导家族）

- **主导家族与单家族占有态**：
  - 主导家族：`Dominance >= 0.60`。
  - 单家族占有态：城市只有一个家族，或 `Dominance >= 0.90`。

#### 【新增】4.2 家族数量约束 — Tech P2

每座城市建议家族数量 3-8 个，硬上限 10 个。超过上限时，低 Power 家族自动合并或退出城市。

---

### 5. 周推进：经营与风险流程
对每座由玩家控制的城市，每周执行：

1. **收入结算**：
   - 田赋：`AgriculturalYield` × 税率 × 从 `PeopleBurden` 推导的履行率。
   - 盐铁 / 商税：`MarketActivity` × 城市类型与盐税政策 × 是否有私盐/封港标志。
2. **支出结算**：
   - 军饷：由 `GarrisonStrength` 推导应发额，按 `MilitaryPriority` 决定优先支付比例。
     - 支付不足 → `ArmyPayArrearsWeeks++`，`GarrisonMorale` 下降，标记 `ARMY_PAY_ISSUE`。
   - 官俸与维护：若支付不足，标记 `OFFICE_PARALYZED_RISK`。
3. **执行周政策 / 动作**：
   - 经济类：授盐引、盐税减免、私盐严打、榷场整顿、抄家罚没、赈济平粜等。
   - 军事类：常设练兵、精兵减众、调防换将、增修城防、建立团练、调军粮赴边等。
4. **更新城市宏观指标**：
   - 重新计算 `PeopleBurden`（税+徭役+兵）、`MarketActivity`、`Prosperity`。
5. **更新家族状态与三份额**：
   - 根据城市指标与家族类型，更新各家族的 `EconShare`、`Militia`、`Popularity`。
   - 根据 `PeopleBurden`、`InstitutionPower`、`CourtStanding`、欠饷、税制等更新 `LoyaltyToLord` / `Grievance`。
6. **计算权力与叛乱风险**：
   - 按 `Power` 和 `Dominance` 计算主导家族。

#### 【修改】5.1 Unrest 公式（归一化版）— Tech P1

**Unrest 计算公式**（所有变量 ∈ [0,1]）：

```
Unrest = 0.45*(1 - LoyaltyToLord) + 0.35*Grievance + 0.25*EffectiveDominance + 0.20*ExternalSupport - 0.50*InstitutionPower + shock
```

**shock 量化表**（shock ∈ [0, 0.35]）：

| Flag | shock 值 | 说明 |
|------|----------|------|
| ARMY_PAY_ISSUE | +0.05 × min(ArmyPayArrearsWeeks, 3) | 欠饷 1 周=0.05，2 周=0.10，3 周+=0.15 |
| SALT_TAX_ISSUE | +0.05 ~ +0.10 | 根据盐税负担程度线性计算 |
| OFFICE_PARALYZED_RISK | +0.10 | 固定值 |
| 无 Flag | 0 | — |

**叛乱触发阈值**：`Unrest >= 0.70 * RevoltThreshold`
- 默认 `RevoltThreshold = 1.0`，即 Unrest ≥ 0.70 时触发

#### 【新增】5.2 时序与边界规则 — Tech P1

- **周内执行顺序**：收入 → 支出 → 政策 → 城市指标 → 家族份额 → Dominance → Unrest，同周内顺序执行、即时生效。本周的份额变化在当周就影响 Unrest 判定。
- **日实例暂停**：任何城市进入 RebellionEpisode 时，周层暂停推进。日实例结束后，结果写回城市/家族状态，然后继续下一周。
- **多城并发**：如同一周内多座城市同时达到叛乱阈值，按各城最大家族 Dominance 降序依次处理，不允许并行。
- **禁止嵌套**：日实例期间不允许触发新的 RebellionEpisode。日实例内的事件不触发周层结算。

---

### 6. 关键危机链
目前已设计两条典型唐末危机链，均通过日事件池驱动：

#### 【新增】6.0 危机链叙事设计原则 — Product P1

每条危机链不仅是机制流程，更是一个玩家应体验到的情感故事。系统应在每个阶段给玩家明确的情感信号。

---

1. **欠饷兵变链（ARMY_PAY_MUTINY_CHAIN）**：

   **叙事目标**（Product）：玩家应该经历"发现问题（传言）→ 尝试压住（小动作）→ 失控（兵变）→ 做出艰难选择"的情感曲线。核心戏剧冲突是"钱不够，但兵要吃饭"——玩家必须在"搜刮百姓"和"纵容军阀"之间选择。

   - 触发条件：`ArmyPayArrearsWeeks >= 2` + 军镇 / 州府。
   - 日事件阶段：
     - RUMOR：军饷积欠传言、营中议劫。
     - CABAL：偏将观望、营中械斗、他镇招抚传言。
     - REVOLT：劫军粮、兵杀军官、牙兵占门。
   - 解决动作：重金抚军、提拔心腹将、调防换将等。

2. **盐引风波链（SALT_PERMIT_TURMOIL_CHAIN）**：

   **叙事目标**（Product）：玩家应该体验到"利益集团博弈"的复杂感。核心戏剧冲突是"盐利是财政命脉，但所有人都在抢这块肉"——玩家无论怎么分配都会得罪一方。镇压之后问题不会真正消失，盐利分配的矛盾始终存在。

   - 触发条件：盐城/河港 + 高盐税/私盐盛行。
   - 日事件阶段：
     - RUMOR：盐税骤增、盐商会合。
     - CABAL：会馆联名上书、私盐大盛、资助传闻。
     - REVOLT：夺盐仓、商帮募勇、封锁河港。
   - 解决动作：盐税减免、许以盐引、严打私盐、抄家罚没等。

---

### 7. 叛乱日实例（RebellionEpisode）概览

#### 【修改】7.0 叛乱的本质 — Product P0

叛乱不是游戏失败，而是一个高张力的决策回合。玩家面对的不是"能不能活下来"，而是"要付出什么代价来处理这个局面"。

#### 【修改】7.1 阶段状态机 — Tech P1（Phase 1 极简版）

**Phase 1 实现阶段**：

| 阶段 | 说明 |
|------|------|
| SPARK | 叛乱爆发。展示叛乱方实力、参与家族、当前局势。玩家获得一次决策机会。 |
| RESOLUTION | 根据实力对比和玩家选择，确定结局。 |

**Phase 2 扩展**（后续版本）：补全 SPARK → RUMOR → CABAL → REVOLT → AFTERMATH 五阶段，加入每日事件抽取、AP 多轮博弈、Aftermath 清算流程。

#### 【修改】7.2 关键数值 — Tech P1

- `RebelMomentum`（叛乱势能，∈ [0,1]）
- `LordControl`（领主控制力，∈ [0,1]）
- `ExternalSupport`、`LeaderFamilyId`、`CoalitionFamilyIds[]`

#### 【修改】7.3 实力对比与玩家抉择 — Product P0 + Tech P1

**实力对比计算**：
- `LordPower = City.GarrisonStrength × (1 + 0.5 × LordLoyaltyAvg)`
- `RebelPower = Σ(rebel_family.Militia × rebel_family.Dominance) × (1 + ExternalSupport)`

**玩家在 SPARK 阶段的选择**（消耗 AP）：

| 实力关系 | 可选策略 | 结局 | 写回周层效果 |
|----------|----------|------|-------------|
| LordPower > RebelPower × 1.2 | **武力镇压** | 镇压成功 | 叛乱家族 Loyalty↓、Militia↓；己方兵力损耗 20%-40%；GarrisonMorale 恢复 |
| LordPower > RebelPower × 1.2 | **谈判让步** | 妥协协议 | 给叛乱家族让渡份额（EconShare/Militia +10-20）；短期稳定但 EffectiveDominance↑ |
| 0.8 ≤ ratio ≤ 1.2 | **强攻** | 惨胜 | 双方大幅兵力损耗 40%-60%；Prosperity↓、PeopleBurden↑、Treasury 大幅消耗 |
| 0.8 ≤ ratio ≤ 1.2 | **谈判** | 妥协协议 | 同上，但谈判筹码更少，让步更大 |
| LordPower < RebelPower × 0.8 | **让步改革** | 赦免改革 | 叛乱家族 Loyalty 短期飙升（80+）但 Dominance↑↑；其他忠诚家族 Loyalty↓ |
| LordPower < RebelPower × 0.8 | **弃城** | 失守翻面 | 城市脱离控制；Prestige↓↓；BorderThreat↑ |

#### 【新增】7.4 结局后的恢复流程 — Product P0

无论哪种结局，叛乱日实例结束后进入恢复期（2-4 周）：
- 每周 Unrest 自然衰减（-5% 到 -10%）
- 参与叛乱家族的情绪逐步稳定（Grievance 自然下降，Loyalty 缓慢回升）
- 城市经济指标逐步恢复（Prosperity、MarketActivity 回弹）
- 恢复期内若再触发叛乱条件，Unrest 阈值降低 20%（连锁叛乱风险）

#### 【新增】7.5 叙事记录 — Product P2

每次叛乱结局自动生成一条"节度使回忆录"条目，记录触发原因、玩家选择、最终结果。格式示例："光启二年春，牙将李氏因欠饷积怨，率牙兵据门。节度使重金抚之，李氏暂安，然军费告罄，民负日重。"

#### 【新增】7.6 嵌套防护规则 — Tech P1

- 日实例期间，该城市的周层指标冻结（收入/支出暂停结算）。
- 其他未进入日实例的城市照常执行周推进（不暂停）。
- 其他城市**不能在本轮触发新的叛乱**（延迟到下一周判定），防止玩家同时应对多场危机。
- 日实例结束后，结果写回 City 和 Family 状态，下一周正常推进。

---

### 8. 已有 C# 原型实现内容
当前仓库内准备实现的 C# 原型包含：

- **枚举与数据结构**：
  - `Season`、`CityController`、`CityType`、`KeyFacility`、`FamilyType`。
  - `World`：全局周数、季节、合法性、朝廷评价、边患压力、环境参数、城市列表。
  - `City`：类型、设施、控制者、资源与指标、城市政策 `CityPolicy`、家族列表。
  - `Family`：类型、忠诚/怨气/依赖、三份额 `EconShare/Militia/Popularity`（均 ∈ [0,1]）、`Power`、`Dominance`。

#### 【新增】8.1 C# 架构建议 — Tech

**Enum Flags 城市类型**：
```csharp
[Flags]
public enum CityType
{
    None = 0,
    ProvincialCapital = 1 << 0,
    MilitaryGarrisonTown = 1 << 1,
    TradeHubSalt = 1 << 2,
    TradeHubTeaHorse = 1 << 3,
    RiverPortCity = 1 << 4,
    AgriculturalCountyTown = 1 << 5,
    ReligiousCenter = 1 << 6,
}
```

**周流水线模式（Pipeline）**：
```csharp
public interface IWeeklyStep
{
    void Execute(City city, World world);
}

public class WeeklyPipeline
{
    private readonly List<IWeeklyStep> _steps = new()
    {
        new IncomeCollectionStep(),
        new ExpensePaymentStep(),
        new PolicyExecutionStep(),
        new CityMetricsUpdateStep(),
        new FamilyShareUpdateStep(),
        new DominanceRecalcStep(),
        new RebellionGateCheckStep(),
    };

    public void Run(City city, World world)
    {
        foreach (var step in _steps)
            step.Execute(city, world);
    }
}
```

**Phase 1 极简版叛乱状态机**：
```csharp
public enum EpisodePhase { Spark, Resolution }
public enum EpisodeResolution { Suppress, CostlySuppress, Autonomy, CityFlipped, GrantReform }
```

---

### 【新增】9. 玩家目标与游戏节奏 — Product P1

#### 9.1 无固定终点模式

- 游戏以"唐末年表"为隐性时间线（公元 875-907 年，黄巢起义至唐亡）。
- 玩家可随时在 UI 中查看"当前年份"和"历史大事件年表"作为背景参照。
- 游戏不会因时间线走完而强制结束，但会在 907 年后提供"你的藩镇故事"完整回顾。
- 玩家可选择继续游玩进入五代十国时期（扩展内容），或在此结束。

#### 9.2 自设目标暗示（叙事驱动，非系统强制）

游戏不提供任务列表或目标面板，而是通过叙事环境让玩家自然产生目标感：

| 叙事手段 | 玩家自然产生的目标 |
|----------|-------------------|
| 朝廷定期下诏"削藩""征调" | "我要保住自己的地盘"（生存动机） |
| 家族成员出生、成长、死亡、联姻 | "我要让家族延续下去"（传承动机） |
| 邻镇扩张、吞并、结盟 | "我不能落后于人"（竞争动机） |
| 民间灾荒、流民、起义 | "我要治理好这一方百姓"（成就动机） |
| 京城政变、宦官废立皇帝 | "我要在乱世中站对队"（博弈动机） |

这些动机不互斥，玩家可能同时追求多个——这正是"体验过程"定位的优势所在。

#### 9.3 体验节奏设计

一局游戏的体验应该由多个"经营-危机"循环组成。每个循环建议 6-12 周：

```
平静期（2-4 周）
  → 日常经营，玩家优化经济和家族关系
  → 偶尔出现小事件（税收小波动、家族成员闲聊）

积累期（1-2 周）
  → 问题开始浮现：欠饷周数增加、某个家族不满加剧、私盐传闻
  → 系统通过 UI 提示和事件文本给玩家预警信号
  → 玩家有机会提前干预（调拨军饷、安抚家族、打击私盐）

危机期（触发叛乱日实例）
  → 高张力决策，玩家在压力下选择应对方式
  → 核心戏剧冲突集中爆发

恢复期（2-4 周）
  → 危机善后，忠诚度重新稳定，新的权力格局形成
  → 为下一轮循环埋下新的隐患种子
```

**节奏平衡原则**：
- 平静期不应超过 4 周，否则玩家会感到无聊
- 危机之间至少间隔 6 周，否则玩家会感到疲于奔命
- 如果系统检测到连续 8 周无任何事件触发，应自动注入一个小事件（哪怕只是"邻镇派使者来访"这种无害事件），维持叙事节奏

---

### 10. 下一步实现建议
1. 在当前目录下建立一个简单的 C# 项目（或仅用 `Program.cs` + 上述结构）跑通周循环模拟。
2. 验证：
   - 税收 / 军饷 / 私盐 / 欠饷 等数值的变化趋势。
   - 家族三份额与 `Dominance/Unrest` 的演化是否符合预期（例如长期欠饷 → 军门家族 `Dominance` 上升）。
3. **优先级排序**（来自 Product + Tech 合并）：

| 优先级 | 修改项 | 负责 |
|--------|--------|------|
| **P0** | §1 体验宣言 + §7 叛乱四分支 + 极简日实例 | Product + Tech |
| **P1** | 全文公式归一化 + shock 量化 | Tech |
| **P1** | §5 时序规则 + 嵌套防护 | Tech |
| **P1** | §10 玩家目标与节奏 + §6 危机链叙事目标 | Product |
| **P2** | §3 信息分层展示 + §4 家族数量约束 | Product + Tech |
| **P2** | §11 家族日志（节度使回忆录） | Product |

4. 之后再逐步接入：
   - JSON 驱动的经济 / 军事 / 权谋动作。
   - JSON 日事件池与两条危机链。
   - 叛乱日实例的 Phase 2 完整版（五阶段 + AP 多轮博弈）。

---

> **修改汇总**：共新增/修改 15 处，涉及 §1（体验哲学）、§3（信息分层）、§4（数值归一化+家族约束）、§5（Unrest公式+时序规则）、§6（危机链叙事）、§7（叛乱四分支+恢复+嵌套防护）、§8（C#架构）、§9（玩家目标，全新章节）。
