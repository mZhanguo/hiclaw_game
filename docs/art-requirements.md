# 《官途浮沉》美术资源需求文档

> **版本：** v1.0  
> **日期：** 2026-04-06  
> **风格基调：** 水墨国风 + 古典官场 + 卡牌游戏  
> **目标平台：** PC / Mobile (Unity 2D)

---

## 一、整体美术风格

### 核心风格
- **水墨写意风**：以中国传统水墨画为基础，笔触留白，意境悠远
- **色彩基调**：墨色为主，辅以朱砂红（官印/重点）、藤黄（金/权力）、花青（水/智慧）
- **UI风格**：仿古卷轴/奏折/公文样式，宣纸纹理底色
- **时代参考**：唐代中晚期（安史之乱后），建筑、服饰、器物参考唐代风格

### 参考游戏/作品
- 《江南百景图》（水墨国风UI）
- 《墨魂》（水墨角色立绘）
- 《皇帝成长计划》（官场题材）
- 《月圆之夜》（卡牌美术风格）

---

## 二、NPC立绘

### 规格
| 项目 | 规格 |
|---|---|
| **尺寸** | 1024×1536px（2:3竖版半身像） |
| **格式** | PNG（透明背景） |
| **风格** | 水墨半写实，线条清晰，细节适度 |
| **表情变体** | 每个NPC 3-4种表情（共用身体，换头部表情） |

### NPC清单

#### 🔴 优先级 P0（核心NPC，首批必须）

| NPC ID | 名称 | 身份 | 表情变体 | 描述/参考方向 |
|---|---|---|---|---|
| `npc_qinhu` | 秦虎 | 折冲都尉 | normal, angry, happy, suspicious | 魁梧武将，面如赤铁，蓄络腮胡。身着铠甲，豪爽直率。参考：典韦/张飞类型，但更英武 |
| `npc_governor` | 韩文远 | 刺史 | normal, angry, pleased, stern | 中年文官，面相威严，三缕长须。官服整洁，气度不凡。参考：包拯/司马懿类型 |
| `npc_mentor` | 陈学儒 | 恩师 | normal, disappointed, proud, thoughtful | 老年儒者，鹤发童颜，温文尔雅。青衫布衣，手持书卷。参考：诸葛亮老年形象 |
| `npc_rival` | 赵嘉明 | 同科对手 | normal, smug, angry, scheming | 青年文官，面容俊朗但眼神阴鸷。锦袍华服，风度翩翩但暗藏心机。参考：司马昭类型 |
| `npc_merchant` | 钱万通 | 富商 | normal, happy, nervous, greedy | 肥胖商人，满面堆笑，手持算盘。华服绸缎，金饰满身。典型"奸商"形象但不过分丑化 |

#### 🟡 优先级 P1（次要NPC，第二批）

| NPC ID | 名称 | 身份 | 表情变体 | 描述/参考方向 |
|---|---|---|---|---|
| `npc_eunuch` | 李德全 | 宦官 | normal, sly, angry, flattering | 中年宦官，面白无须，身形消瘦。深色宦官服，笑里藏刀。参考：典型宦官形象 |
| `npc_censor` | 魏正言 | 御史 | normal, righteous, angry, suspicious | 中年谏官，面容刚正，目光如炬。朝服端正，铁面无私。参考：魏征形象 |
| `npc_princess` | 李婉清 | 公主 | normal, elegant, angry, sad | 青年女性，端庄美丽，气质高贵。华丽宫装，凤冠霞帔。唐代贵族女性形象 |

#### 🟢 优先级 P2（扩展NPC，后续补充）

| NPC ID | 名称 | 身份 | 表情变体 | 描述/参考方向 |
|---|---|---|---|---|
| `npc_bandit` | 张铁牛 | 匪首 | normal, fierce, laughing | 壮硕莽汉，满脸横肉，刀疤醒目。粗布衣衫，持大刀 |
| `npc_clerk` | 孙有才 | 师爷 | normal, worried, pleased, scheming | 瘦削中年人，尖嘴猴腮，精明干练。布衫方巾，手持折扇 |

---

## 三、卡牌美术

### 卡牌整体规格
| 项目 | 规格 |
|---|---|
| **卡牌尺寸** | 512×720px（5:7比例） |
| **插图区域** | 512×360px（卡牌上半部分） |
| **格式** | PNG |
| **风格** | 水墨风插画，主题鲜明，辨识度高 |

### 卡牌边框（按稀有度）

| 稀有度 | 文件名 | 描述 |
|---|---|---|
| Common | `frame_common.png` | 素纸边框，简约水墨线条 |
| Uncommon | `frame_uncommon.png` | 青铜色边框，简单纹饰 |
| Rare | `frame_rare.png` | 朱砂红边框，祥云纹饰 |
| Epic | `frame_epic.png` | 鎏金边框，龙凤纹饰 |
| Legendary | `frame_legendary.png` | 紫金边框，九龙纹饰，发光效果 |

### 卡牌插图（按类型）

| 类型 | 文件前缀 | 描述/意象 | 优先级 |
|---|---|---|---|
| Policy | `card_policy` | 奏折铺开、御笔批红、公文案牍 | P0 |
| Crisis | `card_crisis` | 烽烟/乌云/裂缝、紧迫危机感 | P0 |
| Relationship | `card_relationship` | 把酒言欢/对弈/密谈 | P0 |
| Opportunity | `card_opportunity` | 祥云破晓/金光乍现 | P0 |
| Military | `card_military` | 兵符/战旗/阵图 | P1 |
| Economic | `card_economic` | 算盘/铜钱/粮仓 | P1 |
| Cultural | `card_cultural` | 书卷/琴/棋盘 | P1 |
| Random | `card_random` | 骰子/风云变幻/问号卷轴 | P2 |

### 卡牌背面
| 文件名 | 描述 |
|---|---|
| `card_back.png` | 水墨祥云图案，中间"官"字印章 |

---

## 四、场景背景

### 规格
| 项目 | 规格 |
|---|---|
| **尺寸** | 1920×1080px（16:9横版） |
| **格式** | JPG/PNG |
| **风格** | 水墨山水/建筑长卷风格，可做视差滚动 |

### 背景清单

#### 🔴 P0 - 县衙阶段（县令/县丞）

| 文件名 | 场景 | 描述 |
|---|---|---|
| `bg_county_office.png` | 县衙大堂 | 简朴的县衙正厅，匾额"明镜高悬"，两侧堂柱。水墨淡彩风 |
| `bg_county_court.png` | 县衙公堂 | 升堂审案场景，惊堂木、令旗 |
| `bg_county_street.png` | 县城街道 | 小城集市一角，远处城墙轮廓 |
| `bg_county_garden.png` | 县衙后院 | 小花园/书房，竹林、石桌 |

#### 🟡 P1 - 州府阶段（刺史/别驾）

| 文件名 | 场景 | 描述 |
|---|---|---|
| `bg_prefecture_office.png` | 州府正堂 | 气派的州府大堂，雕梁画栋，比县衙宏大 |
| `bg_prefecture_court.png` | 州府议事厅 | 官员们分列两侧议事 |
| `bg_prefecture_street.png` | 州城街景 | 繁华州城，酒楼茶肆 |
| `bg_prefecture_garden.png` | 州府花园 | 精致园林，假山流水 |

#### 🟡 P1 - 朝堂阶段（御史/尚书/宰相）

| 文件名 | 场景 | 描述 |
|---|---|---|
| `bg_court_office.png` | 官署书房 | 高级官员书房，大量书架、名贵摆件 |
| `bg_court_court.png` | 朝堂大殿 | 宏伟金銮殿，龙柱、御座（远景） |
| `bg_court_palace.png` | 宫殿走廊 | 朱红宫墙、琉璃瓦、长廊 |
| `bg_court_garden.png` | 宫廷花园 | 皇家园林，牡丹盛开 |

#### 🔴 P0 - 宰相专属

| 文件名 | 场景 | 描述 |
|---|---|---|
| `bg_court_grand_office.png` | 宰相府 | 极尽奢华的相府，权倾天下的氛围 |
| `bg_court_grand_study.png` | 相府书房 | 堆满奏章的案头，天下大事尽在掌中 |

---

## 五、UI图标

### 规格
| 项目 | 规格 |
|---|---|
| **尺寸** | 128×128px |
| **格式** | PNG（透明背景） |
| **风格** | 水墨线描 + 淡彩点缀，简洁辨识度高 |

### 属性图标

| 文件名 | 属性 | 意象 | 优先级 |
|---|---|---|---|
| `icon_intellect.png` | 智谋 | 棋子/灯/书卷 | P0 |
| `icon_charisma.png` | 魅力 | 折扇/明月 | P0 |
| `icon_martial.png` | 武力 | 剑/虎 | P0 |
| `icon_reputation.png` | 声望 | 旌旗/星 | P0 |
| `icon_loyalty.png` | 忠诚 | 玉/心 | P0 |
| `icon_corruption.png` | 贪腐 | 铜钱/蛇 | P0 |

### 阵营图标

| 文件名 | 阵营 | 意象 | 优先级 |
|---|---|---|---|
| `icon_reformist.png` | 改革派 | 新芽/旭日 | P1 |
| `icon_conservative.png` | 保守派 | 古鼎/城墙 | P1 |
| `icon_military.png` | 军方 | 兵符/盾 | P1 |
| `icon_eunuch.png` | 宦官 | 拂尘/暗影 | P1 |
| `icon_neutral.png` | 中立 | 天秤/水 | P1 |

### 官职图标

| 文件名 | 官职 | 意象 | 优先级 |
|---|---|---|---|
| `icon_county_magistrate.png` | 县令 | 小印/县衙 | P0 |
| `icon_prefect.png` | 刺史 | 大印/州旗 | P1 |
| `icon_censor.png` | 御史 | 笏板/铁笔 | P1 |
| `icon_minister.png` | 尚书 | 金印/朝笏 | P1 |
| `icon_grand_councilor.png` | 宰相 | 相印/龙纹 | P2 |

### UI功能图标

| 文件名 | 功能 | 意象 | 优先级 |
|---|---|---|---|
| `icon_action_point.png` | 行动点 | 沙漏/日晷 | P0 |
| `icon_favor.png` | 好感度 | 酒杯/笑脸 | P0 |
| `icon_trust.png` | 信任度 | 握手/玉佩 | P0 |
| `icon_turn.png` | 回合 | 日月交替 | P0 |

---

## 六、其他美术素材

### 卷轴/UI框架

| 文件名 | 用途 | 规格 | 优先级 |
|---|---|---|---|
| `scroll_horizontal.png` | 横向卷轴（信息面板） | 1200×200px 九宫格切片 | P0 |
| `scroll_vertical.png` | 纵向卷轴（侧栏） | 300×800px 九宫格切片 | P0 |
| `panel_paper.png` | 宣纸面板底图 | 800×600px 可平铺 | P0 |
| `dialog_frame.png` | 对话框边框 | 900×250px 九宫格切片 | P0 |
| `button_normal.png` | 按钮常态 | 256×80px | P0 |
| `button_pressed.png` | 按钮按下 | 256×80px | P0 |
| `button_disabled.png` | 按钮禁用 | 256×80px | P0 |

### 特效/粒子

| 文件名 | 用途 | 规格 | 优先级 |
|---|---|---|---|
| `fx_ink_splash.png` | 水墨泼洒效果序列帧 | 256×256px × 8帧 | P1 |
| `fx_stamp_glow.png` | 印章盖印光效 | 128×128px | P1 |
| `fx_promotion_light.png` | 升迁光效 | 512×512px | P2 |
| `fx_page_turn.png` | 翻页效果遮罩 | 1920×1080px | P2 |

### Logo/标题

| 文件名 | 用途 | 规格 | 优先级 |
|---|---|---|---|
| `logo_title.png` | 游戏标题"官途浮沉" | 1024×512px 透明背景 | P0 |
| `logo_icon.png` | 应用图标 | 512×512px | P0 |

---

## 七、音效资源需求（供音效师参考）

> 详细配置见 `Assets/Resources/Audio/AudioConfig.json`

### BGM（8首）
| 键名 | 描述 | 时长 | 优先级 |
|---|---|---|---|
| main_menu | 主菜单曲 - 大气古风，古筝+笛子 | 2-3min 循环 | P0 |
| morning_court | 早朝曲 - 庄严肃穆，编钟+鼓 | 2-3min 循环 | P0 |
| decision | 决策曲 - 沉思紧张，古琴+轻打击乐 | 2-3min 循环 | P0 |
| review | 复盘曲 - 舒缓平和，二胡+箫 | 2-3min 循环 | P1 |
| evaluation | 考评曲 - 紧张期待，渐强鼓+弦乐 | 1-2min 循环 | P1 |
| promotion | 升迁短曲 - 喜庆，唢呐+锣鼓 | 10-15s 单次 | P1 |
| demotion | 贬官短曲 - 悲凉，单箫 | 10-15s 单次 | P1 |
| game_over | 结束曲 - 苍凉，古琴缓奏 | 30s-1min | P2 |

### SFX（16个）
| 键名 | 描述 | 时长 | 优先级 |
|---|---|---|---|
| button_click | 木鱼/竹筒敲击 | <0.5s | P0 |
| page_flip | 宣纸翻页沙沙声 | <1s | P0 |
| stamp | 官印盖印（厚重） | <1s | P0 |
| card_draw | 竹简展开 | <1s | P0 |
| card_play | 奏折递出 | <1s | P0 |
| promotion_sfx | 短促锣鼓喜声 | 1-2s | P1 |
| demotion_sfx | 低沉铜锣声 | 1-2s | P1 |
| notification | 清脆磬声 | <1s | P1 |
| scroll_open | 丝绸滑动声 | <1s | P1 |
| scroll_close | 竹卷收拢声 | <1s | P1 |
| coin | 铜钱碰撞 | <0.5s | P1 |
| relation_up | 和弦上行 | <1s | P2 |
| relation_down | 和弦下行 | <1s | P2 |
| crisis | 急促鼓点 | 1-2s | P1 |
| turn_start | 晨钟一声 | 1-2s | P1 |
| turn_end | 暮鼓一声 | 1-2s | P1 |

### 环境音（6个）
| 键名 | 描述 | 时长 | 优先级 |
|---|---|---|---|
| brush_writing | 毛笔书写沙沙声（循环） | 30s+ 循环 | P0 |
| guqin | 古琴随机拨弦（循环） | 1-2min 循环 | P0 |
| court_murmur | 朝堂群臣低语 | 30s+ 循环 | P1 |
| rain | 屋外细雨 | 30s+ 循环 | P2 |
| birds | 庭院鸟鸣 | 30s+ 循环 | P2 |
| market | 远处集市声 | 30s+ 循环 | P2 |

---

## 八、文件命名规范

```
Art/
├── NPCs/                    # NPC立绘
│   ├── npc_qinhu_normal.png
│   ├── npc_qinhu_angry.png
│   └── ...
├── Cards/                   # 卡牌相关
│   ├── card_back.png
│   ├── Frames/
│   │   ├── frame_common.png
│   │   └── ...
│   └── Illustrations/
│       ├── card_policy.png
│       └── ...
├── Backgrounds/             # 场景背景
│   ├── bg_county_office.png
│   ├── bg_prefecture_court.png
│   └── ...
├── Icons/                   # UI图标
│   ├── Attributes/
│   ├── Factions/
│   ├── Ranks/
│   └── UI/
├── UI/                      # UI框架素材
│   ├── scroll_horizontal.png
│   └── ...
├── FX/                      # 特效素材
│   └── ...
├── Placeholders/            # 占位图（开发用）
│   ├── placeholder_npc.png
│   ├── placeholder_card.png
│   ├── placeholder_background.png
│   └── placeholder_icon.png
└── Logo/
    ├── logo_title.png
    └── logo_icon.png

Audio/
├── BGM/
│   ├── bgm_main_menu.ogg
│   └── ...
├── SFX/
│   ├── sfx_button_click.ogg
│   └── ...
└── Ambient/
    ├── amb_brush_writing.ogg
    └── ...
```

### 命名规则
- 全部小写，下划线分隔
- NPC立绘：`npc_{id}_{expression}.png`
- 卡牌：`card_{type}.png`
- 背景：`bg_{stage}_{scene}.png`
- 图标：`icon_{name}.png`
- BGM：`bgm_{name}.ogg`
- SFX：`sfx_{name}.ogg`
- 环境音：`amb_{name}.ogg`
- 音频格式统一用 `.ogg`（Unity推荐）

---

## 九、资源统计

| 类别 | P0 | P1 | P2 | 合计 |
|---|---|---|---|---|
| NPC立绘（含表情） | 19张 | 11张 | 7张 | **37张** |
| 卡牌插图 | 4张 | 3张 | 1张 | **8张** |
| 卡牌边框 | 5张 | - | - | **5张** |
| 场景背景 | 6张 | 8张 | - | **14张** |
| UI图标 | 10个 | 7个 | 1个 | **18个** |
| UI素材 | 7张 | - | - | **7张** |
| 特效素材 | - | 2组 | 2组 | **4组** |
| Logo | 2张 | - | - | **2张** |
| BGM | 3首 | 4首 | 1首 | **8首** |
| SFX | 5个 | 8个 | 3个 | **16个** |
| 环境音 | 2个 | 1个 | 3个 | **6个** |

**P0总计（首批必须）：** ~57项资源  
**全部总计：** ~125项资源

---

## 十、交付要求

1. 所有图片资源交付 **PNG格式**（立绘/图标需透明背景）
2. 背景可用 **JPG**（品质95%+）
3. 音频统一 **OGG格式**，44.1kHz，立体声
4. 每批交付时附带 **预览图/效果图** 便于确认
5. 源文件（PSD/AI/项目文件）另行保存，便于后续修改
6. **P0资源优先交付**，确保核心玩法可演示
