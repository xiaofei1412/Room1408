using UnityEngine;
using System;

/// <summary>
/// 全局状态机（单例）
/// 管理关卡 / 房间状态（Normal / Decay / Horror / Level_2 等）
/// </summary>
public class Logic_StateManager : MonoBehaviour
{
    /// <summary>
    /// 状态枚举（可自由扩展）
    /// </summary>
    public enum GameState
    {
        Normal,
        Decay,
        Horror,
        Level_2
    }

    /// <summary>
    /// 单例实例
    /// </summary>
    public static Logic_StateManager Instance;

    /// <summary>
    /// 当前状态（Inspector 可查看）
    /// </summary>
    [SerializeField] private GameState currentState = GameState.Normal;

    /// <summary>
    /// 状态改变事件（供外部监听）
    /// </summary>
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        // 移除了 DontDestroyOnLoad，让它随着场景重载正常销毁，切断污染源！
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 获取当前状态
    /// </summary>
    public GameState GetState()
    {
        return currentState;
    }

    /// <summary>
    /// 外部调用：切换状态
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (newState == currentState) return;

        currentState = newState;

        Debug.Log("状态切换为: " + newState);

        // 广播事件
        OnStateChanged?.Invoke(currentState);
    }
}
