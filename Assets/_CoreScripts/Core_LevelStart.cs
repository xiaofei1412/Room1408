using UnityEngine;
using System.Collections;

public class Core_LevelStart : MonoBehaviour
{
    [Header("视觉 UI 绑定")]
    public CanvasGroup blackScreenCanvasGroup;

    [Header("物理时序配置")]
    public float blackScreenHoldTime = 2.0f; // 初始黑屏保持时间
    public float fadeOutDuration = 4.0f;     // 视觉恢复所需的插值秒数

    private Core_FirstPersonController playerController;
    private Core_Raycaster raycaster;

    private void Start()
    {
        playerController = FindObjectOfType<Core_FirstPersonController>();
        raycaster = FindObjectOfType<Core_Raycaster>();
        
        StartCoroutine(WakeUpSequence());
    }

    private IEnumerator WakeUpSequence()
    {
        // 1. 物理状态冻结与视觉封锁
        if (blackScreenCanvasGroup != null) blackScreenCanvasGroup.alpha = 1f;
        if (playerController != null) playerController.enabled = false;
        if (raycaster != null) raycaster.enabled = false; // 禁止在黑屏时瞎点

        // 2. 模拟意识停顿的延迟
        yield return new WaitForSeconds(blackScreenHoldTime);

        // 3. 视觉线性渐隐 (Alpha 1 -> 0)
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            if (blackScreenCanvasGroup != null)
            {
                blackScreenCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            }
            yield return null;
        }
        if (blackScreenCanvasGroup != null) blackScreenCanvasGroup.alpha = 0f;

        // 4. 解除物理冻结，交还控制权
        if (playerController != null) playerController.enabled = true;
        if (raycaster != null) raycaster.enabled = true;

        // 5. 触发全英文内心独白队列
        if (Core_MonologueManager.Instance != null)
        {
            Core_MonologueManager.Instance.ShowMonologue("Where am I? My head is splitting...");
            Core_MonologueManager.Instance.ShowMonologue("This looks like a hotel room. How did I end up here?");
            Core_MonologueManager.Instance.ShowMonologue("I should check that main door first.");
        }
        raycaster.focusTarget = GameObject.Find("Prop_Lock_MainEntrance");
    }
}