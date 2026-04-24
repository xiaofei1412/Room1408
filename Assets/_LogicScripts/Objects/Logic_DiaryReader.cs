using UnityEngine;

public class Logic_DiaryReader : MonoBehaviour
{
    [SerializeField] private string textID;
    [SerializeField] private Logic_SanityTrigger sanityTrigger;

    private bool hasRead = false;

    public void Read()
    {
        if (hasRead) return;

        Debug.Log("读取文本: " + textID);

        sanityTrigger.TriggerSanityLoss();

        hasRead = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Read();
        }
    }
}
