using UnityEngine;

public class Logic_SafeInteract : MonoBehaviour
{
    public void OnInteract()
    {
        if (UI_SafeKeypad.Instance != null)
        {
            UI_SafeKeypad.Instance.OpenKeypad();
        }
    }
}