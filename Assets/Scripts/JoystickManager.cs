using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class JoystickManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public GameObject _joystickPrefab; // ���̽�ƽ ������
    public GameObject _touchPointPrefab; // ��ġ ����Ʈ ������
    public Animator _player; // �÷��̾� �ִϸ�����
    public PlayerInteraction _playerInteraction; // �÷��̾� ��ȣ�ۿ� ����

    private GameObject _joystickInstance; // ���� Ȱ��ȭ�� ���̽�ƽ
    private GameObject _touchPointInstance; // ���� Ȱ��ȭ�� ��ġ ����Ʈ
    private RectTransform _joystickBase; // ���̽�ƽ Base
    private RectTransform _joystickKnob; // ���̽�ƽ Knob

    private string _currentAnimationState; // ���� �ִϸ��̼� ����
    public float _joystickRadius = 100f; // ���̽�ƽ �ݰ�
    public float _knobMoveDuration = 0.2f; // Knob �̵� �ӵ�
    private Vector2 _inputDirection; // �Է� ����

    public Vector2 InputDirection => _inputDirection;

    private void Start()
    {
        // �� ���� ��ȭ �̺�Ʈ ����
        _playerInteraction.OnBreadStateChanged += UpdateIdleAnimation;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        _playerInteraction.OnBreadStateChanged -= UpdateIdleAnimation;
    }

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
        _joystickBase = _joystickInstance.transform.Find("Base")?.GetComponent<RectTransform>();
        _joystickKnob = _joystickBase.transform.Find("Knob")?.GetComponent<RectTransform>();

        if (_joystickBase == null || _joystickKnob == null)
        {
            Debug.LogError("Base �Ǵ� Knob�� ã�� �� �����ϴ�.");
            Destroy(_joystickInstance);
            return;
        }

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
        if (_joystickBase == null || _joystickKnob == null || _touchPointInstance == null) return;

        Vector2 direction = _touchPointInstance.GetComponent<RectTransform>().anchoredPosition - _joystickBase.anchoredPosition;
        Vector2 clampedDirection = Vector2.ClampMagnitude(direction, _joystickRadius);
        _inputDirection = clampedDirection / _joystickRadius;

        if (_joystickKnob != null)
        {
            _joystickKnob.DOAnchorPos(clampedDirection, _knobMoveDuration).SetEase(Ease.Linear);
        }
    }

    private void DestroyJoystick()
    {
        if (_joystickKnob != null)
        {
            DOTween.Kill(_joystickKnob);
        }

        if (_joystickInstance != null)
        {
            Destroy(_joystickInstance);
        }

        _inputDirection = Vector2.zero;
    }

    private void DestroyTouchPoint()
    {
        if (_touchPointInstance != null)
        {
            DOTween.Kill(_touchPointInstance.GetComponent<RectTransform>());
            Destroy(_touchPointInstance);
        }
    }

    private void UpdateMoveAnimation(bool isMoving)
    {
        string targetState = isMoving
            ? (_playerInteraction._stackedBreads.Count > 0 ? "Stack_Move" : "Default_Move")
            : (_playerInteraction._stackedBreads.Count > 0 ? "Stack_Idle" : "Default_Idle");

        if (_currentAnimationState == targetState) return;
        _currentAnimationState = targetState;

        _player.SetTrigger(targetState);
    }

    private void UpdateIdleAnimation(bool hasBread)
    {
        string targetState = hasBread ? "Stack_Idle" : "Default_Idle";

        if (_currentAnimationState == targetState) return;
        _currentAnimationState = targetState;

        _player.SetTrigger(targetState);
    }
}
