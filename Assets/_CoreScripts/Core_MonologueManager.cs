using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Core_MonologueManager : MonoBehaviour
{
    public static Core_MonologueManager Instance;

    [Header("UI 绑定")]
    public TextMeshProUGUI monologueText;
    public CanvasGroup textCanvasGroup; // 用于控制透明度渐变

    [Header("时间物理参数")]
    public float fadeDuration = 0.5f;   // 渐入渐出所需秒数
    public float baseDisplayTime = 2.0f; // 基础停留秒数
    public float timePerCharacter = 0.1f; // 根据字数动态延长的秒数

    private Queue<string> monologueQueue = new Queue<string>();
    private bool isDisplaying = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始物理状态强制隐藏
        if (textCanvasGroup != null) textCanvasGroup.alpha = 0f;
    }

    // 外部数据推入接口
    public void ShowMonologue(string text)
    {
        monologueQueue.Enqueue(text);
        
        // 如果当前没有在播放，则启动渲染泵
        if (!isDisplaying)
        {
            StartCoroutine(DisplayRoutine());
        }
    }

    private IEnumerator DisplayRoutine()
    {
        isDisplaying = true;

        while (monologueQueue.Count > 0)
        {
            // 从队列头部抽取数据
            string currentLine = monologueQueue.Dequeue();
            monologueText.text = currentLine;

            // 计算该句子的物理停留时间
            float displayDuration = baseDisplayTime + (currentLine.Length * timePerCharacter);

            // 1. Alpha 渐入
            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));

            // 2. 悬停展示
            yield return new WaitForSeconds(displayDuration);

            // 3. Alpha 渐出
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDuration));

            // 句与句之间的微小物理呼吸间隔
            yield return new WaitForSeconds(0.2f);
        }

        isDisplaying = false;
    }

    // 底层线性透明度插值发生器
    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        textCanvasGroup.alpha = endAlpha;
    }
}