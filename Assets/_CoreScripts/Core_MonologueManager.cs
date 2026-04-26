using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Core_MonologueManager : MonoBehaviour
{
    public static Core_MonologueManager Instance;

    [Header("UI 绑定")]
    public TextMeshProUGUI monologueText;
    public CanvasGroup textCanvasGroup; 

    [Header("时间物理参数")]
    public float fadeDuration = 0.5f;   
    public float baseDisplayTime = 2.0f; 
    public float timePerCharacter = 0.1f; 

    private Queue<string> monologueQueue = new Queue<string>();
    private bool isDisplaying = false;

    private void Awake()
    {
        Instance = this; 
        isDisplaying = false;
        monologueQueue.Clear();

        // 初始态净化：彻底消灭开局的 "New Text" 占位符穿帮
        if (monologueText != null) monologueText.text = "";
        if (textCanvasGroup != null) textCanvasGroup.alpha = 0f;
    }

    // 外部数据推入接口
    public void ShowMonologue(string text)
    {
        // 终极防御机制：如果发现 UI 因为场景重载被销毁 (MissingReferenceException)，则动态寻回它！
        if (textCanvasGroup == null || monologueText == null)
        {
            // 通过 GameObject 的名字在当前活跃场景中强行寻找幸存的 UI
            GameObject uiObj = GameObject.Find("UI_MonologueText"); 
            if (uiObj != null)
            {
                textCanvasGroup = uiObj.GetComponent<CanvasGroup>();
                monologueText = uiObj.GetComponent<TextMeshProUGUI>();
                
                // 找到后立刻净化防穿帮
                monologueText.text = "";
                textCanvasGroup.alpha = 0f;
            }
            else
            {
                Debug.LogWarning("无法找到 UI_MonologueText，请检查它是否改名了！独白将被跳过。");
                return;
            }
        }

        monologueQueue.Enqueue(text);
        
        if (!isDisplaying && gameObject.activeInHierarchy)
        {
            StartCoroutine(DisplayRoutine());
        }
    }

    private IEnumerator DisplayRoutine()
    {
        isDisplaying = true;

        while (monologueQueue.Count > 0)
        {
            string currentLine = monologueQueue.Dequeue();
            
            // 每次播放前进行防御性二次检查
            if (monologueText == null || textCanvasGroup == null) break;

            monologueText.text = currentLine;
            float displayDuration = baseDisplayTime + (currentLine.Length * timePerCharacter);

            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDuration));

            yield return new WaitForSeconds(0.2f);
        }

        isDisplaying = false;
    }

    // 底层线性透明度插值发生器 (防闪退版)
    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration)
    {
        if (textCanvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 防止在渐变的中途，UI 突然被销毁导致报错
            if (textCanvasGroup == null) yield break; 
            
            elapsed += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        
        if (textCanvasGroup != null) textCanvasGroup.alpha = endAlpha;
    }
}