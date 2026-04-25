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

        // 防遮挡遮罩：必须排斥 Player 和 Ignore Raycast
        int exclusionMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        if (Physics.Raycast(ray, out hit, 5f, exclusionMask))
        {
            GameObject hitObj = hit.collider.gameObject;
            Transform currentTransform = hitObj.transform;
            GameObject validInteractable = null;

            // 向上溯源逻辑：打中子网格，能顺藤摸瓜找到父类的标签
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

        if (Input.GetMouseButtonDown(0) && currentInteractableObj != null)
        {
            Logic_MainDoor doorLogic = currentInteractableObj.GetComponentInParent<Logic_MainDoor>();
            if (doorLogic != null) { doorLogic.OnInteract(); return; }

            if (currentInteractableObj.CompareTag("Pickable"))
            {
                // 复合交互检测：这是地上的录音机吗？
                Logic_CassettePlayerInteract playerInteract = currentInteractableObj.GetComponent<Logic_CassettePlayerInteract>();
                if (playerInteract != null)
                {
                    // 是录音机，转交复合逻辑 (无论手上有无磁带，内部会判定)
                    playerInteract.OnInteractWithTape();
                    return;
                }

                // 常规拾取逻辑
                Logic_ItemPickup pickup = currentInteractableObj.GetComponent<Logic_ItemPickup>();
                if (pickup != null && pickup.itemData != null)
                {
                    if (Core_InventoryManager.Instance.AddItem(pickup.itemData)) 
                        currentInteractableObj.SetActive(false);
                }
            }
            else if (currentInteractableObj.CompareTag("Inspectable"))
            {
                inspectSystem.StartInspect(currentInteractableObj);
            }
            else if (currentInteractableObj.CompareTag("Readable"))
            {
                Logic_NoteText note = currentInteractableObj.GetComponent<Logic_NoteText>();
                Logic_ProgressiveDecay decayScript = currentInteractableObj.GetComponent<Logic_ProgressiveDecay>();
                if (note != null && noteReader != null)
                    noteReader.DisplayNote(note.noteTitle, note.noteContent, decayScript, currentInteractableObj);

                Logic_SanityTrigger sanityTrigger = currentInteractableObj.GetComponent<Logic_SanityTrigger>();
                if (sanityTrigger != null) sanityTrigger.TriggerSanityLoss();
            }
            else if (currentInteractableObj.CompareTag("Operable"))
            {
                // 优先级 1：平移画框
                Logic_SlidingProp slidingProp = currentInteractableObj.GetComponentInParent<Logic_SlidingProp>();
                if (slidingProp != null) { slidingProp.StartDrag(); return; }

                // 优先级 2：旋转门
                Logic_RotatableDoor rotDoor = currentInteractableObj.GetComponentInParent<Logic_RotatableDoor>();
                if (rotDoor != null) { rotDoor.StartDrag(); return; }

                // 优先级 3：镜子特写
                Logic_MirrorInteract mirror = currentInteractableObj.GetComponentInParent<Logic_MirrorInteract>();
                if (mirror != null) { mirror.OnInteract(); return; }

                // 优先级 4：电视机交互
                Logic_TVInteract tv = currentInteractableObj.GetComponentInParent<Logic_TVInteract>();
                if (tv != null) { tv.OnInteract(); return; }

                // 优先级 5：座钟钟面交互
                Logic_ClockInteract clock = currentInteractableObj.GetComponentInParent<Logic_ClockInteract>();
                if (clock != null) { clock.OnInteract(); return; }

                // 优先级 6：保险箱密码盘
                Logic_SafeInteract safe = currentInteractableObj.GetComponentInParent<Logic_SafeInteract>();
                if (safe != null && safe.enabled) { safe.OnInteract(); return; }
            }
        }

        if (Input.GetMouseButtonDown(0) && currentInteractableObj == null) HandleItemDrop();
    }

    private void HandleItemDrop()
    {
        if (Core_InventoryManager.Instance.currentSelectedIndex == -1) return;

        ItemData itemToDrop = Core_InventoryManager.Instance.GetSelectedItem();
        if (itemToDrop != null && itemToDrop.itemPrefab != null)
        {
            Vector3 startPos = mainCamera.transform.position;
            Vector3 dropDirection = mainCamera.transform.forward;
            
            // 默认丢弃点：正前方 1.5 米
            Vector3 dropPosition = startPos + dropDirection * 1.5f;

            // 防穿模射线检测
            RaycastHit hit;
            // 探测前方 1.5 米内是否有阻挡物（忽略玩家自身）
            if (Physics.Raycast(startPos, dropDirection, out hit, 1.5f, ~LayerMask.GetMask("Player", "Ignore Raycast")))
            {
                // 如果撞到地板或墙壁，在击中点沿着表面法线向外退回 0.1 米生成，绝对防止嵌入
                dropPosition = hit.point + hit.normal * 0.1f; 
            }

            // 实例化模型
            GameObject droppedObj = Instantiate(itemToDrop.itemPrefab, dropPosition, Quaternion.identity);
            droppedObj.tag = "Pickable"; 

            // 灵魂注入
            if (droppedObj.GetComponent<Collider>() == null)
            {
                droppedObj.AddComponent<BoxCollider>();
            }

            Logic_ItemPickup pickupScript = droppedObj.GetComponent<Logic_ItemPickup>();
            if (pickupScript == null)
            {
                pickupScript = droppedObj.AddComponent<Logic_ItemPickup>();
            }
            pickupScript.itemData = itemToDrop;

            Core_InventoryManager.Instance.RemoveItemAtIndex(Core_InventoryManager.Instance.currentSelectedIndex);
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