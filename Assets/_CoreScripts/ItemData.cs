using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("道具基础信息")]
    public string itemName;
    public Sprite itemIcon; // 背包中显示的 2D 图标

    [Header("物理映射")]
    public GameObject itemPrefab; // 丢弃到场景中时生成的 3D 预制体
}