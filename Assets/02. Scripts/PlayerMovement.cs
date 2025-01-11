using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

[RequireComponent(typeof(CharacterController))] // 캐릭터 컨트롤러 필수
public class PlayerMovement : MonoBehaviour
{
    public JoystickManager _joystickManager; // 조이스틱 매니저 참조
    public float _moveSpeed = 5f; // 이동 속도
    public float _rotationDuration = 0.2f; // 회전 애니메이션 지속 시간 (초)

    private CharacterController _characterController; // 캐릭터 컨트롤러
    private Vector3 _moveDirection; // 현재 이동 방향

    private void Awake()
    {
        // 캐릭터 컨트롤러 초기화
        _characterController = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        // 조이스틱 입력 방향 가져오기
        Vector2 inputDirection = _joystickManager.InputDirection;

        // 입력이 없는 경우 이동하지 않음
        if (inputDirection == Vector2.zero)
        {
            _moveDirection = Vector3.zero; // 이동 멈춤
            return;
        }

        // 입력 방향 계산
        _moveDirection = new Vector3(inputDirection.x, 0, inputDirection.y).normalized * _moveSpeed;

        // 이동 처리
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);

        // 캐릭터 회전 처리
        HandleRotation(_moveDirection);
    }

    // 캐릭터 회전 처리
    private void HandleRotation(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero) return;

        // 목표 회전 값 계산
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        // DOTween을 사용한 부드러운 회전
        transform.DORotateQuaternion(targetRotation, _rotationDuration);
    }
}
