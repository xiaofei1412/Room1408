using UnityEngine;

/// <summary>
/// 通用理智扣除触发器（可限制触发次数）
/// </summary>
public class Logic_SanityTrigger : MonoBehaviour
{
    [SerializeField] private int sanityCost = 10;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    public void TriggerSanityLoss()
    {
        if (triggerOnce && hasTriggered) return;

        Core_SanityManager.Instance.DecreaseSanity(sanityCost);

        hasTriggered = true;
    }
}