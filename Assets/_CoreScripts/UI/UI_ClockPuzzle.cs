using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_ClockPuzzle : MonoBehaviour
{
    public static UI_ClockPuzzle Instance;

    [Header("UI 物理绑定")]
    public GameObject clockCanvasPanel;
    public RectTransform minuteHand; // 分针 UI
    public RectTransform hourHand;   // 时针 UI

    [Header("逻辑参数")]
    // 14:08 -> 2小时 * 60 + 8分钟 = 128分钟
    public float targetMinutes = 128f;
    public float tolerance = 2f; // 容许误差 2 分钟

    [Header("音效阵列")]
    public AudioClip tickSound;
    public AudioClip unlockSound;
    private AudioSource audioSource;

    // 核心状态机：范围 0 到 719 (12小时 * 60分钟)
    private float currentTotalMinutes = 0f;
    private bool isInteracting = false;
    private bool isUnlocked = false;
    private float tickTimer = 0f;

    private Core_FirstPersonController playerController;
    private Core_Raycaster raycaster;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        
        playerController = Object.FindFirstObjectByType<Core_FirstPersonController>();
        raycaster = Object.FindFirstObjectByType<Core_Raycaster>();
    }

    public void OpenClock()
    {
        if (isUnlocked) return;

        isInteracting = true;
        clockCanvasPanel.SetActive(true);

        if (playerController != null) playerController.enabled = false;
        if (raycaster != null) raycaster.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 随机生成一个初始时间打乱局面
        currentTotalMinutes = Random.Range(0f, 720f);
        UpdateHandsDisplay();
    }

    // 开放给拖拽模块的接口
    public void AddRotation(bool isHourHand, float clockwiseAngleDelta)
    {
        if (isHourHand)
        {
            // 拖拽时针：转360度 = 720分钟。所以 1度 = 2分钟
            currentTotalMinutes += clockwiseAngleDelta * 2f;
        }
        else
        {
            // 拖拽分针：转360度 = 60分钟。所以 1度 = 1/6分钟
            currentTotalMinutes += clockwiseAngleDelta / 6f;
        }

        // 闭环时间计算：钳位在 12 小时内
        if (currentTotalMinutes < 0) currentTotalMinutes += 720f;
        if (currentTotalMinutes >= 720f) currentTotalMinutes -= 720f;

        UpdateHandsDisplay();
        PlayTickSound(clockwiseAngleDelta);
    }

    private void UpdateHandsDisplay()
    {
        // 渲染映射：
        float minuteAngle = (currentTotalMinutes % 60f) * 6f; 
        float hourAngle = (currentTotalMinutes / 720f) * 360f;

        // Unity 2D UI 旋转：Z轴负方向为顺时针
        minuteHand.localRotation = Quaternion.Euler(0, 0, -minuteAngle);
        hourHand.localRotation = Quaternion.Euler(0, 0, -hourAngle);
    }

    private void PlayTickSound(float angleChange)
    {
        // 限制滴答声频率，防止高频刺耳
        tickTimer += Mathf.Abs(angleChange);
        if (tickTimer > 15f) // 每物理转过 15 度播放一次
        {
            if (tickSound != null) audioSource.PlayOneShot(tickSound);
            tickTimer = 0f;
        }
    }

    public void OnSubmitPressed()
    {
        // 绝对死锁：如果已经解开，直接拦截后续所有点击
        if (isUnlocked) return; 

        if (Mathf.Abs(currentTotalMinutes - targetMinutes) <= tolerance)
        {
            isUnlocked = true; // 状态锁定
            if (unlockSound != null) audioSource.PlayOneShot(unlockSound);
            
            Core_MonologueManager.Instance.ShowMonologue("Something heavy shifted inside the clock case.");

            // 核心逻辑：触发月相盘翻转序列
            GameObject moonDial = GameObject.Find("MoonDial");
            if (moonDial != null)
            {
                Logic_MoonDialSequence seq = moonDial.GetComponent<Logic_MoonDialSequence>();
                if (seq != null) seq.PlayRevealSequence();
            }

            // 物理封锁：彻底剥夺座钟主体的交互权限，防止二次唤醒 UI
            GameObject tallClock = GameObject.Find("TallClock");
            if (tallClock != null) tallClock.tag = "Untagged";

            StartCoroutine(CloseAfterDelay(1.5f));
        }
        else
        {
            Core_SanityManager.Instance.DecreaseSanity(5);
            Core_MonologueManager.Instance.ShowMonologue("That's not the right time. Nothing happened.");
        }
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseClock();
    }

    public void CloseClock()
    {
        isInteracting = false;
        clockCanvasPanel.SetActive(false);

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
        if (isInteracting && Input.GetKeyDown(KeyCode.Escape)) CloseClock();
    }
}