using UnityEngine;
using TMPro;

/// <summary>
/// 理智 HUD 显示
/// </summary>
public class Logic_SanityHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text sanityText;

    private void Start()
    {
        Core_SanityManager.Instance.OnSanityChanged += UpdateUI;
        UpdateUI(Core_SanityManager.Instance.GetSanity());
    }

    private void UpdateUI(int value)
    {
        sanityText.text = "Sanity: " + value;
    }

    private void OnDestroy()
    {
        if (Core_SanityManager.Instance != null)
        {
            Core_SanityManager.Instance.OnSanityChanged -= UpdateUI;
        }
    }
}