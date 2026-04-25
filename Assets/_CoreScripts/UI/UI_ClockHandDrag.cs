using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ClockHandDrag : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("配置")]
    public bool isHourHand; // 若挂在时针上请勾选，分针不勾
    
    private Vector2 lastMousePos;
    private RectTransform parentRect;

    private void Start()
    {
        // 获取父级容器（通常是钟面背景）作为计算坐标系的基础
        parentRect = transform.parent as RectTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 记录鼠标按下的初始极坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out lastMousePos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (UI_ClockPuzzle.Instance == null) return;

        // 获取当前的局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out Vector2 currentMousePos);

        // 计算带符号的角度差值（核心极坐标数学计算）
        float angleDelta = Vector2.SignedAngle(lastMousePos, currentMousePos);
        
        // 在 2D 直角坐标系中，SignedAngle 计算顺时针是负值。我们将其取反，统一为“顺时针增加”的物理逻辑
        float clockwiseAngleToAdd = -angleDelta;

        // 设定微小的防抖死区
        if (Mathf.Abs(clockwiseAngleToAdd) > 0.1f)
        {
            // 将角度变化传回中枢系统
            UI_ClockPuzzle.Instance.AddRotation(isHourHand, clockwiseAngleToAdd);
            
            // 刷新锚点
            lastMousePos = currentMousePos;
        }
    }
}