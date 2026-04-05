// ============================================================================
// 官途浮沉 - 存档系统
// SaveSystem.cs — 本地JSON存档，管理单局存档和Meta永久进度
// ============================================================================

using System;
using System.IO;
using UnityEngine;
using GuantuFucheng.Core;
using GuantuFucheng.Models;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// 存档系统 — 基于JSON的本地持久化
    /// 
    /// 存档策略：
    /// 1. 单局存档（RunState）：每回合结束自动保存，只保留一个槽位
    ///    - 文件：{persistentDataPath}/save_run.json
    ///    - Roguelite设计：死亡后存档删除（不能SL）
    /// 
    /// 2. Meta进度（MetaProgressData）：永久保存，独立于单局
    ///    - 文件：{persistentDataPath}/save_meta.json
    ///    - 记录：永久升级、解锁内容、历史战绩、成就
    /// 
    /// 设计考量：
    /// - 使用JsonUtility序列化（Unity内置，无需第三方库）
    /// - 存档加密/校验可在后续版本加入
    /// - 支持导出/导入存档（便于测试和分享）
    /// </summary>
    public class SaveSystem : Singleton<SaveSystem>
    {
        // ======================== 路径 ========================

        private string RunSavePath => Path.Combine(Application.persistentDataPath, "save_run.json");
        private string MetaSavePath => Path.Combine(Application.persistentDataPath, "save_meta.json");
        private string BackupDir => Path.Combine(Application.persistentDataPath, "backups");

        // ======================== 单局存档 ========================

        /// <summary>保存当前局状态</summary>
        public void SaveRunState(RunState state)
        {
            try
            {
                string json = JsonUtility.ToJson(state, true);
                File.WriteAllText(RunSavePath, json);
                Debug.Log($"[SaveSystem] 单局存档已保存：{RunSavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 保存失败：{e.Message}");
            }
        }

        /// <summary>加载单局存档</summary>
        public RunState LoadRunState()
        {
            try
            {
                if (!File.Exists(RunSavePath))
                {
                    Debug.Log("[SaveSystem] 没有找到单局存档");
                    return null;
                }

                string json = File.ReadAllText(RunSavePath);
                var state = JsonUtility.FromJson<RunState>(json);
                Debug.Log("[SaveSystem] 单局存档已加载");
                return state;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 加载失败：{e.Message}");
                return null;
            }
        }

        /// <summary>删除单局存档（Roguelite：死亡后清除）</summary>
        public void DeleteRunSave()
        {
            try
            {
                if (File.Exists(RunSavePath))
                {
                    File.Delete(RunSavePath);
                    Debug.Log("[SaveSystem] 单局存档已删除");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 删除存档失败：{e.Message}");
            }
        }

        /// <summary>是否存在单局存档</summary>
        public bool HasRunSave()
        {
            return File.Exists(RunSavePath);
        }

        // ======================== Meta永久进度 ========================

        /// <summary>保存Meta进度</summary>
        public void SaveMetaProgress(MetaProgressData meta)
        {
            try
            {
                string json = JsonUtility.ToJson(meta, true);
                File.WriteAllText(MetaSavePath, json);
                Debug.Log("[SaveSystem] Meta进度已保存");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Meta保存失败：{e.Message}");
            }
        }

        /// <summary>加载Meta进度</summary>
        public MetaProgressData LoadMetaProgress()
        {
            try
            {
                if (!File.Exists(MetaSavePath))
                {
                    Debug.Log("[SaveSystem] 没有找到Meta存档，创建新进度");
                    return new MetaProgressData();
                }

                string json = File.ReadAllText(MetaSavePath);
                var meta = JsonUtility.FromJson<MetaProgressData>(json);
                Debug.Log($"[SaveSystem] Meta进度已加载：{meta.TotalRuns}局游戏记录，{meta.TotalGuanYun}官运");
                return meta;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Meta加载失败：{e.Message}");
                return new MetaProgressData();
            }
        }

        // ======================== 备份 ========================

        /// <summary>创建存档备份</summary>
        public void CreateBackup()
        {
            try
            {
                if (!Directory.Exists(BackupDir))
                    Directory.CreateDirectory(BackupDir);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                if (File.Exists(RunSavePath))
                {
                    string backupPath = Path.Combine(BackupDir, $"save_run_{timestamp}.json");
                    File.Copy(RunSavePath, backupPath);
                }

                if (File.Exists(MetaSavePath))
                {
                    string backupPath = Path.Combine(BackupDir, $"save_meta_{timestamp}.json");
                    File.Copy(MetaSavePath, backupPath);
                }

                Debug.Log($"[SaveSystem] 备份已创建：{timestamp}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 备份失败：{e.Message}");
            }
        }

        /// <summary>导出存档为JSON字符串（用于分享/调试）</summary>
        public string ExportRunState()
        {
            if (File.Exists(RunSavePath))
                return File.ReadAllText(RunSavePath);
            return null;
        }

        /// <summary>从JSON字符串导入存档</summary>
        public bool ImportRunState(string json)
        {
            try
            {
                // 验证JSON格式
                var test = JsonUtility.FromJson<RunState>(json);
                if (test == null) return false;

                File.WriteAllText(RunSavePath, json);
                Debug.Log("[SaveSystem] 存档导入成功");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 导入失败：{e.Message}");
                return false;
            }
        }

        // ======================== 清理 ========================

        /// <summary>清除所有存档（谨慎使用）</summary>
        public void ClearAllSaves()
        {
            DeleteRunSave();
            if (File.Exists(MetaSavePath))
                File.Delete(MetaSavePath);
            Debug.Log("[SaveSystem] 所有存档已清除");
        }
    }
}
