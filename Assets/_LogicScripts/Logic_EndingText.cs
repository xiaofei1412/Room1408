using UnityEngine;
using TMPro;
using System.Collections;

public class Logic_EndingText : MonoBehaviour
{
    public static Logic_EndingText Instance;
    
    public TextMeshProUGUI bloodText;
    public float typingSpeed = 0.1f;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowEndingText(string message)
    {
        bloodText.text = "";
        StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        foreach (char letter in message.ToCharArray())
        {
            bloodText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void ClearText()
    {
        if (bloodText != null)
        {
            bloodText.text = "";
        }
        gameObject.SetActive(false); // 直接关闭整个面板
    }
}