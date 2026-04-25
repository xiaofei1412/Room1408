using UnityEngine;

public class Core_InspectSystem : MonoBehaviour
{
    [Header("检视设置")]
    public float rotateSpeed = 2f;
    public float inspectDistance = 1.0f; 
    
    // 替换了原来的倍率。0.3f 表示无论物品原本多大，检视时其最长边都会变成 0.3 米
    public float desiredPhysicalSize = 0.3f; 

    private GameObject targetObject;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 originalScale; 
    private Transform originalParent;

    public bool isInspecting = false;
    private Camera mainCamera;
    private Core_Raycaster raycaster;
    private Core_FirstPersonController playerController;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        raycaster = GetComponent<Core_Raycaster>();
        playerController = Object.FindFirstObjectByType<Core_FirstPersonController>();
    }

    public void StartInspect(GameObject obj)
    {
        if (obj == null || isInspecting) return;

        targetObject = obj;
        originalPos = obj.transform.position;
        originalRot = obj.transform.rotation;
        originalScale = obj.transform.localScale; 
        originalParent = obj.transform.parent;

        obj.transform.SetParent(null);
        
        // 算出屏幕正中央的世界坐标
        Vector3 centerPoint = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, inspectDistance));
        
        // 调用核心算法：自适应缩放与几何居中
        NormalizeObjectScaleAndCenter(obj, centerPoint, desiredPhysicalSize);

        isInspecting = true;

        if (raycaster != null) raycaster.enabled = false;
        if (playerController != null) playerController.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 核心算法：包围盒归一化与几何偏移修正
    /// </summary>
    private void NormalizeObjectScaleAndCenter(GameObject obj, Vector3 targetCenter, float desiredSize)
    {
        // 1. 先重置缩放为 1，以测量纯网格的原始大小
        obj.transform.localScale = Vector3.one;

        // 2. 获取物体及所有子物体的渲染器
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            obj.transform.position = targetCenter;
            return;
        }

        // 3. 计算合并的世界坐标包围盒
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        // 4. 获取最长的一条边
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        // 5. 等比缩放 (比如磁带只有0.05，除以之后 scaleFactor 就会变成 6，自动放大6倍)
        if (maxDimension > 0.001f)
        {
            float scaleFactor = desiredSize / maxDimension;
            obj.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }

        // 6. 锚点偏移修正：缩放后重新获取 Bounds，计算出几何中心与物体现有坐标的偏差，强行对齐到屏幕中心
        Bounds newBounds = renderers[0].bounds;
        foreach (Renderer r in renderers) newBounds.Encapsulate(r.bounds);
        
        Vector3 pivotOffset = obj.transform.position - newBounds.center;
        obj.transform.position = targetCenter + pivotOffset;
    }

    private void Update()
    {
        if (!isInspecting || targetObject == null) return;

        float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;
        targetObject.transform.Rotate(mainCamera.transform.up, -mouseX, Space.World);
        targetObject.transform.Rotate(mainCamera.transform.right, mouseY, Space.World);

        if (Input.GetMouseButtonDown(1)) StopInspect();
    }

    private void StopInspect()
    {
        // 退出时，把刚才自动拉伸的缩放值还原回它原本在抽屉里的大小
        targetObject.transform.localScale = originalScale;
        targetObject.transform.position = originalPos;
        targetObject.transform.rotation = originalRot;
        targetObject.transform.SetParent(originalParent);

        Logic_ItemPickup pickup = targetObject.GetComponentInParent<Logic_ItemPickup>();
        if (pickup != null && pickup.itemData != null)
        {
            if (Core_InventoryManager.Instance.AddItem(pickup.itemData))
            {
                targetObject.SetActive(false);
            }
        }

        isInspecting = false;
        if (raycaster != null) { raycaster.enabled = true; raycaster.currentInteractableObj = null; }
        if (playerController != null) playerController.enabled = true;
        targetObject = null;
    }
}