using UnityEngine;

public class Logic_MirrorInteract : MonoBehaviour
{
    public void OnInteract()
    {
        if (UI_MirrorCloseup.Instance != null)
        {
            UI_MirrorCloseup.Instance.StartMirrorInteraction();
        }
    }
}