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
        // 强制清洗 1：移除 TMP 特有的隐藏零宽字符 (\u200B)
        // 强制清洗 2：Trim() 移除首尾的所有常规空格与换行符
        string input = inputField.text.Replace("\u200B", "").Trim();
        string target = correctPassword.Trim();

        // ❌ 密码错误
        if (input != target)
        {
            // 输出带边界符的日志，用于物理排查是否仍有异物（如打印出 "[1408]"）
            Debug.Log($"密码错误. 玩家输入:[{input}], 目标密码:[{target}]");

            // 接入全局音效管理器播放 2D UI 音效
            if (errorSound != null && Audio_SoundManager.Instance != null)
            {
                Audio_SoundManager.Instance.PlaySFX2D(errorSound);
            }

            // 👉 清空输入
            inputField.text = "";
            inputField.ActivateInputField();

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