# 🏗️ 《官途浮沉》开发环境搭建指南

> 从零开始，5分钟跑起来。

---

## 环境要求

| 工具 | 版本 | 说明 |
|------|------|------|
| **Unity** | 2022.3 LTS 或更高 | 推荐 2022.3.x（长期支持版） |
| **Unity Hub** | 3.x | 管理Unity版本和项目 |
| **Git** | 2.30+ | 克隆项目 |
| **.NET** | Unity内置 | 无需额外安装 |

> ⚠️ 本项目使用 C# 和 Unity 原生 UI (uGUI)，无需额外安装任何 Package。

---

## 第一次运行（完整步骤）

### 1. 克隆项目

```bash
git clone <仓库地址> guantu-fucheng
cd guantu-fucheng
```

### 2. 用 Unity Hub 打开

1. 打开 Unity Hub → **Projects** → **Open**
2. 选择 `guantu-fucheng/` 根目录（包含 `Assets/`、`ProjectSettings/` 的那一层）
3. 如果提示Unity版本不匹配，选择你已安装的 **2022.3.x** 版本打开
4. 等待首次导入（1-3分钟，取决于机器性能）

### 3. 一键搭建场景

Unity编译完成后（底部进度条消失）：

1. 菜单栏 → **官途浮沉** → **一键搭建场景**
2. 弹出"搭建完成"对话框 → 点击"好的"
3. 场景自动保存到 `Assets/Scenes/MainScene.unity`

### 4. 运行游戏

1. 点击编辑器顶部 **▶ Play**
2. 观察 Console 窗口输出：

```
╔══════════════════════════════════════════╗
║        《官途浮沉》— 游戏启动中...       ║
╚══════════════════════════════════════════╝
[Bootstrap] 正在初始化系统...
[Bootstrap] ✓ 所有系统初始化完成
[Bootstrap] 正在加载游戏数据...
[Bootstrap] ✓ 数据加载完成：46张卡牌, 9个NPC, ...
[Bootstrap] 开始新游戏...
```

看到这些日志就说明一切正常！🎉

### 5. 验证数据（可选）

菜单栏 → **官途浮沉** → **数据检查** → 查看是否全部通过

---

## 项目架构

```
guantu-fucheng/
├── Assets/
│   ├── Editor/
│   │   └── QuickSetup.cs          ← 编辑器工具（菜单栏扩展）
│   │
│   ├── Resources/
│   │   └── GameData/
│   │       ├── CardDatabase.json   ← 46张决策卡牌
│   │       ├── NPCDatabase.json    ← 9个NPC数据
│   │       ├── RankDatabase.json   ← 官职升迁路线
│   │       └── EvaluationConfig.json ← 考评配置
│   │
│   ├── Scenes/
│   │   ├── MainScene.unity         ← 主场景（一键生成）
│   │   └── 说明.md                 ← 场景使用指南
│   │
│   └── Scripts/
│       ├── Core/                   ← 核心框架
│       │   ├── Singleton.cs           泛型单例基类
│       │   ├── GameManager.cs         游戏主管理器（状态机）
│       │   ├── TurnManager.cs         回合管理器（4阶段循环）
│       │   ├── GameBootstrap.cs       启动入口（数据加载）
│       │   ├── SceneSetup.cs          场景一键初始化
│       │   └── ConsoleGameRunner.cs   控制台调试运行器
│       │
│       ├── Systems/                ← 游戏子系统
│       │   ├── CardSystem.cs          卡牌系统（卡池/抽取/结算）
│       │   ├── NPCRelationshipGraph.cs NPC关系图谱
│       │   ├── ActionPointSystem.cs   行动力系统（6点分配）
│       │   ├── OfficialRankSystem.cs  官职升迁系统
│       │   ├── RogueliteMetaSystem.cs Roguelite跨局进度
│       │   ├── SaveSystem.cs          JSON存档系统
│       │   └── DataLoader.cs          JSON数据加载器
│       │
│       ├── UI/                     ← UI面板
│       │   ├── UIPanel.cs             面板抽象基类
│       │   ├── UIManager.cs           全局UI管理器
│       │   ├── UIConfig.cs            水墨风配色/样式常量
│       │   ├── HUDPanel.cs            常驻HUD
│       │   ├── MainMenuPanel.cs       主菜单
│       │   ├── MorningBriefingPanel.cs 早朝简报
│       │   ├── ActionAllocationPanel.cs 行动分配
│       │   ├── CardDecisionPanel.cs   卡牌决策
│       │   ├── ReviewPanel.cs         回合复盘
│       │   ├── NPCRelationshipPanel.cs NPC关系图谱
│       │   └── EvaluationPanel.cs     吏部考评
│       │
│       ├── Models/                 ← 数据模型
│       │   ├── GameStateModel.cs      运行时状态
│       │   └── MetaProgress.cs        永久进度
│       │
│       ├── Data/                   ← JSON反序列化数据类
│       │   ├── CardData.cs
│       │   ├── NPCData.cs
│       │   ├── OfficialRankData.cs
│       │   └── EventData.cs
│       │
│       ├── Enums/                  ← 枚举定义
│       │   └── GameEnums.cs
│       │
│       └── Events/                 ← 事件系统
│           └── GameEvents.cs
│
├── ProjectSettings/                ← Unity项目设置
├── SETUP_GUIDE.md                  ← （本文件）
└── README.md                       ← 项目说明
```

---

## 系统架构图

```
┌─────────────────────────────────────────────────────┐
│                    SceneSetup                        │
│        （一键创建所有对象，挂到空物体即可）             │
└────────────────────┬────────────────────────────────┘
                     │ 创建
        ┌────────────┼────────────┐
        ▼            ▼            ▼
  ┌──────────┐ ┌──────────┐ ┌──────────────┐
  │  Canvas  │ │EventSys  │ │GameBootstrap │
  │(1920x1080)│ │          │ │ (数据加载)   │
  └────┬─────┘ └──────────┘ └──────┬───────┘
       │                           │
       ▼                           ▼ 初始化 + 注入数据
  ┌─────────┐        ┌─────────────────────────────┐
  │UIManager│◄──────▶│        GameManager           │
  │ (面板调度)│       │     (全局状态机)              │
  └────┬────┘        └──────────┬──────────────────┘
       │                        │
       ▼                        ▼
  ┌─────────────┐    ┌──────────────────┐
  │  8个UI面板   │    │   TurnManager     │
  │             │    │ (回合4阶段循环)    │
  │ • HUD      │    └────────┬─────────┘
  │ • 主菜单    │             │ 调度
  │ • 早朝简报  │    ┌────────┼────────┐
  │ • 行动分配  │    ▼        ▼        ▼
  │ • 卡牌决策  │  Card    NPC关系   Action
  │ • 复盘     │  System   Graph    PointSys
  │ • NPC关系  │    │        │        │
  │ • 吏部考评  │    ▼        ▼        ▼
  └─────────────┘  OfficialRank  Roguelite  Save
                    System      MetaSys    System
                                             │
                                             ▼
                                     JSON 本地存档
```

---

## 游戏核心循环

```
每回合（约1-2分钟）：

  ┌──────────────┐
  │  1. 早朝简报  │  展示局势、触发事件、NPC行为、抽取卡牌
  └──────┬───────┘
         ▼
  ┌──────────────┐
  │ 2. 行动分配   │  6点行动力分配到：政务/交际/情报/修身/谋略/休息
  └──────┬───────┘
         ▼
  ┌──────────────┐
  │ 3. 执行结算   │  依次执行行动，触发卡牌事件，计算结果
  └──────┬───────┘
         ▼
  ┌──────────────┐
  │  4. 复盘总结  │  属性变化、关系变动、升迁检查、自动保存
  └──────┬───────┘
         ▼
    下一回合 / 考评 / 游戏结束
```

---

## 常见问题

### Q: 支持哪些Unity版本？
**A:** 2022.3 LTS 及以上。代码使用 C# 9.0 特性，需要 Unity 2021.2+，推荐 2022.3。

### Q: 需要安装额外的Package吗？
**A:** 不需要。项目使用 Unity 原生 uGUI 和 JsonUtility，零依赖。

### Q: 编译报错 "The type or namespace 'xxx' could not be found"
**A:** 通常是文件夹结构不对。确保 `Assets/Scripts/` 下的子目录名称正确（Core、Systems、UI、Models、Data、Enums、Events）。

### Q: 如何修改初始玩家名或官职？
**A:** 选中 Hierarchy 中的 `[SceneSetup]` 对象，在 Inspector 中找到 `GameBootstrap` 组件，修改 `Player Name` 和 `Starting Rank`。

### Q: 如何添加新的卡牌或NPC？
**A:** 编辑 `Assets/Resources/GameData/` 下的 JSON 文件，遵循现有格式添加条目。运行"数据检查"验证格式正确。

### Q: 存档在哪里？
**A:**
- **Windows:** `%USERPROFILE%/AppData/LocalLow/<CompanyName>/<ProductName>/`
- **macOS:** `~/Library/Application Support/<CompanyName>/<ProductName>/`
- **Linux:** `~/.config/unity3d/<CompanyName>/<ProductName>/`

---

## 联系方式

遇到问题？查看 `Assets/Scenes/说明.md` 或提交 Issue。
