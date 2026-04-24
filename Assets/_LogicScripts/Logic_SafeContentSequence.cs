using UnityEngine;

public class Logic_SafeContentSequence : MonoBehaviour
{
    public void OnSafeDoorOpened()
    {
        Core_MonologueManager.Instance.ShowMonologue("A cassette player... and another note.");
        
        // 1. 激活录音机
        GameObject playerObj = GameObject.Find("Prop_CassettePlayer");
        if (playerObj != null) 
        {
            playerObj.tag = "Inspectable"; 
            SetLayerRecursive(playerObj, 0); // 恢复 Default 图层
        }

        // 2. 激活保险箱纸条
        GameObject safeNote = GameObject.Find("Note_Safe");
        if (safeNote != null)
        {
            safeNote.tag = "Readable";
            SetLayerRecursive(safeNote, 0);
        }
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform) SetLayerRecursive(child.gameObject, layer);
    }
}