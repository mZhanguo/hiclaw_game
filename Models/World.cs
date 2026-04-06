// ============================================================
// World.cs — 世界全局状态
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace TangMo
{
    /// <summary>
    /// 世界：全局状态容器。
    /// 包含时间、合法性、环境参数，以及所有城市列表。
    /// </summary>
    public class World
    {
        // ── 时间 ──
        public int Week { get; set; } = 1;
        public Season CurrentSeason { get; set; } = Season.Spring;

        /// <summary>游戏起始年份（公元 880 年，黄巢入长安前后）</summary>
        public int StartYear { get; set; } = 880;

        /// <summary>当前年份（推算）</summary>
        public int CurrentYear => StartYear + (Week - 1) / 52;

        /// <summary>当前周在年中的序号（1-52）</summary>
        public int WeekInYear => ((Week - 1) % 52) + 1;

        // ── 全局指标（∈ [0,1]）──
        public double Legitimacy    { get; set; } = 0.60;  // 名义合法性
        public double Prestige      { get; set; } = 0.50;  // 威望
        public double CourtStanding { get; set; } = 0.50;  // 朝中评价
        public double BorderThreat  { get; set; } = 0.30;  // 边患压力

        // ── 难度 / 环境参数（∈ [0,1]）──
        public double InstitutionGain       { get; set; } = 0.50;
        public double ClanCoalitionPower    { get; set; } = 0.50;
        public double RevoltThreshold       { get; set; } = 1.00;  // 叛乱触发阈值系数
        public double RevoltEscalationRate  { get; set; } = 0.50;
        public double IntelClarity          { get; set; } = 0.50;
        public double PurgeBacklash         { get; set; } = 0.50;

        // ── 城市列表 ──
        public List<City> Cities { get; set; } = new();

        // ── 叙事日志 ──
        public List<string> Chronicle { get; set; } = new();

        // ── 方法 ──

        /// <summary>推进一周，自动更新季节</summary>
        public void AdvanceWeek()
        {
            Week++;
            // 每 13 周换季
            int seasonIndex = ((Week - 1) / 13) % 4;
            CurrentSeason = (Season)seasonIndex;
        }

        /// <summary>获取玩家控制的城市</summary>
        public IEnumerable<City> PlayerCities =>
            Cities.Where(c => c.Controller == CityController.Player);

        /// <summary>添加叙事记录</summary>
        public void Log(string entry)
        {
            string tag = $"[第{Week}周 {CurrentYear}年 {CurrentSeason}]";
            Chronicle.Add($"{tag} {entry}");
        }

        /// <summary>季节对田赋的乘数</summary>
        public double SeasonYieldMultiplier => CurrentSeason switch
        {
            Season.Spring => 0.8,
            Season.Summer => 1.2,
            Season.Autumn => 1.5,  // 秋收
            Season.Winter => 0.5,
            _ => 1.0
        };

        /// <summary>季节对市场活动的乘数</summary>
        public double SeasonMarketMultiplier => CurrentSeason switch
        {
            Season.Spring => 1.0,
            Season.Summer => 1.1,
            Season.Autumn => 1.3,
            Season.Winter => 0.7,
            _ => 1.0
        };

        public override string ToString() =>
            $"=== 第{Week}周 | {CurrentYear}年 {CurrentSeason} ===\n" +
            $"合法性={Legitimacy:F2} 威望={Prestige:F2} 朝评={CourtStanding:F2} 边患={BorderThreat:F2}";
    }
}
