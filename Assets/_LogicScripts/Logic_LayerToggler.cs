using UnityEngine;

/// <summary>
/// 根据状态机切换不同物体组（Normal / Horror）
/// </summary>
public class Logic_LayerToggler : MonoBehaviour
{
    [Header("普通状态物体")]
    [SerializeField] private GameObject[] normalObjects;

    [Header("恐怖状态物体")]
    [SerializeField] private GameObject[] horrorObjects;

    private void Start()
    {
        // 监听状态变化
        Logic_StateManager.Instance.OnStateChanged += OnStateChanged;

        // 初始化一次
        OnStateChanged(Logic_StateManager.Instance.GetState());
    }

    private void OnDestroy()
    {
        if (Logic_StateManager.Instance != null)
            Logic_StateManager.Instance.OnStateChanged -= OnStateChanged;
    }

    /// <summary>
    /// 状态变化回调
    /// </summary>
    private void OnStateChanged(Logic_StateManager.GameState state)
    {
        bool isHorror = (state == Logic_StateManager.GameState.Horror);

        // 普通层
        foreach (var obj in normalObjects)
        {
            if (obj != null)
                obj.SetActive(!isHorror);
        }

        // 恐怖层
        foreach (var obj in horrorObjects)
        {
            if (obj != null)
                obj.SetActive(isHorror);
        }
    }
}