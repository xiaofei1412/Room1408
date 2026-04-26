using UnityEngine;
using TMPro;
using System.Collections;

public class UI_TVTuning : MonoBehaviour
{
    public static UI_TVTuning Instance;

    [Header("UI 物理绑定")]
    public GameObject tvCanvasPanel;      // 调频界面的根节点
    public TextMeshProUGUI channelDisplay;// 屏幕顶部的频道数字显示

    [Header("物理调频参数")]
    public int targetChannel = 1408;
    public int coarseTuneAmount = 10;     // 齿轮旋钮的粗调幅度

    [Header("音效阵列")]
    public AudioClip staticNoiseSFX;      // 持续的背景白噪音
    public AudioClip switchClickSFX;      // 旋钮咔哒声
    public AudioClip correctSFX;          // 密码正确的音效
    public AudioClip errorSFX;            // 密码错误的音效
    private AudioSource audioSource;

    private int currentChannel = 0;
    private bool isInteracting = false;

    private Core_FirstPersonController playerController;
    private Core_Raycaster raycaster;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        playerController = Object.FindFirstObjectByType<Core_FirstPersonController>();
        raycaster = Object.FindFirstObjectByType<Core_Raycaster>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // 白噪音需要循环
    }

    public void OpenTVTuning()
    {
        isInteracting = true;
        // 物理初始化：每次打开给一个随机的三/四位频道制造失真感
        currentChannel = Random.Range(100, 9999); 
        UpdateDisplay();

        tvCanvasPanel.SetActive(true);

        if (playerController != null) playerController.enabled = false;
        if (raycaster != null) raycaster.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (staticNoiseSFX != null)
        {
            audioSource.clip = staticNoiseSFX;
            audioSource.Play();
        }
    }

    public void FineTune(int amount)
    {
        ApplyChannelChange(amount);
    }

    public void CoarseTune(int steps)
    {
        ApplyChannelChange(steps * coarseTuneAmount);
    }

    private void ApplyChannelChange(int amount)
    {
        currentChannel += amount;
        
        // 物理钳位：限制在 0000 到 9999 之间，支持循环跳转
        if (currentChannel < 0) currentChannel = 9999;
        if (currentChannel > 9999) currentChannel = 0;
        
        UpdateDisplay();
        
        if (switchClickSFX != null) 
            audioSource.PlayOneShot(switchClickSFX);
    }

    private void UpdateDisplay()
    {
        // 强制格式化为 4 位数字 (如 0042)
        channelDisplay.text = currentChannel.ToString("D4"); 
    }

    public void OnSubmit()
    {
        if (currentChannel == targetChannel)
        {
            if (correctSFX != null) audioSource.PlayOneShot(correctSFX);

            Core_MonologueManager.Instance.ShowMonologue("The static is clearing. Something is coming through.");

            // 物理封锁：彻底剥夺电视机的交互权限，防止无限重玩
            GameObject tvObj = GameObject.Find("Prop_TV_Living");
            if (tvObj != null) tvObj.tag = "Untagged";

            // 触发异象
            if (UI_TVAnomalySequence.Instance != null)
            {
                UI_TVAnomalySequence.Instance.PlaySequence();
            }
        }
        else
        {
            if (errorSFX != null) audioSource.PlayOneShot(errorSFX);

            Core_SanityManager.Instance.DecreaseSanity(5);
            Core_MonologueManager.Instance.ShowMonologue("Just white noise... This isn't the right frequency.");
        }
    }

    public void CloseTVTuning()
    {
        isInteracting = false;
        tvCanvasPanel.SetActive(false);
        if (audioSource.isPlaying) audioSource.Stop();

        if (playerController != null) playerController.enabled = true;
        if (raycaster != null)
        {
            raycaster.enabled = true;
            raycaster.currentInteractableObj = null;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTVTuning();
        }
    }
}