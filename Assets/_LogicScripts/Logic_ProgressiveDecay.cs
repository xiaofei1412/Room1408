using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Logic_ProgressiveDecay : MonoBehaviour
{
    [Header("腐化渲染配置")]
    public float requiredReadingTime = 4.0f;
    
    [Header("向 UI 镜像输出的贴图数据")]
    public Texture basePaperTexture;
    public Texture decayTexture;

    private Renderer targetRenderer;
    private MaterialPropertyBlock propBlock;
    private int decayAmountID;
    
    // 暴露只读属性给 UI 系统
    public float CurrentDecay { get; private set; } = 0f;
    private bool isFullyDecayed = false;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        decayAmountID = Shader.PropertyToID("_Decay_Amount"); // 3D Shader 的 Reference
    }

    public void AddReadingTime(float deltaTime)
    {
        if (isFullyDecayed) return;

        CurrentDecay += deltaTime / requiredReadingTime;
        CurrentDecay = Mathf.Clamp01(CurrentDecay);

        targetRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(decayAmountID, CurrentDecay);
        targetRenderer.SetPropertyBlock(propBlock);

        if (CurrentDecay >= 1f) isFullyDecayed = true;
    }
}