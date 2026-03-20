// ============================================================
// Program.cs — 入口：1 城 × 3 家族 × 10 周模拟
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using TangMo.Simulation;

namespace TangMo;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║   唐末藩镇 — 家族治理与叛乱模拟器 v0.1   ║");
        Console.WriteLine("║   Phase 0: 1 城 × 3 家族 × 10 周        ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();

        // ── 创建世界 ──
        var world = CreateWorld();

        // ── 创建流水线 ──
        var pipeline = new WeeklyPipeline();

        // ── 模拟 10 周 ──
        int totalWeeks = 10;
        var city = world.Cities[0];

        for (int i = 0; i < totalWeeks; i++)
        {
            Console.WriteLine($"──────────────────────────────────────────");
            Console.WriteLine($"第 {world.Week} 周 | {world.CurrentYear}年 {SeasonName(world.CurrentSeason)}");
            Console.WriteLine($"──────────────────────────────────────────");

            // 执行周结算
            pipeline.RunAll(world);

            // 打印城市状态
            PrintCityStatus(city);

            // 打印家族状态
            PrintFamilyStatus(city);

            Console.WriteLine();

            // 推进一周
            world.AdvanceWeek();
        }

        // ── 最终总结 ──
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║              模拟结束                    ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();
        PrintFinalSummary(world, city);

        // ── 输出叙事日志 ──
        Console.WriteLine();
        Console.WriteLine("📜 节度使回忆录：");
        Console.WriteLine("──────────────────────────────────────────");
        foreach (var entry in world.Chronicle)
        {
            if (entry.Contains("回忆录"))
                Console.WriteLine($"  {entry}");
        }

        Console.WriteLine();
        Console.WriteLine("完整日志已省略（共 " + world.Chronicle.Count + " 条记录）");
    }

    /// <summary>
    /// 创建模拟世界：1 座州府城市，3 个家族
    /// </summary>
    static World CreateWorld()
    {
        var world = new World
        {
            StartYear = 880,
            RevoltThreshold = 1.0,
        };

        // ── 创建 3 个家族 ──
        var liFamily = new Family
        {
            Id = "li",
            Name = "李氏牙将",
            Type = FamilyType.MilitaryClan,
            LoyaltyToLord = 0.45,
            Grievance = 0.25,
            Fear = 0.30,
            Dependence = 0.40,
            EconShare = 0.20,
            Militia = 0.50,  // 军事家族武力高
            Popularity = 0.15,
        };

        var wangFamily = new Family
        {
            Id = "wang",
            Name = "王氏盐商",
            Type = FamilyType.MerchantClan,
            LoyaltyToLord = 0.60,
            Grievance = 0.10,
            Fear = 0.20,
            Dependence = 0.60,
            EconShare = 0.55, // 商贾经济强
            Militia = 0.10,
            Popularity = 0.20,
        };

        var zhangFamily = new Family
        {
            Id = "zhang",
            Name = "张氏士绅",
            Type = FamilyType.GentryClan,
            LoyaltyToLord = 0.70,
            Grievance = 0.05,
            Fear = 0.15,
            Dependence = 0.70,
            EconShare = 0.25,
            Militia = 0.05,
            Popularity = 0.45, // 士绅民望高
        };

        // ── 创建州府城市 ──
        var city = new City
        {
            Id = "yunzhou",
            Name = "郓州",
            Type = CityType.ProvincialCapital | CityType.TradeHubSalt,
            Controller = CityController.Player,
            GranaryGrain = 500,
            Treasury = 300,
            AgriculturalYield = 50,
            Prosperity = 0.50,
            MarketActivity = 0.40,
            GarrisonStrength = 120,
            GarrisonMorale = 0.65,
            PeopleBurden = 0.35,
            InstitutionPower = 0.45,
            TaxRate = 0.30,
            SaltTaxRate = 0.20,
            MilitaryPriority = 0.50,
            Facilities = new HashSet<KeyFacility>
            {
                KeyFacility.YamenGovernmentHall,
                KeyFacility.JiedushiResidence,
                KeyFacility.ChangpingGranary,
                KeyFacility.Armory,
                KeyFacility.SaltIronOffice,
                KeyFacility.MarketEast,
            },
            Families = new List<Family> { liFamily, wangFamily, zhangFamily },
        };

        world.Cities.Add(city);

        // 初始化 Dominance
        double totalPower = city.Families.Sum(f => f.Power);
        foreach (var f in city.Families)
            f.Dominance = f.Power / totalPower;

        return world;
    }

    static void PrintCityStatus(City city)
    {
        Console.WriteLine($"  📍 {city.Name}");
        Console.WriteLine($"  💰 财政: 粮={city.GranaryGrain:F0} 钱={city.Treasury:F0}");
        Console.WriteLine($"  ⚔️ 军事: 兵力={city.GarrisonStrength:F0} 士气={city.GarrisonMorale:F2} " +
                          $"欠饷={city.ArmyPayArrearsWeeks}周");
        Console.WriteLine($"  👥 治理: 民负={city.PeopleBurden:F2} 制度={city.InstitutionPower:F2} " +
                          $"Unrest={city.Unrest:F2}");
        Console.WriteLine($"  🏷️ 标志: {city.Flags}");

        // 健康度指示
        string eco = city.EconomicHealth == HealthStatus.Green ? "🟢" :
                     city.EconomicHealth == HealthStatus.Yellow ? "🟡" : "🔴";
        string mil = city.MilitaryHealth == HealthStatus.Green ? "🟢" :
                     city.MilitaryHealth == HealthStatus.Yellow ? "🟡" : "🔴";
        string pop = city.PopulaceHealth == HealthStatus.Green ? "🟢" :
                     city.PopulaceHealth == HealthStatus.Yellow ? "🟡" : "🔴";
        string fam = city.FamilyHealth == HealthStatus.Green ? "🟢" :
                     city.FamilyHealth == HealthStatus.Yellow ? "🟡" : "🔴";
        Console.WriteLine($"  📊 健康度: 经济{eco} 军事{mil} 民心{pop} 家族{fam}");
    }

    static void PrintFamilyStatus(City city)
    {
        Console.WriteLine($"  🏯 家族：");
        foreach (var f in city.Families)
        {
            string domStar = f.IsDominant ? " ⭐主导" : "";
            Console.WriteLine($"    {f.Name}: Power={f.Power:F2} Dom={f.Dominance:F2} " +
                              $"[经{f.EconShare:F2} 武{f.Militia:F2} 民{f.Popularity:F2}] " +
                              $"忠={f.LoyaltyToLord:F2} 怨={f.Grievance:F2}{domStar}");
        }
    }

    static void PrintFinalSummary(World world, City city)
    {
        Console.WriteLine($"最终状态：第 {world.Week - 1} 周 | {world.CurrentYear}年");
        Console.WriteLine($"  合法性={world.Legitimacy:F2} 威望={world.Prestige:F2} " +
                          $"朝评={world.CourtStanding:F2} 边患={world.BorderThreat:F2}");
        Console.WriteLine($"  城市：粮={city.GranaryGrain:F0} 钱={city.Treasury:F0} " +
                          $"Unrest={city.Unrest:F2}");

        Console.WriteLine();
        Console.WriteLine("  家族最终排名：");
        var ranked = city.Families.OrderByDescending(f => f.Power).ToList();
        for (int i = 0; i < ranked.Count; i++)
        {
            var f = ranked[i];
            Console.WriteLine($"    {i + 1}. {f.Name} — Power={f.Power:F2} Dominance={f.Dominance:F2}");
        }
    }

    static string SeasonName(Season s) => s switch
    {
        Season.Spring => "春",
        Season.Summer => "夏",
        Season.Autumn => "秋",
        Season.Winter => "冬",
        _ => ""
    }
}
