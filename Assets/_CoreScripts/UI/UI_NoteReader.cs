using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_NoteReader : MonoBehaviour
{
    private GameObject currentActiveNoteObject;

    [Header("UI 绑定")]
    public TextMeshProUGUI tmpTitle;
    public TextMeshProUGUI tmpContent;
    public RawImage uiBloodOverlay;

    [Header("UI 渲染组件")]
    // 统一退回 RawImage，并使用 Texture，消除类型冲突
    public RawImage uiPaperBackground; 
    public Texture defaultCleanPaper;

    [Header("打字机配置")]
    public float typingSpeed = 0.05f;

    private Core_FirstPersonController playerController;
    private Core_Raycaster raycaster;
    private Logic_ProgressiveDecay activeDecayScript;
    private Coroutine typingCoroutine;
    private string fullContent;

    void Awake()
    {
        // 使用最新的寻址接口，消除 CS0618 警告
        playerController = Object.FindFirstObjectByType<Core_FirstPersonController>();
        raycaster = Object.FindFirstObjectByType<Core_Raycaster>();
    }

    public void DisplayNote(string title, string content, Logic_ProgressiveDecay decayScript = null, GameObject noteObj = null)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        tmpTitle.text = title;
        tmpContent.text = "";
        fullContent = content;
        activeDecayScript = decayScript;
        currentActiveNoteObject = noteObj;

        // 严密的材质分流逻辑
        if (activeDecayScript != null && activeDecayScript.basePaperTexture != null)
        {
            // 腐化纸条模式
            uiPaperBackground.texture = activeDecayScript.basePaperTexture;
            uiBloodOverlay.texture = activeDecayScript.decayTexture;
            SetOverlayAlpha(activeDecayScript.CurrentDecay);
        }
        else
        {
            // 正常纸条模式 (Fallback)
            uiPaperBackground.texture = defaultCleanPaper;
            if (uiBloodOverlay != null)
            {
                uiBloodOverlay.texture = null;
                SetOverlayAlpha(0f); // 确保干净纸条上没有隐形血迹
            }
        }

        uiPaperBackground.gameObject.SetActive(true);

        if (playerController != null) playerController.enabled = false;
        if (raycaster != null) raycaster.enabled = false;

        typingCoroutine = StartCoroutine(TypeText());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator TypeText()
    {
        foreach (char letter in fullContent.ToCharArray())
        {
            tmpContent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (uiBloodOverlay == null) return;
        Color c = uiBloodOverlay.color;
        c.a = alpha; 
        uiBloodOverlay.color = c;
    }

    public void CloseNote()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
    
        uiPaperBackground.gameObject.SetActive(false);
        activeDecayScript = null;

        // 直接使用全局 raycaster，消除 CS0136 报错
        if (raycaster != null)
        {
            if (raycaster.focusTarget != null && raycaster.focusTarget.name == "Note_MainDoor")
            {
                // 1. 解除焦点
                raycaster.focusTarget = null;
                
                // 2. 触发独白
                Core_MonologueManager.Instance.ShowMonologue("'Chamber of white tiles'... 'Silver eyes'...");
                Core_MonologueManager.Instance.ShowMonologue("The bathroom mirror? I should go check the bathroom.");

                // 3. 解锁卫生间交互
                GameObject bathroomDoor = GameObject.Find("Prop_Door_Bathroom");
                if (bathroomDoor != null) bathroomDoor.tag = "Operable";
            }
            
            raycaster.enabled = true;
            raycaster.currentInteractableObj = null;
        }

        // 延迟物理结算：判断是否存入背包
        if (currentActiveNoteObject != null)
        {
            // 强制向上溯源：无论击中哪个子物体，都能精准定位到父节点的 Pickup 脚本
            Logic_ItemPickup pickup = currentActiveNoteObject.GetComponentInParent<Logic_ItemPickup>();
            
            if (pickup != null)
            {
                if (pickup.itemData != null)
                {
                    if (Core_InventoryManager.Instance.AddItem(pickup.itemData))
                    {
                        // 销毁带有 Pickup 脚本的根节点，完成物权转移
                        pickup.gameObject.SetActive(false); 
                    }
                    else
                    {
                        Debug.LogWarning("【系统警告】背包已满，无法拾取！");
                    }
                }
                else
                {
                    Debug.LogError("【物理断层】Logic_ItemPickup 存在，但未装填 ItemData 数据！");
                }
            }
            currentActiveNoteObject = null;
        }

        if (playerController != null) playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (uiPaperBackground.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseNote();
                return;
            }

            if (activeDecayScript != null)
            {
                activeDecayScript.AddReadingTime(Time.deltaTime);
                SetOverlayAlpha(activeDecayScript.CurrentDecay);
            }
        }
    }
}