using UnityEngine;
using System.Collections;

public class Logic_MainDoor : MonoBehaviour
{
    [Header("纸条实体与动画")]
    public GameObject noteObject;     // 场景中初始隐藏的纸条
    public float slideDistance = 0.3f; // X 轴弹出距离
    public float slideDuration = 1.0f; // 弹出动画时长

    [Header("音效配置")]
    public AudioClip lockedSound;     // 门锁死的音效

    private bool firstClickTriggered = false;

    public void OnInteract()
    {
        if (firstClickTriggered) return;
        firstClickTriggered = true;

        // 1. 物理音效反馈
        if (lockedSound != null && Audio_SoundManager.Instance != null)
        {
            Audio_SoundManager.Instance.PlaySFX2D(lockedSound);
        }

        // 2. 独白序列
        Core_MonologueManager.Instance.ShowMonologue("The door is bolted from the outside. I can't leave...");

        // 3. 延迟触发纸条弹出
        Invoke("TriggerNoteSpawn", 2.0f);
    }

    private void TriggerNoteSpawn()
    {
        if (noteObject != null)
        {
            noteObject.SetActive(true);
            // 启动纸条平移动画协程
            StartCoroutine(SlideNoteRoutine());
            
            Core_MonologueManager.Instance.ShowMonologue("Wait... I saw something sliding through the crack.");

            // 4. 强制焦点转移：现在全场景只能点这张纸条 (修复 Unity 6 警告)
            Core_Raycaster raycaster = Object.FindFirstObjectByType<Core_Raycaster>();
            if (raycaster != null) raycaster.focusTarget = noteObject;
        }
    }

    private IEnumerator SlideNoteRoutine()
    {
        Vector3 startPos = noteObject.transform.position;
        // 仅在 X 轴产生位移量，保持 Y 和 Z 不变
        Vector3 endPos = startPos - new Vector3(slideDistance, 0, 0); 
        
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            noteObject.transform.position = Vector3.Lerp(startPos, endPos, elapsed / slideDuration);
            yield return null;
        }
        noteObject.transform.position = endPos;
    }
}