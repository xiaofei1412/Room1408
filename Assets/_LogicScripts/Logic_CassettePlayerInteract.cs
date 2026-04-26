using UnityEngine;
using System.Collections;

public class Logic_CassettePlayerInteract : MonoBehaviour
{
    private bool hasTapePlayed = false;
    private AudioSource audioSource;
    public AudioClip tapePlaySFX; // 磁带插入并播放的诡异音效
    public AudioClip whisperSFX;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // 由 Core_Raycaster 触发
    public void OnInteractWithTape()
    {
        if (hasTapePlayed) return;

        // 1. 验证玩家手里是否拿着磁带
        ItemData selectedItem = Core_InventoryManager.Instance.GetSelectedItem();
        if (selectedItem != null && selectedItem.itemName == "Cassette") 
        {
            hasTapePlayed = true;

            // 2. 消耗背包里的磁带
            Core_InventoryManager.Instance.RemoveItem("Cassette");

            // 3. 播放音效与独白
            if (tapePlaySFX != null) audioSource.PlayOneShot(tapePlaySFX);
            if (whisperSFX != null) audioSource.PlayOneShot(whisperSFX); // 播放单次耳语
            Core_MonologueManager.Instance.ShowMonologue("The tape clicked into place. A voice... raspy and cold.");

            // 4. 触发终局演出序列
            StartCoroutine(EndgameSequence());
        }
        else
        {
            // 如果玩家空手或拿着其他东西点录音机，可以将其重新收入背包
            Logic_ItemPickup pickup = GetComponent<Logic_ItemPickup>();
            if (pickup != null && pickup.itemData != null)
            {
                if (Core_InventoryManager.Instance.AddItem(pickup.itemData))
                    gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator EndgameSequence()
    {
        // 留白时间让音效播放
        yield return new WaitForSeconds(3.0f);

        // 5. 屏幕显现终局血字 (隐喻指向床垫)
        string message = "THE TRUTH SLEEPS BENEATH THE DREAMER.\nMOVE THE STONE.\nENTER THE VOID.";
        if (Logic_EndingText.Instance != null)
        {
            Logic_EndingText.Instance.ShowEndingText(message);
        }

        // 6. 物理权限解锁：解除床垫的封锁
        GameObject mattress = GameObject.Find("SM_Bed_Mattress");
        if (mattress != null)
        {
            mattress.tag = "Operable";
            // 剥离录音机自身的交互权限
            gameObject.tag = "Untagged";
        }
        else
        {
            Debug.LogError("【物理断层】未找到 SM_Bed_Mattress，请检查命名！");
        }
    }
}