using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryBar : MonoBehaviour
{
    [Header("物理槽位映射")]
    public Image[] slotIcons;       // 每个格子的 2D 图标 Image
    public GameObject[] highlights; // 每个格子的 高亮白框 GameObject

    private void Start()
    {
        if (Core_InventoryManager.Instance != null)
        {
            Core_InventoryManager.Instance.OnInventoryUpdated += RefreshUI;
            Core_InventoryManager.Instance.OnSlotSelected += UpdateHighlight;
            
            RefreshUI();
            UpdateHighlight(-1);
        }
    }

    private void OnDestroy()
    {
        if (Core_InventoryManager.Instance != null)
        {
            Core_InventoryManager.Instance.OnInventoryUpdated -= RefreshUI;
            Core_InventoryManager.Instance.OnSlotSelected -= UpdateHighlight;
        }
    }

    private void RefreshUI()
    {
        ItemData[] inventory = Core_InventoryManager.Instance.inventory;

        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (inventory[i] != null)
            {
                slotIcons[i].sprite = inventory[i].itemIcon;
                slotIcons[i].color = Color.white; // 显现图标
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = new Color(1, 1, 1, 0); // 没有道具时完全透明隐藏
            }
        }
    }

    private void UpdateHighlight(int selectedIndex)
    {
        for (int i = 0; i < highlights.Length; i++)
        {
            // 只有当循环序号等于当前选中的序号时，才激活高亮框
            highlights[i].SetActive(i == selectedIndex);
        }
    }
}