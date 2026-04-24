using UnityEngine;

public class Core_InspectSystem : MonoBehaviour
{
    [Header("检视设置")]
    public float rotateSpeed = 2f;
    [SerializeField] private float inspectDistance = 1.5f;

    private GameObject targetObject;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Transform originalParent;

    public bool isInspecting = false;

    private Camera mainCamera;
    private Core_Raycaster raycaster;
    // 新增：引用玩家控制器
    private Core_FirstPersonController playerController;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        raycaster = GetComponent<Core_Raycaster>();
        // 获取玩家控制器
        playerController = FindObjectOfType<Core_FirstPersonController>();
    }

    public void StartInspect(GameObject obj)
    {
        if (obj == null || isInspecting) return;

        targetObject = obj;
        originalPos = obj.transform.position;
        originalRot = obj.transform.rotation;
        originalParent = obj.transform.parent;

        obj.transform.SetParent(null);
        obj.transform.position = mainCamera.transform.position + mainCamera.transform.forward * inspectDistance;

        isInspecting = true;

        // 禁用射线 & 完全冻结玩家（不能走、不能转视角）
        if (raycaster != null)
            raycaster.enabled = false;
        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!isInspecting || targetObject == null) return;

        float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;

        targetObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
        targetObject.transform.Rotate(Vector3.right, mouseY, Space.World);

        // 右键退出
        if (Input.GetMouseButtonDown(1))
        {
            StopInspect();
        }
    }

    private void StopInspect()
    {
        // 1. 先将其还原到物理世界中的原始位置，以免发生变换矩阵错误
        targetObject.transform.position = originalPos;
        targetObject.transform.rotation = originalRot;
        targetObject.transform.SetParent(originalParent);

        // 延迟物理结算：退出检视时执行拾取
        Logic_ItemPickup pickup = targetObject.GetComponent<Logic_ItemPickup>();
        if (pickup != null && pickup.itemData != null)
        {
            if (Core_InventoryManager.Instance.AddItem(pickup.itemData))
            {
                targetObject.SetActive(false);
            }
        }

        // 2. 恢复射线与玩家控制
        isInspecting = false;
        if (raycaster != null)
        {
            raycaster.enabled = true;
            raycaster.currentInteractableObj = null;
        }
        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetObject = null;
    }
}