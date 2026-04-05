// ============================================================================
// 官途浮沉 - Roguelite Meta系统
// RogueliteMetaSystem.cs — 管理单局状态与跨局永久进度
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Core;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;
using GuantuFucheng.Models;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// Roguelite Meta系统
    /// 
    /// 设计理念：
    /// 每一局游戏都是独立的"仕途人生"，失败是常态。
    /// 但每次失败都会积累"官运"点数，用于解锁永久升级。
    /// 
    /// 永久升级分类：
    /// 1. 属性加成：初始属性提升
    /// 2. 行动力：初始行动力+1
    /// 3. 卡池扩展：解锁新的事件卡/机遇卡
    /// 4. 角色背景：解锁不同出身（影响初始关系和属性）
    /// 5. 情报：初始就揭示部分NPC性格
    /// 
    /// 成就系统：
    /// - 达成特定条件解锁成就
    /// - 成就提供额外官运奖励
    /// - 部分成就解锁隐藏内容
    /// </summary>
    public class RogueliteMetaSystem : Singleton<RogueliteMetaSystem>
    {
        // ======================== 永久升级定义 ========================

        /// <summary>所有可购买的永久升级（硬编码，后续可改为配置文件）</summary>
        private static readonly MetaUpgrade[] DEFAULT_UPGRADES = new MetaUpgrade[]
        {
            new MetaUpgrade
            {
                UpgradeId = "extra_ap",
                Name = "老练",
                Description = "每局初始行动力+1",
                MaxLevel = 2,
                CostPerLevel = 50
            },
            new MetaUpgrade
            {
                UpgradeId = "base_intellect",
                Name = "饱读诗书",
                Description = "每局初始才学+3",
                MaxLevel = 5,
                CostPerLevel = 30
            },
            new MetaUpgrade
            {
                UpgradeId = "base_charisma",
                Name = "左右逢源",
                Description = "每局初始人望+3",
                MaxLevel = 5,
                CostPerLevel = 30
            },
            new MetaUpgrade
            {
                UpgradeId = "base_scheming",
                Name = "城府渐深",
                Description = "每局初始权谋+3",
                MaxLevel = 5,
                CostPerLevel = 30
            },
            new MetaUpgrade
            {
                UpgradeId = "base_health",
                Name = "铁打的身子",
                Description = "每局初始体魄+10",
                MaxLevel = 3,
                CostPerLevel = 40
            },
            new MetaUpgrade
            {
                UpgradeId = "initial_gold",
                Name = "殷实家底",
                Description = "每局初始金银+50",
                MaxLevel = 3,
                CostPerLevel = 20
            },
            new MetaUpgrade
            {
                UpgradeId = "reveal_npc",
                Name = "洞察秋毫",
                Description = "每局开始随机揭示1个NPC性格",
                MaxLevel = 3,
                CostPerLevel = 60
            },
            new MetaUpgrade
            {
                UpgradeId = "extra_card_draw",
                Name = "命中注定",
                Description = "每回合多抽1张卡",
                MaxLevel = 2,
                CostPerLevel = 80
            }
        };

        // ======================== 生命周期 ========================

        private void OnEnable()
        {
            GameEvents.OnNewGameStarted += ApplyMetaUpgrades;
        }

        private void OnDisable()
        {
            GameEvents.OnNewGameStarted -= ApplyMetaUpgrades;
        }

        protected override void OnSingletonAwake()
        {
            EnsureDefaultUpgrades();
        }

        // ======================== 公开方法 ========================

        /// <summary>
        /// 购买/升级一个永久升级
        /// </summary>
        /// <param name="upgradeId">升级ID</param>
        /// <returns>是否购买成功</returns>
        public bool PurchaseUpgrade(string upgradeId)
        {
            var meta = GameManager.Instance.MetaProgress;
            var upgrade = meta.Upgrades.Find(u => u.UpgradeId == upgradeId);

            if (upgrade == null)
            {
                Debug.LogWarning($"[Meta] 找不到升级：{upgradeId}");
                return false;
            }

            if (upgrade.IsMaxed)
            {
                Debug.LogWarning($"[Meta] {upgrade.Name} 已达最高等级");
                return false;
            }

            int cost = upgrade.CostPerLevel * (upgrade.CurrentLevel + 1);
            if (meta.TotalGuanYun < cost)
            {
                Debug.LogWarning($"[Meta] 官运不足：需要{cost}，当前{meta.TotalGuanYun}");
                return false;
            }

            meta.TotalGuanYun -= cost;
            upgrade.CurrentLevel++;

            Debug.Log($"[Meta] ✨ 升级成功：{upgrade.Name} Lv.{upgrade.CurrentLevel}");
            GameEvents.MetaUpgradeUnlocked(upgradeId);

            // 保存
            SaveSystem.Instance.SaveMetaProgress(meta);

            return true;
        }

        /// <summary>
        /// 解锁新卡牌进入卡池
        /// </summary>
        public void UnlockCard(string cardId)
        {
            var meta = GameManager.Instance.MetaProgress;
            if (!meta.UnlockedCards.Contains(cardId))
            {
                meta.UnlockedCards.Add(cardId);
                Debug.Log($"[Meta] 🃏 解锁新卡牌：{cardId}");
                SaveSystem.Instance.SaveMetaProgress(meta);
            }
        }

        /// <summary>
        /// 解锁成就
        /// </summary>
        public void UnlockAchievement(string achievementId, string name, int bonusGuanYun = 10)
        {
            var meta = GameManager.Instance.MetaProgress;
            if (!meta.HasAchievement(achievementId))
            {
                meta.Achievements.Add(achievementId);
                meta.TotalGuanYun += bonusGuanYun;
                Debug.Log($"[Meta] 🏆 成就达成：{name}（+{bonusGuanYun}官运）");
                SaveSystem.Instance.SaveMetaProgress(meta);
            }
        }

        /// <summary>获取所有升级及其状态</summary>
        public List<MetaUpgrade> GetAllUpgrades()
        {
            return GameManager.Instance.MetaProgress.Upgrades;
        }

        /// <summary>获取当前官运余额</summary>
        public int GetGuanYunBalance()
        {
            return GameManager.Instance.MetaProgress.TotalGuanYun;
        }

        // ======================== 内部方法 ========================

        /// <summary>确保默认升级列表存在</summary>
        private void EnsureDefaultUpgrades()
        {
            var meta = GameManager.Instance?.MetaProgress;
            if (meta == null) return;

            if (meta.Upgrades.Count == 0)
            {
                foreach (var template in DEFAULT_UPGRADES)
                {
                    meta.Upgrades.Add(new MetaUpgrade
                    {
                        UpgradeId = template.UpgradeId,
                        Name = template.Name,
                        Description = template.Description,
                        MaxLevel = template.MaxLevel,
                        CurrentLevel = 0,
                        CostPerLevel = template.CostPerLevel
                    });
                }
            }
        }

        /// <summary>新局开始时应用所有已购买的永久升级</summary>
        private void ApplyMetaUpgrades()
        {
            var meta = GameManager.Instance.MetaProgress;
            var player = GameManager.Instance.CurrentRun.Player;

            foreach (var upgrade in meta.Upgrades)
            {
                if (upgrade.CurrentLevel == 0) continue;

                switch (upgrade.UpgradeId)
                {
                    case "base_intellect":
                        player.Intellect += 3 * upgrade.CurrentLevel;
                        break;
                    case "base_charisma":
                        player.Charisma += 3 * upgrade.CurrentLevel;
                        break;
                    case "base_scheming":
                        player.Scheming += 3 * upgrade.CurrentLevel;
                        break;
                    case "base_health":
                        player.Health += 10 * upgrade.CurrentLevel;
                        break;
                    case "initial_gold":
                        player.Gold += 50 * upgrade.CurrentLevel;
                        break;
                    case "reveal_npc":
                        // 随机揭示NPC性格
                        for (int i = 0; i < upgrade.CurrentLevel; i++)
                        {
                            // TODO: 从未揭示的NPC中随机选一个
                        }
                        break;
                }
            }

            Debug.Log("[Meta] 已应用所有永久升级");
        }
    }
}
