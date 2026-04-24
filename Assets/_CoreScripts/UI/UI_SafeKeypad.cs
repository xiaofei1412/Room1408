using UnityEngine;
using TMPro;
using System.Collections;

public class UI_SafeKeypad : MonoBehaviour
{
    public static UI_SafeKeypad Instance;

    [Header("物理 UI 绑定")]
    public GameObject keypadCanvasPanel; // 特写界面的根节点
    public TextMeshProUGUI displayScreen; // 密码显示屏

    [Header("锁扣逻辑参数")]
    public string correctPassword = "1408";
    public int maxDigits = 4;
    public int sanityPenalty = 5;

    [Header("音效阵列")]
    public AudioClip clickSound;
    public AudioClip errorSound;
    public AudioClip successSound;
    private AudioSource audioSource;

    private string currentInput = "";
    private bool isInteracting = false;
    private bool isUnlocked = false;

    private Core_FirstPersonController playerController;
    private Core_Raycaster raycaster;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        playerController = Object.FindFirstObjectByType<Core_FirstPersonController>();
        raycaster = Object.FindFirstObjectByType<Core_Raycaster>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OpenKeypad()
    {
        if (isUnlocked) return; // 物理锁死：已解锁则不再打开

        isInteracting = true;
        currentInput = "";
        displayScreen.text = "";
        keypadCanvasPanel.SetActive(true);

        if (playerController != null) playerController.enabled = false;
        if (raycaster != null) raycaster.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 按钮事件接口：由 UI Button 的 OnClick() 调用
    public void OnNumberPressed(string number)
    {
        if (currentInput.Length < maxDigits)
        {
            currentInput += number;
            displayScreen.text = currentInput;
            if (clickSound != null) audioSource.PlayOneShot(clickSound);
        }
    }

    public void OnClearPressed()
    {
        currentInput = "";
        displayScreen.text = "";
        if (clickSound != null) audioSource.PlayOneShot(clickSound);
    }

    public void OnSubmitPressed()
    {
        if (currentInput == correctPassword)
        {
            isUnlocked = true;
            if (successSound != null) audioSource.PlayOneShot(successSound);
            
            Core_MonologueManager.Instance.ShowMonologue("It clicked. Seems like it worked.");

            GameObject safeMain = GameObject.Find("SM_Safe");
            GameObject safeDoor = GameObject.Find("Hinge_Safe_Door");

            // 物理操作 1：无差别剥夺主体权限，并使其对射线“物理透明”
            if (safeMain != null)
            {
                Logic_SafeInteract interactScript = safeMain.GetComponent<Logic_SafeInteract>();
                if (interactScript != null) interactScript.enabled = false;
                
                Transform[] allChildren = safeMain.GetComponentsInChildren<Transform>();
                foreach (Transform t in allChildren)
                {
                    // 保护门体
                    if (safeDoor != null && (t == safeDoor.transform || t.IsChildOf(safeDoor.transform))) 
                        continue; 
                    
                    // 🚨 保护核心道具：绝对不允许将录音机及其子网格设为透明
                    if (t.name.Contains("CassettePlayer")) 
                        continue; 

                    if (t.CompareTag("Operable")) t.tag = "Untagged";
                    t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); 
                }
            }

            // 物理操作 2：精准赋予门体交互权限
            if (safeDoor != null)
            {
                Collider[] doorColliders = safeDoor.GetComponentsInChildren<Collider>();
                foreach (Collider col in doorColliders)
                {
                    col.gameObject.tag = "Operable"; 
                }
            }

            if (Logic_StateManager.Instance != null)
                Logic_StateManager.Instance.ChangeState(Logic_StateManager.GameState.Level_2);

            StartCoroutine(CloseAfterDelay(1.0f));
        }
        else
        {
            if (errorSound != null) audioSource.PlayOneShot(errorSound);
            if (Core_SanityManager.Instance != null)
                Core_SanityManager.Instance.DecreaseSanity(sanityPenalty);
            displayScreen.text = "ERR";
            currentInput = "";
            StartCoroutine(ResetScreenAfterError());
        }
    }

    private IEnumerator ResetScreenAfterError()
    {
        yield return new WaitForSeconds(0.5f);
        displayScreen.text = "";
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseKeypad();
    }

    public void CloseKeypad()
    {
        isInteracting = false;
        keypadCanvasPanel.SetActive(false);

        if (playerController != null) playerController.enabled = true;
        if (raycaster != null)
        {
            raycaster.enabled = true;
            raycaster.currentInteractableObj = null; // 清除当前目标防止连击
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 允许通过物理键盘 ESC 强制退出
        if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseKeypad();
        }
    }
}