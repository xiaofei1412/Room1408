using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_SanityHearts : MonoBehaviour
{
    [Header("素材映射")]
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    [Header("对象引用")]
    public Image[] heartImages;
    
    [Header("物理表现配置 (Juice)")]
    [Tooltip("拖入挂载了 Horizontal Layout Group 的那个父节点容器")]
    public RectTransform heartContainer; 
    public Color highSanityColor = Color.white; // 满理智原色
    public Color lowSanityColor = new Color(0.6f, 0.1f, 0.1f); // 极低理智时的暗红/灰暗色调
    public float shakeIntensity = 10f; // 抖动像素偏移最大值
    public float shakeDuration = 0.4f; // 抖动持续秒数

    private int previousSanity = 100;
    private Coroutine shakeCoroutine;
    private Vector2 originalContainerPos;

    private void Start()
    {
        // 记录容器的初始物理坐标
        if (heartContainer != null)
        {
            originalContainerPos = heartContainer.anchoredPosition;
        }

        if (Core_SanityManager.Instance != null)
        {
            Core_SanityManager.Instance.OnSanityChanged += UpdateHeartDisplay;
            UpdateHeartDisplay(Core_SanityManager.Instance.GetSanity());
        }
    }

    private void OnDestroy()
    {
        if (Core_SanityManager.Instance != null)
        {
            Core_SanityManager.Instance.OnSanityChanged -= UpdateHeartDisplay;
        }
    }

    private void UpdateHeartDisplay(int currentSanity)
    {
        // 1. 颜色修正计算 (Color Correction)
        // 将 0-100 的理智值归一化为 0.0 - 1.0 的浮点数
        float sanityPercent = Mathf.Clamp01(currentSanity / 100f);
        // 基于当前理智百分比，在“低理智色”与“高理智色”之间进行物理线性插值
        Color targetColor = Color.Lerp(lowSanityColor, highSanityColor, sanityPercent);

        // 2. 贴图映射与色彩覆写
        for (int i = 0; i < heartImages.Length; i++)
        {
            int heartThreshold = (i + 1) * 10;

            if (currentSanity >= heartThreshold)
            {
                heartImages[i].sprite = fullHeart;
            }
            else if (currentSanity >= heartThreshold - 5)
            {
                heartImages[i].sprite = halfHeart;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }

            // 强制覆写 Image 组件的 Color 通道
            heartImages[i].color = targetColor;
        }

        // 3. 物理受创抖动判定 (Damage Shake)
        // 仅在理智值发生“下降”时触发抖动，回复理智时不抖动
        if (currentSanity < previousSanity && heartContainer != null)
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeRoutine());
        }

        previousSanity = currentSanity;
    }

    // 底层物理震动发生器
    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            
            // 随时间推移，计算衰减系数 (1 -> 0)
            float damping = 1f - (elapsed / shakeDuration);
            
            // 生成随机的二维坐标像素偏移量
            float offsetX = Random.Range(-1f, 1f) * shakeIntensity * damping;
            float offsetY = Random.Range(-1f, 1f) * shakeIntensity * damping;

            heartContainer.anchoredPosition = originalContainerPos + new Vector2(offsetX, offsetY);

            yield return null; // 等待至下一帧渲染
        }

        // 震动结束，坐标绝对强制归位，防止位置偏移累积
        heartContainer.anchoredPosition = originalContainerPos;
        shakeCoroutine = null;
    }
}