// ============================================================
// Program.cs — 入口：正常模式 + 压力测试模式
// ============================================================
// 用法:
//   dotnet run              — 正常模式：1 城 × 3 家族 × 10 周
//   dotnet run -- --stress  — 压力测试：加速叛乱，20 周模拟
//   dotnet run -- --stress30 — 压力测试：30 周极限推演
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using TangMo.Simulation;
using TangMo.Rebellion;

namespace TangMo;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        bool stressMode = args.Contains("--stress") || args.Contains("--stress30");
        int totalWeeks = args.Contains("--stress30") ? 30 : stressMode ? 20 : 10;

        if (stressMode)
            RunStressTest(totalWeeks);
        else
            RunNormalSimulation(totalWeeks);
    }

    // ═══════════════════════════════════════════
    //  正常模式
    // ═══════════════════════════════════════════

    static void RunNormalSimulation(int totalWeeks)
    {
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║   唐末藩镇 — 家族治理与叛乱模拟器 v0.2   ║");
        Console.WriteLine($"║   正常模式: 1 城 × 3 家族 × {totalWeeks} 周        ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();

        var world = CreateWorld();
        var pipeline = new WeeklyPipeline();
        var city = world.Cities[0];

        for (int i = 0; i < totalWeeks; i++)
        {
            Console.WriteLine($"──────────────────────────────────────────");
            Console.WriteLine($"第 {world.Week} 周 | {world.CurrentYear}年 {SeasonName(world.CurrentSeason)}");
            Console.WriteLine($"──────────────────────────────────────────");

            pipeline.RunAll(world);
            PrintCityStatus(city);
            PrintFamilyStatus(city);
            Console.WriteLine();
            world.AdvanceWeek();
        }

        PrintEndBanner(world, city);
    }

    // ═══════════════════════════════════════════
    //  压力测试模式 — 加速叛乱
    // ═══════════════════════════════════════════

    static void RunStressTest(int totalWeeks)
    {
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║   ⚡ 压力测试 — 加速叛乱推演 ⚡          ║");
        Console.WriteLine($"║   1 城 × 4 家族 × {totalWeeks} 周              ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine("  配置：低制度力 / 高欠饷 / 高税率 / 低忠诚");
        Console.WriteLine("  目标：验证 Unrest 公式 → 叛乱触发 → 三结局路径");
        Console.WriteLine();

        var world = CreateStressWorld();
        var pipeline = new WeeklyPipeline();
        var city = world.Cities[0];

        bool rebellionTriggered = false;

        for (int i = 0; i < totalWeeks; i++)
        {
            Console.WriteLine($"══════════════════════════════════════════");
            Console.WriteLine($"第 {world.Week} 周 | {world.CurrentYear}年 {SeasonName(world.CurrentSeason)}");
            Console.WriteLine($"══════════════════════════════════════════");

            pipeline.RunAll(world);

            // 检查是否触发了叛乱（通过日志检测）
            if (!rebellionTriggered && world.Chronicle.Any(c => c.Contains("叛乱触发")))
            {
                rebellionTriggered = true;
                Console.WriteLine();
                Console.WriteLine("  🔥🔥🔥 叛乱已触发！检查结局...");
            }

            PrintCityStatus(city);
            PrintFamilyStatus(city);

            // Unrest 趋势标记
            if (city.Unrest >= 0.55 && city.Unrest < 0.70)
                Console.WriteLine($"  ⚡ 黄灯预警区间!");
            else if (city.Unrest >= 0.70)
                Console.WriteLine($"  🔴 叛乱红线区间!");

            Console.WriteLine();
            world.AdvanceWeek();
        }

        // ── 压力测试报告 ──
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║           压力测试报告                   ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");

        var rebellionLogs = world.Chronicle.Where(c => c.Contains("叛乱")).ToList();
        var warningLogs = world.Chronicle.Where(c => c.Contains("⚠️")).ToList();
        var memoirLogs = world.Chronicle.Where(c => c.Contains("回忆录")).ToList();

        Console.WriteLine($"  总周数: {world.Week - 1}");
        Console.WriteLine($"  预警次数: {warningLogs.Count}");
        Console.WriteLine($"  叛乱事件: {rebellionLogs.Count}");
        Console.WriteLine($"  叙事记录: {memoirLogs.Count}");
        Console.WriteLine();

        if (rebellionTriggered)
        {
            Console.WriteLine("  ✅ 叛乱触发验证通过");
            // 检查结局
            if (memoirLogs.Any(l => l.Contains("镇压")))
                Console.WriteLine("  ✅ 镇压结局路径可达");
            if (memoirLogs.Any(l => l.Contains("惨胜")))
                Console.WriteLine("  ✅ 惨胜结局路径可达");
            if (memoirLogs.Any(l => l.Contains("赦免") || l.Contains("许以")))
                Console.WriteLine("  ✅ 赦免改革结局路径可达");
        }
        else
        {
            Console.WriteLine("  ⚠️ 未触发叛乱 — 可能需要更高压力配置");
            Console.WriteLine($"  最终 Unrest = {city.Unrest:F2} (阈值 = {0.70 * world.RevoltThreshold:F2})");
        }

        PrintEndBanner(world, city);
    }

    /// <summary>
    /// 压力测试世界：低制度力、高欠饷、高税率、低忠诚、多家族
    /// </summary>
    static World CreateStressWorld()
    {
        var world = new World
        {
            StartYear = 880,
            RevoltThreshold = 1.0,  // 标准阈值
        };

        // ── 4 个家族，忠诚度普遍偏低 ──
        var liFamily = new Family
        {
            Id = "li", Name = "李氏牙将", Type = FamilyType.MilitaryClan,
            LoyaltyToLord = 0.30, Grievance = 0.40, Fear = 0.20, Dependence = 0.30,
            EconShare = 0.15, Militia = 0.55, Popularity = 0.10,
        };

        var wangFamily = new Family
        {
            Id = "wang", Name = "王氏盐商", Type = FamilyType.MerchantClan,
            LoyaltyToLord = 0.50, Grievance = 0.20, Fear = 0.15, Dependence = 0.50,
            EconShare = 0.50, Militia = 0.15, Popularity = 0.15,
        };

        var zhangFamily = new Family
        {
            Id = "zhang", Name = "张氏士绅", Type = FamilyType.GentryClan,
            LoyaltyToLord = 0.55, Grievance = 0.15, Fear = 0.10, Dependence = 0.55,
            EconShare = 0.20, Militia = 0.08, Popularity = 0.40,
        };

        var chenFamily = new Family
        {
            Id = "chen", Name = "陈氏将门", Type = FamilyType.MilitaryClan,
            LoyaltyToLord = 0.35, Grievance = 0.35, Fear = 0.25, Dependence = 0.35,
            EconShare = 0.10, Militia = 0.45, Popularity = 0.12,
        };

        var city = new City
        {
            Id = "yunzhou", Name = "郓州",
            Type = CityType.ProvincialCapital | CityType.TradeHubSalt,
            Controller = CityController.Player,
            GranaryGrain = 300,         // 粮少
            Treasury = 100,             // 钱少 — 加速欠饷
            AgriculturalYield = 40,
            Prosperity = 0.35,
            MarketActivity = 0.30,
            GarrisonStrength = 150,     // 兵多 — 军饷压力大
            GarrisonMorale = 0.50,      // 士气低
            ArmyPayArrearsWeeks = 1,    // 初始已欠饷 1 周
            PeopleBurden = 0.50,        // 高民负
            InstitutionPower = 0.20,    // 低制度力 — 关键！
            TaxRate = 0.45,             // 高税率
            SaltTaxRate = 0.30,
            MilitaryPriority = 0.40,    // 军费优先级不够
            Flags = CityFlags.ARMY_PAY_ISSUE, // 初始标记欠饷
            Facilities = new HashSet<KeyFacility>
            {
                KeyFacility.YamenGovernmentHall,
                KeyFacility.JiedushiResidence,
                KeyFacility.SaltIronOffice,
            },
            Families = new List<Family> { liFamily, wangFamily, zhangFamily, chenFamily },
        };

        world.Cities.Add(city);

        double totalPower = city.Families.Sum(f => f.Power);
        foreach (var f in city.Families)
            f.Dominance = f.Power / totalPower;

        return world;
    }

    // ═══════════════════════════════════════════
    //  共用辅助方法
    // ═══════════════════════════════════════════

    static World CreateWorld()
    {
        var world = new World { StartYear = 880, RevoltThreshold = 1.0 };

        var liFamily = new Family
        {
            Id = "li", Name = "李氏牙将", Type = FamilyType.MilitaryClan,
            LoyaltyToLord = 0.45, Grievance = 0.25, Fear = 0.30, Dependence = 0.40,
            EconShare = 0.20, Militia = 0.50, Popularity = 0.15,
        };
        var wangFamily = new Family
        {
            Id = "wang", Name = "王氏盐商", Type = FamilyType.MerchantClan,
            LoyaltyToLord = 0.60, Grievance = 0.10, Fear = 0.20, Dependence = 0.60,
            EconShare = 0.55, Militia = 0.10, Popularity = 0.20,
        };
        var zhangFamily = new Family
        {
            Id = "zhang", Name = "张氏士绅", Type = FamilyType.GentryClan,
            LoyaltyToLord = 0.70, Grievance = 0.05, Fear = 0.15, Dependence = 0.70,
            EconShare = 0.25, Militia = 0.05, Popularity = 0.45,
        };

        var city = new City
        {
            Id = "yunzhou", Name = "郓州",
            Type = CityType.ProvincialCapital | CityType.TradeHubSalt,
            Controller = CityController.Player,
            GranaryGrain = 500, Treasury = 300,
            AgriculturalYield = 50, Prosperity = 0.50, MarketActivity = 0.40,
            GarrisonStrength = 120, GarrisonMorale = 0.65,
            PeopleBurden = 0.35, InstitutionPower = 0.45,
            TaxRate = 0.30, SaltTaxRate = 0.20, MilitaryPriority = 0.50,
            Facilities = new HashSet<KeyFacility>
            {
                KeyFacility.YamenGovernmentHall, KeyFacility.JiedushiResidence,
                KeyFacility.ChangpingGranary, KeyFacility.Armory,
                KeyFacility.SaltIronOffice, KeyFacility.MarketEast,
            },
            Families = new List<Family> { liFamily, wangFamily, zhangFamily },
        };

        world.Cities.Add(city);
        double totalPower = city.Families.Sum(f => f.Power);
        foreach (var f in city.Families) f.Dominance = f.Power / totalPower;
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

    static void PrintEndBanner(World world, City city)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║              模拟结束                    ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
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

        // 叙事日志
        Console.WriteLine();
        Console.WriteLine("📜 节度使回忆录：");
        Console.WriteLine("──────────────────────────────────────────");
        foreach (var entry in world.Chronicle)
        {
            if (entry.Contains("回忆录"))
                Console.WriteLine($"  {entry}");
        }

        // 预警日志
        var warnings = world.Chronicle.Where(c => c.Contains("⚠️")).ToList();
        if (warnings.Any())
        {
            Console.WriteLine();
            Console.WriteLine("⚠️ 预警记录：");
            foreach (var w in warnings)
                Console.WriteLine($"  {w}");
        }

        Console.WriteLine();
        Console.WriteLine($"完整日志共 {world.Chronicle.Count} 条记录");
    }

    static string SeasonName(Season s) => s switch
    {
        Season.Spring => "春", Season.Summer => "夏",
        Season.Autumn => "秋", Season.Winter => "冬", _ => ""
    };
}
