// ============================================================================
// 官途浮沉 - Meta永久进度数据模型
// MetaProgress.cs — Roguelite跨局永久解锁和成就
// ============================================================================

using System;
using System.Collections.Generic;

namespace GuantuFucheng.Models
{
    /// <summary>
    /// 永久升级定义
    /// </summary>
    [Serializable]
    public class MetaUpgrade
    {
        public string UpgradeId;
        public string Name;
        public string Description;
        public int MaxLevel;
        public int CurrentLevel;
        public int CostPerLevel;       // 每级所需"官运"点数

        public bool IsMaxed => CurrentLevel >= MaxLevel;
    }

    /// <summary>
    /// 单局结算记录
    /// </summary>
    [Serializable]
    public class RunRecord
    {
        public string RunId;
        public DateTime StartTime;
        public DateTime EndTime;
        public string PlayerName;
        public string FinalRank;       // 最终官职
        public int TotalTurns;
        public string GameOverReason;
        public int Score;
        public int GuanYunEarned;      // 本局获得的"官运"点数
    }

    /// <summary>
    /// Meta永久进度（跨局持久化）
    /// </summary>
    [Serializable]
    public class MetaProgressData
    {
        public int TotalGuanYun = 0;                               // 累计官运点数（永久货币）
        public int TotalRuns = 0;                                   // 总游玩次数
        public List<MetaUpgrade> Upgrades = new List<MetaUpgrade>();// 已购买的永久升级
        public List<string> UnlockedCharacters = new List<string>();// 已解锁的可选角色背景
        public List<string> UnlockedCards = new List<string>();     // 已解锁进入卡池的卡牌
        public List<string> Achievements = new List<string>();      // 已达成成就
        public List<RunRecord> RunHistory = new List<RunRecord>();  // 历史战绩

        /// <summary>获取指定升级的当前等级</summary>
        public int GetUpgradeLevel(string upgradeId)
        {
            var upgrade = Upgrades.Find(u => u.UpgradeId == upgradeId);
            return upgrade?.CurrentLevel ?? 0;
        }

        /// <summary>检查成就是否已达成</summary>
        public bool HasAchievement(string achievementId)
        {
            return Achievements.Contains(achievementId);
        }
    }
}
