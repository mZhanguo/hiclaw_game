using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuantuFucheng.Systems
{
    /// <summary>
    /// 音效管理器 - 管理BGM、SFX和环境音
    /// 单例模式，跨场景持久化
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float BGMVolume = 0.6f;
        [Range(0f, 1f)] public float SFXVolume = 0.8f;
        [Range(0f, 1f)] public float AmbientVolume = 0.4f;

        [Header("Fade Settings")]
        [SerializeField] private float defaultFadeDuration = 1.5f;

        // 音效配置
        private AudioConfig audioConfig;
        private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private Coroutine bgmFadeCoroutine;
        private Coroutine ambientFadeCoroutine;
        private string currentBGMKey;
        private string currentAmbientKey;

        #region Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            LoadAudioConfig();
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                var bgmObj = new GameObject("BGM_Source");
                bgmObj.transform.SetParent(transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                var sfxObj = new GameObject("SFX_Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            if (ambientSource == null)
            {
                var ambObj = new GameObject("Ambient_Source");
                ambObj.transform.SetParent(transform);
                ambientSource = ambObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
        }

        private void LoadAudioConfig()
        {
            var configText = Resources.Load<TextAsset>("Audio/AudioConfig");
            if (configText != null)
            {
                audioConfig = JsonUtility.FromJson<AudioConfig>(configText.text);
                Debug.Log($"[AudioManager] Loaded {audioConfig.BGM.Count} BGM, {audioConfig.SFX.Count} SFX, {audioConfig.Ambient.Count} Ambient entries.");
            }
            else
            {
                Debug.LogWarning("[AudioManager] AudioConfig.json not found in Resources/Audio/");
                audioConfig = new AudioConfig();
            }
        }

        #endregion

        #region BGM

        /// <summary>
        /// 播放BGM，带淡入淡出
        /// </summary>
        /// <param name="bgmKey">BGM键名，如 "main_menu", "morning_court"</param>
        /// <param name="fadeDuration">淡入淡出时间</param>
        public void PlayBGM(string bgmKey, float fadeDuration = -1f)
        {
            if (bgmKey == currentBGMKey && bgmSource.isPlaying)
                return;

            if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

            var entry = audioConfig.BGM.Find(e => e.Key == bgmKey);
            if (entry == null)
            {
                Debug.LogWarning($"[AudioManager] BGM key not found: {bgmKey}");
                return;
            }

            AudioClip clip = LoadClip(entry.FilePath);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] BGM clip not found: {entry.FilePath}");
                return;
            }

            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(CrossfadeBGM(clip, fadeDuration));
            currentBGMKey = bgmKey;
        }

        /// <summary>
        /// 停止BGM，带淡出
        /// </summary>
        public void StopBGM(float fadeDuration = -1f)
        {
            if (fadeDuration < 0) fadeDuration = defaultFadeDuration;
            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeOut(bgmSource, fadeDuration));
            currentBGMKey = null;
        }

        private IEnumerator CrossfadeBGM(AudioClip newClip, float duration)
        {
            // 淡出当前BGM
            if (bgmSource.isPlaying)
            {
                float startVol = bgmSource.volume;
                float halfDuration = duration * 0.5f;
                for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(startVol, 0f, t / halfDuration);
                    yield return null;
                }
                bgmSource.Stop();
            }

            // 切换并淡入
            bgmSource.clip = newClip;
            bgmSource.volume = 0f;
            bgmSource.Play();

            float targetVol = BGMVolume * MasterVolume;
            float halfDur = duration * 0.5f;
            for (float t = 0; t < halfDur; t += Time.unscaledDeltaTime)
            {
                bgmSource.volume = Mathf.Lerp(0f, targetVol, t / halfDur);
                yield return null;
            }
            bgmSource.volume = targetVol;
        }

        /// <summary>
        /// 根据游戏阶段自动选BGM
        /// </summary>
        public void PlayBGMForPhase(GamePhase phase)
        {
            string bgmKey = phase switch
            {
                GamePhase.MainMenu => "main_menu",
                GamePhase.MorningCourt => "morning_court",
                GamePhase.Decision => "decision",
                GamePhase.Review => "review",
                GamePhase.Evaluation => "evaluation",
                _ => "main_menu"
            };
            PlayBGM(bgmKey);
        }

        #endregion

        #region SFX

        /// <summary>
        /// 播放一次性音效
        /// </summary>
        /// <param name="sfxKey">SFX键名，如 "page_flip", "stamp", "promotion"</param>
        /// <param name="volumeScale">音量缩放（0-1）</param>
        public void PlaySFX(string sfxKey, float volumeScale = 1f)
        {
            var entry = audioConfig.SFX.Find(e => e.Key == sfxKey);
            if (entry == null)
            {
                Debug.LogWarning($"[AudioManager] SFX key not found: {sfxKey}");
                return;
            }

            AudioClip clip = LoadClip(entry.FilePath);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX clip not found: {entry.FilePath}");
                return;
            }

            sfxSource.PlayOneShot(clip, SFXVolume * MasterVolume * volumeScale);
        }

        /// <summary>
        /// 播放UI音效的快捷方法
        /// </summary>
        public void PlayButtonClick() => PlaySFX("button_click");
        public void PlayPageFlip() => PlaySFX("page_flip");
        public void PlayStamp() => PlaySFX("stamp");
        public void PlayPromotion() => PlaySFX("promotion");
        public void PlayDemotion() => PlaySFX("demotion");
        public void PlayCardDraw() => PlaySFX("card_draw");
        public void PlayCardPlay() => PlaySFX("card_play");
        public void PlayNotification() => PlaySFX("notification");

        #endregion

        #region Ambient (环境音)

        /// <summary>
        /// 播放环境音，带淡入
        /// </summary>
        /// <param name="ambientKey">环境音键名，如 "brush_writing", "guqin"</param>
        public void PlayAmbient(string ambientKey, float fadeDuration = -1f)
        {
            if (ambientKey == currentAmbientKey && ambientSource.isPlaying)
                return;

            if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

            var entry = audioConfig.Ambient.Find(e => e.Key == ambientKey);
            if (entry == null)
            {
                Debug.LogWarning($"[AudioManager] Ambient key not found: {ambientKey}");
                return;
            }

            AudioClip clip = LoadClip(entry.FilePath);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] Ambient clip not found: {entry.FilePath}");
                return;
            }

            if (ambientFadeCoroutine != null) StopCoroutine(ambientFadeCoroutine);

            ambientSource.clip = clip;
            ambientSource.volume = 0f;
            ambientSource.Play();
            ambientFadeCoroutine = StartCoroutine(FadeIn(ambientSource, AmbientVolume * MasterVolume, fadeDuration));
            currentAmbientKey = ambientKey;
        }

        /// <summary>
        /// 停止环境音
        /// </summary>
        public void StopAmbient(float fadeDuration = -1f)
        {
            if (fadeDuration < 0) fadeDuration = defaultFadeDuration;
            if (ambientFadeCoroutine != null) StopCoroutine(ambientFadeCoroutine);
            ambientFadeCoroutine = StartCoroutine(FadeOut(ambientSource, fadeDuration));
            currentAmbientKey = null;
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            PlayerPrefs.SetFloat("Audio_Master", MasterVolume);
        }

        /// <summary>
        /// 设置BGM音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            BGMVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            PlayerPrefs.SetFloat("Audio_BGM", BGMVolume);
        }

        /// <summary>
        /// 设置SFX音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("Audio_SFX", SFXVolume);
        }

        /// <summary>
        /// 设置环境音音量
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            AmbientVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            PlayerPrefs.SetFloat("Audio_Ambient", AmbientVolume);
        }

        /// <summary>
        /// 从PlayerPrefs加载音量设置
        /// </summary>
        public void LoadVolumeSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat("Audio_Master", 1f);
            BGMVolume = PlayerPrefs.GetFloat("Audio_BGM", 0.6f);
            SFXVolume = PlayerPrefs.GetFloat("Audio_SFX", 0.8f);
            AmbientVolume = PlayerPrefs.GetFloat("Audio_Ambient", 0.4f);
            ApplyVolumeSettings();
        }

        private void ApplyVolumeSettings()
        {
            if (bgmSource.isPlaying)
                bgmSource.volume = BGMVolume * MasterVolume;
            if (ambientSource.isPlaying)
                ambientSource.volume = AmbientVolume * MasterVolume;
        }

        #endregion

        #region Utilities

        private AudioClip LoadClip(string resourcePath)
        {
            if (clipCache.TryGetValue(resourcePath, out AudioClip cached))
                return cached;

            AudioClip clip = Resources.Load<AudioClip>(resourcePath);
            if (clip != null)
                clipCache[resourcePath] = clip;

            return clip;
        }

        /// <summary>
        /// 预加载音效到缓存
        /// </summary>
        public void PreloadClips(params string[] keys)
        {
            foreach (var key in keys)
            {
                var entry = audioConfig.SFX.Find(e => e.Key == key)
                    ?? audioConfig.BGM.Find(e => e.Key == key) as AudioEntry
                    ?? audioConfig.Ambient.Find(e => e.Key == key);
                if (entry != null) LoadClip(entry.FilePath);
            }
        }

        /// <summary>
        /// 清理未使用的缓存
        /// </summary>
        public void ClearCache()
        {
            clipCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        private IEnumerator FadeIn(AudioSource source, float targetVolume, float duration)
        {
            source.volume = 0f;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(0f, targetVolume, t / duration);
                yield return null;
            }
            source.volume = targetVolume;
        }

        private IEnumerator FadeOut(AudioSource source, float duration)
        {
            float startVol = source.volume;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            source.volume = 0f;
            source.Stop();
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// 游戏阶段枚举（用于自动BGM切换）
    /// </summary>
    public enum GamePhase
    {
        MainMenu,
        MorningCourt,
        Decision,
        Review,
        Evaluation
    }

    /// <summary>
    /// 音效配置数据结构
    /// </summary>
    [Serializable]
    public class AudioConfig
    {
        public List<AudioEntry> BGM = new List<AudioEntry>();
        public List<AudioEntry> SFX = new List<AudioEntry>();
        public List<AudioEntry> Ambient = new List<AudioEntry>();
    }

    [Serializable]
    public class AudioEntry
    {
        public string Key;
        public string FilePath;
        public string Description;
        public float DefaultVolume = 1f;
    }

    #endregion
}
