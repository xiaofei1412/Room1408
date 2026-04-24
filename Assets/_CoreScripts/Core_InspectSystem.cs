using UnityEngine;

public class Core_InspectSystem : MonoBehaviour
{
    [Header("检视设置")]
    public float rotateSpeed = 2f;
    public float inspectDistance = 1.0f; 
    public float inspectScaleFactor = 2.0f; // 放大倍率

    private GameObject targetObject;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 originalScale; // 记录原始缩放
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
        originalScale = obj.transform.localScale; // 记录缩放
        originalParent = obj.transform.parent;

        // 物理操作：移至镜头中心并放大
        obj.transform.SetParent(null);
        
        // 使用 ViewportPoint 确保绝对居中 (0.5, 0.5 是屏幕中心)
        Vector3 centerPoint = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, inspectDistance));
        obj.transform.position = centerPoint;
        obj.transform.localScale = originalScale * inspectScaleFactor;

        isInspecting = true;

        if (raycaster != null) raycaster.enabled = false;
        if (playerController != null) playerController.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!isInspecting || targetObject == null) return;

        // 旋转逻辑保持不变
        float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;
        targetObject.transform.Rotate(mainCamera.transform.up, -mouseX, Space.World);
        targetObject.transform.Rotate(mainCamera.transform.right, mouseY, Space.World);

        if (Input.GetMouseButtonDown(1)) StopInspect();
    }

    private void StopInspect()
    {
        // 还原所有物理属性
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