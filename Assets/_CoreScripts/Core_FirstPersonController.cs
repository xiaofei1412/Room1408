using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class Core_FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2.0f;
    public Transform playerCamera;

    [Header("脚步声系统")]
    public AudioClip[] footstepSounds; // 拖入几个不同的脚步声音效增加随机性
    public float stepInterval = 0.6f;  // 走多远响一步
    private float stepTimer = 0f;
    private AudioSource audioSource;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;  
    private float yRotation = 90.0f;  

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // (鼠标与镜头视角代码保持你原来的不变)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // 移动逻辑
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * walkSpeed * Time.deltaTime);

        // 脚步声逻辑 (绝对排除了下坠时在空中发声的可能)
        if (controller.isGrounded && move.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f; // 停止移动瞬间重置
        }

        // 重力
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void PlayFootstep()
    {
        if (footstepSounds.Length > 0 && audioSource != null)
        {
            int index = Random.Range(0, footstepSounds.Length);
            // 给音高加一点点随机波动，让脚步声听起来不那么机械
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(footstepSounds[index], 0.3f); // 脚步声适当小一点
        }
    }
}