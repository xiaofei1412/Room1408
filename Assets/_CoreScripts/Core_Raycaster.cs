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

    public GameObject focusTarget; // 如果不为 null，则只能与该物体交互

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

        // 定义严谨的物理遮罩。强行排除 Player 层 AND Ignore Raycast 层
        int exclusionMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        // 将 exclusionMask 传入射线函数
        if (Physics.Raycast(ray, out hit, 5f, exclusionMask))
        {
            GameObject hitObj = hit.collider.gameObject;

            // 保持遥测探针，用于核验
            Debug.Log($"【射线遥测】击中目标: {hitObj.name} | 图层: {LayerMask.LayerToName(hitObj.layer)} | 标签: {hitObj.tag}");

            Transform currentTransform = hitObj.transform;
            GameObject validInteractable = null;

            while (currentTransform != null)
            {
                if (currentTransform.CompareTag("Inspectable") || 
                    currentTransform.CompareTag("Readable") ||
                    currentTransform.CompareTag("Operable") || 
                    currentTransform.CompareTag("Pickable"))
                {
                    validInteractable = currentTransform.gameObject;
                    break; 
                }
                currentTransform = currentTransform.parent;
            }

            if (validInteractable != null)
            {
                if (focusTarget != null && validInteractable != focusTarget && !validInteractable.transform.IsChildOf(focusTarget.transform))
                {
                    return; 
                }
                currentInteractableObj = validInteractable;
            }
        }
    }

    /// <summary>
    /// 点击逻辑：严格对应Tag功能
    /// </summary>
    private void CheckClick()
    {
        if (inspectSystem != null && inspectSystem.isInspecting) return;

        // --- 逻辑 A：交互与拾取 ---
        if (Input.GetMouseButtonDown(0) && currentInteractableObj != null)
        {
            Logic_MainDoor doorLogic = currentInteractableObj.GetComponentInParent<Logic_MainDoor>();
            if (doorLogic != null)
            {
                doorLogic.OnInteract();
                return;
            }
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
                // 优先级 1：旋转门 (最具体的物理动作，优先判定)
                Logic_RotatableDoor rotDoor = currentInteractableObj.GetComponentInParent<Logic_RotatableDoor>();
                if (rotDoor != null)
                {
                    rotDoor.StartDrag();
                    return; 
                }

                // 优先级 1.5：平移物体判定 (紧跟在旋转门之后)
                Logic_SlidingProp slidingProp = currentInteractableObj.GetComponentInParent<Logic_SlidingProp>();
                if (slidingProp != null)
                {
                    slidingProp.StartDrag();
                    return; 
                }

                // 优先级 2：镜子特写
                Logic_MirrorInteract mirror = currentInteractableObj.GetComponentInParent<Logic_MirrorInteract>();
                if (mirror != null) { mirror.OnInteract(); return; }

                // 优先级 3：保险箱密码盘 (只有在 Logic_SafeInteract 脚本被勾选激活时才会响应)
                Logic_SafeInteract safe = currentInteractableObj.GetComponentInParent<Logic_SafeInteract>();
                if (safe != null && safe.enabled) 
                { 
                    safe.OnInteract(); 
                    return; 
                }

                // 电视机交互判定
                Logic_TVInteract tv = currentInteractableObj.GetComponentInParent<Logic_TVInteract>();
                if (tv != null)
                {
                    tv.OnInteract();
                    return;
                }
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