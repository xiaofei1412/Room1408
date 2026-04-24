using UnityEngine;
using UnityEngine.Events;

public class Logic_RotatableDoor : MonoBehaviour
{
    [Header("旋转物理设置")]
    public float maxOpenAngle = 90f;   // 开启的最大绝对角度
    public bool isPullDoor = false;    // 勾选则表示向玩家方向拉（负角度）
    public float sensitivity = 5f;    
    public bool reverseMouse = false;  // 若发现鼠标拖拽方向与直觉相反，勾选此项反转映射
    
    [Header("事件触发配置")]
    [Range(0, 1)]
    public float triggerThreshold = 0.5f; 
    public UnityEvent OnThresholdReached; 
    private bool hasTriggeredEvent = false;

    [Header("音效组件")]
    public AudioClip moveSFX;         
    private AudioSource audioSource;

    private float currentYAngle = 0f;
    private bool isInteracting = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void StartDrag()
    {
        isInteracting = true;
        if (moveSFX != null && !audioSource.isPlaying)
            audioSource.PlayOneShot(moveSFX);
    }

    void Update()
    {
        if (isInteracting)
        {
            if (Input.GetMouseButtonUp(0))
            {
                isInteracting = false;
                return;
            }

            // 1. 获取鼠标 X 轴位移
            float mouseDelta = Input.GetAxis("Mouse X") * sensitivity;
            if (reverseMouse) mouseDelta = -mouseDelta;

            // 2. 根据推拉类型执行物理钳位 (Clamp)
            if (isPullDoor)
            {
                // 拉门：角度向负数域延伸
                currentYAngle += mouseDelta; 
                currentYAngle = Mathf.Clamp(currentYAngle, -maxOpenAngle, 0f);
            }
            else
            {
                // 推门：角度向正数域延伸
                currentYAngle -= mouseDelta; 
                currentYAngle = Mathf.Clamp(currentYAngle, 0f, maxOpenAngle);
            }

            // 3. 驱动 3D 实体旋转
            transform.localRotation = Quaternion.Euler(0, currentYAngle, 0);

            // 4. 阈值事件遥测 (取绝对值计算进度)
            float currentProgress = Mathf.Abs(currentYAngle) / maxOpenAngle;
            if (!hasTriggeredEvent && currentProgress >= triggerThreshold)
            {
                hasTriggeredEvent = true;
                OnThresholdReached?.Invoke();
            }
        }
    }
}