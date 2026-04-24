using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_MirrorCloseup : MonoBehaviour
{
    public static UI_MirrorCloseup Instance;

    [Header("UI 物理绑定")]
    public GameObject mirrorCanvasPanel;  
    public RawImage fogOverlay;           
    public Image bloodTextLayer;          
    
    [Header("物理擦除参数")]
    public int textureResolution = 512;   
    public int brushRadius = 25;          
    public float revealThreshold = 0.3f;  
    
    [Header("理智惩罚配置")]
    public float idleThreshold = 3.0f;    // 3秒无动作触发
    public int idleSanityCost = 10;       // 惩罚点数
    
    [Header("音效")]
    public AudioClip wipeSound;

    private Texture2D dynamicFogTexture;
    private Color32[] fogPixels;
    private int totalPixels;
    private int clearedPixels;
    private bool isRevealed = false;
    private bool isInteracting = false;

    // 惩罚系统状态变量
    private float idleTimer = 0f;
    private bool idlePenaltyTriggered = false;

    private Core_FirstPersonController playerController;
    private Core_Raycaster raycaster;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        playerController = Object.FindFirstObjectByType<Core_FirstPersonController>();
        raycaster = Object.FindFirstObjectByType<Core_Raycaster>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
    }

    public void StartMirrorInteraction()
    {
        isInteracting = true;
        isRevealed = false;
        clearedPixels = 0;
        
        // 重置计时器与惩罚标志位
        idleTimer = 0f;
        idlePenaltyTriggered = false;

        if (playerController != null) playerController.enabled = false;
        if (raycaster != null) raycaster.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        bloodTextLayer.color = new Color(1, 1, 1, 0);
        InitializeFogTexture();
        mirrorCanvasPanel.SetActive(true);
    }

    private void InitializeFogTexture()
    {
        dynamicFogTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);
        totalPixels = textureResolution * textureResolution;
        fogPixels = new Color32[totalPixels];
        Color32 fogColor = new Color32(200, 200, 210, 240);
        for (int i = 0; i < totalPixels; i++) fogPixels[i] = fogColor;
        dynamicFogTexture.SetPixels32(fogPixels);
        dynamicFogTexture.Apply();
        fogOverlay.texture = dynamicFogTexture;
    }

    private void Update()
    {
        if (!isInteracting) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitInteraction();
            return;
        }

        // 擦拭动作检测
        if (Input.GetMouseButton(0))
        {
            // 只要有点击或拖拽，立刻重置空闲计时
            idleTimer = 0f; 
            ProcessWiping();
            if (!audioSource.isPlaying && wipeSound != null) audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying) audioSource.Pause();

            // 空闲判定逻辑
            if (!isRevealed && !idlePenaltyTriggered)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleThreshold)
                {
                    ExecuteIdlePenalty();
                }
            }
        }
    }

    private void ExecuteIdlePenalty()
    {
        idlePenaltyTriggered = true;
        
        // 1. 物理扣除理智值
        if (Core_SanityManager.Instance != null)
        {
            Core_SanityManager.Instance.DecreaseSanity(idleSanityCost);
        }

        // 2. 推送全英文犹豫/害怕独白 (All-English Monologue)
        if (Core_MonologueManager.Instance != null)
        {
            Core_MonologueManager.Instance.ShowMonologue("My hands are trembling... I'm too terrified to touch the glass.");
            Core_MonologueManager.Instance.ShowMonologue("Something is staring at me from the other side of this fog. I can feel it.");
        }
    }

    private void ProcessWiping()
    {
        RectTransform rectTrans = fogOverlay.rectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTrans, Input.mousePosition, null, out Vector2 localPoint))
        {
            float normalizedX = (localPoint.x / rectTrans.rect.width) + 0.5f;
            float normalizedY = (localPoint.y / rectTrans.rect.height) + 0.5f;
            int texX = Mathf.RoundToInt(normalizedX * textureResolution);
            int texY = Mathf.RoundToInt(normalizedY * textureResolution);
            bool textureChanged = false;

            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                for (int y = -brushRadius; y <= brushRadius; y++)
                {
                    if (x * x + y * y <= brushRadius * brushRadius)
                    {
                        int targetX = Mathf.Clamp(texX + x, 0, textureResolution - 1);
                        int targetY = Mathf.Clamp(texY + y, 0, textureResolution - 1);
                        int pixelIndex = targetY * textureResolution + targetX;
                        if (fogPixels[pixelIndex].a > 0)
                        {
                            fogPixels[pixelIndex] = new Color32(0, 0, 0, 0);
                            clearedPixels++;
                            textureChanged = true;
                        }
                    }
                }
            }

            if (textureChanged)
            {
                dynamicFogTexture.SetPixels32(fogPixels);
                dynamicFogTexture.Apply();
                if (!isRevealed && (float)clearedPixels / totalPixels > revealThreshold) TriggerReveal();
            }
        }
    }

    private void TriggerReveal()
    {
        isRevealed = true;
        StartCoroutine(FadeInBloodText());
        Core_MonologueManager.Instance.ShowMonologue("1408... 'Seek the silver eyes'...");
        Core_MonologueManager.Instance.ShowMonologue("The drain... it's right behind me.");
        GameObject showerObj = GameObject.Find("SM_Shower");
        if (showerObj != null) showerObj.tag = "Operable";
    }

    private IEnumerator FadeInBloodText()
    {
        float elapsed = 0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            bloodTextLayer.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, elapsed / 1.5f));
            yield return null;
        }
    }

    private void ExitInteraction()
    {
        isInteracting = false;
        mirrorCanvasPanel.SetActive(false);
        if (audioSource.isPlaying) audioSource.Stop();
        if (playerController != null) playerController.enabled = true;
        if (raycaster != null)
        {
            raycaster.enabled = true;
            raycaster.currentInteractableObj = null;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (dynamicFogTexture != null) Destroy(dynamicFogTexture);
    }
}