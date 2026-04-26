using UnityEngine;
using System.Collections;

public class Core_LevelStart : MonoBehaviour
{
    [Header("视觉 UI 绑定")]
    public CanvasGroup blackScreenCanvasGroup;

    [Header("开局参数")]
    public float blackScreenHoldTime = 2.0f; 
    public float fadeOutDuration = 4.0f;     

    private void Start()
    {
        Time.timeScale = 1f;
        // 唤醒 BGM：因为音效管理器跨场景存活，重载时必须手动命令它重新播放
        if (Audio_SoundManager.Instance != null && Audio_SoundManager.Instance.testBGM != null)
        {
            Audio_SoundManager.Instance.PlayBGM(Audio_SoundManager.Instance.testBGM);
        }

        // 洗防止从深渊切回来时相机还是瞎的
        RenderSettings.fog = false;
        RenderSettings.fogDensity = 0.01f;
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.Skybox;
            mainCam.fieldOfView = 60f;
        }

        // 读取由 EndgameFall 写入的结局记忆
        int endingType = PlayerPrefs.GetInt("EndingType", 0);
        PlayerPrefs.SetInt("EndingType", 0); // 读完立刻清零
        PlayerPrefs.Save();

        // 二周目 (结局B) 传入 true，正常开局传入 false
        StartCoroutine(WakeUpSequence(endingType == 2)); 
    }

    private IEnumerator WakeUpSequence(bool isLooping)
    {
        Core_FirstPersonController playerController = Object.FindFirstObjectByType<Core_FirstPersonController>();
        Core_Raycaster raycaster = Object.FindFirstObjectByType<Core_Raycaster>();

        if (blackScreenCanvasGroup != null) 
        {
            blackScreenCanvasGroup.alpha = 1f;
            blackScreenCanvasGroup.blocksRaycasts = true; // 挡住瞎点
        }
        if (playerController != null) playerController.enabled = false;
        if (raycaster != null) raycaster.enabled = false;

        yield return new WaitForSecondsRealtime(blackScreenHoldTime);

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (blackScreenCanvasGroup != null) blackScreenCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        // 彻底关闭黑屏
        if (blackScreenCanvasGroup != null) 
        {
            blackScreenCanvasGroup.alpha = 0f;
            blackScreenCanvasGroup.blocksRaycasts = false;
        }

        if (playerController != null) playerController.enabled = true;
        if (raycaster != null) raycaster.enabled = true;

        if (isLooping)
        {
            Core_MonologueManager.Instance.ShowMonologue("I feel like I've been here before. The air is stale...");
        }
        else
        {
            Core_MonologueManager.Instance.ShowMonologue("Where am I? My head is splitting...");
            Core_MonologueManager.Instance.ShowMonologue("This looks like a hotel room. How did I end up here?");
            Core_MonologueManager.Instance.ShowMonologue("I should check that main door first.");
        }
        
        GameObject mainLock = GameObject.Find("Prop_Lock_MainEntrance");
        if (mainLock != null && raycaster != null) raycaster.focusTarget = mainLock;
    }
}