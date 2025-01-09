using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ���̽�ƽ ��Ʈ��
public class JoystickController : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler
{
    public RectTransform _joystickBackground;
    public RectTransform _joystickHandle;
    public float _joystickRaidus = 250f;

    [SerializeField] private Vector2 _inputDir; // �Է� ����

    // ���̽�ƽ �ڵ� �̵�
    public void OnDrag(PointerEventData eventData)
    {
        UpdateJoystickPos(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateJoystickPos(eventData);
    }

    // ��ġ ����� ���̽�ƽ �ʱ�ȭ
    public void OnPointerUp(PointerEventData eventData)
    {
        _joystickHandle.anchoredPosition = Vector2.zero;
        _inputDir = Vector2.zero;
    }
    
    public Vector2 GetInputDir()
    {
        return _inputDir;
    }

    // ���̽�ƽ�� �����̴µ� ��׶��� �ȿ�����
    private void UpdateJoystickPos(PointerEventData eventData)
    {
        Vector2 _touchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_joystickBackground, eventData.position, eventData.pressEventCamera, out _touchPos);

        // ��ġ ��ġ�� ���� �ǵ���
        _inputDir = Vector2.ClampMagnitude(_touchPos,_joystickRaidus) / _joystickRaidus;

        // ������ ��ġ�� �Է¹���
        _joystickHandle.anchoredPosition = _inputDir * _joystickRaidus;
    }
}
