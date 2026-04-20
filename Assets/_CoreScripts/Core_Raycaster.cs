using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 交互射线系统：负责第一人称视角控制、射线检测可交互物体、准星显示
/// </summary>
public class Core_Raycaster : MonoBehaviour
{
    [Header("准星设置")]
    [SerializeField] private Color normalCrosshairColor = Color.white; // 未命中可交互物体时的准星颜色
    [SerializeField] private Color interactCrosshairColor = Color.blue; // 命中可交互物体时的准星颜色
    [SerializeField] private Texture2D crosshairTexture; // 准星纹理（可选，无则用默认十字）

    [Header("检测设置")]
    [SerializeField] private float rayDistance = 5f; // 射线检测距离

    [Header("视角控制")]
    [SerializeField] private float mouseSensitivity = 1.2f; // 鼠标灵敏度

    public GameObject currentInteractableObj; // 当前检测到的可交互物体（供外部调用）

    private Camera mainCamera; // 主摄像机引用
    private float xRotation = 0f; // 垂直旋转角度（用于限制抬头低头）
    private Core_InspectSystem inspectSystem; // 关联检视系统

    private void Awake()
    {
        // 获取组件引用
        mainCamera = GetComponent<Camera>();
        inspectSystem = GetComponent<Core_InspectSystem>();

        // 锁定鼠标到屏幕中心并隐藏（第一人称标准操作）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnGUI()
    {
        // 摄像机存在时才绘制准星
        if (mainCamera != null)
        {
            DrawCrosshair();
        }
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            // HandleCameraRotation(); // 处理视角旋转
            CastRay(); // 发射射线检测物体
            CheckClick(); // 检测鼠标点击
        }
    }

    /// <summary>
    /// 处理鼠标控制的第一人称视角旋转
    /// </summary>
    private void HandleCameraRotation()
    {
        // 获取鼠标移动量
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 垂直旋转（上下看），限制在-60到60度之间，防止过度抬头低头
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        // 水平旋转（左右看）
        transform.localRotation = Quaternion.Euler(xRotation, transform.localEulerAngles.y + mouseX, 0);
    }

    /// <summary>
    /// 从屏幕中心发射射线，检测带有Interactable或PuzzleUI标签的物体
    /// </summary>
    private void CastRay()
    {
        // 重置当前可交互物体
        currentInteractableObj = null;

        // 定义射线：从屏幕中心发射
        Ray ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        // 发射射线并检测碰撞
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // 判断碰撞物体标签
            if (hit.collider.CompareTag("Interactable") || hit.collider.CompareTag("PuzzleUI"))
            {
                currentInteractableObj = hit.collider.gameObject;
            }
        }
    }

    /// <summary>
    /// 检测鼠标左键点击，触发检视系统
    /// </summary>
    private void CheckClick()
    {
        if (inspectSystem.isInspecting) return;

        // 防 UI 穿透机制：
        // 如果当前场景存在 EventSystem，且鼠标指针正悬停在任何 UI 元素上，直接阻断 3D 交互。
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }

        if (Input.GetMouseButtonDown(0) && currentInteractableObj != null)
        {
            // 分支 1：检视 3D 物品
            if (currentInteractableObj.CompareTag("Interactable"))
            {
                inspectSystem.StartInspect(currentInteractableObj);
            }
            // 分支 2：触发 2D UI 机关
            else if (currentInteractableObj.CompareTag("PuzzleUI"))
            {
                Logic_KeypadLock keypad = currentInteractableObj.GetComponent<Logic_KeypadLock>();
                if (keypad != null)
                {
                    keypad.OpenKeypad();
                }
            }
        }
    }

    /// <summary>
    /// 绘制屏幕中心准星
    /// </summary>
    private void DrawCrosshair()
    {
        // 准星位置：屏幕中心
        Vector2 crosshairPos = new Vector2(Screen.width / 2, Screen.height / 2);
        float crosshairSize = 10f; // 准星大小

        // 根据是否检测到可交互物体切换准星颜色
        GUI.color = currentInteractableObj != null ? interactCrosshairColor : normalCrosshairColor;

        // 绘制准星：有纹理用纹理，无纹理用默认十字
        if (crosshairTexture != null)
        {
            GUI.DrawTexture(new Rect(crosshairPos.x - crosshairSize / 2, crosshairPos.y - crosshairSize / 2, crosshairSize, crosshairSize), crosshairTexture);
        }
        else
        {
            // 绘制垂直和水平线条组成十字准星
            GUI.DrawTexture(new Rect(crosshairPos.x - 1, crosshairPos.y - crosshairSize / 2, 2, crosshairSize), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(crosshairPos.x - crosshairSize / 2, crosshairPos.y - 1, crosshairSize, 2), Texture2D.whiteTexture);
        }

        // 重置GUI颜色，避免影响其他UI
        GUI.color = Color.white;
    }

    /// <summary>
    /// 外部调用接口：获取当前检测到的可交互物体
    /// </summary>
    public GameObject GetCurrentInteractable()
    {
        return currentInteractableObj;
    }
}