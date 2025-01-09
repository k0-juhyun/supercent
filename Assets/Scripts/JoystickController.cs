using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 조이스틱 컨트롤
public class JoystickController : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler
{
    public RectTransform _joystickBackground;
    public RectTransform _joystickHandle;
    public float _joystickRaidus = 250f;

    [SerializeField] private Vector2 _inputDir; // 입력 방향

    // 조이스틱 핸들 이동
    public void OnDrag(PointerEventData eventData)
    {
        UpdateJoystickPos(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateJoystickPos(eventData);
    }

    // 터치 종료시 조이스틱 초기화
    public void OnPointerUp(PointerEventData eventData)
    {
        _joystickHandle.anchoredPosition = Vector2.zero;
        _inputDir = Vector2.zero;
    }
    
    public Vector2 GetInputDir()
    {
        return _inputDir;
    }

    // 조이스틱을 움직이는데 백그라운드 안에서만
    private void UpdateJoystickPos(PointerEventData eventData)
    {
        Vector2 _touchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_joystickBackground, eventData.position, eventData.pressEventCamera, out _touchPos);

        // 터치 위치가 제한 되도록
        _inputDir = Vector2.ClampMagnitude(_touchPos,_joystickRaidus) / _joystickRaidus;

        // 손잡이 위치가 입력방향
        _joystickHandle.anchoredPosition = _inputDir * _joystickRaidus;
    }
}
