using UnityEngine;
using System.Collections;

public class Audio_SoundManager : MonoBehaviour
{
    public static Audio_SoundManager Instance;

    [Header("常规音源 (Audio Sources)")]
    public AudioSource bgmSource;
    public AudioSource sfxSource2D;
    public AudioClip testBGM;

    [Header("理智折磨系统 (Sanity Audio)")]
    public AudioSource breathingSource;     // 呼吸声专用音源
    public AudioSource horrorAmbientSource; // 随机恐怖环境音专用音源
    public AudioClip heavyBreathingSFX;     // 急促呼吸声
    public AudioClip[] randomHorrorSFX;     // 6个随机环境音 (拖入面板)
    public float minRandomDelay = 5f;       // 随机音效最小间隔
    public float maxRandomDelay = 15f;      // 随机音效最大间隔

    [Header("通用交互音效")]
    public AudioClip pickupSFX; // 拾取音效
    public AudioClip dropSFX;   // 丢弃音效

    [Header("音量设置")]
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    private Coroutine randomAmbientCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 动态生成基础音源
            if (bgmSource == null) bgmSource = CreateAudioSource("BGM_Source", true, 0f);
            if (sfxSource2D == null) sfxSource2D = CreateAudioSource("SFX_2D_Source", false, 0f);
            
            // 动态生成理智系统专用音源
            if (breathingSource == null) 
            {
                breathingSource = CreateAudioSource("Breathing_Source", true, 0f);
                breathingSource.volume = 0f; // 初始静音
            }
            if (horrorAmbientSource == null) 
                horrorAmbientSource = CreateAudioSource("HorrorAmbient_Source", false, 0f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private AudioSource CreateAudioSource(string name, bool loop, float spatialBlend)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);
        AudioSource src = obj.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = false;
        src.spatialBlend = spatialBlend;
        return src;
    }

    private void Start()
    {
        if (testBGM != null) PlayBGM(testBGM);
    }

    private void Update()
    {
        // 核心心流：根据理智值和游戏状态接管背景音
        ManageSanityAudio();
    }

    private void ManageSanityAudio()
    {
        // 如果正在坠落或进入结局，绝对屏蔽呼吸声和随机吓人音效
        bool isEnding = Logic_EndgameFall.Instance != null && Logic_EndgameFall.Instance.isEndingActive;
        int currentSanity = Core_SanityManager.Instance != null ? Core_SanityManager.Instance.currentSanity : 100;

        if (!isEnding && currentSanity < 70)
        {
            // 1. 触发急促呼吸声
            if (breathingSource.clip != heavyBreathingSFX)
            {
                breathingSource.clip = heavyBreathingSFX;
                breathingSource.Play();
            }
            breathingSource.volume = Mathf.Lerp(breathingSource.volume, sfxVolume * 0.8f, Time.deltaTime * 2f);

            // 2. 触发随机恐怖环境音循环
            if (randomAmbientCoroutine == null && randomHorrorSFX.Length > 0)
            {
                randomAmbientCoroutine = StartCoroutine(RandomHorrorRoutine());
            }
        }
        else
        {
            // 恢复平静或进入结局时，掐断呼吸声与随机音效
            breathingSource.volume = Mathf.Lerp(breathingSource.volume, 0f, Time.deltaTime * 3f);
            if (breathingSource.volume < 0.01f && breathingSource.isPlaying) breathingSource.Stop();

            if (randomAmbientCoroutine != null)
            {
                StopCoroutine(randomAmbientCoroutine);
                randomAmbientCoroutine = null;
            }
        }
    }

    private IEnumerator RandomHorrorRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minRandomDelay, maxRandomDelay);
            yield return new WaitForSeconds(waitTime);

            if (horrorAmbientSource != null && randomHorrorSFX.Length > 0)
            {
                int randomIndex = Random.Range(0, randomHorrorSFX.Length);
                horrorAmbientSource.PlayOneShot(randomHorrorSFX[randomIndex], sfxVolume);
            }
        }
    }

    // --- 通用接口保持不变，新增拾取丢弃快捷方法 ---
    public void PlayBGM(AudioClip clip) {  if (clip == null) return; if (bgmSource.clip == clip && bgmSource.isPlaying) return; bgmSource.clip = clip; bgmSource.volume = bgmVolume; bgmSource.Play(); }
    public void StopBGM() { if (bgmSource.isPlaying) bgmSource.Stop(); }
    public void PlaySFX(AudioClip clip, Vector3 position) { if (clip != null) AudioSource.PlayClipAtPoint(clip, position, sfxVolume); }
    public void PlaySFX2D(AudioClip clip) { if (clip != null) sfxSource2D.PlayOneShot(clip, sfxVolume); }
    
    public void PlayPickup() { if (pickupSFX != null) PlaySFX2D(pickupSFX); }
    public void PlayDrop() { if (dropSFX != null) PlaySFX2D(dropSFX); }
}