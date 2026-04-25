using UnityEngine;

public class Logic_ClockInteract : MonoBehaviour
{
    public void OnInteract()
    {
        // 呼叫全局的钟表 UI 中枢
        if (UI_ClockPuzzle.Instance != null)
        {
            UI_ClockPuzzle.Instance.OpenClock();
        }
        else
        {
            Debug.LogError("【物理断层】UI_ClockPuzzle 实例未找到，请检查 UI_ClockPanel 是否已挂载该脚本！");
        }
    }
}