using UnityEngine;
using System.Collections;
using TMPro; 
using UnityEngine.SceneManagement;

public class Logic_EndingACinematic : MonoBehaviour
{
    public Camera mainCamera;
    public CanvasGroup blackScreen;
    
    [Header("结局 A 字幕")]
    public TextMeshProUGUI subtitleText; 

    [Header("运镜参数")]
    public float pullDistance = 2.5f;
    public float pullDuration = 8.0f;

    [Header("演职人员滚幕 (Credits)")]
    public RectTransform creditsTransform; 
    public float scrollSpeed = 17.3f;      // 滚动速度 
    public float rollDuration = 135f;      // 滚动持续时间

    [Header("音效与时序卡点 (Audio Sync)")]
    public AudioClip creditsBGM; 
    public float holdDuration = 15f;       // 滚完后的定格悬念时间
    public float audioFadeDuration = 6f;   // 音乐变弱到消失的渐隐时间

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (subtitleText != null) 
            subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, 0f);

        if (creditsTransform != null)
            creditsTransform.gameObject.SetActive(false);

        StartCoroutine(CinematicSequence());
    }

    private IEnumerator CinematicSequence()
    {
        // 1. 睁眼：黑屏渐隐 
        float timer = 0f;
        while (timer < 3.0f)
        {
            timer += Time.deltaTime;
            if (blackScreen != null) blackScreen.alpha = Mathf.Lerp(1f, 0f, timer / 3.0f);
            yield return null;
        }

        // 2. 字幕浮现
        if (subtitleText != null)
        {
            subtitleText.text = "Was it all... just a nightmare?";
            yield return StartCoroutine(FadeText(0f, 1f, 1.0f)); 
            yield return new WaitForSeconds(3.0f);               
            yield return StartCoroutine(FadeText(1f, 0f, 1.0f)); 
        }

        // 3. 电影级后拉运镜 
        Vector3 startPos = mainCamera.transform.localPosition;
        Vector3 targetPos = startPos - new Vector3(pullDistance, 0, 0); 
        
        timer = 0f;
        while (timer < pullDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / pullDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            mainCamera.transform.localPosition = Vector3.Lerp(startPos, targetPos, smoothProgress);
            yield return null;
        }

        // 4. 再次陷入纯黑
        timer = 0f;
        while (timer < 2.0f)
        {
            timer += Time.deltaTime;
            if (blackScreen != null) blackScreen.alpha = Mathf.Lerp(0f, 1f, timer / 2.0f);
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        // 5. 播放终局 BGM
        if (creditsBGM != null && Audio_SoundManager.Instance != null)
        {
            Audio_SoundManager.Instance.PlayBGM(creditsBGM);
        }

        // 6. 演职人员滚幕 (带悬念定格)
        if (creditsTransform != null)
        {
            creditsTransform.gameObject.SetActive(true);
            float rollTimer = 0f;
            
            // 阶段 A：持续向上滚动
            while (rollTimer < rollDuration)
            {
                rollTimer += Time.deltaTime;
                creditsTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
                yield return null;
            }
            
            // 阶段 B：悬念定格！保留最后一行字在屏幕中央
            yield return new WaitForSeconds(holdDuration);
        }

        // 7. BGM 电影级渐隐 (Audio Fade Out)
        if (Audio_SoundManager.Instance != null && Audio_SoundManager.Instance.bgmSource != null)
        {
            float startVol = Audio_SoundManager.Instance.bgmSource.volume;
            float fadeTimer = 0f;

            while (fadeTimer < audioFadeDuration)
            {
                fadeTimer += Time.deltaTime;
                Audio_SoundManager.Instance.bgmSource.volume = Mathf.Lerp(startVol, 0f, fadeTimer / audioFadeDuration);
                
                // 吞噬最后的字幕
                if (blackScreen != null) blackScreen.alpha = Mathf.Lerp(0f, 1f, fadeTimer / audioFadeDuration);
                
                yield return null;
            }
            Audio_SoundManager.Instance.StopBGM();
        }

        yield return new WaitForSeconds(1.0f);

        // 8. 彻底结束
        PlayerPrefs.SetInt("EndingType", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Scene_GameOver"); 
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        if (subtitleText == null) yield break;
        float elapsed = 0f;
        Color c = subtitleText.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            subtitleText.color = c;
            yield return null;
        }
        c.a = endAlpha;
        subtitleText.color = c;
    }
}