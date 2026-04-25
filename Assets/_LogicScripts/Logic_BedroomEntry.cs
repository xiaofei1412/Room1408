using UnityEngine;

public class Logic_BedroomEntry : MonoBehaviour
{
    [Header("触发条件")]
    public string targetKeyName = ""; // 暴露给面板，填入钥匙的真实名称

    private bool hasEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 确保进门的是玩家
        if (!hasEntered && other.CompareTag("Player"))
        {
            hasEntered = true;

            // 检查背包中是否有指定名字的钥匙
            if (!string.IsNullOrEmpty(targetKeyName) && Core_InventoryManager.Instance.HasItem(targetKeyName))
            {
                Core_MonologueManager.Instance.ShowMonologue("Those drawers... Maybe the key I found fits.");
            }
            
            Destroy(gameObject);
        }
    }
}