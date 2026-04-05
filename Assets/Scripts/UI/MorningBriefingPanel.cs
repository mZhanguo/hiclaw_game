// ============================================================================
// 官途浮沉 - 早朝简报面板
// MorningBriefingPanel.cs — 每回合开始时的局势概览
// ============================================================================
// 视觉设计：
//   模拟早朝上朝场景 — 屏幕上方"太极殿"横额
//   中央区域：竹简/奏折样式展示本回合信息
//   - 左侧：当前局势摘要（朝堂势力分布简图）
//   - 右侧：本回合事件预告列表（卷轴展开动画）
//   底部："上朝" 按钮（朱砂色大按钮，点击推进到行动分配）
//   背景：朦胧的宫殿廊柱剪影，淡墨渲染
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GuantuFucheng.Core;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;
using GuantuFucheng.Models;

namespace GuantuFucheng.UI
{
    /// <summary>
    /// 早朝简报面板 — 展示当前回合的局势与事件预告
    /// 
    /// 信息展示：
    /// 1. 回合数 & 当前官职
    /// 2. 朝堂局势概要（哪些势力活跃）
    /// 3. 本回合触发的事件预告
    /// 4. NPC动态提醒（谁在酝酿什么）
    /// 5. 资源概览（金银、影响力）
    /// </summary>
    public class MorningBriefingPanel : UIPanel
    {
        public override string PanelName => "早朝简报";

        [Header("=== 标题区 ===")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI turnInfoText;

        [Header("=== 局势摘要 ===")]
        [Tooltip("局势描述文本")]
        [SerializeField] private TextMeshProUGUI situationText;

        [Header("=== 事件预告 ===")]
        [Tooltip("事件列表的父容器")]
        [SerializeField] private Transform eventListContainer;

        [Tooltip("事件条目预制体")]
        [SerializeField] private GameObject eventItemPrefab;

        [Header("=== NPC动态 ===")]
        [SerializeField] private TextMeshProUGUI npcActivityText;

        [Header("=== 资源概览 ===")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI influenceText;

        [Header("=== 操作按钮 ===")]
        [Tooltip("\"上朝\"按钮 — 确认后进入行动分配阶段")]
        [SerializeField] private Button proceedButton;
        [SerializeField] private TextMeshProUGUI proceedButtonText;

        protected override void Awake()
        {
            base.Awake();
            proceedButton?.onClick.AddListener(OnProceedClicked);
        }

        protected override void OnShow()
        {
            var run = GameManager.Instance.CurrentRun;
            var player = run.Player;
            var turn = TurnManager.Instance.CurrentTurnNumber;

            // 标题：模拟朝堂点名
            if (headerText != null)
            {
                headerText.text = "太极殿 · 早朝";
                headerText.color = UIConfig.InkBlack;
                headerText.fontSize = UIConfig.FontSizeTitle;
            }

            // 回合信息
            if (turnInfoText != null)
            {
                string rankName = GetRankDisplayName(player.CurrentRank);
                turnInfoText.text = $"第 {turn} 回合    {rankName} · {player.PlayerName}";
                turnInfoText.color = UIConfig.InkMedium;
                turnInfoText.fontSize = UIConfig.FontSizeBody;
            }

            // 局势摘要
            RefreshSituation(run);

            // 事件预告
            RefreshEventList();

            // NPC动态
            RefreshNPCActivity(run);

            // 资源
            if (goldText != null)
            {
                goldText.text = $"金银：{player.Gold}";
                goldText.color = UIConfig.OchreGold;
            }
            if (influenceText != null)
            {
                influenceText.text = $"影响力：{player.Influence}";
                influenceText.color = UIConfig.IndigoBlue;
            }

            // 按钮文字
            if (proceedButtonText != null)
            {
                proceedButtonText.text = "上朝";
                proceedButtonText.color = UIConfig.PaperColor;
            }
        }

        // ======================== 数据刷新 ========================

        /// <summary>刷新朝堂局势描述</summary>
        private void RefreshSituation(RunState run)
        {
            if (situationText == null) return;

            // TODO: 从FactionSystem获取当前势力格局
            // 现在用占位文本
            int turn = TurnManager.Instance.CurrentTurnNumber;

            string situation = turn switch
            {
                <= 3 => "朝堂平静，百官各安其位。新科进士初入仕途，诸方势力尚在观望。",
                <= 8 => "近日朝堂暗流涌动，改革派与保守派各执一词，陛下态度不明。",
                <= 15 => "局势渐趋紧张，边关传来急报，几方势力借机生事。",
                _ => "朝堂风云变幻，大势已成定局，最终的博弈即将到来。"
            };

            situationText.text = situation;
            situationText.color = UIConfig.InkBlack;
            situationText.fontSize = UIConfig.FontSizeBody;
        }

        /// <summary>刷新事件预告列表</summary>
        private void RefreshEventList()
        {
            if (eventListContainer == null) return;

            // 清除旧条目
            foreach (Transform child in eventListContainer)
            {
                Destroy(child.gameObject);
            }

            // TODO: 从EventSystem获取本回合事件
            // 暂时用占位数据
            var eventPreviews = new List<string>
            {
                "御史台有人准备弹劾地方官员",
                "吏部即将进行年度考评"
            };

            foreach (var preview in eventPreviews)
            {
                if (eventItemPrefab != null)
                {
                    var item = Instantiate(eventItemPrefab, eventListContainer);
                    var text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = $"◆ {preview}";
                        text.color = UIConfig.InkMedium;
                        text.fontSize = UIConfig.FontSizeBody;
                    }
                }
            }
        }

        /// <summary>刷新NPC动态提醒</summary>
        private void RefreshNPCActivity(RunState run)
        {
            if (npcActivityText == null) return;

            // TODO: 从NPCRelationshipGraph获取本回合NPC行为
            npcActivityText.text = "暂无特别动向。";
            npcActivityText.color = UIConfig.InkLight;
            npcActivityText.fontSize = UIConfig.FontSizeBody;
        }

        // ======================== 按钮回调 ========================

        /// <summary>点击"上朝" → 推进到行动分配阶段</summary>
        private void OnProceedClicked()
        {
            Debug.Log("[MorningBriefing] 玩家确认上朝，进入行动分配");
            TurnManager.Instance.AdvancePhase();
        }

        // ======================== 工具方法 ========================

        /// <summary>获取官职的中文显示名</summary>
        private string GetRankDisplayName(OfficialRank rank)
        {
            return rank switch
            {
                OfficialRank.Candidate => "候补",
                OfficialRank.CountyMagistrate => "县令",
                OfficialRank.Prefect => "州刺史",
                OfficialRank.ViceMinister => "侍郎",
                OfficialRank.Minister => "尚书",
                OfficialRank.MilitaryGovernor => "节度使",
                OfficialRank.GrandCouncilor => "宰相",
                _ => "未知"
            };
        }
    }
}
