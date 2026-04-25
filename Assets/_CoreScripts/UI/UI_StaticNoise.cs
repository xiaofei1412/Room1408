using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UI_StaticNoise : MonoBehaviour
{
    [Header("渲染参数")]
    public int textureResolution = 256; // 低分辨率带来天然的颗粒感
    public int framesCount = 5;         // 生成 5 帧不同的噪点图进行轮播
    public float fps = 30f;             // 轮播帧率
    public Vector2 uvTiling = new Vector2(4, 4); // UV 平铺，防止拉伸失真

    private RawImage rawImage;
    private Texture2D[] noiseTextures;
    private float timer;
    private int currentFrame;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.uvRect = new Rect(0, 0, uvTiling.x, uvTiling.y);
        GenerateNoiseTextures();
    }

    private void GenerateNoiseTextures()
    {
        noiseTextures = new Texture2D[framesCount];
        for (int i = 0; i < framesCount; i++)
        {
            // 创建无压缩、不支持 MIP 的基础纹理
            Texture2D tex = new Texture2D(textureResolution, textureResolution, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Point; // 强制点采样，拒绝平滑模糊，保持锋利的马赛克感
            
            Color[] pixels = new Color[textureResolution * textureResolution];
            for (int p = 0; p < pixels.Length; p++)
            {
                float val = Random.Range(0f, 1f);
                pixels[p] = new Color(val, val, val, 1f); // 随机黑白灰
            }
            tex.SetPixels(pixels);
            tex.Apply();
            noiseTextures[i] = tex;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer -= 1f / fps;
            currentFrame = (currentFrame + 1) % framesCount;
            rawImage.texture = noiseTextures[currentFrame];
        }
    }
}