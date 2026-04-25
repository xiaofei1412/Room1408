using UnityEngine;
using System.Collections;

public class Logic_MoonDialSequence : MonoBehaviour
{
    [Header("机械运动参数")]
    public float rotationDuration = 3.0f; // 翻转耗时
    public float startAngleX = -90f;
    public float targetAngleX = -270f;

    [Header("音效")]
    public AudioClip gearGrindSFX; // 沉重的齿轮摩擦音效
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayRevealSequence()
    {
        StartCoroutine(RotateDialRoutine());
    }

    private IEnumerator RotateDialRoutine()
    {
        if (gearGrindSFX != null) audioSource.PlayOneShot(gearGrindSFX);

        float timeElapsed = 0f;

        while (timeElapsed < rotationDuration)
        {
            // 严格基于 X 轴的线性插值运算
            float currentX = Mathf.Lerp(startAngleX, targetAngleX, timeElapsed / rotationDuration);
            
            // 驱动局部欧拉角
            transform.localEulerAngles = new Vector3(currentX, 0f, 0f);
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 强行对齐最终物理角度
        transform.localEulerAngles = new Vector3(targetAngleX, 0f, 0f);

        // 物理权限移交：激活暗格中的钥匙
        GameObject keyObj = GameObject.Find("Prop_Key");
        if (keyObj != null) keyObj.tag = "Pickable";

        // 物理权限移交：激活暗格中的座钟纸条
        GameObject noteObj = GameObject.Find("Note_Clock");
        if (noteObj != null) noteObj.tag = "Readable";

        // 统一推送独白
        Core_MonologueManager.Instance.ShowMonologue("A hidden compartment... a key, and another note.");
    }
}