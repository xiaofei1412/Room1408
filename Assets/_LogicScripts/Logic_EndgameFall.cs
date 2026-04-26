using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Logic_EndgameFall : MonoBehaviour
{
    public static Logic_EndgameFall Instance;

    [Header("现实剥离目标")]
    public GameObject bedFrameBottom; 
    public Transform tunnelCenter;    

    [Header("玩家物理控制权")]
    public GameObject playerObj;
    public Camera mainCamera;
    
    [Header("坠落参数")]
    public float gravityAcceleration = 15f; 
    public float maxFallSpeed = 60f;        
    public float targetFOV = 120f;          
    public float fovChangeSpeed = 15f;
    public float loopDistance = 30f; 
    public float totalFallDuration = 12f;

    [Header("终局黑屏 UI")]
    public CanvasGroup blackScreenCanvasGroup; 

    [Header("音效")]
    public AudioClip deepBassSFX;   
    public AudioClip fallingWindSFX;
    private AudioSource audioSource;

    [HideInInspector] public bool isEndingActive = false;
    private float currentSpeed = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        if (playerObj == null) playerObj = GameObject.FindGameObjectWithTag("Player");
        if (mainCamera == null && playerObj != null) mainCamera = playerObj.GetComponentInChildren<Camera>();
    }

    public void TriggerAbyssFall()
    {
        if (isEndingActive) return;
        StartCoroutine(FallSequence(false));
    }

    public void TriggerForcedFall()
    {
        if (isEndingActive) return;
        StartCoroutine(FallSequence(true));
    }

    private IEnumerator FallSequence(bool isForced)
    {
        isEndingActive = true;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.black;

        if (!isForced && bedFrameBottom != null) bedFrameBottom.SetActive(false);
        if (deepBassSFX != null) audioSource.PlayOneShot(deepBassSFX);

        Core_Raycaster raycaster = Object.FindFirstObjectByType<Core_Raycaster>();
        if (raycaster != null) raycaster.enabled = false;
        Core_InspectSystem inspect = Object.FindFirstObjectByType<Core_InspectSystem>();
        if (inspect != null) { inspect.isInspecting = false; inspect.enabled = false; }
        if (playerObj.GetComponent<Core_FirstPersonController>() != null) playerObj.GetComponent<Core_FirstPersonController>().enabled = false;
        if (playerObj.GetComponent<CharacterController>() != null) playerObj.GetComponent<CharacterController>().enabled = false;

        Vector3 startPos = playerObj.transform.position;
        if (tunnelCenter != null) playerObj.transform.position = new Vector3(tunnelCenter.position.x, startPos.y, tunnelCenter.position.z);

        if (fallingWindSFX != null) { audioSource.clip = fallingWindSFX; audioSource.loop = true; audioSource.Play(); }

        Quaternion startRot = mainCamera.transform.localRotation;
        Quaternion targetRot = Quaternion.Euler(90f, startRot.eulerAngles.y, 0f);

        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.01f;

        float timer = 0f;
        float currentYOffset = 0f;

        while (timer < totalFallDuration)
        {
            timer += Time.deltaTime;
            currentSpeed += gravityAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxFallSpeed);
            playerObj.transform.Translate(Vector3.down * currentSpeed * Time.deltaTime, Space.World);
            currentYOffset += currentSpeed * Time.deltaTime;

            if (currentYOffset >= loopDistance)
            {
                playerObj.transform.position += new Vector3(0, loopDistance, 0);
                currentYOffset -= loopDistance; 
            }

            mainCamera.transform.localRotation = Quaternion.Slerp(mainCamera.transform.localRotation, targetRot, Time.deltaTime * 3f);
            if (mainCamera.fieldOfView < targetFOV) mainCamera.fieldOfView += fovChangeSpeed * Time.deltaTime;
            RenderSettings.fogDensity = Mathf.Lerp(0.01f, 0.4f, timer / totalFallDuration);
            yield return null;
        }

        if (Logic_EndingText.Instance != null) Logic_EndingText.Instance.ClearText();

        yield return StartCoroutine(FadeToBlack(2.0f));

        int finalSanity = Core_SanityManager.Instance != null ? Core_SanityManager.Instance.currentSanity : 100;

        // 跨场景前，关闭 BGM，防止被带入结局场景
        if (Audio_SoundManager.Instance != null) Audio_SoundManager.Instance.StopBGM();

        // 直接加载对应的物理场景
        if (isForced || finalSanity < 40)
        {
            Debug.Log("【结局 C】进入死亡场景");
            // 这里不需要写入 PlayerPrefs，因为是直接 Game Over
            SceneManager.LoadScene("Scene_GameOver"); 
        }
        else if (finalSanity > 70)
        {
            Debug.Log("【结局 A】进入苏醒运镜场景");
            SceneManager.LoadScene("Scene_EndingA");
        }
        else
        {
            Debug.Log("【结局 B】重载主场景，开启二周目");
            PlayerPrefs.SetInt("EndingType", 2); // 写入 2，告诉主场景这是二周目
            PlayerPrefs.Save();
            SceneManager.LoadScene("Main_1408"); // 请确保这里是你的主场景准确名称
        }
    }

    // 代码模拟手动重启 (Memory Wipe)
    private void SimulateHardRestart(int endingType)
    {
        PlayerPrefs.SetInt("EndingType", endingType);
        PlayerPrefs.Save();

        // 强行砸碎所有单例的静态引用，消除引擎重载时的幽灵污染
        Core_MonologueManager.Instance = null;
        Core_InventoryManager.Instance = null;
        Core_SanityManager.Instance = null;
        Instance = null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator FadeToBlack(float fadeTime)
    {
        float startVolume = audioSource.volume;
        float fadeTimer = 0f;
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / fadeTime);
            if (blackScreenCanvasGroup != null) blackScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer / fadeTime);
            yield return null;
        }
        audioSource.Stop();
    }
}