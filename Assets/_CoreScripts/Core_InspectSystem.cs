using UnityEngine;

/// <summary>
/// 3D物品检视系统：负责将物体拉近、旋转观察、恢复原位
/// </summary>
public class Core_InspectSystem : MonoBehaviour
{
    [Header("检视设置")]
    public float rotateSpeed = 2f; // 物体旋转速度（API要求暴露）
    [SerializeField] private float inspectDistance = 1.5f; // 检视时物体距离相机的距离

    // 存储物体原始数据
    private GameObject targetObject; // 正在检视的物体
    private Vector3 originalPos; // 物体原始位置
    private Quaternion originalRot; // 物体原始旋转
    private Transform originalParent; // 物体原始父物体

    public bool isInspecting = false; // 是否正在检视（用于控制玩家状态）

    private Camera mainCamera; // 主摄像机引用
    private Core_Raycaster raycaster; // 关联射线系统

    private Core_FirstPersonController playerController; // 引入主物理控制器依赖

    private void Awake()
    {
        // 获取组件引用
        mainCamera = GetComponent<Camera>();
        raycaster = GetComponent<Core_Raycaster>();
        playerController = GetComponentInParent<Core_FirstPersonController>();
    }

    /// <summary>
    /// 外部调用接口（API要求）：开始检视物体
    /// </summary>
    public void StartInspect(GameObject obj)
    {
        // 物体为空或正在检视中，不执行
        if (obj == null || isInspecting) return;

        // 记录物体原始数据
        targetObject = obj;
        originalPos = obj.transform.position;
        originalRot = obj.transform.rotation;
        originalParent = obj.transform.parent;

        // 解除物体父物体，避免跟随移动
        obj.transform.SetParent(null);

        // 将物体移动到相机前方指定位置
        obj.transform.position = mainCamera.transform.position + mainCamera.transform.forward * inspectDistance;

        // 锁定玩家控制
        isInspecting = true;
        raycaster.enabled = false; // 禁用射线系统

        // 强制冻结物理位移与视角控制
        if(playerController != null) playerController.enabled = false; 
        
        // 解除鼠标锁定以允许UI交互（若后期需要），此处依原逻辑保持锁定用于拖拽计算
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // 只有正在检视时才执行
        if (!isInspecting || targetObject == null) return;

        // 获取鼠标移动量，控制物体旋转
        float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;

        // 旋转物体：水平绕Y轴，垂直绕X轴
        targetObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
        targetObject.transform.Rotate(Vector3.right, mouseY, Space.World);

        // 鼠标右键点击，退出检视
        if (Input.GetMouseButtonDown(1))
        {
            StopInspect();
        }
    }

    /// <summary>
    /// 停止检视，恢复物体和玩家控制
    /// </summary>
    private void StopInspect()
    {
        // 恢复物体原始状态
        targetObject.transform.position = originalPos;
        targetObject.transform.rotation = originalRot;
        targetObject.transform.SetParent(originalParent);

        // 恢复玩家控制
        isInspecting = false;
        raycaster.enabled = true; // 重新启用射线系统

        // 恢复物理位移与视角控制
        if(playerController != null) playerController.enabled = true;

        targetObject = null; // 清空当前检视物体
    }
}