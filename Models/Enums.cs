// ============================================================
// Enums.cs — 所有枚举定义
// ============================================================
using System;

namespace TangMo
{
    /// <summary>季节循环</summary>
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    /// <summary>城市类型（Flags，可叠加）</summary>
    [Flags]
    public enum CityType
    {
        None                    = 0,
        ProvincialCapital       = 1 << 0,  // 州府 / 节度使治所
        MilitaryGarrisonTown    = 1 << 1,  // 军镇 / 关隘城
        TradeHubSalt            = 1 << 2,  // 盐铁城市
        TradeHubTeaHorse        = 1 << 3,  // 茶马互市城
        RiverPortCity           = 1 << 4,  // 漕运 / 河港城
        AgriculturalCountyTown  = 1 << 5,  // 普通县城
        ReligiousCenter         = 1 << 6,  // 名寺 / 名山 / 大观
    }

    /// <summary>关键设施</summary>
    public enum KeyFacility
    {
        // 粮与军
        ChangpingGranary,   // 常平仓
        ArmyGrainDepot,     // 军粮库
        Armory,             // 军械库
        // 税与市
        SaltIronOffice,     // 盐铁务
        MarketEast,         // 东市
        MarketWest,         // 西市
        RiverPortDock,      // 漕运码头
        // 权力中枢
        YamenGovernmentHall, // 州府/县衙公堂
        JiedushiResidence,  // 帅府
    }

    /// <summary>家族类型（Flags，可多选）</summary>
    [Flags]
    public enum FamilyType
    {
        None            = 0,
        MilitaryClan    = 1 << 0,  // 牙将家族 / 军镇将门
        MerchantClan    = 1 << 1,  // 盐商 / 茶马 / 粮商
        GentryClan      = 1 << 2,  // 士绅 / 幕僚 / 大族
        ReligiousClan   = 1 << 3,  // 寺院 / 道观附属家族
        LocalElderClan  = 1 << 4,  // 乡老 / 里正家族
        CourtTiedClan   = 1 << 5,  // 与宦官/权臣/宗室有关系
    }

    /// <summary>城市控制者</summary>
    public enum CityController
    {
        Player,     // 玩家控制
        Court,      // 朝廷
        Rival,      // 邻镇 / 敌对藩镇
        Rebel,      // 叛军
    }

    /// <summary>城市 Flag 标记</summary>
    [Flags]
    public enum CityFlags
    {
        None                = 0,
        ARMY_PAY_ISSUE      = 1 << 0,  // 欠饷问题
        SALT_TAX_ISSUE      = 1 << 1,  // 盐税问题
        OFFICE_PARALYZED_RISK = 1 << 2, // 官署瘫痪风险
        PRIVATE_SALT_RAMPANT = 1 << 3, // 私盐盛行
        PORT_BLOCKED        = 1 << 4,  // 河港封锁
        FAMINE_RISK         = 1 << 5,  // 饥荒风险
        REBELLION_ACTIVE    = 1 << 6,  // 叛乱进行中
        RECOVERY_MODE       = 1 << 7,  // 恢复期
    }

    /// <summary>叛乱日实例阶段</summary>
    public enum EpisodePhase
    {
        Spark,
        Resolution,
    }

    /// <summary>叛乱结局</summary>
    public enum EpisodeResolution
    {
        None,
        Suppress,        // 武力镇压
        CostlySuppress,  // 惨胜
        Autonomy,        // 谈判让步 / 妥协
        CityFlipped,     // 弃城失守
        GrantReform,     // 赦免改革
    }
}
