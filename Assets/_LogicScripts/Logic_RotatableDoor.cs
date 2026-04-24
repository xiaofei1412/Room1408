using UnityEngine;

public class Logic_RotatableDoor : MonoBehaviour
{
    [Header("旋转设置")]
    public float maxOpenAngle = 90f;   // 最大开启角度
    public float sensitivity = 5f;    // 旋转灵敏度
    
    [Header("音效")]
    public AudioClip moveSFX;         // 开门声
    private AudioSource audioSource;

    private float currentYAngle = 0f;
    private bool isInteracting = false;

    void Start()
    {
        // 自动装配音效组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // 由 Core_Raycaster 在左键按下时调用
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

            // 获取鼠标 X 轴增量。左移为负，-Input 为正，角度增加（即逆时针）
            float mouseDelta = -Input.GetAxis("Mouse X") * sensitivity;

            if (Mathf.Abs(mouseDelta) > 0.01f) // 物理阈值过滤，防止细微抖动
            {
                currentYAngle = Mathf.Clamp(currentYAngle + mouseDelta, 0f, maxOpenAngle);
                transform.localRotation = Quaternion.Euler(0, currentYAngle, 0);
            }
        }
    }
}