using UnityEngine;

public class Logic_ItemPickup : MonoBehaviour
{
    [Header("对应背包的数据资源")]
    public ItemData itemData;

    // 记录初始物理信息，用于“重新开始”时的位置复原
    [HideInInspector] public Vector3 spawnPos;
    [HideInInspector] public Quaternion spawnRot;

    private void Awake()
    {
        spawnPos = transform.position;
        spawnRot = transform.rotation;
    }

    // 物理回归：游戏重新开始时由外部管理器调用
    public void ResetToWorld()
    {
        transform.position = spawnPos;
        transform.rotation = spawnRot;
        gameObject.SetActive(true);
    }
}