#!/usr/bin/env python3
"""批量添加扩展卡牌和NPC到数据库"""
import json, os

data_dir = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Resources', 'GameData')

# ============================================================
# 24张新卡牌
# ============================================================
new_cards = [
  # === 县令专属 (6张) ===
  {
    "CardId": "card_county_granary_e001",
    "Title": "义仓之争",
    "Description": "朝廷推行义仓制度，要求各县按户征粮储备以防灾荒。然而今年秋粮歉收，百姓交完正税已所剩无几。里正们联名上书请求缓征义仓粮，而上级催文却措辞严厉，限期十日内足额入仓。",
    "Type": "Decision", "Rarity": "Common", "Repeatable": False,
    "MinTurn": 2, "MaxTurn": 0, "MinRank": "CountyMagistrate", "MaxRank": "Prefect",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "如期足额征收，不打折扣", "Tooltip": "严格执行朝令，但民怨沸腾",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": -8}, {"Attribute": "Intellect", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["yicang_harsh"],
       "RelationshipEffects": [{"NpcId": "npc_governor", "FavorDelta": 8, "TrustDelta": 5}]},
      {"ChoiceText": "私下挪用官库补足缺额", "Tooltip": "暗度陈仓，风险极大",
       "RequiredAttribute": "Scheming", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Scheming", "Value": 5}, {"Attribute": "Reputation", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["yicang_embezzle"],
       "RelationshipEffects": [{"NpcId": "npc_registrar", "FavorDelta": 5, "TrustDelta": -8}, {"NpcId": "npc_censor", "FavorDelta": -5, "TrustDelta": -10}]},
      {"ChoiceText": "据实上报，请求宽限", "Tooltip": "实事求是但得罪上级",
       "RequiredAttribute": "Charisma", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Reputation", "Value": 5}, {"Attribute": "Charisma", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["yicang_delay"],
       "RelationshipEffects": [{"NpcId": "npc_governor", "FavorDelta": -10, "TrustDelta": -5}]}
    ]
  },
  {
    "CardId": "card_county_merchant_e002",
    "Title": "胡商案",
    "Description": "一队粟特胡商途经本县时与本地商户发生冲突，双方各执一词。胡商自称持有鸿胪寺通关文牒，要求按蕃商律令处理；本地商户则叫嚷\u201c非我族类\u201d。驿站外已聚集不少看热闹的百姓。",
    "Type": "Decision", "Rarity": "Uncommon", "Repeatable": False,
    "MinTurn": 3, "MaxTurn": 0, "MinRank": "CountyMagistrate", "MaxRank": "Prefect",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "依唐律秉公断案，一视同仁", "Tooltip": "公正之举",
       "RequiredAttribute": "Intellect", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Reputation", "Value": 6}, {"Attribute": "Intellect", "Value": 5}, {"Attribute": "Charisma", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["hushang_fair"],
       "RelationshipEffects": [{"NpcId": "npc_governor", "FavorDelta": 5, "TrustDelta": 8}]},
      {"ChoiceText": "偏袒本地商户，驱逐胡商", "Tooltip": "短期民心但有后患",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": 5}, {"Attribute": "Charisma", "Value": -5}],
       "FollowUpCardId": "", "SetFlags": ["hushang_expel"],
       "RelationshipEffects": [{"NpcId": "npc_gentry", "FavorDelta": 10, "TrustDelta": 5}, {"NpcId": "npc_governor", "FavorDelta": -8, "TrustDelta": -5}]},
      {"ChoiceText": "居中调停，各打五十大板", "Tooltip": "和稀泥",
       "RequiredAttribute": "Charisma", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Charisma", "Value": 3}, {"Attribute": "Scheming", "Value": 2}],
       "FollowUpCardId": "", "SetFlags": ["hushang_mediate"],
       "RelationshipEffects": []}
    ]
  },
  {
    "CardId": "card_county_feast_e003",
    "Title": "乡饮酒礼",
    "Description": "一年一度的乡饮酒礼将至，按制由县令主持，宴请本县耆老贤达。这是彰显教化、笼络地方的大好时机。但今年府库吃紧，上一任留下的烂摊子还没收拾干净\u2026\u2026",
    "Type": "Opportunity", "Rarity": "Common", "Repeatable": False,
    "MinTurn": 2, "MaxTurn": 0, "MinRank": "CountyMagistrate", "MaxRank": "Prefect",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "隆重操办，借钱也要面子", "Tooltip": "大撒银子",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": 10}, {"Attribute": "Charisma", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["feast_grand"],
       "RelationshipEffects": [{"NpcId": "npc_gentry", "FavorDelta": 8, "TrustDelta": 3}, {"NpcId": "npc_abbot", "FavorDelta": 5, "TrustDelta": 3}]},
      {"ChoiceText": "从简操办，量入为出", "Tooltip": "节俭务实",
       "RequiredAttribute": "Intellect", "RequiredValue": 8,
       "Modifiers": [{"Attribute": "Reputation", "Value": 3}, {"Attribute": "Intellect", "Value": 2}],
       "FollowUpCardId": "", "SetFlags": ["feast_modest"],
       "RelationshipEffects": [{"NpcId": "npc_deputy", "FavorDelta": 5, "TrustDelta": 5}]},
      {"ChoiceText": "借机宣讲新政，化宴为政", "Tooltip": "借题发挥",
       "RequiredAttribute": "Charisma", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Charisma", "Value": 8}, {"Attribute": "Reputation", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["feast_political"],
       "RelationshipEffects": [{"NpcId": "npc_governor", "FavorDelta": 5, "TrustDelta": 5}, {"NpcId": "npc_gentry", "FavorDelta": -5, "TrustDelta": -3}]}
    ]
  },
  {
    "CardId": "card_county_spy_e004",
    "Title": "坊正密报",
    "Description": "西市坊正深夜来报：城西废宅中近日频繁出入可疑人等，似有暗中铸造私钱之嫌。坊正战战兢兢，说为首之人\u201c像是衙门里的人\u201d。",
    "Type": "Character", "Rarity": "Uncommon", "Repeatable": False,
    "MinTurn": 4, "MaxTurn": 0, "MinRank": "CountyMagistrate", "MaxRank": "Prefect",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "亲率衙役连夜突袭", "Tooltip": "雷厉风行",
       "RequiredAttribute": "Martial", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Martial", "Value": 5}, {"Attribute": "Health", "Value": -5}, {"Attribute": "Reputation", "Value": 8}],
       "FollowUpCardId": "", "SetFlags": ["counterfeit_raid"],
       "RelationshipEffects": [{"NpcId": "npc_registrar", "FavorDelta": -20, "TrustDelta": -15}]},
      {"ChoiceText": "暗中监视，收集证据", "Tooltip": "稳扎稳打，后续有大戏",
       "RequiredAttribute": "Scheming", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Scheming", "Value": 5}],
       "FollowUpCardId": "card_county_counterfeit_e005", "SetFlags": ["counterfeit_watch"],
       "RelationshipEffects": []},
      {"ChoiceText": "知会县丞，让他去查", "Tooltip": "撒手掌柜",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [],
       "FollowUpCardId": "", "SetFlags": ["counterfeit_delegate"],
       "RelationshipEffects": [{"NpcId": "npc_deputy", "FavorDelta": 3, "TrustDelta": 5}]}
    ]
  },
  {
    "CardId": "card_county_counterfeit_e005",
    "Title": "私铸案发",
    "Description": "监视数日后查明真相\u2014\u2014私铸铜钱的幕后之人竟是主簿孙伯仁！他利用职务之便调运铜料，暗中牟取暴利。证据确凿，但孙伯仁背后站着豪绅王家，一旦动手恐怕牵连甚广。",
    "Type": "Crisis", "Rarity": "Rare", "Repeatable": False,
    "MinTurn": 5, "MaxTurn": 0, "MinRank": "CountyMagistrate", "MaxRank": "Prefect",
    "RequiredFlags": ["counterfeit_watch"], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "铁证如山，依律严办", "Tooltip": "高风险高回报",
       "RequiredAttribute": "Martial", "RequiredValue": 8,
       "Modifiers": [{"Attribute": "Reputation", "Value": 15}, {"Attribute": "Martial", "Value": 3}, {"Attribute": "Intellect", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["registrar_arrested"],
       "RelationshipEffects": [{"NpcId": "npc_registrar", "FavorDelta": -50, "TrustDelta": -50}, {"NpcId": "npc_gentry", "FavorDelta": -20, "TrustDelta": -15}, {"NpcId": "npc_censor", "FavorDelta": 15, "TrustDelta": 15}, {"NpcId": "npc_governor", "FavorDelta": 5, "TrustDelta": 10}]},
      {"ChoiceText": "私下施压，逼他吐出赃款", "Tooltip": "黑吃黑",
       "RequiredAttribute": "Scheming", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Scheming", "Value": 8}],
       "FollowUpCardId": "", "SetFlags": ["registrar_blackmailed"],
       "RelationshipEffects": [{"NpcId": "npc_registrar", "FavorDelta": -10, "TrustDelta": -30}]},
      {"ChoiceText": "按下不表，留作日后筹码", "Tooltip": "留一手",
       "RequiredAttribute": "Scheming", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Scheming", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["registrar_leverage"],
       "RelationshipEffects": [{"NpcId": "npc_registrar", "FavorDelta": 5, "TrustDelta": -20}]}
    ]
  },
  {
    "CardId": "card_county_land_e006",
    "Title": "均田纠纷",
    "Description": "均田令下，本县需重新丈量土地、编定户籍。然而豪族隐田瞒户成风，王家名下登记三百亩，实际恐怕不下千亩。丈量到王家地界时，佃农们目光闪烁，无人敢说实话。",
    "Type": "Decision", "Rarity": "Uncommon", "Repeatable": False,
    "MinTurn": 4, "MaxTurn": 0, "MinRank": "CountyMagistrate", "MaxRank": "Prefect",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "严格丈量，据实登记", "Tooltip": "触动豪族命脉",
       "RequiredAttribute": "Martial", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Reputation", "Value": 12}, {"Attribute": "Intellect", "Value": 5}, {"Attribute": "Martial", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["land_survey_strict"],
       "RelationshipEffects": [{"NpcId": "npc_gentry", "FavorDelta": -30, "TrustDelta": -20}, {"NpcId": "npc_governor", "FavorDelta": 10, "TrustDelta": 8}, {"NpcId": "npc_censor", "FavorDelta": 8, "TrustDelta": 10}]},
      {"ChoiceText": "与王家私下协商，各退一步", "Tooltip": "妥协维稳",
       "RequiredAttribute": "Charisma", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Charisma", "Value": 5}, {"Attribute": "Scheming", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["land_survey_compromise"],
       "RelationshipEffects": [{"NpcId": "npc_gentry", "FavorDelta": 10, "TrustDelta": 5}, {"NpcId": "npc_governor", "FavorDelta": -3, "TrustDelta": -3}]},
      {"ChoiceText": "敷衍了事，按旧册抄录", "Tooltip": "最安全也最无能",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": -5}, {"Attribute": "Scheming", "Value": 2}],
       "FollowUpCardId": "", "SetFlags": ["land_survey_fake"],
       "RelationshipEffects": [{"NpcId": "npc_gentry", "FavorDelta": 5, "TrustDelta": 3}, {"NpcId": "npc_censor", "FavorDelta": -10, "TrustDelta": -15}]}
    ]
  },
  # === 刺史专属 (6张) ===
  {
    "CardId": "card_prefect_army_e007", "Title": "藩镇军饷",
    "Description": "驻军折冲府的粮饷已拖欠三月，士卒怨声载道。折冲都尉秦虎亲自登门讨要，言语间隐含威胁。而州府库银刚被调拨去修河堤\u2026\u2026",
    "Type": "Crisis", "Rarity": "Uncommon", "Repeatable": False,
    "MinTurn": 6, "MaxTurn": 0, "MinRank": "Prefect", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "挪用河堤款项先发军饷", "Tooltip": "解燃眉之急",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Martial", "Value": 5}, {"Attribute": "Reputation", "Value": -8}],
       "FollowUpCardId": "", "SetFlags": ["army_paid_diverted"],
       "RelationshipEffects": [{"NpcId": "npc_general", "FavorDelta": 15, "TrustDelta": 10}]},
      {"ChoiceText": "向辖下各县加征临时税", "Tooltip": "转嫁压力",
       "RequiredAttribute": "Scheming", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Scheming", "Value": 3}, {"Attribute": "Reputation", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["emergency_tax"],
       "RelationshipEffects": [{"NpcId": "npc_general", "FavorDelta": 8, "TrustDelta": 5}]},
      {"ChoiceText": "上书朝廷请拨军饷，安抚秦虎", "Tooltip": "远水不解近渴",
       "RequiredAttribute": "Charisma", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Charisma", "Value": 5}, {"Attribute": "Intellect", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["army_petition"],
       "RelationshipEffects": [{"NpcId": "npc_general", "FavorDelta": -5, "TrustDelta": -8}]}
    ]
  },
  {
    "CardId": "card_prefect_canal_e008", "Title": "漕运改道",
    "Description": "转运使衙门来文：因黄河泥沙淤积，漕运将改走新渠道，途经你管辖的渭北三县。若承接漕运中转，本州商贸将迎来黄金期；但修建转运码头耗资巨大。",
    "Type": "Decision", "Rarity": "Rare", "Repeatable": False,
    "MinTurn": 7, "MaxTurn": 0, "MinRank": "Prefect", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "全力争取，倾州之力修建码头", "Tooltip": "大手笔投资",
       "RequiredAttribute": "Intellect", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Intellect", "Value": 8}, {"Attribute": "Reputation", "Value": 5}],
       "FollowUpCardId": "card_prefect_canal_crisis_e009", "SetFlags": ["canal_build"],
       "RelationshipEffects": [{"NpcId": "npc_governor", "FavorDelta": 10, "TrustDelta": 8}]},
      {"ChoiceText": "与邻州联合，共建共享", "Tooltip": "折中稳妥",
       "RequiredAttribute": "Charisma", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Charisma", "Value": 8}, {"Attribute": "Intellect", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["canal_cooperate"],
       "RelationshipEffects": []},
      {"ChoiceText": "暗中使绊，让邻州出丑", "Tooltip": "阴谋",
       "RequiredAttribute": "Scheming", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Scheming", "Value": 8}, {"Attribute": "Reputation", "Value": -5}],
       "FollowUpCardId": "", "SetFlags": ["canal_sabotage"],
       "RelationshipEffects": []}
    ]
  },
  {
    "CardId": "card_prefect_canal_crisis_e009", "Title": "漕运风波",
    "Description": "码头建成通航不到两月，一艘漕船在辖区沉没，三千石官粮落入水底。转运使大怒。有人密报：沉船并非意外，而是有人故意凿穿船底\u2026\u2026",
    "Type": "Crisis", "Rarity": "Rare", "Repeatable": False,
    "MinTurn": 8, "MaxTurn": 0, "MinRank": "Prefect", "MaxRank": "GrandCouncilor",
    "RequiredFlags": ["canal_build"], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "组织打捞，同时追查凶手", "Tooltip": "文武双全",
       "RequiredAttribute": "Martial", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Martial", "Value": 5}, {"Attribute": "Intellect", "Value": 5}, {"Attribute": "Health", "Value": -8}],
       "FollowUpCardId": "", "SetFlags": ["canal_investigated"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 8, "TrustDelta": 10}]},
      {"ChoiceText": "自掏州库赔偿，息事宁人", "Tooltip": "花钱消灾",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": -5}],
       "FollowUpCardId": "", "SetFlags": ["canal_compensated"],
       "RelationshipEffects": [{"NpcId": "npc_governor", "FavorDelta": 5, "TrustDelta": -5}]},
      {"ChoiceText": "栽赃邻州，反咬一口", "Tooltip": "极端阴谋",
       "RequiredAttribute": "Scheming", "RequiredValue": 20,
       "Modifiers": [{"Attribute": "Scheming", "Value": 10}, {"Attribute": "Reputation", "Value": -8}],
       "FollowUpCardId": "", "SetFlags": ["canal_framed"],
       "RelationshipEffects": []}
    ]
  },
  {
    "CardId": "card_prefect_conscript_e010", "Title": "节度使征兵",
    "Description": "节度使府传来军令：征发本州壮丁三千人，限一月内送至幽州前线。正值农忙时节，壮丁离乡则秋粮无人收割。但抗命不遵，则是违抗军令的大罪。",
    "Type": "Decision", "Rarity": "Uncommon", "Repeatable": False,
    "MinTurn": 7, "MaxTurn": 0, "MinRank": "Prefect", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "照令征发，不折不扣", "Tooltip": "服从军令",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": -12}, {"Attribute": "Martial", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["conscript_full"],
       "RelationshipEffects": [{"NpcId": "npc_general", "FavorDelta": 12, "TrustDelta": 8}]},
      {"ChoiceText": "以老弱充数，暗中保护壮丁", "Tooltip": "暗度陈仓",
       "RequiredAttribute": "Scheming", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Scheming", "Value": 8}, {"Attribute": "Reputation", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["conscript_fake"],
       "RelationshipEffects": [{"NpcId": "npc_general", "FavorDelta": -10, "TrustDelta": -15}]},
      {"ChoiceText": "上书陈情，请求减免名额", "Tooltip": "据理力争",
       "RequiredAttribute": "Charisma", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Charisma", "Value": 5}, {"Attribute": "Intellect", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["conscript_petition"],
       "RelationshipEffects": [{"NpcId": "npc_general", "FavorDelta": -5, "TrustDelta": -3}]}
    ]
  },
  {
    "CardId": "card_prefect_exam_e011", "Title": "州试舞弊案",
    "Description": "州试放榜后，落第士子集体到州府门前喊冤，控诉今年的解元乃是考官亲侄，试卷中竟有三道题与考前泄露的秘题完全一致。此事若处理不当，恐怕会惊动朝廷。",
    "Type": "Decision", "Rarity": "Rare", "Repeatable": False,
    "MinTurn": 8, "MaxTurn": 0, "MinRank": "Prefect", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "彻查科场，革除考官，重新开考", "Tooltip": "正本清源",
       "RequiredAttribute": "Intellect", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Reputation", "Value": 15}, {"Attribute": "Intellect", "Value": 8}],
       "FollowUpCardId": "", "SetFlags": ["exam_purge"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 15, "TrustDelta": 15}]},
      {"ChoiceText": "安抚士子，暗中处理考官", "Tooltip": "低调处理",
       "RequiredAttribute": "Charisma", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Charisma", "Value": 5}, {"Attribute": "Scheming", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["exam_quiet"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -5, "TrustDelta": -8}]},
      {"ChoiceText": "压下此事，以维护科举权威为名", "Tooltip": "掩盖真相",
       "RequiredAttribute": "Scheming", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Scheming", "Value": 5}, {"Attribute": "Reputation", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["exam_coverup"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -15, "TrustDelta": -20}]}
    ]
  },
  {
    "CardId": "card_prefect_salt_e012", "Title": "盐铁之利",
    "Description": "本州境内发现一处天然盐泉。按制，盐铁归朝廷专营，应即刻上报。但若暗中先行开采，收益极为可观。幕僚分成两派，争论不休。",
    "Type": "Opportunity", "Rarity": "Uncommon", "Repeatable": False,
    "MinTurn": 7, "MaxTurn": 0, "MinRank": "Prefect", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "立即上报朝廷，请求设盐监", "Tooltip": "光明正大",
       "RequiredAttribute": "Intellect", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Reputation", "Value": 8}, {"Attribute": "Intellect", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["salt_reported"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 10, "TrustDelta": 10}, {"NpcId": "npc_governor", "FavorDelta": 5, "TrustDelta": 5}]},
      {"ChoiceText": "先行开采三月，赚够再报", "Tooltip": "先捞一把",
       "RequiredAttribute": "Scheming", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Scheming", "Value": 8}],
       "FollowUpCardId": "", "SetFlags": ["salt_secret_mine"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -10, "TrustDelta": -15}]},
      {"ChoiceText": "与盐商合营，利益均沾", "Tooltip": "官商勾结",
       "RequiredAttribute": "Charisma", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Charisma", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["salt_partnership"],
       "RelationshipEffects": [{"NpcId": "npc_gentry", "FavorDelta": 10, "TrustDelta": 8}]}
    ]
  },
  # === 侍郎专属 (6张) ===
  {
    "CardId": "card_vice_debate_e013", "Title": "朝堂论战",
    "Description": "大朝会上，改革派首辅提出两税法改革。保守派群臣激烈反对。天子目光落在你身上：\u201c卿以为如何？\u201d",
    "Type": "Decision", "Rarity": "Common", "Repeatable": False,
    "MinTurn": 10, "MaxTurn": 0, "MinRank": "ViceMinister", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "力挺两税法，陈述地方实情", "Tooltip": "旗帜鲜明",
       "RequiredAttribute": "Intellect", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Reputation", "Value": 10}, {"Attribute": "Intellect", "Value": 8}],
       "FollowUpCardId": "", "SetFlags": ["support_reform"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 15, "TrustDelta": 10}, {"NpcId": "npc_zhongshu_cui", "FavorDelta": -15, "TrustDelta": -10}]},
      {"ChoiceText": "附和保守派，维护祖制", "Tooltip": "保守安全",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": -5}, {"Attribute": "Charisma", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["support_tradition"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -15, "TrustDelta": -10}, {"NpcId": "npc_zhongshu_cui", "FavorDelta": 10, "TrustDelta": 8}]},
      {"ChoiceText": "折中发言，各取所长", "Tooltip": "高难度圆滑",
       "RequiredAttribute": "Charisma", "RequiredValue": 20,
       "Modifiers": [{"Attribute": "Charisma", "Value": 10}, {"Attribute": "Intellect", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["reform_moderate"],
       "RelationshipEffects": []}
    ]
  },
  {
    "CardId": "card_vice_eunuch_e014", "Title": "宦官矫诏",
    "Description": "深夜密报：内侍省的高力借传旨之机，私自篡改了一道关于边将任免的诏书。宦官集团正在染指军权。你手中握有证据，但高力耳目遍布宫禁\u2026\u2026",
    "Type": "Crisis", "Rarity": "Rare", "Repeatable": False,
    "MinTurn": 11, "MaxTurn": 0, "MinRank": "ViceMinister", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "联合御史台，公开弹劾", "Tooltip": "正面对决",
       "RequiredAttribute": "Intellect", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Reputation", "Value": 15}, {"Attribute": "Health", "Value": -15}],
       "FollowUpCardId": "card_vice_coup_e015", "SetFlags": ["eunuch_impeached"],
       "RelationshipEffects": [{"NpcId": "npc_eunuch", "FavorDelta": -50, "TrustDelta": -50}, {"NpcId": "npc_censor", "FavorDelta": 20, "TrustDelta": 20}]},
      {"ChoiceText": "密奏天子，私下进言", "Tooltip": "暗中行动",
       "RequiredAttribute": "Scheming", "RequiredValue": 20,
       "Modifiers": [{"Attribute": "Scheming", "Value": 10}, {"Attribute": "Health", "Value": -8}],
       "FollowUpCardId": "", "SetFlags": ["eunuch_secret_report"],
       "RelationshipEffects": [{"NpcId": "npc_eunuch", "FavorDelta": -20, "TrustDelta": -30}]},
      {"ChoiceText": "隐忍不发，等待时机", "Tooltip": "留得青山在",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Scheming", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["eunuch_evidence_held"],
       "RelationshipEffects": []}
    ]
  },
  {
    "CardId": "card_vice_coup_e015", "Title": "宫禁之变",
    "Description": "弹劾奏章呈上的当夜，高力先发制人\u2014\u2014调动神策军封锁宫门，以\u201c清君侧\u201d为名软禁天子。你与御史台数名官员被困在中书省，外面甲士环伺。生死就在今夜。",
    "Type": "Crisis", "Rarity": "Epic", "Repeatable": False,
    "MinTurn": 12, "MaxTurn": 0, "MinRank": "ViceMinister", "MaxRank": "GrandCouncilor",
    "RequiredFlags": ["eunuch_impeached"], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "联络禁军旧部，发动反击", "Tooltip": "孤注一掷",
       "RequiredAttribute": "Martial", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Martial", "Value": 10}, {"Attribute": "Reputation", "Value": 20}, {"Attribute": "Health", "Value": -20}],
       "FollowUpCardId": "", "SetFlags": ["coup_crushed"],
       "RelationshipEffects": [{"NpcId": "npc_eunuch", "FavorDelta": -50, "TrustDelta": -50}, {"NpcId": "npc_censor", "FavorDelta": 25, "TrustDelta": 25}]},
      {"ChoiceText": "从密道出逃，联络外镇勤王", "Tooltip": "走为上计",
       "RequiredAttribute": "Scheming", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Scheming", "Value": 8}, {"Attribute": "Health", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["coup_escaped"],
       "RelationshipEffects": []},
      {"ChoiceText": "投降高力，以退为进", "Tooltip": "卧薪尝胆？",
       "RequiredAttribute": "Scheming", "RequiredValue": 20,
       "Modifiers": [{"Attribute": "Scheming", "Value": 12}, {"Attribute": "Reputation", "Value": -20}, {"Attribute": "Charisma", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["coup_surrendered"],
       "RelationshipEffects": [{"NpcId": "npc_eunuch", "FavorDelta": 15, "TrustDelta": -10}, {"NpcId": "npc_censor", "FavorDelta": -30, "TrustDelta": -30}]}
    ]
  },
  {
    "CardId": "card_vice_personnel_e016", "Title": "吏部铨选",
    "Description": "你奉命主持本年吏部铨选。恩师暗示安排自己人，改革派送来推荐名单，御史台检举名单中有三人涉嫌行贿买官。",
    "Type": "Decision", "Rarity": "Common", "Repeatable": False,
    "MinTurn": 10, "MaxTurn": 0, "MinRank": "ViceMinister", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "严格按考核选拔，不徇私情", "Tooltip": "刚正不阿",
       "RequiredAttribute": "Intellect", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Reputation", "Value": 12}, {"Attribute": "Intellect", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["select_fair"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 10, "TrustDelta": 15}]},
      {"ChoiceText": "照顾各方面子，平衡分配", "Tooltip": "和事佬",
       "RequiredAttribute": "Charisma", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Charisma", "Value": 5}, {"Attribute": "Scheming", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["select_balanced"],
       "RelationshipEffects": []},
      {"ChoiceText": "卖人情给改革派，换取支持", "Tooltip": "政治交易",
       "RequiredAttribute": "Scheming", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Scheming", "Value": 8}, {"Attribute": "Reputation", "Value": -5}],
       "FollowUpCardId": "", "SetFlags": ["select_political"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -5, "TrustDelta": -5}]}
    ]
  },
  {
    "CardId": "card_vice_party_e017", "Title": "牛李党争",
    "Description": "朝堂上牛党与李党之争已达白热化。两党各有宰相撑腰，中间地带越来越窄。据说天子即将做出抉择，站错队的人将万劫不复。",
    "Type": "Decision", "Rarity": "Epic", "Repeatable": False,
    "MinTurn": 11, "MaxTurn": 0, "MinRank": "ViceMinister", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "投身牛党，赌科举派得势", "Tooltip": "押注改革",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Charisma", "Value": 5}, {"Attribute": "Intellect", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["join_niu"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 20, "TrustDelta": 15}, {"NpcId": "npc_zhongshu_cui", "FavorDelta": -20, "TrustDelta": -15}]},
      {"ChoiceText": "投身李党，赌世族派得势", "Tooltip": "押注保守",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Martial", "Value": 5}, {"Attribute": "Scheming", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["join_li"],
       "RelationshipEffects": [{"NpcId": "npc_zhongshu_cui", "FavorDelta": 20, "TrustDelta": 15}, {"NpcId": "npc_censor", "FavorDelta": -20, "TrustDelta": -15}]},
      {"ChoiceText": "两面下注，暗中脚踩两条船", "Tooltip": "高难度走钢丝",
       "RequiredAttribute": "Scheming", "RequiredValue": 22,
       "Modifiers": [{"Attribute": "Scheming", "Value": 10}, {"Attribute": "Reputation", "Value": -8}],
       "FollowUpCardId": "", "SetFlags": ["party_double"],
       "RelationshipEffects": []}
    ]
  },
  {
    "CardId": "card_vice_lecture_e018", "Title": "经筵进讲",
    "Description": "天子诏令你在经筵上进讲《贞观政要》。这是直达天听的绝佳机会。你需要在歌功颂德和借古讽今之间拿捏分寸。",
    "Type": "Opportunity", "Rarity": "Uncommon", "Repeatable": False,
    "MinTurn": 10, "MaxTurn": 0, "MinRank": "ViceMinister", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "借太宗纳谏之事，暗谏当今弊政", "Tooltip": "以古讽今",
       "RequiredAttribute": "Intellect", "RequiredValue": 20,
       "Modifiers": [{"Attribute": "Reputation", "Value": 15}, {"Attribute": "Intellect", "Value": 10}, {"Attribute": "Health", "Value": -5}],
       "FollowUpCardId": "", "SetFlags": ["lecture_admonish"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 15, "TrustDelta": 15}, {"NpcId": "npc_eunuch", "FavorDelta": -15, "TrustDelta": -10}]},
      {"ChoiceText": "中规中矩，稳扎稳打", "Tooltip": "不出彩也不出错",
       "RequiredAttribute": "Intellect", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Intellect", "Value": 5}, {"Attribute": "Charisma", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["lecture_safe"],
       "RelationshipEffects": []},
      {"ChoiceText": "极尽颂扬，讨天子欢心", "Tooltip": "阿谀奉承",
       "RequiredAttribute": "Charisma", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Charisma", "Value": 5}, {"Attribute": "Reputation", "Value": -5}],
       "FollowUpCardId": "", "SetFlags": ["lecture_flatter"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -10, "TrustDelta": -10}, {"NpcId": "npc_eunuch", "FavorDelta": 8, "TrustDelta": 5}]}
    ]
  },
  # === 高阶通用 (2张) ===
  {
    "CardId": "card_universal_famine_e019", "Title": "天灾人祸",
    "Description": "辖区遭遇百年不遇的大旱，紧接着蝗灾铺天盖地。饿殍遍野，流民四起。朝廷忙于边患，赈灾拨款遥遥无期。",
    "Type": "Crisis", "Rarity": "Uncommon", "Repeatable": False,
    "MinTurn": 5, "MaxTurn": 0, "MinRank": "CountyMagistrate", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "开常平仓，同时组织以工代赈", "Tooltip": "教科书级应对",
       "RequiredAttribute": "Intellect", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Reputation", "Value": 15}, {"Attribute": "Intellect", "Value": 8}],
       "FollowUpCardId": "", "SetFlags": ["famine_managed"],
       "RelationshipEffects": [{"NpcId": "npc_governor", "FavorDelta": 5, "TrustDelta": 10}]},
      {"ChoiceText": "向富户劝捐，实为逼捐", "Tooltip": "劫富济贫",
       "RequiredAttribute": "Martial", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Scheming", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["famine_forced_donation"],
       "RelationshipEffects": [{"NpcId": "npc_gentry", "FavorDelta": -20, "TrustDelta": -15}]},
      {"ChoiceText": "封锁消息，防止流民涌入", "Tooltip": "消极应对",
       "RequiredAttribute": "Scheming", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Scheming", "Value": 3}, {"Attribute": "Reputation", "Value": -15}],
       "FollowUpCardId": "", "SetFlags": ["famine_sealed"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -15, "TrustDelta": -20}]}
    ]
  },
  {
    "CardId": "card_universal_old_friend_e020", "Title": "故人来访",
    "Description": "一位旧日同窗找上门来。才华横溢却屡试不第，穷困潦倒。收留他或许能得一幕僚之才，但此人性情孤傲。",
    "Type": "Character", "Rarity": "Common", "Repeatable": False,
    "MinTurn": 3, "MaxTurn": 0, "MinRank": "CountyMagistrate", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "延为幕僚，量才录用", "Tooltip": "得一才士",
       "RequiredAttribute": "Charisma", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Intellect", "Value": 8}, {"Attribute": "Charisma", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["friend_hired"],
       "RelationshipEffects": [{"NpcId": "npc_deputy", "FavorDelta": -3, "TrustDelta": -3}]},
      {"ChoiceText": "赠以盘缠，婉言相送", "Tooltip": "人情周到",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": 3}, {"Attribute": "Charisma", "Value": 2}],
       "FollowUpCardId": "", "SetFlags": [],
       "RelationshipEffects": []},
      {"ChoiceText": "举荐给他人，借花献佛", "Tooltip": "人情到位",
       "RequiredAttribute": "Scheming", "RequiredValue": 10,
       "Modifiers": [{"Attribute": "Scheming", "Value": 3}, {"Attribute": "Charisma", "Value": 3}],
       "FollowUpCardId": "", "SetFlags": ["friend_referred"],
       "RelationshipEffects": [{"NpcId": "npc_governor", "FavorDelta": 8, "TrustDelta": 5}]}
    ]
  },
  # === 历史事件卡 (4张) ===
  {
    "CardId": "card_hist_anshi_h001", "Title": "安史之乱爆发",
    "Description": "天宝十四载十一月，范阳节度使安禄山以清君侧为名起兵叛乱，十五万铁骑南下。洛阳陷落、潼关失守，天子仓皇西逃。乱世降临，你将何去何从？",
    "Type": "Crisis", "Rarity": "Legendary", "Repeatable": False,
    "MinTurn": 15, "MaxTurn": 0, "MinRank": "Prefect", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "追随天子，护驾入蜀", "Tooltip": "忠心耿耿",
       "RequiredAttribute": "Intellect", "RequiredValue": 0,
       "Modifiers": [{"Attribute": "Reputation", "Value": 10}, {"Attribute": "Martial", "Value": -5}, {"Attribute": "Health", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["anshi_follow_emperor"],
       "RelationshipEffects": [{"NpcId": "npc_fuma_zheng", "FavorDelta": 20, "TrustDelta": 15}]},
      {"ChoiceText": "坚守辖地，组织抵抗", "Tooltip": "力挽狂澜",
       "RequiredAttribute": "Martial", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Martial", "Value": 15}, {"Attribute": "Reputation", "Value": 20}, {"Attribute": "Health", "Value": -20}],
       "FollowUpCardId": "", "SetFlags": ["anshi_resist"],
       "RelationshipEffects": [{"NpcId": "npc_general", "FavorDelta": 25, "TrustDelta": 20}]},
      {"ChoiceText": "审时度势，暂降叛军", "Tooltip": "苟活但声名扫地",
       "RequiredAttribute": "Scheming", "RequiredValue": 20,
       "Modifiers": [{"Attribute": "Scheming", "Value": 10}, {"Attribute": "Reputation", "Value": -30}, {"Attribute": "Charisma", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["anshi_surrender"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -30, "TrustDelta": -40}, {"NpcId": "npc_general", "FavorDelta": -30, "TrustDelta": -40}]}
    ]
  },
  {
    "CardId": "card_hist_keju_h002", "Title": "科举改革",
    "Description": "天子颁诏改革科举：废除公荐旧制，改由糊名阅卷。寒门拥护，世族抵制。作为朝中要员，你不可能置身事外。",
    "Type": "Decision", "Rarity": "Epic", "Repeatable": False,
    "MinTurn": 10, "MaxTurn": 0, "MinRank": "ViceMinister", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "全力推动改革，亲自监督施行", "Tooltip": "站在历史正确一边",
       "RequiredAttribute": "Intellect", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Reputation", "Value": 15}, {"Attribute": "Intellect", "Value": 10}],
       "FollowUpCardId": "", "SetFlags": ["keju_reform_push"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 20, "TrustDelta": 15}, {"NpcId": "npc_zhongshu_cui", "FavorDelta": -20, "TrustDelta": -15}]},
      {"ChoiceText": "阳奉阴违，暗中留下操作空间", "Tooltip": "两面三刀",
       "RequiredAttribute": "Scheming", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Scheming", "Value": 10}],
       "FollowUpCardId": "", "SetFlags": ["keju_reform_fake"],
       "RelationshipEffects": [{"NpcId": "npc_zhongshu_cui", "FavorDelta": 10, "TrustDelta": 5}]},
      {"ChoiceText": "上书建议渐进改革，折中推行", "Tooltip": "温和路线",
       "RequiredAttribute": "Intellect", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Intellect", "Value": 5}, {"Attribute": "Charisma", "Value": 5}],
       "FollowUpCardId": "", "SetFlags": ["keju_reform_gradual"],
       "RelationshipEffects": []}
    ]
  },
  {
    "CardId": "card_hist_fanzhen_h003", "Title": "藩镇割据",
    "Description": "河朔三镇已成事实上的独立王国。你所辖之州正处于朝廷直辖区与藩镇势力范围的交界地带。两边都在拉拢你。",
    "Type": "Crisis", "Rarity": "Epic", "Repeatable": False,
    "MinTurn": 8, "MaxTurn": 0, "MinRank": "Prefect", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "坚定站在朝廷一边，拒绝藩镇拉拢", "Tooltip": "忠于朝廷",
       "RequiredAttribute": "Martial", "RequiredValue": 12,
       "Modifiers": [{"Attribute": "Reputation", "Value": 12}, {"Attribute": "Martial", "Value": 5}, {"Attribute": "Health", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["fanzhen_resist"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": 15, "TrustDelta": 15}, {"NpcId": "npc_general", "FavorDelta": -10, "TrustDelta": -8}]},
      {"ChoiceText": "暗中与藩镇勾连，两头下注", "Tooltip": "骑墙",
       "RequiredAttribute": "Scheming", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Scheming", "Value": 10}, {"Attribute": "Reputation", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["fanzhen_double"],
       "RelationshipEffects": [{"NpcId": "npc_general", "FavorDelta": 10, "TrustDelta": 5}]},
      {"ChoiceText": "效仿藩镇，壮大自己的地方势力", "Tooltip": "割据一方",
       "RequiredAttribute": "Martial", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Martial", "Value": 10}, {"Attribute": "Scheming", "Value": 8}, {"Attribute": "Reputation", "Value": -15}],
       "FollowUpCardId": "", "SetFlags": ["fanzhen_self"],
       "RelationshipEffects": [{"NpcId": "npc_censor", "FavorDelta": -25, "TrustDelta": -30}, {"NpcId": "npc_general", "FavorDelta": 15, "TrustDelta": 10}]}
    ]
  },
  {
    "CardId": "card_hist_ganlu_h004", "Title": "甘露之变",
    "Description": "大和九年，天子与宰相密谋诛杀宦官仇士良。消息走漏，仇士良反扑。你在含元殿，殿外兵刃相交，宫门即将关闭。",
    "Type": "Crisis", "Rarity": "Legendary", "Repeatable": False,
    "MinTurn": 13, "MaxTurn": 0, "MinRank": "ViceMinister", "MaxRank": "GrandCouncilor",
    "RequiredFlags": [], "ExcludedFlags": [], "RequiresMetaUnlock": False, "MetaUnlockId": "",
    "Choices": [
      {"ChoiceText": "冲出宫门，逃出长安", "Tooltip": "保命要紧",
       "RequiredAttribute": "Martial", "RequiredValue": 15,
       "Modifiers": [{"Attribute": "Martial", "Value": 8}, {"Attribute": "Health", "Value": -15}, {"Attribute": "Reputation", "Value": -5}],
       "FollowUpCardId": "", "SetFlags": ["ganlu_escaped"],
       "RelationshipEffects": []},
      {"ChoiceText": "留在殿中，与天子共进退", "Tooltip": "忠烈千秋",
       "RequiredAttribute": "Charisma", "RequiredValue": 18,
       "Modifiers": [{"Attribute": "Reputation", "Value": 20}, {"Attribute": "Charisma", "Value": 10}, {"Attribute": "Health", "Value": -25}],
       "FollowUpCardId": "", "SetFlags": ["ganlu_loyal"],
       "RelationshipEffects": [{"NpcId": "npc_fuma_zheng", "FavorDelta": 30, "TrustDelta": 25}]},
      {"ChoiceText": "伪装效忠宦官，暗中保护同僚", "Tooltip": "暗中周旋",
       "RequiredAttribute": "Scheming", "RequiredValue": 22,
       "Modifiers": [{"Attribute": "Scheming", "Value": 12}, {"Attribute": "Reputation", "Value": -10}],
       "FollowUpCardId": "", "SetFlags": ["ganlu_undercover"],
       "RelationshipEffects": [{"NpcId": "npc_shence_qiu", "FavorDelta": 10, "TrustDelta": -5}, {"NpcId": "npc_censor", "FavorDelta": 20, "TrustDelta": 20}]}
    ]
  }
]

# ============================================================
# 6个新NPC
# ============================================================
new_npcs = [
  {
    "NpcId": "npc_changshi_pei", "DisplayName": "长史 裴行俭",
    "Biography": "州长史，出身河东裴氏旁支，明经及第后在地方辗转二十余年。精通唐律，但性情极为执拗。曾一举扳倒违法的上任刺史，在州府中人人忌惮。",
    "Faction": "Reformist", "Rank": "Prefect",
    "Traits": ["Righteous", "Loyal"],
    "InitialFavor": 5, "InitialTrust": -5,
    "Reactions": [
      {"ActionType": "Politics", "FavorReaction": 3, "TrustReaction": 8},
      {"ActionType": "Social", "FavorReaction": -1, "TrustReaction": -2},
      {"ActionType": "Intelligence", "FavorReaction": 5, "TrustReaction": 5},
      {"ActionType": "SelfImprove", "FavorReaction": 2, "TrustReaction": 3},
      {"ActionType": "Scheme", "FavorReaction": -8, "TrustReaction": -12},
      {"ActionType": "Rest", "FavorReaction": -1, "TrustReaction": -1}
    ],
    "StartsActive": False, "AppearTurn": 7, "AppearMinRank": "Prefect",
    "PersonalCardIds": [],
    "AggressionWeight": 0.15, "AllianceWeight": 0.7, "BetrayalWeight": 0.01
  },
  {
    "NpcId": "npc_tuanlian_han", "DisplayName": "团练使 韩令坤",
    "Biography": "团练使，出身行伍，与吐蕃边战立功。武艺超群、治军有方，瞧不起科举出身的文官。与折冲都尉秦虎是旧交。",
    "Faction": "Military", "Rank": "Prefect",
    "Traits": ["Ambitious", "Loyal"],
    "InitialFavor": 0, "InitialTrust": -10,
    "Reactions": [
      {"ActionType": "Politics", "FavorReaction": -2, "TrustReaction": -1},
      {"ActionType": "Social", "FavorReaction": 3, "TrustReaction": 2},
      {"ActionType": "Intelligence", "FavorReaction": -1, "TrustReaction": -2},
      {"ActionType": "SelfImprove", "FavorReaction": 5, "TrustReaction": 5},
      {"ActionType": "Scheme", "FavorReaction": -5, "TrustReaction": -8},
      {"ActionType": "Rest", "FavorReaction": -2, "TrustReaction": -2}
    ],
    "StartsActive": False, "AppearTurn": 7, "AppearMinRank": "Prefect",
    "PersonalCardIds": [],
    "AggressionWeight": 0.4, "AllianceWeight": 0.4, "BetrayalWeight": 0.15
  },
  {
    "NpcId": "npc_zhuanyun_liu", "DisplayName": "转运判官 柳如烟",
    "Biography": "转运判官，精于财务。真实身份是宦官高力安插在地方的耳目，但她有自己的野心，并非简单的走狗。好感度和信任度经常反向变动。",
    "Faction": "Neutral", "Rank": "Prefect",
    "Traits": ["Cunning", "Greedy"],
    "InitialFavor": 15, "InitialTrust": -15,
    "Reactions": [
      {"ActionType": "Politics", "FavorReaction": 1, "TrustReaction": -2},
      {"ActionType": "Social", "FavorReaction": 5, "TrustReaction": 3},
      {"ActionType": "Intelligence", "FavorReaction": 2, "TrustReaction": -3},
      {"ActionType": "SelfImprove", "FavorReaction": 0, "TrustReaction": 0},
      {"ActionType": "Scheme", "FavorReaction": 3, "TrustReaction": 2},
      {"ActionType": "Rest", "FavorReaction": 1, "TrustReaction": 0}
    ],
    "StartsActive": False, "AppearTurn": 8, "AppearMinRank": "Prefect",
    "PersonalCardIds": [],
    "AggressionWeight": 0.2, "AllianceWeight": 0.5, "BetrayalWeight": 0.45
  },
  {
    "NpcId": "npc_zhongshu_cui", "DisplayName": "中书舍人 崔玄靖",
    "Biography": "清河崔氏，五姓七望之首。才华出众但骨子里傲慢，是李党中坚力量。掌管诏令起草权，一字之差就能改变政令走向。",
    "Faction": "Conservative", "Rank": "ViceMinister",
    "Traits": ["Cunning", "Ambitious"],
    "InitialFavor": -5, "InitialTrust": -15,
    "Reactions": [
      {"ActionType": "Politics", "FavorReaction": 2, "TrustReaction": 3},
      {"ActionType": "Social", "FavorReaction": 3, "TrustReaction": 2},
      {"ActionType": "Intelligence", "FavorReaction": -3, "TrustReaction": -3},
      {"ActionType": "SelfImprove", "FavorReaction": 5, "TrustReaction": 3},
      {"ActionType": "Scheme", "FavorReaction": 2, "TrustReaction": -1},
      {"ActionType": "Rest", "FavorReaction": 0, "TrustReaction": -1}
    ],
    "StartsActive": False, "AppearTurn": 10, "AppearMinRank": "ViceMinister",
    "PersonalCardIds": [],
    "AggressionWeight": 0.35, "AllianceWeight": 0.3, "BetrayalWeight": 0.25
  },
  {
    "NpcId": "npc_shence_qiu", "DisplayName": "神策军中尉 仇士良",
    "Biography": "宦官政治的终极代表，入宫四十年，操纵过两次皇位更替。掌控禁军，对权力嗅觉超群。极端记仇，爱钱如命。",
    "Faction": "Eunuch", "Rank": "ViceMinister",
    "Traits": ["Cunning", "Vengeful", "Greedy"],
    "InitialFavor": 0, "InitialTrust": -20,
    "Reactions": [
      {"ActionType": "Politics", "FavorReaction": -1, "TrustReaction": -2},
      {"ActionType": "Social", "FavorReaction": 5, "TrustReaction": 3},
      {"ActionType": "Intelligence", "FavorReaction": 3, "TrustReaction": -3},
      {"ActionType": "SelfImprove", "FavorReaction": -2, "TrustReaction": -1},
      {"ActionType": "Scheme", "FavorReaction": 3, "TrustReaction": 2},
      {"ActionType": "Rest", "FavorReaction": 0, "TrustReaction": 0}
    ],
    "StartsActive": False, "AppearTurn": 10, "AppearMinRank": "ViceMinister",
    "PersonalCardIds": ["card_hist_ganlu_h004", "card_vice_eunuch_e014"],
    "AggressionWeight": 0.5, "AllianceWeight": 0.2, "BetrayalWeight": 0.6
  },
  {
    "NpcId": "npc_fuma_zheng", "DisplayName": "驸马都尉 郑颢",
    "Biography": "荥阳郑氏才子，状元及第被招为驸马。内心矛盾——有治世之才却因驸马身份受限。朝堂上最特殊的存在，既是天子至亲又是最孤立的局外人。",
    "Faction": "Neutral", "Rank": "ViceMinister",
    "Traits": ["Loyal", "Righteous"],
    "InitialFavor": 10, "InitialTrust": 5,
    "Reactions": [
      {"ActionType": "Politics", "FavorReaction": 0, "TrustReaction": -1},
      {"ActionType": "Social", "FavorReaction": 3, "TrustReaction": 2},
      {"ActionType": "Intelligence", "FavorReaction": 0, "TrustReaction": -1},
      {"ActionType": "SelfImprove", "FavorReaction": 5, "TrustReaction": 5},
      {"ActionType": "Scheme", "FavorReaction": -3, "TrustReaction": -5},
      {"ActionType": "Rest", "FavorReaction": 2, "TrustReaction": 1}
    ],
    "StartsActive": False, "AppearTurn": 11, "AppearMinRank": "ViceMinister",
    "PersonalCardIds": [],
    "AggressionWeight": 0.05, "AllianceWeight": 0.6, "BetrayalWeight": 0.05
  }
]

# ============================================================
# 合并写入
# ============================================================
card_path = os.path.join(data_dir, 'CardDatabase.json')
npc_path = os.path.join(data_dir, 'NPCDatabase.json')

with open(card_path, 'r', encoding='utf-8') as f:
    card_db = json.load(f)
with open(npc_path, 'r', encoding='utf-8') as f:
    npc_db = json.load(f)

existing_card_ids = {c['CardId'] for c in card_db['Cards']}
existing_npc_ids = {n['NpcId'] for n in npc_db['NPCs']}

cards_added = 0
for card in new_cards:
    if card['CardId'] not in existing_card_ids:
        card_db['Cards'].append(card)
        cards_added += 1
    else:
        print(f"跳过已存在: {card['CardId']}")

npcs_added = 0
for npc in new_npcs:
    if npc['NpcId'] not in existing_npc_ids:
        npc_db['NPCs'].append(npc)
        npcs_added += 1
    else:
        print(f"跳过已存在: {npc['NpcId']}")

with open(card_path, 'w', encoding='utf-8') as f:
    json.dump(card_db, f, ensure_ascii=False, indent=2)
with open(npc_path, 'w', encoding='utf-8') as f:
    json.dump(npc_db, f, ensure_ascii=False, indent=2)

print(f"\n✅ 数据合并完成！")
print(f"   卡牌：{len(card_db['Cards'])} 张（新增 {cards_added} 张）")
print(f"   NPC：{len(npc_db['NPCs'])} 个（新增 {npcs_added} 个）")
