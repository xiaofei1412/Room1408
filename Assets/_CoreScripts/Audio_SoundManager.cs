using UnityEngine;

public class Audio_SoundManager : MonoBehaviour
{
    public static Audio_SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource2D;
    public AudioClip testBGM;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (bgmSource == null)
            {
                GameObject bgmObj = new GameObject("BGM_Source");
                bgmObj.transform.SetParent(transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.spatialBlend = 0f;
            }

            if (sfxSource2D == null)
            {
                GameObject sfxObj = new GameObject("SFX_2D_Source");
                sfxObj.transform.SetParent(transform);
                sfxSource2D = sfxObj.AddComponent<AudioSource>();
                sfxSource2D.playOnAwake = false;
                sfxSource2D.spatialBlend = 0f;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("PlayBGM failed: clip is null.");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying)
            bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        if (clip == null)
        {
            Debug.LogWarning("PlaySFX failed: clip is null.");
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
    }

    public void PlaySFX2D(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("PlaySFX2D failed: clip is null.");
            return;
        }

        sfxSource2D.PlayOneShot(clip, sfxVolume);
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    // Start 保证在 Awake 动态生成 AudioSource 之后执行
    private void Start()
    {
        if (testBGM != null)
        {
            PlayBGM(testBGM);
        }
    }
}