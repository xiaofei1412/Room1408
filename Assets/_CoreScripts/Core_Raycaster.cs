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

        if (Physics.Raycast(ray, out hit, 20f, ~LayerMask.GetMask("Player")))
        {
            // 3种交互类型
            if (hit.collider.CompareTag("Inspectable") ||
                hit.collider.CompareTag("Readable") ||
                hit.collider.CompareTag("Operable"))
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

        if (Input.GetMouseButtonDown(0) && currentInteractableObj != null)
        {
            // 1. Inspectable = 3D检视（旋转物品）
            if (currentInteractableObj.CompareTag("Inspectable"))
            {
                inspectSystem.StartInspect(currentInteractableObj);
            }
            // 2. Readable = 2D阅读（打开笔记UI）
            else if (currentInteractableObj.CompareTag("Readable"))
            {
                Logic_NoteText note = currentInteractableObj.GetComponent<Logic_NoteText>();
                if (note != null && noteReader != null)
                {
                    noteReader.DisplayNote(note.noteTitle, note.noteContent);
                }
            }
            // 3. Operable = 机械交互（预留接口，给逻辑C使用）
            else if (currentInteractableObj.CompareTag("Operable"))
            {
                // 占位：后续C写门/抽屉/密码锁逻辑
                Debug.Log("触发Operable机械交互");
            }
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