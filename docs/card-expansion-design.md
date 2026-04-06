# 《官途浮沉》扩展卡牌设计文档

> 策划：老K | 版本：v1.0 | 日期：2026-04-06

---

## 设计目标

模拟器跑通后发现现有卡池（24张）集中在县令阶段，内容量严重不足。本次扩展：

1. **按官职分层**——县令专属、刺史专属、侍郎专属、高阶通用
2. **加入连锁事件**——选择触发后续卡，形成多回合叙事弧
3. **加入历史事件卡**——安史之乱、科举改革、藩镇割据等唐朝大事件
4. **总计新增24张卡牌**，含4条连锁链、4张历史事件卡

---

## 一、县令专属卡牌（6张）

### 卡牌 E-001：义仓之争

| 字段 | 内容 |
|------|------|
| **CardId** | `card_county_granary_e001` |
| **类型** | Decision |
| **稀有度** | Common |
| **描述** | 朝廷推行义仓制度，要求各县按户征粮储备以防灾荒。然而今年秋粮歉收，百姓交完正税已所剩无几。里正们联名上书请求缓征义仓粮，而上级催文却措辞严厉，限期十日内足额入仓。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **如期足额征收，不打折扣** | 无 | Reputation -8, Intellect +3 | `yicang_harsh` | npc_governor: 好感+8, 信任+5 |
| **私下挪用官库补足缺额** | Scheming ≥ 10 | Scheming +5, Reputation +3 | `yicang_embezzle` | npc_registrar: 好感+5, 信任-8 / npc_censor: 好感-5, 信任-10 |
| **据实上报，请求宽限** | Charisma ≥ 12 | Reputation +5, Charisma +3 | `yicang_delay` | npc_governor: 好感-10, 信任-5 |

> **设计意图：** 体现唐代义仓制度的基层执行困境。挪用官库是条暗线——若后续被查出，将触发连锁危机。

---

### 卡牌 E-002：胡商案

| 字段 | 内容 |
|------|------|
| **CardId** | `card_county_merchant_e002` |
| **类型** | Decision |
| **稀有度** | Uncommon |
| **描述** | 一队粟特胡商途经本县时与本地商户发生冲突，双方各执一词。胡商自称持有鸿胪寺通关文牒，要求按蕃商律令处理；本地商户则叫嚷着"非我族类"。驿站外已聚集不少看热闹的百姓。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **依唐律秉公断案，一视同仁** | Intellect ≥ 12 | Reputation +6, Intellect +5, Charisma +3 | `hushang_fair` | npc_governor: 好感+5, 信任+8 |
| **偏袒本地商户，驱逐胡商** | 无 | Reputation +5（短期民心）, Charisma -5 | `hushang_expel` | npc_gentry: 好感+10, 信任+5 / npc_governor: 好感-8, 信任-5 |
| **居中调停，各打五十大板** | Charisma ≥ 10 | Charisma +3, Scheming +2 | `hushang_mediate` | — |

> **设计意图：** 唐朝作为丝路重镇，胡商纠纷是真实历史场景。选择偏袒本地的玩家，后续可能因"阻塞商路"被弹劾。

---

### 卡牌 E-003：乡饮酒礼

| 字段 | 内容 |
|------|------|
| **CardId** | `card_county_feast_e003` |
| **类型** | Opportunity |
| **稀有度** | Common |
| **描述** | 一年一度的乡饮酒礼将至，按制由县令主持，宴请本县耆老贤达。这是彰显教化、笼络地方的大好时机。但今年府库吃紧，上一任留下的烂摊子还没收拾干净…… |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **隆重操办，借钱也要面子** | 无 | Reputation +10, Charisma +5, 储备-40 | `feast_grand` | npc_gentry: 好感+8, 信任+3 / npc_abbot: 好感+5, 信任+3 |
| **从简操办，量入为出** | Intellect ≥ 8 | Reputation +3, Intellect +2 | `feast_modest` | npc_deputy: 好感+5, 信任+5 |
| **借机宣讲新政，化宴为政** | Charisma ≥ 15 | Charisma +8, Reputation +5 | `feast_political` | npc_governor: 好感+5, 信任+5 / npc_gentry: 好感-5, 信任-3 |

---

### 卡牌 E-004：坊正密报（🔗 连锁起点 → E-005）

| 字段 | 内容 |
|------|------|
| **CardId** | `card_county_spy_e004` |
| **类型** | Character |
| **稀有度** | Uncommon |
| **描述** | 西市坊正深夜来报：城西废宅中近日频繁出入可疑人等，似有暗中铸造私钱之嫌。坊正战战兢兢，说为首之人"像是衙门里的人"。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **亲率衙役连夜突袭** | Martial ≥ 10 | Martial +5, Health -5, Reputation +8 | `counterfeit_raid` | npc_registrar: 好感-20, 信任-15 |
| **暗中监视，收集证据** | Scheming ≥ 12 | Scheming +5 | `counterfeit_watch` | — |
| **知会县丞，让他去查** | 无 | 无 | `counterfeit_delegate` | npc_deputy: 好感+3, 信任+5 |

> ⚡ **连锁机制：** 选择"暗中监视"后，2回合内触发 E-005《私铸案发》。

---

### 卡牌 E-005：私铸案发（🔗 连锁后续 ← E-004）

| 字段 | 内容 |
|------|------|
| **CardId** | `card_county_counterfeit_e005` |
| **类型** | Crisis |
| **稀有度** | Rare |
| **前置** | RequiredFlags: `counterfeit_watch` |
| **描述** | 监视数日后查明真相——私铸铜钱的幕后之人竟是主簿孙伯仁！他利用职务之便调运铜料，暗中牟取暴利。证据确凿，但孙伯仁背后站着豪绅王家，一旦动手恐怕牵连甚广。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **铁证如山，依律严办** | Martial ≥ 8 | Reputation +15, Martial +3, Intellect +5 | `registrar_arrested` | npc_registrar: 好感-50, 信任-50 / npc_gentry: 好感-20, 信任-15 / npc_censor: 好感+15, 信任+15 / npc_governor: 好感+5, 信任+10 |
| **私下施压，逼他吐出赃款** | Scheming ≥ 18 | Scheming +8, 储备+60 | `registrar_blackmailed` | npc_registrar: 好感-10, 信任-30 |
| **按下不表，留作日后筹码** | Scheming ≥ 15 | Scheming +5 | `registrar_leverage` | npc_registrar: 好感+5, 信任-20 |

> **设计意图：** 与E-004构成完整的"私铸案"连锁。严办主簿是高风险高回报的选择——失去理财能手但赢得清名。黑吃黑路线则为后续埋下隐患。

---

### 卡牌 E-006：均田纠纷

| 字段 | 内容 |
|------|------|
| **CardId** | `card_county_land_e006` |
| **类型** | Decision |
| **稀有度** | Uncommon |
| **描述** | 均田令下，本县需重新丈量土地、编定户籍。然而豪族隐田瞒户成风，王家名下登记三百亩，实际恐怕不下千亩。丈量到王家地界时，佃农们目光闪烁，无人敢说实话。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **严格丈量，据实登记** | Martial ≥ 10, Intellect ≥ 10 | Reputation +12, Intellect +5, Martial +3 | `land_survey_strict` | npc_gentry: 好感-30, 信任-20 / npc_governor: 好感+10, 信任+8 / npc_censor: 好感+8, 信任+10 |
| **与王家私下协商，各退一步** | Charisma ≥ 12 | Charisma +5, Scheming +3 | `land_survey_compromise` | npc_gentry: 好感+10, 信任+5 / npc_governor: 好感-3, 信任-3 |
| **敷衍了事，按旧册抄录** | 无 | Reputation -5, Scheming +2 | `land_survey_fake` | npc_gentry: 好感+5, 信任+3 / npc_censor: 好感-10, 信任-15 |

> **设计意图：** 唐代均田制崩坏是核心历史主题。严查土地直接触动豪族命脉，是县令阶段最激烈的对抗之一。

---

## 二、刺史专属卡牌（6张）

### 卡牌 E-007：藩镇军饷

| 字段 | 内容 |
|------|------|
| **CardId** | `card_prefect_army_e007` |
| **类型** | Crisis |
| **稀有度** | Uncommon |
| **MinRank** | Prefect |
| **描述** | 驻军折冲府的粮饷已拖欠三月，士卒怨声载道。折冲都尉秦虎亲自登门讨要，言语间隐含威胁："兄弟们饿着肚子，可保不了这一州太平。"而州府库银刚被调拨去修河堤…… |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **挪用河堤款项先发军饷** | 无 | Martial +5, Reputation -8 | `army_paid_diverted` | npc_general: 好感+15, 信任+10 |
| **向辖下各县加征临时税** | Scheming ≥ 10 | Scheming +3, Reputation -10 | `emergency_tax` | npc_general: 好感+8, 信任+5 |
| **上书朝廷请拨军饷，安抚秦虎** | Charisma ≥ 15 | Charisma +5, Intellect +3 | `army_petition` | npc_general: 好感-5, 信任-8（远水不解近渴） |

> **设计意图：** 刺史阶段引入军政矛盾。唐中后期藩镇军饷问题是导致割据的核心因素之一。

---

### 卡牌 E-008：漕运改道（🔗 连锁起点 → E-009）

| 字段 | 内容 |
|------|------|
| **CardId** | `card_prefect_canal_e008` |
| **类型** | Decision |
| **稀有度** | Rare |
| **MinRank** | Prefect |
| **描述** | 转运使衙门来文：因黄河泥沙淤积，漕运将改走新渠道，途经你管辖的渭北三县。若承接漕运中转，本州商贸将迎来黄金期；但修建转运码头耗资巨大，且漕船带来的流民和纠纷也会激增。更棘手的是，邻州刺史也在争抢这条线路。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **全力争取，倾州之力修建码头** | Intellect ≥ 15 | Intellect +8, Reputation +5, 储备-80 | `canal_build` | npc_governor（转运使）: 好感+10, 信任+8 |
| **与邻州联合，共建共享** | Charisma ≥ 18 | Charisma +8, Intellect +3 | `canal_cooperate` | — |
| **暗中使绊，让邻州出丑** | Scheming ≥ 15 | Scheming +8, Reputation -5 | `canal_sabotage` | — |

> ⚡ **连锁机制：** 选择"全力争取"且成功后，3回合内触发 E-009《漕运风波》。

---

### 卡牌 E-009：漕运风波（🔗 连锁后续 ← E-008）

| 字段 | 内容 |
|------|------|
| **CardId** | `card_prefect_canal_crisis_e009` |
| **类型** | Crisis |
| **稀有度** | Rare |
| **MinRank** | Prefect |
| **前置** | RequiredFlags: `canal_build` |
| **描述** | 码头建成通航不到两月，一艘漕船在你的辖区内沉没，船上三千石官粮落入水底。转运使大怒，勒令你十日内打捞完毕并赔偿损失。与此同时，有人密报：沉船并非意外，而是有人故意凿穿船底…… |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **组织打捞，同时追查凶手** | Martial ≥ 12, Intellect ≥ 12 | Martial +5, Intellect +5, Health -8 | `canal_investigated` | npc_censor: 好感+8, 信任+10 |
| **自掏州库赔偿，息事宁人** | 无 | Reputation -5, 储备-60 | `canal_compensated` | npc_governor: 好感+5, 信任-5 |
| **栽赃邻州，反咬一口** | Scheming ≥ 20 | Scheming +10, Reputation -8 | `canal_framed` | — |

---

### 卡牌 E-010：节度使征兵

| 字段 | 内容 |
|------|------|
| **CardId** | `card_prefect_conscript_e010` |
| **类型** | Decision |
| **稀有度** | Uncommon |
| **MinRank** | Prefect |
| **描述** | 节度使府传来军令：征发本州壮丁三千人，限一月内送至幽州前线。正值农忙时节，壮丁离乡则秋粮无人收割。但抗命不遵，则是违抗军令的大罪。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **照令征发，不折不扣** | 无 | Reputation -12, Martial +5 | `conscript_full` | npc_general: 好感+12, 信任+8 |
| **以老弱充数，暗中保护壮丁** | Scheming ≥ 15 | Scheming +8, Reputation +5 | `conscript_fake` | npc_general: 好感-10, 信任-15 |
| **上书陈情，请求减免名额** | Charisma ≥ 15, Intellect ≥ 12 | Charisma +5, Intellect +3 | `conscript_petition` | npc_general: 好感-5, 信任-3 |

> **设计意图：** 唐代府兵制崩坏后的强制征兵，是民变频发的直接原因。刺史夹在军方与百姓之间的两难。

---

### 卡牌 E-011：州试舞弊案

| 字段 | 内容 |
|------|------|
| **CardId** | `card_prefect_exam_e011` |
| **类型** | Decision |
| **稀有度** | Rare |
| **MinRank** | Prefect |
| **描述** | 州试放榜后，落第士子集体到州府门前喊冤，控诉今年的解元乃是考官亲侄，试卷中竟有三道题与考前泄露的"秘题"完全一致。此事若处理不当，恐怕会惊动朝廷；若包庇考官，自己也脱不了干系。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **彻查科场，革除考官，重新开考** | Intellect ≥ 15 | Reputation +15, Intellect +8 | `exam_purge` | npc_censor: 好感+15, 信任+15 |
| **安抚士子，暗中处理考官** | Charisma ≥ 12, Scheming ≥ 10 | Charisma +5, Scheming +5 | `exam_quiet` | npc_censor: 好感-5, 信任-8 |
| **压下此事，以维护科举权威为名** | Scheming ≥ 12 | Scheming +5, Reputation -10 | `exam_coverup` | npc_censor: 好感-15, 信任-20 |

---

### 卡牌 E-012：盐铁之利

| 字段 | 内容 |
|------|------|
| **CardId** | `card_prefect_salt_e012` |
| **类型** | Opportunity |
| **稀有度** | Uncommon |
| **MinRank** | Prefect |
| **描述** | 本州境内发现一处天然盐泉。按制，盐铁归朝廷专营，应即刻上报。但若暗中先行开采，收益极为可观——足以解决州府财政困难。幕僚分成两派，争论不休。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **立即上报朝廷，请求设盐监** | Intellect ≥ 10 | Reputation +8, Intellect +5 | `salt_reported` | npc_censor: 好感+10, 信任+10 / npc_governor: 好感+5, 信任+5 |
| **先行开采三月，赚够再报** | Scheming ≥ 15 | Scheming +8, 储备+100 | `salt_secret_mine` | npc_censor: 好感-10, 信任-15（若被查出） |
| **与盐商合营，利益均沾** | Charisma ≥ 12 | Charisma +5, 储备+50 | `salt_partnership` | npc_gentry: 好感+10, 信任+8 |

---

## 三、侍郎专属卡牌（6张）

### 卡牌 E-013：朝堂论战

| 字段 | 内容 |
|------|------|
| **CardId** | `card_vice_debate_e013` |
| **类型** | Decision |
| **稀有度** | Common |
| **MinRank** | ViceMinister |
| **描述** | 大朝会上，改革派首辅提出"两税法"改革——废除租庸调，改按田亩和资产征税。保守派群臣激烈反对，称此举动摇国本。天子目光扫过群臣，落在你身上："卿以为如何？" |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **力挺两税法，陈述地方实情** | Intellect ≥ 18 | Reputation +10, Intellect +8 | `support_reform` | 改革派NPC: 好感+15, 信任+10 / 保守派NPC: 好感-15, 信任-10 |
| **附和保守派，维护祖制** | 无 | Reputation -5, Charisma +3 | `support_tradition` | 改革派NPC: 好感-15, 信任-10 / 保守派NPC: 好感+10, 信任+8 |
| **折中发言，各取所长** | Charisma ≥ 20, Intellect ≥ 15 | Charisma +10, Intellect +5 | `reform_moderate` | — |

> **设计意图：** 两税法是唐代最重要的税制改革（建中元年，780年）。侍郎阶段的核心矛盾从地方转向朝堂派系。

---

### 卡牌 E-014：宦官矫诏（🔗 连锁起点 → E-015）

| 字段 | 内容 |
|------|------|
| **CardId** | `card_vice_eunuch_e014` |
| **类型** | Crisis |
| **稀有度** | Rare |
| **MinRank** | ViceMinister |
| **描述** | 深夜，你的心腹密报：内侍省的高力借传旨之机，私自篡改了一道关于边将任免的诏书，将自己的亲信安插到灵武节度使的位子上。这意味着宦官集团正在染指军权——大唐的根基正在被蛀空。你手中握有证据，但高力耳目遍布宫禁…… |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **联合御史台，公开弹劾** | Intellect ≥ 15, Charisma ≥ 15 | Reputation +15, Health -15 | `eunuch_impeached` | npc_eunuch: 好感-50, 信任-50 / npc_censor: 好感+20, 信任+20 |
| **密奏天子，私下进言** | Scheming ≥ 20 | Scheming +10, Health -8 | `eunuch_secret_report` | npc_eunuch: 好感-20, 信任-30 |
| **隐忍不发，等待时机** | 无 | Scheming +3 | `eunuch_evidence_held` | — |

> ⚡ **连锁机制：** 选择"联合御史台"后，1-2回合内触发 E-015《宫禁之变》。

---

### 卡牌 E-015：宫禁之变（🔗 连锁后续 ← E-014）

| 字段 | 内容 |
|------|------|
| **CardId** | `card_vice_coup_e015` |
| **类型** | Crisis |
| **稀有度** | Epic |
| **MinRank** | ViceMinister |
| **前置** | RequiredFlags: `eunuch_impeached` |
| **描述** | 弹劾奏章呈上的当夜，高力先发制人——调动神策军封锁宫门，以"清君侧"为名软禁天子。你与御史台数名官员被困在中书省，外面甲士环伺。消息传来：高力正在逐一清洗参与弹劾的朝臣。生死就在今夜。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **联络禁军旧部，发动反击** | Martial ≥ 15, Scheming ≥ 15 | Martial +10, Reputation +20, Health -20 | `coup_crushed` | npc_eunuch: 好感-100（彻底敌对） / npc_censor: 好感+25, 信任+25 |
| **从密道出逃，联络外镇勤王** | Scheming ≥ 18 | Scheming +8, Health -10 | `coup_escaped` | — |
| **投降高力，以退为进** | Scheming ≥ 20 | Scheming +12, Reputation -20, Charisma -10 | `coup_surrendered` | npc_eunuch: 好感+15, 信任-10 / npc_censor: 好感-30, 信任-30 |

> **设计意图：** 以"甘露之变"（835年）为原型。这是侍郎阶段最高风险的连锁事件，生死攸关。

---

### 卡牌 E-016：吏部铨选

| 字段 | 内容 |
|------|------|
| **CardId** | `card_vice_personnel_e016` |
| **类型** | Decision |
| **稀有度** | Common |
| **MinRank** | ViceMinister |
| **描述** | 你奉命主持本年吏部铨选，需从三百名候选官员中选拔地方要员。你的恩师暗示你安排几个"自己人"，改革派首辅则送来一份"推荐名单"。而你案头还压着一份御史台的检举——名单中有三人涉嫌行贿买官。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **严格按考核选拔，不徇私情** | Intellect ≥ 18 | Reputation +12, Intellect +5 | `select_fair` | 改革派: 好感-8, 信任-5 / npc_censor: 好感+10, 信任+15 |
| **照顾各方面子，平衡分配** | Charisma ≥ 15 | Charisma +5, Scheming +3 | `select_balanced` | 改革派: 好感+5 / 保守派: 好感+5 |
| **卖人情给改革派，换取支持** | Scheming ≥ 12 | Scheming +8, Reputation -5 | `select_political` | 改革派: 好感+15, 信任+10 |

---

### 卡牌 E-017：牛李党争

| 字段 | 内容 |
|------|------|
| **CardId** | `card_vice_party_e017` |
| **类型** | Decision |
| **稀有度** | Epic |
| **MinRank** | ViceMinister |
| **描述** | 朝堂上"牛党"与"李党"之争已达白热化。牛党主张科举取士、重文轻武；李党则力推门荫入仕、强军备边。两党各有宰相撑腰，中间地带越来越窄。你的同年好友分属两派，都在拉你"站最后一班车"——据说天子即将做出抉择，站错队的人将万劫不复。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **投身牛党，赌科举派得势** | 无 | Charisma +5, Intellect +5 | `join_niu` | 牛党NPC: 好感+20, 信任+15 / 李党NPC: 好感-20, 信任-15 |
| **投身李党，赌世族派得势** | 无 | Martial +5, Scheming +5 | `join_li` | 李党NPC: 好感+20, 信任+15 / 牛党NPC: 好感-20, 信任-15 |
| **两面下注，暗中脚踩两条船** | Scheming ≥ 22 | Scheming +10, Reputation -8 | `party_double` | — |

> **设计意图：** 牛李党争（808-846年）是唐朝中后期最持久的政治斗争，直接影响玩家的升迁路线和结局。

---

### 卡牌 E-018：经筵进讲

| 字段 | 内容 |
|------|------|
| **CardId** | `card_vice_lecture_e018` |
| **类型** | Opportunity |
| **稀有度** | Uncommon |
| **MinRank** | ViceMinister |
| **描述** | 天子诏令你在经筵上进讲《贞观政要》。这是直达天听的绝佳机会——讲得好则圣眷日隆，讲砸了则贻笑朝堂。你需要在"歌功颂德"和"借古讽今"之间拿捏分寸。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **借太宗纳谏之事，暗谏当今弊政** | Intellect ≥ 20, Charisma ≥ 15 | Reputation +15, Intellect +10, Health -5 | `lecture_admonish` | npc_censor: 好感+15, 信任+15 / npc_eunuch: 好感-15, 信任-10 |
| **中规中矩，稳扎稳打** | Intellect ≥ 12 | Intellect +5, Charisma +3 | `lecture_safe` | — |
| **极尽颂扬，讨天子欢心** | Charisma ≥ 12 | Charisma +5, Reputation -5 | `lecture_flatter` | npc_censor: 好感-10, 信任-10 / npc_eunuch: 好感+8, 信任+5 |

---

## 四、高阶通用卡牌（2张）

### 卡牌 E-019：天灾人祸

| 字段 | 内容 |
|------|------|
| **CardId** | `card_universal_famine_e019` |
| **类型** | Crisis |
| **稀有度** | Uncommon |
| **MinRank** | CountyMagistrate |
| **MaxRank** | GrandCouncilor |
| **描述** | 辖区遭遇百年不遇的大旱，紧接着蝗灾铺天盖地。饿殍遍野，流民四起。更雪上加霜的是，朝廷此时正忙于边患，赈灾拨款遥遥无期。你必须在没有外援的情况下独自应对这场浩劫。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **开常平仓，同时组织以工代赈** | Intellect ≥ 15 | Reputation +15, Intellect +8, 储备-60 | `famine_managed` | 上级NPC: 好感+5, 信任+10 |
| **向富户"劝捐"，实为逼捐** | Martial ≥ 10, Scheming ≥ 10 | Scheming +5, 储备+40 | `famine_forced_donation` | npc_gentry: 好感-20, 信任-15 |
| **封锁消息，防止流民涌入** | Scheming ≥ 12 | Scheming +3, Reputation -15 | `famine_sealed` | npc_censor: 好感-15, 信任-20 |

> **设计意图：** 各阶段都可能遭遇的天灾，但应对手段因官职不同而差异巨大。县令只能开自家小仓，刺史可调度一州，侍郎可奏请朝廷。

---

### 卡牌 E-020：故人来访

| 字段 | 内容 |
|------|------|
| **CardId** | `card_universal_old_friend_e020` |
| **类型** | Character |
| **稀有度** | Common |
| **MinRank** | CountyMagistrate |
| **MaxRank** | GrandCouncilor |
| **描述** | 一位旧日同窗找上门来。此人才华横溢却命运多舛，屡试不第，如今穷困潦倒。他不求高官厚禄，只望你给一碗饭吃。收留他或许能得一幕僚之才，但此人性情孤傲、不善交际，怕是难以融入你的幕府。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **延为幕僚，量才录用** | Charisma ≥ 10 | Intellect +8, Charisma +5 | `friend_hired` | npc_deputy: 好感-3, 信任-3 |
| **赠以盘缠，婉言相送** | 无 | Reputation +3, Charisma +2 | — | — |
| **举荐给他人，借花献佛** | Scheming ≥ 10 | Scheming +3, Charisma +3 | `friend_referred` | 被举荐的NPC: 好感+8, 信任+5 |

---

## 五、历史事件卡（4张）

> 历史事件卡是特殊机制：在特定条件下强制触发，无法回避。它们代表唐朝历史的大转折点，玩家只能在时代洪流中做出自己的选择。

### 卡牌 H-001：安史之乱爆发

| 字段 | 内容 |
|------|------|
| **CardId** | `card_hist_anshi_h001` |
| **类型** | HistoricalCrisis |
| **稀有度** | Legendary |
| **触发条件** | 游戏进行到中后期（回合≥40），自动触发 |
| **描述** | 天宝十四载（755年）十一月，范阳节度使安禄山以"清君侧、诛杨国忠"为名起兵叛乱，十五万铁骑南下。消息传来时，朝野震惊——承平日久的大唐，竟无人料到叛军如此势如破竹。洛阳陷落、潼关失守的急报接踵而至，天子仓皇西逃。乱世降临，你将何去何从？ |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **追随天子，护驾入蜀** | 无 | Reputation +10, Martial -5, Health -10 | `anshi_follow_emperor` | 天子阵营NPC: 好感+20, 信任+15 |
| **坚守辖地，组织抵抗** | Martial ≥ 18, Intellect ≥ 15 | Martial +15, Reputation +20, Health -20 | `anshi_resist` | npc_general: 好感+25, 信任+20 / 全部NPC: 信任+10 |
| **审时度势，暂降叛军** | Scheming ≥ 20 | Scheming +10, Reputation -30, Charisma -10 | `anshi_surrender` | 所有正面NPC: 好感-30, 信任-40 |

> **设计意图：** 安史之乱是唐朝由盛转衰的历史拐点。这张卡会彻底改变游戏后半段的走向——选择抵抗的玩家进入"战时路线"，属性要求和事件类型大幅变化。

---

### 卡牌 H-002：科举改革

| 字段 | 内容 |
|------|------|
| **CardId** | `card_hist_keju_h002` |
| **类型** | HistoricalEvent |
| **稀有度** | Epic |
| **触发条件** | 侍郎阶段自动触发 |
| **描述** | 天子颁诏改革科举：废除"公荐"旧制，改由糊名阅卷，以断绝权贵操控科场之弊。此举得到寒门士子的热烈拥护，却遭到世族门阀的强烈抵制。作为朝中要员，你不可能置身事外。改革若成，寒门崛起；改革若败，世族更加肆无忌惮。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **全力推动改革，亲自监督施行** | Intellect ≥ 18, Charisma ≥ 15 | Reputation +15, Intellect +10 | `keju_reform_push` | 改革派: 好感+20, 信任+15 / 保守派: 好感-20, 信任-15 |
| **阳奉阴违，暗中留下操作空间** | Scheming ≥ 18 | Scheming +10 | `keju_reform_fake` | 保守派: 好感+10, 信任+5 |
| **上书建议渐进改革，折中推行** | Intellect ≥ 15, Charisma ≥ 12 | Intellect +5, Charisma +5 | `keju_reform_gradual` | — |

---

### 卡牌 H-003：藩镇割据

| 字段 | 内容 |
|------|------|
| **CardId** | `card_hist_fanzhen_h003` |
| **类型** | HistoricalCrisis |
| **稀有度** | Epic |
| **触发条件** | 刺史阶段中后期自动触发 |
| **描述** | 安史之乱平定后，朝廷为安抚降将，大量设置藩镇。如今河朔三镇已成事实上的独立王国——不奉朝命、自征赋税、父死子继。你所辖之州正处于朝廷直辖区与藩镇势力范围的交界地带。两边都在拉拢你，而你的选择将决定这片土地未来数十年的命运。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **坚定站在朝廷一边，拒绝藩镇拉拢** | Martial ≥ 12 | Reputation +12, Martial +5, Health -10 | `fanzhen_resist` | npc_censor: 好感+15, 信任+15 / npc_general: 好感-10, 信任-8 |
| **暗中与藩镇勾连，两头下注** | Scheming ≥ 18 | Scheming +10, Reputation -10 | `fanzhen_double` | npc_general: 好感+10, 信任+5 |
| **效仿藩镇，壮大自己的地方势力** | Martial ≥ 15, Scheming ≥ 15 | Martial +10, Scheming +8, Reputation -15 | `fanzhen_self` | npc_censor: 好感-25, 信任-30 / npc_general: 好感+15, 信任+10 |

---

### 卡牌 H-004：甘露之变

| 字段 | 内容 |
|------|------|
| **CardId** | `card_hist_ganlu_h004` |
| **类型** | HistoricalCrisis |
| **稀有度** | Legendary |
| **触发条件** | 侍郎阶段后期，且存在宦官势力冲突 |
| **描述** | 大和九年（835年），天子与宰相密谋诛杀宦官头目仇士良。计划是在禁中左金吾仗院的石榴树下设伏，诱宦官前来后一举擒杀。然而消息走漏，仇士良反扑。你在事变发生时正在含元殿，殿外已传来兵刃相交之声和惨叫。宫门即将关闭，你必须在一刻钟内做出抉择。 |

**选项：**

| 选项 | 要求 | 效果 | Flag | NPC影响 |
|------|------|------|------|---------|
| **冲出宫门，逃出长安** | Martial ≥ 15 | Martial +8, Health -15, Reputation -5 | `ganlu_escaped` | — |
| **留在殿中，与天子共进退** | Charisma ≥ 18 | Reputation +20, Charisma +10, Health -25 | `ganlu_loyal` | 天子阵营: 好感+30, 信任+25 |
| **伪装效忠宦官，暗中保护同僚** | Scheming ≥ 22 | Scheming +12, Reputation -10 | `ganlu_undercover` | npc_eunuch: 好感+10, 信任-5 / 被保护NPC: 好感+20, 信任+20 |

> **设计意图：** 甘露之变是唐朝宦官专权的巅峰事件。与E-014/E-015的宦官连锁线形成呼应——如果玩家此前已经与宦官交恶，这张卡的难度和风险将大幅提升。

---

## 六、连锁事件总览

| 连锁链 | 起点卡 | 后续卡 | 触发条件 | 叙事弧 |
|--------|--------|--------|---------|---------|
| **私铸案** | E-004 坊正密报 | E-005 私铸案发 | 选择"暗中监视"后2回合 | 县令揭发下属犯罪 |
| **漕运风波** | E-008 漕运改道 | E-009 漕运风波 | 选择"全力争取"后3回合 | 刺史的基建政绩遭暗算 |
| **宫禁之变** | E-014 宦官矫诏 | E-015 宫禁之变 | 选择"联合弹劾"后1-2回合 | 侍郎对抗宦官集团的生死局 |
| **安史之乱线** | H-001 安史之乱 | （可扩展为3-4张后续卡） | 自动触发后根据选择分支 | 大时代中的个人命运 |

### 连锁设计原则

1. **选择即后果**——连锁不是"做对了才触发"，而是"任何选择都有后续波澜"
2. **间隔2-3回合**——给玩家喘息和准备的时间，但又不至于忘记前因
3. **后续卡难度递增**——第一张卡是伏笔，第二张卡是爆发，风险和收益都翻倍
4. **允许规避**——如果玩家在第一张卡选择了保守选项，连锁不触发，但也失去了高回报机会

---

## 七、卡牌分布统计

| 官职阶段 | 现有卡牌 | 本次新增 | 总计 | 建议最低池容量 |
|---------|---------|---------|------|-------------|
| 县令专属 | 20 | 6 | 26 | 30 |
| 刺史专属 | 0 | 6 | 6 | 20 |
| 侍郎专属 | 0 | 6 | 6 | 20 |
| 高阶通用 | 4 | 2 | 6 | 10 |
| 历史事件 | 0 | 4 | 4 | 6 |
| **合计** | **24** | **24** | **48** | **86** |

> ⚠️ **注意：** 刺史和侍郎阶段的卡池仍然不足，本次扩展只是"第一批内容填充"。建议后续至少再补充14张刺史卡和14张侍郎卡，才能保证重玩不重复。

---

## 八、后续扩展建议

1. **季节事件卡**——春耕、夏税、秋收、冬赈，按回合周期触发
2. **异族/边境卡**——吐蕃入侵、回纥互市、南诏叛乱等边疆事件
3. **文化事件卡**——诗会、画展、佛道之争等提升Charisma和Intellect的软性事件
4. **Meta解锁卡**——通过永久成长系统解锁的特殊卡牌，增加重玩动力
