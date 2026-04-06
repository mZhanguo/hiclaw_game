// ============================================================================
// 官途浮沉 - 编辑器快捷工具
// QuickSetup.cs — 菜单栏一键操作：搭建场景 / 运行模拟 / 数据检查
// ============================================================================
// 菜单路径：
//   官途浮沉/一键搭建场景     — 在当前场景自动创建所有必需对象
//   官途浮沉/运行模拟(6回合)  — 在编辑器内运行6回合模拟并输出日志
//   官途浮沉/数据检查         — 验证所有JSON数据文件的完整性
// ============================================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuantuFucheng.Editor
{
    public static class QuickSetup
    {
        // ================================================================
        // 一键搭建场景
        // ================================================================

        [MenuItem("官途浮沉/一键搭建场景 %#B", false, 1)]
        public static void BuildScene()
        {
            Debug.Log("══════════════════════════════════════════");
            Debug.Log("  《官途浮沉》一键搭建场景");
            Debug.Log("══════════════════════════════════════════");

            // 1. 检查当前场景是否需要保存
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.isDirty)
            {
                if (!EditorUtility.DisplayDialog("场景未保存",
                    "当前场景有未保存的更改，是否先保存？", "保存并继续", "取消"))
                {
                    return;
                }
                EditorSceneManager.SaveScene(scene);
            }

            // 2. 创建 [SceneSetup] 根物体
            var setupObj = GameObject.Find("[SceneSetup]");
            if (setupObj == null)
            {
                setupObj = new GameObject("[SceneSetup]");
                Undo.RegisterCreatedObjectUndo(setupObj, "创建 SceneSetup");
            }

            // 3. 挂载 SceneSetup 组件（如果没有）
            var setupScript = setupObj.GetComponent<Core.SceneSetup>();
            if (setupScript == null)
            {
                setupScript = Undo.AddComponent<Core.SceneSetup>(setupObj);
            }

            // 4. 执行场景搭建
            setupScript.Setup();

            // 5. 标记场景已修改
            EditorSceneManager.MarkSceneDirty(scene);

            // 6. 保存场景到 Assets/Scenes/
            EnsureScenesFolder();
            string scenePath = "Assets/Scenes/MainScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            EditorUtility.DisplayDialog("搭建完成",
                $"场景已搭建并保存到:\n{scenePath}\n\n" +
                "包含：\n" +
                "• EventSystem\n" +
                "• Canvas (1920×1080)\n" +
                "• 8个核心Manager单例\n" +
                "• 8个UI面板 + 过渡遮罩\n" +
                "• GameBootstrap 数据加载器\n\n" +
                "点击 Play 即可运行游戏！",
                "好的");

            Debug.Log("[QuickSetup] ✓ 场景搭建完成！点击Play运行游戏。");
        }

        // ================================================================
        // 运行模拟（6回合）
        // ================================================================

        [MenuItem("官途浮沉/运行模拟(6回合) %#R", false, 2)]
        public static void RunSimulation()
        {
            Debug.Log("══════════════════════════════════════════");
            Debug.Log("  《官途浮沉》6回合模拟");
            Debug.Log("══════════════════════════════════════════");

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("无法模拟", "游戏正在运行中，请先停止Play模式。", "好的");
                return;
            }

            // 加载并验证数据
            string dataPath = Path.Combine(Application.dataPath, "Resources/GameData");
            if (!Directory.Exists(dataPath))
            {
                EditorUtility.DisplayDialog("数据缺失",
                    $"找不到数据目录：\n{dataPath}\n\n请确保JSON数据文件已放置。", "好的");
                return;
            }

            try
            {
                // 加载JSON数据
                var cardsJson = LoadJsonResource("GameData/CardDatabase");
                var npcsJson = LoadJsonResource("GameData/NPCDatabase");
                var ranksJson = LoadJsonResource("GameData/RankDatabase");
                var evalJson = LoadJsonResource("GameData/EvaluationConfig");

                Debug.Log("[模拟] 数据加载成功");

                // 解析卡牌
                var cardWrapper = JsonUtility.FromJson<CardDatabaseWrapper>(cardsJson);
                var npcWrapper = JsonUtility.FromJson<NPCDatabaseWrapper>(npcsJson);

                int cardCount = cardWrapper?.cards?.Length ?? 0;
                int npcCount = npcWrapper?.npcs?.Length ?? 0;

                Debug.Log($"[模拟] 卡牌: {cardCount}张, NPC: {npcCount}个");
                Debug.Log("");

                // 模拟6回合
                System.Random rng = new System.Random();
                string[] actionTypes = { "政务", "交际", "情报", "修身", "谋略", "休息" };
                string[] ranks = { "候补", "县令", "知州", "知府", "布政使", "巡抚", "总督", "入阁" };

                int politics = 30, charisma = 25, intelligence = 28, resolve = 22, strategy = 20;
                int reputation = 50, wealth = 100;
                int currentRankIdx = 1; // 县令
                int actionPoints = 6;

                Debug.Log($"═══ 初始状态 ═══");
                Debug.Log($"  官职: {ranks[currentRankIdx]}");
                Debug.Log($"  属性: 政务{politics} 交际{charisma} 情报{intelligence} 修身{resolve} 谋略{strategy}");
                Debug.Log($"  声望: {reputation}  银两: {wealth}");
                Debug.Log("");

                for (int turn = 1; turn <= 6; turn++)
                {
                    Debug.Log($"╔══ 第{turn}回合 ══════════════════════════╗");

                    // 早朝简报
                    Debug.Log($"║ 【早朝简报】");
                    if (cardCount > 0)
                    {
                        int eventIdx = rng.Next(cardCount);
                        var card = cardWrapper.cards[eventIdx];
                        Debug.Log($"║   事件卡牌: [{card.id}] {card.title}");
                    }

                    // 行动分配（随机分配6点）
                    Debug.Log($"║ 【行动分配】{actionPoints}点行动力");
                    int[] allocation = RandomAllocate(actionPoints, 6, rng);
                    for (int i = 0; i < 6; i++)
                    {
                        if (allocation[i] > 0)
                            Debug.Log($"║   {actionTypes[i]}: {allocation[i]}点");
                    }

                    // 执行结算
                    int dpol = allocation[0] * rng.Next(2, 5);
                    int dcha = allocation[1] * rng.Next(2, 5);
                    int dint = allocation[2] * rng.Next(1, 4);
                    int dres = allocation[3] * rng.Next(2, 4);
                    int dstr = allocation[4] * rng.Next(1, 5);
                    int drep = rng.Next(-5, 10);
                    int dwea = rng.Next(-20, 30);

                    politics += dpol; charisma += dcha; intelligence += dint;
                    resolve += dres; strategy += dstr;
                    reputation += drep; wealth += dwea;

                    // NPC互动
                    if (npcCount > 0)
                    {
                        int npcIdx = rng.Next(npcCount);
                        var npc = npcWrapper.npcs[npcIdx];
                        int favorDelta = rng.Next(-10, 15);
                        Debug.Log($"║ 【NPC互动】{npc.name} 好感{(favorDelta >= 0 ? "+" : "")}{favorDelta}");
                    }

                    // 复盘
                    Debug.Log($"║ 【复盘】");
                    Debug.Log($"║   属性变化: 政务+{dpol} 交际+{dcha} 情报+{dint} 修身+{dres} 谋略+{dstr}");
                    Debug.Log($"║   声望{(drep >= 0 ? "+" : "")}{drep}={reputation}  银两{(dwea >= 0 ? "+" : "")}{dwea}={wealth}");

                    // 检查升迁（每3回合考评）
                    if (turn % 3 == 0 && reputation > 60 && currentRankIdx < ranks.Length - 1)
                    {
                        currentRankIdx++;
                        Debug.Log($"║ 【吏部考评】★ 升迁至 {ranks[currentRankIdx]}！");
                    }

                    Debug.Log($"╚══════════════════════════════════════════╝");
                    Debug.Log("");
                }

                Debug.Log($"═══ 最终状态 ═══");
                Debug.Log($"  官职: {ranks[currentRankIdx]}");
                Debug.Log($"  属性: 政务{politics} 交际{charisma} 情报{intelligence} 修身{resolve} 谋略{strategy}");
                Debug.Log($"  声望: {reputation}  银两: {wealth}");

                EditorUtility.DisplayDialog("模拟完成",
                    $"6回合模拟已完成！\n\n" +
                    $"最终官职: {ranks[currentRankIdx]}\n" +
                    $"声望: {reputation} | 银两: {wealth}\n\n" +
                    "详细日志请查看Console窗口。",
                    "好的");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[模拟] 错误: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("模拟失败", $"运行出错：\n{ex.Message}", "好的");
            }
        }

        // ================================================================
        // 数据检查
        // ================================================================

        [MenuItem("官途浮沉/数据检查 %#D", false, 3)]
        public static void ValidateData()
        {
            Debug.Log("══════════════════════════════════════════");
            Debug.Log("  《官途浮沉》数据完整性检查");
            Debug.Log("══════════════════════════════════════════");

            string dataPath = Path.Combine(Application.dataPath, "Resources/GameData");
            int errors = 0;
            int warnings = 0;

            // 1. 检查数据目录
            if (!Directory.Exists(dataPath))
            {
                LogError(ref errors, $"数据目录不存在: {dataPath}");
                ShowResult(errors, warnings);
                return;
            }
            LogPass("数据目录存在");

            // 2. 检查必需的JSON文件
            string[] requiredFiles = {
                "CardDatabase.json",
                "NPCDatabase.json",
                "RankDatabase.json",
                "EvaluationConfig.json"
            };

            foreach (var file in requiredFiles)
            {
                string fullPath = Path.Combine(dataPath, file);
                if (!File.Exists(fullPath))
                {
                    LogError(ref errors, $"缺少数据文件: {file}");
                    continue;
                }

                var content = File.ReadAllText(fullPath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    LogError(ref errors, $"文件为空: {file}");
                    continue;
                }

                // 基础JSON格式检查
                content = content.Trim();
                if (!content.StartsWith("{") && !content.StartsWith("["))
                {
                    LogError(ref errors, $"JSON格式错误: {file}（不以 {{ 或 [ 开头）");
                    continue;
                }

                long size = new FileInfo(fullPath).Length;
                LogPass($"{file} ({size / 1024f:F1}KB)");
            }

            // 3. 详细数据验证
            Debug.Log("");
            Debug.Log("── 详细数据验证 ──");

            // 卡牌数据
            try
            {
                var cardsJson = LoadJsonFile(Path.Combine(dataPath, "CardDatabase.json"));
                if (cardsJson != null)
                {
                    var wrapper = JsonUtility.FromJson<CardDatabaseWrapper>(cardsJson);
                    int count = wrapper?.cards?.Length ?? 0;

                    if (count == 0)
                        LogError(ref errors, "卡牌数据为空（0张卡）");
                    else
                    {
                        LogPass($"卡牌数据: {count}张卡牌");

                        // 检查卡牌ID唯一性
                        var ids = new HashSet<string>();
                        foreach (var card in wrapper.cards)
                        {
                            if (string.IsNullOrEmpty(card.id))
                                LogError(ref errors, "存在ID为空的卡牌");
                            else if (!ids.Add(card.id))
                                LogWarn(ref warnings, $"卡牌ID重复: {card.id}");

                            if (string.IsNullOrEmpty(card.title))
                                LogWarn(ref warnings, $"卡牌 {card.id} 缺少标题");
                        }
                    }
                }
            }
            catch (Exception ex) { LogError(ref errors, $"卡牌数据解析失败: {ex.Message}"); }

            // NPC数据
            try
            {
                var npcsJson = LoadJsonFile(Path.Combine(dataPath, "NPCDatabase.json"));
                if (npcsJson != null)
                {
                    var wrapper = JsonUtility.FromJson<NPCDatabaseWrapper>(npcsJson);
                    int count = wrapper?.npcs?.Length ?? 0;

                    if (count == 0)
                        LogError(ref errors, "NPC数据为空（0个NPC）");
                    else
                    {
                        LogPass($"NPC数据: {count}个NPC");

                        foreach (var npc in wrapper.npcs)
                        {
                            if (string.IsNullOrEmpty(npc.id))
                                LogError(ref errors, "存在ID为空的NPC");
                            if (string.IsNullOrEmpty(npc.name))
                                LogWarn(ref warnings, $"NPC {npc.id} 缺少名称");
                        }
                    }
                }
            }
            catch (Exception ex) { LogError(ref errors, $"NPC数据解析失败: {ex.Message}"); }

            // 官职数据
            try
            {
                var ranksJson = LoadJsonFile(Path.Combine(dataPath, "RankDatabase.json"));
                if (ranksJson != null)
                {
                    var wrapper = JsonUtility.FromJson<RankDatabaseWrapper>(ranksJson);
                    int count = wrapper?.ranks?.Length ?? 0;

                    if (count == 0)
                        LogError(ref errors, "官职数据为空");
                    else
                        LogPass($"官职数据: {count}个官职等级");
                }
            }
            catch (Exception ex) { LogError(ref errors, $"官职数据解析失败: {ex.Message}"); }

            // 考评数据
            try
            {
                var evalJson = LoadJsonFile(Path.Combine(dataPath, "EvaluationConfig.json"));
                if (evalJson != null)
                {
                    LogPass("考评配置数据存在");
                }
            }
            catch (Exception ex) { LogError(ref errors, $"考评配置解析失败: {ex.Message}"); }

            // 4. 检查脚本文件完整性
            Debug.Log("");
            Debug.Log("── 脚本完整性检查 ──");

            string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
            string[] requiredScriptDirs = { "Core", "Systems", "UI", "Models", "Data", "Enums", "Events" };

            foreach (var dir in requiredScriptDirs)
            {
                string dirPath = Path.Combine(scriptsPath, dir);
                if (Directory.Exists(dirPath))
                {
                    int csFiles = Directory.GetFiles(dirPath, "*.cs").Length;
                    LogPass($"Scripts/{dir}/ — {csFiles}个脚本");
                }
                else
                {
                    LogWarn(ref warnings, $"Scripts/{dir}/ 目录不存在");
                }
            }

            // 5. 总结
            ShowResult(errors, warnings);
        }

        // ================================================================
        // 辅助方法
        // ================================================================

        private static void EnsureScenesFolder()
        {
            string scenesPath = Path.Combine(Application.dataPath, "Scenes");
            if (!Directory.Exists(scenesPath))
            {
                Directory.CreateDirectory(scenesPath);
                AssetDatabase.Refresh();
            }
        }

        private static string LoadJsonResource(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", path + ".json");
            if (File.Exists(fullPath))
                return File.ReadAllText(fullPath);
            return null;
        }

        private static string LoadJsonFile(string path)
        {
            if (File.Exists(path))
                return File.ReadAllText(path);
            return null;
        }

        private static int[] RandomAllocate(int total, int slots, System.Random rng)
        {
            int[] result = new int[slots];
            for (int i = 0; i < total; i++)
            {
                result[rng.Next(slots)]++;
            }
            return result;
        }

        private static void LogPass(string msg) => Debug.Log($"  ✅ {msg}");
        private static void LogError(ref int count, string msg) { count++; Debug.LogError($"  ❌ {msg}"); }
        private static void LogWarn(ref int count, string msg) { count++; Debug.LogWarning($"  ⚠️ {msg}"); }

        private static void ShowResult(int errors, int warnings)
        {
            Debug.Log("");
            Debug.Log("══════════════════════════════════════════");
            Debug.Log($"  检查完成: {errors}个错误, {warnings}个警告");
            Debug.Log("══════════════════════════════════════════");

            string icon = errors > 0 ? "❌" : (warnings > 0 ? "⚠️" : "✅");
            string status = errors > 0 ? "存在错误需修复" : (warnings > 0 ? "有警告但可运行" : "全部通过！");

            EditorUtility.DisplayDialog($"数据检查 {icon}",
                $"{status}\n\n错误: {errors}\n警告: {warnings}\n\n详情请查看Console窗口。",
                "好的");
        }

        // ================================================================
        // 简化的JSON解析辅助类（仅用于编辑器模拟，不依赖运行时Data类）
        // ================================================================

        [Serializable] private class CardDatabaseWrapper { public SimpleCard[] cards; }
        [Serializable] private class SimpleCard { public string id; public string title; public string category; }

        [Serializable] private class NPCDatabaseWrapper { public SimpleNPC[] npcs; }
        [Serializable] private class SimpleNPC { public string id; public string name; public string personality; }

        [Serializable] private class RankDatabaseWrapper { public SimpleRank[] ranks; }
        [Serializable] private class SimpleRank { public string id; public string name; public int level; }
    }
}
#endif
