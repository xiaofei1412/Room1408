using UnityEngine;

public class Logic_DrawerReveal : MonoBehaviour
{
    [System.Serializable]
    public struct HiddenItemInfo
    {
        public GameObject item;
        public string targetTag; 
    }

    [Header("要暴露的内部道具列表")]
    public HiddenItemInfo[] hiddenItems; 
    public string discoverMonologue = "A blood-stained diary...";

    public void RevealContent()
    {
        if (hiddenItems != null && hiddenItems.Length > 0)
        {
            foreach (var info in hiddenItems)
            {
                // 防呆锁：绝不允许改变挂载此脚本的抽屉本身的标签
                if (info.item != null && info.item != this.gameObject)
                {
                    info.item.tag = info.targetTag;
                }
            }
        }
        
        if (!string.IsNullOrEmpty(discoverMonologue))
        {
            Core_MonologueManager.Instance.ShowMonologue(discoverMonologue);
        }
    }
}