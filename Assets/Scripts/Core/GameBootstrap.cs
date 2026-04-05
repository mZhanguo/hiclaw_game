// ============================================================================
// 官途浮沉 - 游戏启动入口
// GameBootstrap.cs — 初始化所有系统，加载数据，创建初始状态
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using GuantuFucheng.Enums;
using GuantuFucheng.Systems;
using GuantuFucheng.Data;

namespace GuantuFucheng.Core
{
    /// <summary>
    /// 游戏启动入口 — 挂载到场景中的空物体上
    /// 
    /// 职责：
    /// 1. 确保所有Singleton系统实例已创建
    /// 2. 通过DataLoader加载JSON数据并注入各系统
    /// 3. 自动开始新游戏（县令起步）
    /// 
    /// 使用方式：
    /// 在场景中创建空GameObject，挂载此脚本即可。
    /// 所有System的Singleton会在首次访问时自动创建。
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("游戏配置")]
        [SerializeField] private string _playerName = "林青云";
        [SerializeField] private int _randomSeed = 0; // 0 = 随机
        [SerializeField] private bool _autoStartGame = true;
        [SerializeField] private OfficialRank _startingRank = OfficialRank.CountyMagistrate;

        [Header("调试")]
        [SerializeField] private bool _enableConsoleRunner = true;

        // 缓存加载的数据
        private List<CardData> _loadedCards;
        private List<NPCData> _loadedNPCs;
        private List<OfficialRankData> _loadedRanks;
        private EvaluationConfigData _loadedEvalConfig;

        private void Awake()
        {
            Debug.Log("╔══════════════════════════════════════════╗");
            Debug.Log("║        《官途浮沉》— 游戏启动中...       ║");
            Debug.Log("╚══════════════════════════════════════════╝");
        }

        private void Start()
        {
            InitializeSystems();
            LoadGameData();
            InjectDataToSystems();

            if (_autoStartGame)
            {
                StartNewGame();
            }

            if (_enableConsoleRunner)
            {
                // 添加ConsoleGameRunner组件
                var runner = gameObject.GetComponent<ConsoleGameRunner>();
                if (runner == null)
                    runner = gameObject.AddComponent<ConsoleGameRunner>();
                runner.Initialize(_loadedCards, _loadedNPCs, _loadedRanks, _loadedEvalConfig);
            }
        }

        /// <summary>
        /// 确保所有Singleton系统实例已创建
        /// 访问.Instance会自动创建GameObject和组件
        /// </summary>
        private void InitializeSystems()
        {
            Debug.Log("[Bootstrap] 正在初始化系统...");

            // 触发所有Singleton的懒加载
            var _ = GameManager.Instance;
            var __ = TurnManager.Instance;
            var ___ = CardSystem.Instance;
            var ____ = NPCRelationshipGraph.Instance;
            var _____ = ActionPointSystem.Instance;
            var ______ = OfficialRankSystem.Instance;
            var _______ = RogueliteMetaSystem.Instance;
            var ________ = SaveSystem.Instance;

            Debug.Log("[Bootstrap] ✓ 所有系统初始化完成");
        }

        /// <summary>
        /// 通过DataLoader加载所有JSON数据
        /// </summary>
        private void LoadGameData()
        {
            Debug.Log("[Bootstrap] 正在加载游戏数据...");

            _loadedCards = DataLoader.LoadCards();
            _loadedNPCs = DataLoader.LoadNPCs();
            _loadedRanks = DataLoader.LoadRanks();
            _loadedEvalConfig = DataLoader.LoadEvaluationConfig();

            Debug.Log($"[Bootstrap] ✓ 数据加载完成：" +
                $"{_loadedCards.Count}张卡牌, " +
                $"{_loadedNPCs.Count}个NPC, " +
                $"{_loadedRanks.Count}个官职, " +
                $"考评维度{_loadedEvalConfig?.Dimensions?.Count ?? 0}个");
        }

        /// <summary>
        /// 将加载的数据注入到各系统中
        /// 通过反射设置私有字段（因为这些字段原本设计为Inspector拖拽赋值）
        /// </summary>
        private void InjectDataToSystems()
        {
            Debug.Log("[Bootstrap] 正在注入数据到系统...");

            // 注入卡牌数据到CardSystem
            InjectField(CardSystem.Instance, "_allCards", _loadedCards);

            // 注入NPC数据到NPCRelationshipGraph
            InjectField(NPCRelationshipGraph.Instance, "_allNPCs", _loadedNPCs);

            // 注入官职数据到OfficialRankSystem
            InjectField(OfficialRankSystem.Instance, "_rankDataList", _loadedRanks);

            Debug.Log("[Bootstrap] ✓ 数据注入完成");
        }

        /// <summary>
        /// 通过反射注入私有字段值
        /// </summary>
        private void InjectField<T>(object target, string fieldName, T value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(target, value);
                Debug.Log($"[Bootstrap]   注入 {type.Name}.{fieldName} 成功");
            }
            else
            {
                Debug.LogWarning($"[Bootstrap]   找不到字段 {type.Name}.{fieldName}");
            }
        }

        /// <summary>
        /// 开始新游戏，以县令起步
        /// </summary>
        private void StartNewGame()
        {
            Debug.Log("[Bootstrap] 开始新游戏...");

            GameManager.Instance.StartNewGame(_playerName, _randomSeed);

            // 如果起始官职不是候补，直接设置
            if (_startingRank != OfficialRank.Candidate)
            {
                var player = GameManager.Instance.CurrentRun.Player;
                player.CurrentRank = _startingRank;
                Debug.Log($"[Bootstrap] 起始官职设置为：{_startingRank}");
            }

            Debug.Log("╔══════════════════════════════════════════╗");
            Debug.Log($"║  玩家：{_playerName}");
            Debug.Log($"║  官职：{GameManager.Instance.CurrentRun.Player.CurrentRank}");
            Debug.Log($"║  随机种子：{GameManager.Instance.CurrentRun.Seed}");
            Debug.Log("╚══════════════════════════════════════════╝");
        }
    }
}
