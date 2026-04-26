using UnityEngine;
using System;

/// <summary>
/// 全局理智管理器
/// </summary>
public class Core_SanityManager : MonoBehaviour
{
    public static Core_SanityManager Instance;

    [Header("理智值")]
    [SerializeField] private int maxSanity = 100;
    
    public int currentSanity = 100; 

    // 组员的 UI 更新事件委托
    public event Action<int> OnSanityChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentSanity = maxSanity;
        OnSanityChanged?.Invoke(currentSanity);
    }

    /// <summary>
    /// 扣除理智
    /// </summary>
    public void DecreaseSanity(int amount)
    {
        // 阻止在结局演出中重复扣除理智
        if (Logic_EndgameFall.Instance != null && Logic_EndgameFall.Instance.isEndingActive) return;

        currentSanity -= amount;
        if (currentSanity < 0) currentSanity = 0;
        
        // 事件触发 UI 刷新，解决 CS0103 报错
        OnSanityChanged?.Invoke(currentSanity);

        // 结局 C 判定：理智低于 40，强制坠入深渊
        if (currentSanity < 40)
        {
            if (Logic_EndgameFall.Instance != null)
            {
                Logic_EndgameFall.Instance.TriggerForcedFall();
            }
        }
    }

    public int GetSanity()
    {
        return currentSanity;
    }
}