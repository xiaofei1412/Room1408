using UnityEngine;
using UnityEngine.EventSystems;

// 继承 UI 拖拽接口
public class UI_TVDial : MonoBehaviour, IDragHandler
{
    [Header("拖拽物理映射")]
    public float dragSensitivity = 2.0f; // 鼠标滑动多少像素触发一次频道跳动
    public float visualRotationMultiplier = 5.0f; // 齿轮UI随之旋转的倍数

    private float accumulatedDelta = 0f;

    // 当鼠标按住该 UI 元素并移动时持续触发
    public void OnDrag(PointerEventData eventData)
    {
        if (UI_TVTuning.Instance == null) return;

        // 提取鼠标 X 轴的滑动增量
        accumulatedDelta += eventData.delta.x; 

        // 物理阈值判断：累积位移是否超过了设定灵敏度
        if (Mathf.Abs(accumulatedDelta) >= dragSensitivity)
        {
            // 计算需要跳动多少步
            int steps = Mathf.FloorToInt(accumulatedDelta / dragSensitivity);
            
            // 将步数传递给中枢进行粗调 (+/-)
            UI_TVTuning.Instance.CoarseTune(steps);
            
            // 物理反馈：让齿轮 UI 本身旋转
            transform.Rotate(Vector3.forward, -steps * visualRotationMultiplier); 

            // 扣除已消耗的位移，保留余数防止手感断层
            accumulatedDelta -= steps * dragSensitivity;
        }
    }
}