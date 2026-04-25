using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Logic_EndgameFall : MonoBehaviour
{
    [Header("现实剥离目标")]
    public GameObject bedFrameBottom; 
    public Transform tunnelCenter;    

    [Header("玩家物理控制权")]
    public GameObject playerObj;
    public Camera mainCamera;
    
    [Header("无限坠落与雾效参数")]
    public float gravityAcceleration = 15f; 
    public float maxFallSpeed = 60f;        
    public float targetFOV = 120f;          
    public float fovChangeSpeed = 15f;
    
    [Tooltip("当玩家下落超过这个距离时，瞬间传送回起点 (制造无限循环)")]
    public float loopDistance = 30f; 
    [Tooltip("整个深渊坠落演出的总时长 (秒)")]
    public float totalFallDuration = 12f;

    [Header("终局黑屏 UI")]
    public CanvasGroup blackScreenCanvasGroup; // 用于最后淡出至死寂

    [Header("音效")]
    public AudioClip deepBassSFX;   
    public AudioClip fallingWindSFX;
    private AudioSource audioSource;

    private bool isFalling = false;
    private float currentSpeed = 0f;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (playerObj == null) playerObj = GameObject.FindGameObjectWithTag("Player");
        if (mainCamera == null && playerObj != null) mainCamera = playerObj.GetComponentInChildren<Camera>();
    }

    public void TriggerAbyssFall()
    {
        if (isFalling) return;
        StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        isFalling = true;

        // 彻底抹除现实光效，强行将相机底色设为纯黑，消灭青色闪烁
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.black;

        if (bedFrameBottom != null) bedFrameBottom.SetActive(false);
        if (deepBassSFX != null) audioSource.PlayOneShot(deepBassSFX);

        // 剥夺权限
        if (playerObj.GetComponent<Core_FirstPersonController>() != null) playerObj.GetComponent<Core_FirstPersonController>().enabled = false;
        if (playerObj.GetComponent<CharacterController>() != null) playerObj.GetComponent<CharacterController>().enabled = false;
        if (playerObj.GetComponent<Collider>() != null) playerObj.GetComponent<Collider>().enabled = false;

        // 对齐中心点
        Vector3 startPos = playerObj.transform.position;
        if (tunnelCenter != null)
        {
            playerObj.transform.position = new Vector3(tunnelCenter.position.x, startPos.y, tunnelCenter.position.z);
        }

        if (fallingWindSFX != null)
        {
            audioSource.clip = fallingWindSFX;
            audioSource.loop = true;
            audioSource.Play();
        }

        Quaternion startRot = mainCamera.transform.localRotation;
        Quaternion targetRot = Quaternion.Euler(90f, startRot.eulerAngles.y, 0f);

        // 接管渲染引擎：开启纯黑色的深渊雾效
        bool originalFogState = RenderSettings.fog;
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.01f;

        float timer = 0f;
        float currentYOffset = 0f; // 记录当前循环周期内的下落位移

        // 坠落循环演出
        while (timer < totalFallDuration)
        {
            timer += Time.deltaTime;

            currentSpeed += gravityAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxFallSpeed);

            float dropDelta = currentSpeed * Time.deltaTime;
            playerObj.transform.Translate(Vector3.down * dropDelta, Space.World);
            currentYOffset += dropDelta;

            // 无限空间循环的核心魔术：如果掉得太深，瞬间拉回起点！
            if (currentYOffset >= loopDistance)
            {
                playerObj.transform.position += new Vector3(0, loopDistance, 0);
                currentYOffset -= loopDistance; // 重置偏移量，做到无缝衔接
            }

            // 视角与 FOV 动态变化
            mainCamera.transform.localRotation = Quaternion.Slerp(mainCamera.transform.localRotation, targetRot, Time.deltaTime * 3f);
            if (mainCamera.fieldOfView < targetFOV) mainCamera.fieldOfView += fovChangeSpeed * Time.deltaTime;

            // 深渊吞噬：随着时间推移，黑色雾气越来越浓，彻底遮蔽底部的光线和穿帮边缘
            RenderSettings.fogDensity = Mathf.Lerp(0.01f, 0.4f, timer / totalFallDuration);

            yield return null;
        }

        // 演出结束：切入死寂
        yield return StartCoroutine(FadeToBlack());
    }

    private IEnumerator FadeToBlack()
    {
        // 风声渐弱
        float startVolume = audioSource.volume;
        float fadeTimer = 0f;
        float fadeTime = 2.0f;

        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / fadeTime);
            
            // 屏幕黑场 UI 渐显
            if (blackScreenCanvasGroup != null)
            {
                blackScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer / fadeTime);
            }
            yield return null;
        }

        audioSource.Stop();
        Debug.Log("【游戏通关】玩家已被深渊彻底吞噬，可在此处加载制作人员名单 (Credits)。");
        
        // 可选：在此处加载结束场景
        // UnityEngine.SceneManagement.SceneManager.LoadScene("EndCredits");
    }
}