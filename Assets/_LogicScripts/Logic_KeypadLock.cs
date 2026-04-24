using UnityEngine;
using UnityEngine.Events;
using TMPro;   // ✅ 使用 TMP

/// <summary>
/// 通用密码锁系统（UI 版本）
/// </summary>
public class Logic_KeypadLock : MonoBehaviour
{
    [Header("正确密码")]
    public string correctPassword = "1408";

    [Header("UI 组件")]
    [SerializeField] private GameObject keypadCanvas;
    [SerializeField] private TMP_InputField inputField;   // ✅ 改为 TMP

    [Header("音效")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip errorSound;

    [Header("解锁事件（Inspector拖拽）")]
    public UnityEvent OnUnlocked;

    [Header("解锁后切换的状态")]
    [SerializeField] private Logic_StateManager.GameState nextState = Logic_StateManager.GameState.Level_2;

    [Header("玩家控制（可选）")]
    [SerializeField] private MonoBehaviour playerController;

    private bool isUnlocked = false;

    /// <summary>
    /// 打开密码UI
    /// </summary>
    public void OpenKeypad()
    {
        // ❗防止没绑定
        if (keypadCanvas == null || inputField == null)
        {
            Debug.LogError("Keypad UI 未绑定！");
            return;
        }

        keypadCanvas.SetActive(true);

        // 🔒 锁定玩家移动
        if (playerController != null)
            playerController.enabled = false;

        // 🖱 解锁鼠标（FPS 必备）
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 清空输入
        inputField.text = "";
        inputField.ActivateInputField(); // 自动聚焦输入框
    }

    /// <summary>
    /// 点击提交按钮
    /// </summary>
    public void OnSubmit()
    {
        string input = inputField.text;

        // ❌ 密码错误
        if (input != correctPassword)
        {
            Debug.Log("密码错误");

            if (audioSource != null && errorSound != null)
                audioSource.PlayOneShot(errorSound);

            // 👉 清空输入（体验更好）
            inputField.text = "";
            inputField.ActivateInputField();

            Core_SanityManager.Instance.DecreaseSanity(5); // ⭐新增

            return;
        }

        // ✅ 防止重复触发
        if (isUnlocked) return;
        isUnlocked = true;

        Debug.Log("密码正确，解锁成功");

        // 👉 触发 Inspector 事件（Cube_C / Cube_D）
        OnUnlocked?.Invoke();

        // 👉 切换状态机
        if (Logic_StateManager.Instance != null)
            Logic_StateManager.Instance.ChangeState(nextState);
        else
            Debug.LogWarning("StateManager 不存在！");

        // 👉 关闭UI
        CloseKeypad();
    }

    /// <summary>
    /// 关闭UI
    /// </summary>
    public void CloseKeypad()
    {
        keypadCanvas.SetActive(false);

        // 🎮 恢复玩家控制
        if (playerController != null)
            playerController.enabled = true;

        // 🔒 锁回鼠标（FPS）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}