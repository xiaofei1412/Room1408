using UnityEngine;
using UnityEngine.Events;

public class Logic_SlidingProp : MonoBehaviour
{
    public enum SlideAxis { X, Y, Z }
    public enum InputAxis { MouseX, MouseY } // 鼠标输入轴向选择
    public enum ClampMode { BiDirectional, ForwardOnly } // 钳位模式

    [Header("平移物理设置")]
    public SlideAxis slideAxis = SlideAxis.X; 
    public InputAxis inputAxis = InputAxis.MouseX; // 抽屉建议选 MouseY
    public ClampMode clampMode = ClampMode.BiDirectional; // 抽屉必须选 ForwardOnly

    public float maxSlideDistance = 1.0f;     
    public float sensitivity = 0.05f;         
    public bool reverseDirection = false;     

    [Header("锁钥阻断系统")]
    public string requiredItemID = "";        
    public bool isLocked = false;             

    [Header("事件触发配置")]
    [Range(0, 1)]
    public float triggerThreshold = 0.5f; 
    public UnityEvent OnThresholdReached; 
    private bool hasTriggeredEvent = false;

    [Header("音效组件")]
    public AudioClip slideSFX;         
    public AudioClip lockedSFX;        
    public AudioClip unlockSFX;        
    private AudioSource audioSource;

    private float currentOffset = 0f;
    private bool isInteracting = false;
    private Vector3 initialLocalPos;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        if (!string.IsNullOrEmpty(requiredItemID)) isLocked = true;
    }

    public void StartDrag()
    {
        if (isLocked)
        {
            if (Core_InventoryManager.Instance.HasItem(requiredItemID))
            {
                isLocked = false;
                if (unlockSFX != null) audioSource.PlayOneShot(unlockSFX);
                Core_MonologueManager.Instance.ShowMonologue("The key turned smoothly. The drawer is unlocked.");
            }
            else
            {
                if (lockedSFX != null) audioSource.PlayOneShot(lockedSFX);
                Core_MonologueManager.Instance.ShowMonologue("It's locked tight. I need to find the key.");
                return; 
            }
        }

        isInteracting = true;
        if (slideSFX != null && !audioSource.isPlaying)
        {
            audioSource.clip = slideSFX;
            audioSource.loop = true; 
            audioSource.Play();
        }
    }

    void Update()
    {
        if (isInteracting)
        {
            if (Input.GetMouseButtonUp(0))
            {
                isInteracting = false;
                if (audioSource.isPlaying) audioSource.Stop();
                return;
            }

            // 核心逻辑 1：读取对应轴向的鼠标输入
            float mouseDelta = (inputAxis == InputAxis.MouseX) ? Input.GetAxis("Mouse X") : Input.GetAxis("Mouse Y");
            mouseDelta *= sensitivity;
            if (reverseDirection) mouseDelta = -mouseDelta;

            // 核心逻辑 2：物理钳位区分。 ForwardOnly 保证抽屉推到 0 就绝对停下，不陷入桌子
            float minBound = (clampMode == ClampMode.BiDirectional) ? -maxSlideDistance : 0f;
            currentOffset = Mathf.Clamp(currentOffset + mouseDelta, minBound, maxSlideDistance);

            // 驱动 3D 实体平移
            Vector3 newPos = initialLocalPos;
            switch (slideAxis)
            {
                case SlideAxis.X: newPos.x = initialLocalPos.x + currentOffset; break;
                case SlideAxis.Y: newPos.y = initialLocalPos.y + currentOffset; break;
                case SlideAxis.Z: newPos.z = initialLocalPos.z + currentOffset; break;
            }
            transform.localPosition = newPos;

            // 阈值事件判定
            float currentProgress = Mathf.Abs(currentOffset) / maxSlideDistance;
            if (!hasTriggeredEvent && currentProgress >= triggerThreshold)
            {
                hasTriggeredEvent = true;
                OnThresholdReached?.Invoke();
            }
        }
    }
}