using UnityEngine;
using System;

/// <summary>
/// 全局理智管理器（Singleton）
/// </summary>
public class Core_SanityManager : MonoBehaviour
{
    public static Core_SanityManager Instance;

    [Header("理智值")]
    [SerializeField] private int maxSanity = 100;
    [SerializeField] private int currentSanity = 100;

    public event Action<int> OnSanityChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);

        Debug.Log("Sanity: " + currentSanity);

        OnSanityChanged?.Invoke(currentSanity);
    }

    public int GetSanity()
    {
        return currentSanity;
    }
}
