using UnityEngine;

public class Logic_ShowerSequence : MonoBehaviour
{
    public void OnDiscoverSafe()
    {
        if (Core_MonologueManager.Instance != null)
        {
            // 全英文独白推送：好奇与恐惧并存
            Core_MonologueManager.Instance.ShowMonologue("Wait... What is this? A safe hidden in the shower?");
            Core_MonologueManager.Instance.ShowMonologue("What is the passcode? Could it be...");
            Core_MonologueManager.Instance.ShowMonologue("I feel a cold shiver. Why would someone hide this here?");
        }

        // 解锁保险箱交互权限
        GameObject safeObj = GameObject.Find("SM_Safe");
        if (safeObj != null)
        {
            safeObj.tag = "Operable"; // 允许玩家点击保险箱开启密码锁 UI
        }
    }
}