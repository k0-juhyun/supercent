using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class JoystickManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public GameObject _joystickPrefab;
    public GameObject _touchPointPrefab;
    public Animator _playerAnimator;
    public PlayerInteraction _playerInteraction;

    private GameObject _joystickInstance;
    private GameObject _touchPointInstance;
    private RectTransform _joystickBase;
    private RectTransform _joystickKnob;

    private Vector2 _inputDirection;
    public float _joystickRadius = 100f;
    public float _knobMoveDuration = 0.2f;

    public Vector2 InputDirection => _inputDirection;

    public void OnPointerDown(PointerEventData eventData)
    {
        CreateJoystick(eventData.position, eventData.pressEventCamera);
        CreateTouchPoint(eventData.position, eventData.pressEventCamera);
        UpdateMoveAnimation(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_joystickInstance == null || _touchPointInstance == null) return;

        UpdateTouchPoint(eventData.position, eventData.pressEventCamera);
        UpdateJoystick();
        UpdateMoveAnimation(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DestroyJoystick();
        DestroyTouchPoint();
        UpdateMoveAnimation(false);
    }

    private void CreateJoystick(Vector2 screenPosition, Camera eventCamera)
    {
        if (_joystickInstance != null)
            Destroy(_joystickInstance);

        _joystickInstance = Instantiate(_joystickPrefab, transform);
        _joystickBase = _joystickInstance.transform.Find("Base").GetComponent<RectTransform>();
        _joystickKnob = _joystickBase.transform.Find("Knob").GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform, screenPosition, eventCamera, out Vector2 localPoint);

        _joystickBase.anchoredPosition = localPoint;
        _joystickKnob.anchoredPosition = Vector2.zero;
    }

    private void CreateTouchPoint(Vector2 screenPosition, Camera eventCamera)
    {
        if (_touchPointInstance != null)
            Destroy(_touchPointInstance);

        _touchPointInstance = Instantiate(_touchPointPrefab, transform);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform, screenPosition, eventCamera, out Vector2 localPoint);

        _touchPointInstance.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }

    private void UpdateTouchPoint(Vector2 screenPosition, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform, screenPosition, eventCamera, out Vector2 localPoint);

        _touchPointInstance.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }

    private void UpdateJoystick()
    {
        Vector2 direction = _touchPointInstance.GetComponent<RectTransform>().anchoredPosition - _joystickBase.anchoredPosition;
        Vector2 clampedDirection = Vector2.ClampMagnitude(direction, _joystickRadius);
        _inputDirection = clampedDirection / _joystickRadius;

        _joystickKnob.DOAnchorPos(clampedDirection, _knobMoveDuration).SetEase(Ease.Linear);
    }

    private void DestroyJoystick()
    {
        if (_joystickInstance != null)
            Destroy(_joystickInstance);

        _inputDirection = Vector2.zero;
    }

    private void DestroyTouchPoint()
    {
        if (_touchPointInstance != null)
            Destroy(_touchPointInstance);
    }

    private void UpdateMoveAnimation(bool isMoving)
    {
        _playerAnimator.SetBool("IsMoving", isMoving);
    }
}
