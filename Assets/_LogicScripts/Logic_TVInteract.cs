using UnityEngine;

public class Logic_TVInteract : MonoBehaviour
{
    public void OnInteract()
    {
        if (UI_TVTuning.Instance != null)
        {
            UI_TVTuning.Instance.OpenTVTuning();
        }
    }
}