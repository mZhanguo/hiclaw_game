// ============================================================================
// 官途浮沉 - 全局枚举定义
// GameEnums.cs — 所有系统共用的枚举类型集中管理
// ============================================================================

namespace GuantuFucheng.Enums
{
    /// <summary>
    /// 游戏整体状态机
    /// </summary>
    public enum GameState
    {
        MainMenu,       // 主菜单
        NewGame,        // 新局初始化
        InTurn,         // 回合进行中
        Paused,         // 暂停
        GameOver,       // 单局结束（被贬/被杀/退休）
        Victory         // 通关（位极人臣）
    }

    /// <summary>
    /// 回合阶段 — 每回合固定四个阶段
    /// 早朝简报 → 行动分配 → 执行结算 → 复盘总结
    /// </summary>
    public enum TurnPhase
    {
        MorningBriefing,    // 早朝简报：展示当前局势、新事件、NPC动态
        ActionAllocation,   // 行动分配：玩家将6点行动力分配到不同行动槽
        Execution,          // 执行结算：依次执行已分配的行动，触发卡牌事件
        Review              // 复盘总结：展示本回合结果、数值变化、关系变动
    }

    /// <summary>
    /// 行动类型 — 玩家可以把行动力投入的方向
    /// </summary>
    public enum ActionType
    {
        Politics,       // 政务：处理公文、推行政策、治理地方
        Social,         // 交际：拜访NPC、宴请、送礼、结盟
        Intelligence,   // 情报：打探消息、收集把柄、监视对手
        SelfImprove,    // 修身：读书、练武、养生、提升个人属性
        Scheme,         // 谋略：设局、布局、暗中操作
        Rest            // 休息：恢复状态，但可能错过机会
    }

    /// <summary>
    /// 卡牌稀有度
    /// </summary>
    public enum CardRarity
    {
        Common,         // 白卡：日常政务
        Uncommon,       // 绿卡：有一定影响的事件
        Rare,           // 蓝卡：重要抉择
        Epic,           // 紫卡：改变命运的关键事件
        Legendary       // 金卡：历史级大事件（科举舞弊案、安史之乱等）
    }

    /// <summary>
    /// 卡牌类型
    /// </summary>
    public enum CardType
    {
        Event,          // 事件卡：被动触发的随机事件
        Decision,       // 决策卡：需要玩家做出选择
        Crisis,         // 危机卡：必须立即处理的紧急事件
        Opportunity,    // 机遇卡：可选择抓住的机会
        Character       // 人物卡：与特定NPC相关的剧情
    }

    /// <summary>
    /// 官职品级（从低到高）
    /// 对应唐代官制，简化为关键节点
    /// </summary>
    public enum OfficialRank
    {
        Candidate,      // 候补：刚中进士，等待授官
        CountyMagistrate,   // 县令（正七品）：起始官职
        Prefect,        // 州刺史（从四品）：地方大员
        ViceMinister,   // 侍郎（正四品）：中央副职
        Minister,       // 尚书（正三品）：六部主官 — 文官路线
        MilitaryGovernor,   // 节度使（从二品）：军政大权 — 武官路线
        GrandCouncilor  // 宰相（正二品）：位极人臣，通关目标
    }

    /// <summary>
    /// NPC阵营倾向
    /// </summary>
    public enum Faction
    {
        Neutral,        // 中立
        Reformist,      // 改革派：主张变法
        Conservative,   // 保守派：维护旧制
        Eunuch,         // 宦官集团
        Military,       // 军功集团
        Imperial        // 皇室/外戚
    }

    /// <summary>
    /// NPC性格标签（影响交互策略）
    /// </summary>
    public enum PersonalityTrait
    {
        Loyal,          // 忠义
        Cunning,        // 狡诈
        Greedy,         // 贪婪
        Righteous,      // 正直
        Ambitious,      // 野心勃勃
        Timid,          // 胆小谨慎
        Generous,       // 慷慨大方
        Vengeful        // 睚眦必报
    }

    /// <summary>
    /// 单局结束原因
    /// </summary>
    public enum GameOverReason
    {
        Impeached,      // 被弹劾罢官
        Executed,       // 被处死（得罪皇帝/卷入谋反）
        Exiled,         // 被流放
        Retired,        // 主动告老还乡
        Assassinated,   // 被暗杀
        Victory         // 登上宰相之位
    }

    /// <summary>
    /// 玩家属性维度
    /// </summary>
    public enum PlayerAttribute
    {
        Intellect,      // 才学：影响政务能力和科举
        Charisma,       // 人望：影响社交和NPC好感
        Scheming,       // 权谋：影响情报和阴谋成功率
        Martial,        // 武略：影响军事相关事件
        Health,         // 体魄：生命值上限，0则暴毙
        Reputation      // 声望：影响升迁速度，负值则危险
    }
}
