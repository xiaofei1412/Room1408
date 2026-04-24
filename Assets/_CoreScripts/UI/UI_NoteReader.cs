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
    public RawImage uiPaperBackground; // 底层纸张
    public RawImage uiBloodOverlay;    // 顶层血渍/污渍

    [Header("打字机配置")]
    public float typingSpeed = 0.05f;

    private Core_FirstPersonController playerController;
    private Core_Raycaster raycaster;
    private Logic_ProgressiveDecay activeDecayScript;
    private Coroutine typingCoroutine;
    private string fullContent;

    void Awake()
    {
        playerController = FindObjectOfType<Core_FirstPersonController>();
        raycaster = FindObjectOfType<Core_Raycaster>();
    }

    public void DisplayNote(string title, string content, Logic_ProgressiveDecay decayScript = null, GameObject noteObj = null)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        tmpTitle.text = title;
        tmpContent.text = "";
        fullContent = content;
        activeDecayScript = decayScript;
        currentActiveNoteObject = noteObj; // 缓存物理实体
        
        // 核心逻辑：变量化贴图推送
        if (activeDecayScript != null)
        {
            // 推送底图
            uiPaperBackground.texture = activeDecayScript.basePaperTexture;
            // 推送血渍图
            uiBloodOverlay.texture = activeDecayScript.decayTexture;
            
            // 实时同步初始透明度
            SetOverlayAlpha(activeDecayScript.CurrentDecay);
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

    // 物理控制透明度的方法
    private void SetOverlayAlpha(float alpha)
    {
        if (uiBloodOverlay == null)
        {
            // 如果槽位是空的，立刻报警！
            Debug.LogError("【致命物理断层】UI_BloodOverlay 未在 Inspector 中绑定！");
            return;
        }

        Color c = uiBloodOverlay.color;
        c.a = alpha; 
        uiBloodOverlay.color = c;
    }

    public void CloseNote()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        uiPaperBackground.gameObject.SetActive(false);
        activeDecayScript = null;

        // 延迟物理结算：关闭阅读器时，判定该物体是否允许被拾取
        if (currentActiveNoteObject != null)
        {
            Logic_ItemPickup pickup = currentActiveNoteObject.GetComponent<Logic_ItemPickup>();
            if (pickup != null && pickup.itemData != null)
            {
                if (Core_InventoryManager.Instance.AddItem(pickup.itemData))
                {
                    currentActiveNoteObject.SetActive(false); // 物权转移，模型消失
                }
            }
            currentActiveNoteObject = null; // 清除缓存
        }

        if (playerController != null) playerController.enabled = true;
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
        if (uiPaperBackground.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseNote();
                return;
            }

            if (activeDecayScript != null)
            {
                // 1. 推进 3D 物理时间泵
                activeDecayScript.AddReadingTime(Time.deltaTime);
                
                // 2. 物理同步 UI 透明度
                SetOverlayAlpha(activeDecayScript.CurrentDecay);

                // 将底层物理数据实时打印到控制台
                if (uiBloodOverlay != null)
                {
                    Debug.Log($"[数据泵遥测] 目标纸条腐化值: {activeDecayScript.CurrentDecay:F3} | 实际写入UI透明度: {uiBloodOverlay.color.a:F3}");
                }
            }
        }
    }
}