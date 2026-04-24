using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// 交互射线系统：负责第一人称视角控制、射线检测可交互物体、准星显示
/// 适配新Tag：Inspectable / Readable / Operable
/// </summary>
public class Core_Raycaster : MonoBehaviour
{
    [Header("准星设置")]
    [SerializeField] private Color normalCrosshairColor = Color.white;
    [SerializeField] private Color interactCrosshairColor = Color.blue;
    [SerializeField] private Texture2D crosshairTexture;

    [Header("检测设置")]
    [SerializeField] private float rayDistance = 5f;

    [Header("视角控制")]
    [SerializeField] private float mouseSensitivity = 1.2f;

    public GameObject currentInteractableObj;

    private Camera mainCamera;
    private float xRotation = 0f;
    private Core_InspectSystem inspectSystem;
    private UI_NoteReader noteReader;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        inspectSystem = GetComponent<Core_InspectSystem>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        noteReader = FindObjectOfType<UI_NoteReader>();
    }

    private void OnGUI()
    {
        if (mainCamera != null)
        {
            DrawCrosshair();
        }
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            CastRay();
            CheckClick();
        }
    }

    /// <summary>
    /// 射线检测：适配Tag规范
    /// </summary>
    private void CastRay()
    {
        currentInteractableObj = null;
        Ray ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, ~LayerMask.GetMask("Player")))
        {
            // 扩大物理侦测面：追加 Pickable 标签
            if (hit.collider.CompareTag("Inspectable") ||
                hit.collider.CompareTag("Readable") ||
                hit.collider.CompareTag("Operable") ||
                hit.collider.CompareTag("Pickable")) 
            {
                currentInteractableObj = hit.collider.gameObject;
            }
        }
    }

    /// <summary>
    /// 点击逻辑：严格对应Tag功能
    /// </summary>
    private void CheckClick()
    {
        if (inspectSystem.isInspecting) return;

        // --- 逻辑 A：交互与拾取 ---
        if (Input.GetMouseButtonDown(0) && currentInteractableObj != null)
        {
            // 1. 纯粹的拾取物品，不走交互，直接拿走
            if (currentInteractableObj.CompareTag("Pickable"))
            {
                Logic_ItemPickup pickup = currentInteractableObj.GetComponent<Logic_ItemPickup>();
                if (pickup != null && pickup.itemData != null)
                {
                    if (Core_InventoryManager.Instance.AddItem(pickup.itemData))
                    {
                        currentInteractableObj.SetActive(false);
                    }
                }
            }
            // 2. 3D检视：看完再拿
            else if (currentInteractableObj.CompareTag("Inspectable"))
            {
                inspectSystem.StartInspect(currentInteractableObj);
            }
            // 3. 2D阅读：读完再拿
            else if (currentInteractableObj.CompareTag("Readable"))
            {
                Logic_NoteText note = currentInteractableObj.GetComponent<Logic_NoteText>();
                Logic_ProgressiveDecay decayScript = currentInteractableObj.GetComponent<Logic_ProgressiveDecay>();

                if (note != null && noteReader != null)
                {
                    // 必须将实体指针传入 UI
                    noteReader.DisplayNote(note.noteTitle, note.noteContent, decayScript, currentInteractableObj);
                }

                // 理智扣除维持独立运算
                Logic_SanityTrigger sanityTrigger = currentInteractableObj.GetComponent<Logic_SanityTrigger>();
                if (sanityTrigger != null) sanityTrigger.TriggerSanityLoss();
            }
            // 4. 机械交互（密码锁等）
            else if (currentInteractableObj.CompareTag("Operable"))
            {
                Debug.Log("触发Operable机械交互");
            }
        }

        // --- 逻辑 B：背包物品反向投放至世界---
        if (Input.GetMouseButtonDown(0) && currentInteractableObj == null)
        {
            HandleItemDrop();
        }
    }

    private void HandleItemDrop()
    {
        ItemData selectedItem = Core_InventoryManager.Instance.GetSelectedItem();
        if (selectedItem == null || selectedItem.itemPrefab == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            // 物理预测：实例化 3D 模型
            GameObject droppedObj = Instantiate(selectedItem.itemPrefab, hit.point, Quaternion.identity);
            
            // 防穿模修正：基于包围盒高度的一半进行 y 轴抬升
            Collider col = droppedObj.GetComponent<Collider>();
            if (col != null)
            {
                droppedObj.transform.position += hit.normal * col.bounds.extents.y;
            }

            // 从背包中物理移除该道具数据
            int currentIdx = Core_InventoryManager.Instance.currentSelectedIndex;
            Core_InventoryManager.Instance.inventory[currentIdx] = null;
            Core_InventoryManager.Instance.ToggleSelection(currentIdx); // 取消高亮
            Core_InventoryManager.Instance.UpdateInventoryUI(); // 通知 UI 刷新
        }
    }

    private void DrawCrosshair()
    {
        Vector2 crosshairPos = new Vector2(Screen.width / 2, Screen.height / 2);
        float crosshairSize = 10f;

        GUI.color = currentInteractableObj != null ? interactCrosshairColor : normalCrosshairColor;

        if (crosshairTexture != null)
        {
            GUI.DrawTexture(new Rect(crosshairPos.x - crosshairSize / 2, crosshairPos.y - crosshairSize / 2, crosshairSize, crosshairSize), crosshairTexture);
        }
        else
        {
            GUI.DrawTexture(new Rect(crosshairPos.x - 1, crosshairPos.y - crosshairSize / 2, 2, crosshairSize), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(crosshairPos.x - crosshairSize / 2, crosshairPos.y - 1, crosshairSize, 2), Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
    }

    public GameObject GetCurrentInteractable()
    {
        return currentInteractableObj;
    }
}