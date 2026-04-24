using UnityEngine;

/// <summary>
/// TV 错误频道计数 → 扣理智
/// </summary>
public class Logic_TVSanity : MonoBehaviour
{
    [SerializeField] private int wrongCount = 0;
    [SerializeField] private int maxWrongCount = 3;
    [SerializeField] private int sanityCost = 15;

    private bool hasTriggered = false;

    /// <summary>
    /// 外部调用：切换频道
    /// </summary>
    public void SwitchChannel(string channel)
    {
        // 正确频道（不扣）
        if (channel == "1408")
        {
            Debug.Log("频道正确");
            return;
        }

        // 错误频道计数
        wrongCount++;
        Debug.Log("错误频道次数: " + wrongCount);

        // 达到阈值
        if (wrongCount >= maxWrongCount && !hasTriggered)
        {
            Core_SanityManager.Instance.DecreaseSanity(sanityCost);

            Debug.Log("TV 触发理智扣除");

            hasTriggered = true;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchChannel("1111");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchChannel("2222");

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchChannel("3333");

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SwitchChannel("1408");
    }
}
