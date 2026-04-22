using UnityEngine;
using TMPro;

public class UI_NoteReader : MonoBehaviour
{
    [Header("UI 绑定")]
    public TextMeshProUGUI tmpTitle;
    public TextMeshProUGUI tmpContent;
    public GameObject notePanel;

    private Core_FirstPersonController playerController;
    private Core_Raycaster raycaster ;
    

    void Awake()
    {
        // 提前获取玩家控制器，不找射线
        playerController = FindObjectOfType<Core_FirstPersonController>();
        raycaster = FindObjectOfType<Core_Raycaster>();
    }

    // 显示笔记
    public void DisplayNote(string title, string content)
    {
        tmpTitle.text = title;
        tmpContent.text = content;
        notePanel.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;

        // 禁用射线
        if (raycaster != null)
            raycaster.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 关闭笔记
    public void CloseNote()
    {
        notePanel.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        // 重新启用射线 + 清空状态
        if (raycaster != null)
        {
            raycaster.enabled = true;
            raycaster.currentInteractableObj = null;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ESC关闭
        if (Input.GetKeyDown(KeyCode.Escape) && notePanel.activeSelf)
        {
            CloseNote();
        }
    }
}