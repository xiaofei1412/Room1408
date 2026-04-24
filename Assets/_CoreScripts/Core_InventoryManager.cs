using UnityEngine;
using System;

public class Core_InventoryManager : MonoBehaviour
{
    public static Core_InventoryManager Instance;

    [Header("库存配置")]
    public int maxSlots = 9;
    public ItemData[] inventory;
    
    // -1 表示未选中任何格子，0~8 表示选中对应的格子
    public int currentSelectedIndex = -1; 

    // 委托事件：当背包数据或选中状态发生改变时，通知 UI 刷新
    public event Action OnInventoryUpdated;
    public event Action<int> OnSlotSelected;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        inventory = new ItemData[maxSlots];
    }

    private void Update()
    {
        HandleInput();
    }

    // 处理键盘 1-9 的物理按键输入
    private void HandleInput()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ToggleSelection(i);
                break;
            }
        }
    }

    // 选中/取消选中逻辑
    public void ToggleSelection(int index)
    {
        if (currentSelectedIndex == index)
        {
            currentSelectedIndex = -1; // 再次点击取消选中
        }
        else
        {
            currentSelectedIndex = index;
        }
        
        OnSlotSelected?.Invoke(currentSelectedIndex);
    }

    // 拾取道具写入空槽位
    public bool AddItem(ItemData data)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = data;
                OnInventoryUpdated?.Invoke(); // 触发 UI 渲染泵
                return true;
            }
        }
        return false;
    }
    
    // 强制 UI 刷新接口
    public void UpdateInventoryUI() => OnInventoryUpdated?.Invoke();

    // 获取当前选中的道具数据
    public ItemData GetSelectedItem()
    {
        if (currentSelectedIndex == -1) return null;
        return inventory[currentSelectedIndex];
    }
}