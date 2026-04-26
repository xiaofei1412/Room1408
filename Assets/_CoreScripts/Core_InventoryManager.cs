using UnityEngine;
using System;

public class Core_InventoryManager : MonoBehaviour
{
    public static Core_InventoryManager Instance;

    [Header("库存配置")]
    public int maxSlots = 10; // 容量扩展至 10
    public ItemData[] inventory;
    
    public int currentSelectedIndex = -1; 

    public event Action OnInventoryUpdated;
    public event Action<int> OnSlotSelected;

    private void Awake()
    {
        Instance = this;
        inventory = new ItemData[maxSlots]; // 保留初始化数组的逻辑
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            // 第10个格子（索引9）对应数字键 0
            KeyCode targetKey = (i == 9) ? KeyCode.Alpha0 : (KeyCode.Alpha1 + i);
            
            if (Input.GetKeyDown(targetKey))
            {
                ToggleSelection(i);
                break;
            }
        }
    }

    public void ToggleSelection(int index)
    {
        if (currentSelectedIndex == index)
        {
            currentSelectedIndex = -1; 
        }
        else
        {
            currentSelectedIndex = index;
        }
        
        OnSlotSelected?.Invoke(currentSelectedIndex);
    }

    // 拾取道具：从左往右找第一个空位
    public bool AddItem(ItemData data)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = data;
                OnInventoryUpdated?.Invoke(); 
                return true;
            }
        }
        return false;
    }

    // 检查是否包含物品
    public bool HasItem(string targetID)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] != null && inventory[i].itemName == targetID) return true;
        }
        return false;
    }

    // 根据 ID 移除物品
    public void RemoveItem(string targetID)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] != null && inventory[i].itemName == targetID) 
            {
                RemoveItemAtIndex(i);
                return;
            }
        }
    }

    // 根据格子索引精确移除物品（用于丢弃逻辑）
    public void RemoveItemAtIndex(int index)
    {
        if (index >= 0 && index < inventory.Length && inventory[index] != null)
        {
            inventory[index] = null;
            if (currentSelectedIndex == index) ToggleSelection(index); 
            OnInventoryUpdated?.Invoke(); 
        }
    }

    public void UpdateInventoryUI() => OnInventoryUpdated?.Invoke();

    public ItemData GetSelectedItem()
    {
        if (currentSelectedIndex == -1) return null;
        return inventory[currentSelectedIndex];
    }
}