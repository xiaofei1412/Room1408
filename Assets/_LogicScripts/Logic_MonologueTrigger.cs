using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Logic_MonologueTrigger : MonoBehaviour
{
    [Header("独白内容")]
    [TextArea(2, 5)]
    public string monologueLine;

    [Header("触发限制")]
    public bool triggerOnce = true;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;

        // 碰撞过滤：仅允许玩家触发
        if (other.CompareTag("Player"))
        {
            if (Core_MonologueManager.Instance != null)
            {
                Core_MonologueManager.Instance.ShowMonologue(monologueLine);
                hasTriggered = true;
            }
        }
    }
}