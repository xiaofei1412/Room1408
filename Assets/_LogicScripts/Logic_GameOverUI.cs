using UnityEngine;
using UnityEngine.SceneManagement;

public class Logic_GameOverUI : MonoBehaviour
{
    private void Start()
    {
        // 确保鼠标光标显示出来并解锁，否则玩家点不到按钮
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 将这个方法绑定到你创建的 Restart 按钮的 OnClick() 事件上
    public void RestartGame()
    {
        // 彻底清空结局标记，以全新的状态重载主场景
        PlayerPrefs.SetInt("EndingType", 0);
        PlayerPrefs.Save();
        
        // 切回主场景
        SceneManager.LoadScene("Main_1408"); 
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}