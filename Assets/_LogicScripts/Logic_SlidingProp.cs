using UnityEngine;
using UnityEngine.Events;

public class Logic_SlidingProp : MonoBehaviour
{
    [Header("平移物理设置")]
    public float maxSlideDistance = 1.0f; // 最大滑动距离
    public float sensitivity = 0.05f;     // 滑动灵敏度
    public bool reverseDirection = false; // 是否反转鼠标X轴映射
    
    [Header("事件触发配置")]
    [Range(0, 1)]
    public float triggerThreshold = 0.5f; // 滑动超过 50% 触发事件
    public UnityEvent OnThresholdReached; 
    private bool hasTriggeredEvent = false;

    [Header("音效组件")]
    public AudioClip slideSFX;         
    private AudioSource audioSource;

    private float currentXOffset = 0f;
    private bool isInteracting = false;
    private Vector3 initialLocalPos;

    void Start()
    {
        // 记录初始物理锚点
        initialLocalPos = transform.localPosition;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void StartDrag()
    {
        isInteracting = true;
        if (slideSFX != null && !audioSource.isPlaying)
        {
            audioSource.clip = slideSFX;
            audioSource.loop = true; // 拖拽时循环播放摩擦音
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

            // 1. 获取鼠标物理位移
            float mouseDelta = Input.GetAxis("Mouse X") * sensitivity;
            if (reverseDirection) mouseDelta = -mouseDelta;

            // 2. 物理钳位：限制在 0 到 maxSlideDistance 之间
            currentXOffset = Mathf.Clamp(currentXOffset + mouseDelta, 0f, maxSlideDistance);

            // 3. 驱动 3D 实体平移 (基于本地坐标系的 X 轴)
            transform.localPosition = new Vector3(initialLocalPos.x + currentXOffset, initialLocalPos.y, initialLocalPos.z);

            // 4. 阈值事件遥测
            float currentProgress = currentXOffset / maxSlideDistance;
            if (!hasTriggeredEvent && currentProgress >= triggerThreshold)
            {
                hasTriggeredEvent = true;
                OnThresholdReached?.Invoke();
            }
        }
    }
}