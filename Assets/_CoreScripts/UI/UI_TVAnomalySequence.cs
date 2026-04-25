using UnityEngine;
using TMPro;
using System.Collections;

public class UI_TVAnomalySequence : MonoBehaviour
{
    public static UI_TVAnomalySequence Instance;

    [Header("异象 UI 绑定")]
    public GameObject tuningControlsGroup; // 包含所有按钮、齿轮、绿色数字的父节点
    public TextMeshProUGUI anomalyText;    // 屏幕中央用于闪烁红字的文本框
    public GameObject staticBackground;    // 你的雪花噪点背景

    [Header("音效组件")]
    public AudioSource tvAudioSource;      // 播放白噪音的那个 AudioSource
    public AudioClip loudStaticSFX;        // 刺耳的静电峰值音效
    public AudioClip clockChimeSFX;        // 沉重的座钟钟声

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlaySequence()
    {
        StartCoroutine(AnomalyRoutine());
    }

    private IEnumerator AnomalyRoutine()
    {
        // 1. 瞬间抹除所有调频 UI，只留雪花背景
        tuningControlsGroup.SetActive(false);
        anomalyText.text = "";
        anomalyText.gameObject.SetActive(true);

        // 音效突变：播放刺耳静电
        if (loudStaticSFX != null && tvAudioSource != null)
            tvAudioSource.PlayOneShot(loudStaticSFX);

        yield return new WaitForSeconds(1.5f);

        // 2. 第一段闪烁：T I C K
        staticBackground.SetActive(false); // 关掉雪花，屏幕纯黑
        anomalyText.text = "<color=#8B0000>T I C K</color>"; // 血液般的深红色
        yield return new WaitForSeconds(0.2f);

        // 恢复雪花
        staticBackground.SetActive(true);
        anomalyText.text = "";
        yield return new WaitForSeconds(0.8f);

        // 3. 第二段闪烁：T O C K
        staticBackground.SetActive(false);
        anomalyText.text = "<color=#8B0000>T O C K</color>";
        yield return new WaitForSeconds(0.2f);

        // 4. 彻底死寂
        staticBackground.SetActive(false);
        anomalyText.text = "";
        if (tvAudioSource != null) tvAudioSource.Stop(); // 切断所有静电音

        yield return new WaitForSeconds(1.0f);

        // 5. 异象终结：座钟钟声
        if (clockChimeSFX != null && tvAudioSource != null)
            tvAudioSource.PlayOneShot(clockChimeSFX);

        // 给玩家 2 秒钟时间在黑暗中听钟声回荡
        yield return new WaitForSeconds(2.0f);

        // 6. 物理权限移交与系统还原
        EndSequence();
    }

    private void EndSequence()
    {
        // 调用 Tuning 中枢关闭整个电视界面
        if (UI_TVTuning.Instance != null)
            UI_TVTuning.Instance.CloseTVTuning();

        // 恢复调频 UI 的初始状态（为以后可能再次打开做准备）
        tuningControlsGroup.SetActive(true);
        anomalyText.gameObject.SetActive(false);
        staticBackground.SetActive(true);

        // 移交物理权限给座钟
        Core_MonologueManager.Instance.ShowMonologue("That sound... The tall clock in the corner of the room.");
        GameObject clock = GameObject.Find("TallClock");
        if (clock != null)
        {
            clock.tag = "Operable";
        }
        else
        {
            Debug.LogError("【物理断层】场景中未找到名为 TallClock 的实体！");
        }
    }
}