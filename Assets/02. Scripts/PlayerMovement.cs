using UnityEngine;
using DG.Tweening; // DOTween ���ӽ����̽� �߰�

[RequireComponent(typeof(CharacterController))] // ĳ���� ��Ʈ�ѷ� �ʼ�
public class PlayerMovement : MonoBehaviour
{
    public JoystickManager _joystickManager; // ���̽�ƽ �Ŵ��� ����
    public float _moveSpeed = 5f; // �̵� �ӵ�
    public float _rotationDuration = 0.2f; // ȸ�� �ִϸ��̼� ���� �ð� (��)

    private CharacterController _characterController; // ĳ���� ��Ʈ�ѷ�
    private Vector3 _moveDirection; // ���� �̵� ����

    private void Awake()
    {
        // ĳ���� ��Ʈ�ѷ� �ʱ�ȭ
        _characterController = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        // ���̽�ƽ �Է� ���� ��������
        Vector2 inputDirection = _joystickManager.InputDirection;

        // �Է��� ���� ��� �̵����� ����
        if (inputDirection == Vector2.zero)
        {
            _moveDirection = Vector3.zero; // �̵� ����
            return;
        }

        // �Է� ���� ���
        _moveDirection = new Vector3(inputDirection.x, 0, inputDirection.y).normalized * _moveSpeed;

        // �̵� ó��
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);

        // ĳ���� ȸ�� ó��
        HandleRotation(_moveDirection);
    }

    // ĳ���� ȸ�� ó��
    private void HandleRotation(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero) return;

        // ��ǥ ȸ�� �� ���
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        // DOTween�� ����� �ε巯�� ȸ��
        transform.DORotateQuaternion(targetRotation, _rotationDuration);
    }
}
