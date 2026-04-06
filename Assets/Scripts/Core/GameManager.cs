// ============================================================================
// 官途浮沉 - 游戏主管理器
// GameManager.cs — 游戏生命周期控制，系统初始化，全局状态机
// ============================================================================

using UnityEngine;
using GuantuFucheng.Enums;
using GuantuFucheng.Events;
using GuantuFucheng.Models;
using GuantuFucheng.Systems;

namespace GuantuFucheng.Core
{
    /// <summary>
    /// 游戏主管理器 — 最顶层的控制中心
    /// 
    /// 职责：
    /// 1. 管理游戏全局状态机（菜单→游戏→暂停→结算）
    /// 2. 持有当前局的运行时数据（RunState）
    /// 3. 协调各子系统的初始化和销毁
    /// 4. 提供全局访问入口
    /// 
    /// 不做：
    /// - 不处理具体的回合逻辑（交给TurnManager）
    /// - 不处理UI逻辑（交给UIManager）
    /// - 不处理具体的系统逻辑（交给各System）
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        // ======================== 公开属性 ========================

        /// <summary>当前游戏状态</summary>
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        /// <summary>当前局的运行时数据</summary>
        public RunState CurrentRun { get; private set; }

        /// <summary>Meta永久进度</summary>
        public MetaProgressData MetaProgress { get; private set; }

        /// <summary>是否正在游戏中</summary>
        public bool IsPlaying => CurrentState == GameState.InTurn;

        // ======================== 生命周期 ========================

        protected override void OnSingletonAwake()
        {
            // 加载Meta进度（永久数据）
            MetaProgress = SaveSystem.Instance.LoadMetaProgress();
            Debug.Log("[GameManager] 初始化完成，已加载Meta进度");
        }

        private void OnEnable()
        {
            // 订阅全局事件
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= HandleGameOver;
        }

        // ======================== 公开方法 ========================

        /// <summary>
        /// 开始新的一局游戏
        /// </summary>
        /// <param name="playerName">玩家角色名</param>
        /// <param name="seed">随机种子（0=随机生成）</param>
        public void StartNewGame(string playerName = "无名氏", int seed = 0)
        {
            Debug.Log($"[GameManager] 开始新局：{playerName}");

            // 创建新的运行时状态
            CurrentRun = new RunState
            {
                Seed = seed == 0 ? Random.Range(1, int.MaxValue) : seed,
                Player = new PlayerState
                {
                    PlayerName = playerName,
                    CurrentRank = OfficialRank.Candidate,
                    CurrentTurn = 1,
                    // 根据Meta进度调整初始属性
                    MaxActionPoints = 6 + MetaProgress.GetUpgradeLevel("extra_ap"),
                    CurrentActionPoints = 6 + MetaProgress.GetUpgradeLevel("extra_ap")
                }
            };

            // 初始化随机种子
            Random.InitState(CurrentRun.Seed);

            // 切换状态
            ChangeState(GameState.InTurn);

            // 通知所有系统
            GameEvents.NewGameStarted();

            // 启动第一个回合
            TurnManager.Instance.StartNewTurn();
        }

        /// <summary>暂停游戏</summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.InTurn)
            {
                ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        /// <summary>恢复游戏</summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.InTurn);
                Time.timeScale = 1f;
            }
        }

        /// <summary>保存当前游戏</summary>
        public void SaveGame()
        {
            if (CurrentRun != null)
            {
                SaveSystem.Instance.SaveRunState(CurrentRun);
                SaveSystem.Instance.SaveMetaProgress(MetaProgress);
                GameEvents.GameSaved();
                Debug.Log("[GameManager] 游戏已保存");
            }
        }

        /// <summary>加载存档继续游戏</summary>
        public void LoadGame()
        {
            var loadedRun = SaveSystem.Instance.LoadRunState();
            if (loadedRun != null)
            {
                CurrentRun = loadedRun;
                Random.InitState(CurrentRun.Seed);
                ChangeState(GameState.InTurn);
                GameEvents.GameLoaded();
                Debug.Log("[GameManager] 存档加载成功");
            }
            else
            {
                Debug.LogWarning("[GameManager] 没有找到存档");
            }
        }

        /// <summary>返回主菜单</summary>
        public void ReturnToMainMenu()
        {
            CurrentRun = null;
            ChangeState(GameState.MainMenu);
            Time.timeScale = 1f;
        }

        // ======================== 内部方法 ========================

        private void ChangeState(GameState newState)
        {
            var oldState = CurrentState;
            CurrentState = newState;
            Debug.Log($"[GameManager] 状态切换：{oldState} → {newState}");
            GameEvents.GameStateChanged(newState);
        }

        private void HandleGameOver(GameOverReason reason)
        {
            Debug.Log($"[GameManager] 游戏结束：{reason}");

            // 结算本局
            int score = CalculateScore();
            int guanYun = CalculateGuanYun(score);

            // 记录历史
            var record = new RunRecord
            {
                RunId = System.Guid.NewGuid().ToString(),
                StartTime = System.DateTime.Now, // 简化处理
                EndTime = System.DateTime.Now,
                PlayerName = CurrentRun.Player.PlayerName,
                FinalRank = CurrentRun.Player.CurrentRank.ToString(),
                TotalTurns = CurrentRun.Player.CurrentTurn,
                GameOverReason = reason.ToString(),
                Score = score,
                GuanYunEarned = guanYun
            };

            MetaProgress.TotalGuanYun += guanYun;
            MetaProgress.TotalRuns++;
            MetaProgress.RunHistory.Add(record);

            // 保存Meta进度
            SaveSystem.Instance.SaveMetaProgress(MetaProgress);

            ChangeState(GameState.GameOver);
        }

        /// <summary>计算本局得分</summary>
        private int CalculateScore()
        {
            if (CurrentRun == null) return 0;
            var p = CurrentRun.Player;

            int score = 0;
            score += (int)p.CurrentRank * 100;    // 官职贡献
            score += p.CurrentTurn * 10;           // 存活回合
            score += p.Reputation * 2;             // 声望贡献
            score += p.Intellect + p.Charisma + p.Scheming;  // 属性总和

            return Mathf.Max(0, score);
        }

        /// <summary>计算本局获得的官运点数</summary>
        private int CalculateGuanYun(int score)
        {
            return Mathf.FloorToInt(score * 0.1f);
        }
    }
}
