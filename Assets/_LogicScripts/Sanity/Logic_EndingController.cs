using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 深渊结局控制
/// </summary>
public class Logic_EndingController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject endingCanvas;
    [SerializeField] private TMP_Text endingText;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            TriggerEnding();
        }
    }

    void TriggerEnding()
    {
        int sanity = Core_SanityManager.Instance.GetSanity();

        string result = "";

        if (sanity > 70)
        {
            result = "Ending A：你惊醒，一切如常，但手中握着黄铜钥匙。";
        }
        else if (sanity >= 40)
        {
            result = "Ending B：你坠入深渊，又回到了1408房间，循环开始。";
        }
        else
        {
            result = "Ending C：你被深渊吞噬……";
        }

        endingCanvas.SetActive(true);
        endingText.text = result;

        Time.timeScale = 0f; // 冻结游戏
    }
}
