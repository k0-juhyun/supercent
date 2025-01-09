using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

[RequireComponent(typeof(CharacterController))] // 캐릭터 컨트롤러 필수
public class PlayerInteraction : MonoBehaviour
{
    private CharacterController _characterController; // 캐릭터 컨트롤러
    public Transform[] _stackPositions; // 빵 스택 위치 배열 (최대 8칸)
    public Animator _playerAnimator; // 플레이어 애니메이터
    public int _maxStackSize = 8; // 최대 스택 크기
    public Stack<GameObject> _stackedBreads = new Stack<GameObject>(); // 현재 스택된 빵들 (FILO)

    private BreadFac _breadFac; // BreadFac 참조
    private bool _isOnOvenPlane = false; // OvenPlane 위에 있는지 여부

    // 빵 상태 변화 이벤트
    public event System.Action<bool> OnBreadStateChanged;

    private void Awake()
    {
        // 캐릭터 컨트롤러 초기화
        _characterController = GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OvenPlane"))
        {
            _isOnOvenPlane = true; // OvenPlane 위에 있음
            Debug.Log("Player entered OvenPlane.");

            // OvenPlane의 BreadFac 찾기
            _breadFac = other.GetComponentInChildren<BreadFac>();
            if (_breadFac != null)
            {
                Debug.Log("BreadFac found.");
                StartCoroutine(CollectBreadFromFac());
            }
            else
            {
                Debug.LogError("BreadFac not found in OvenPlane.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OvenPlane"))
        {
            _isOnOvenPlane = false; // OvenPlane에서 벗어남
            Debug.Log("Player exited OvenPlane.");
        }
    }

    private IEnumerator CollectBreadFromFac()
    {
        if (_breadFac == null)
        {
            Debug.LogError("BreadFac is not set.");
            yield break;
        }

        int spaceLeft = _maxStackSize - _stackedBreads.Count;
        if (spaceLeft <= 0)
        {
            Debug.Log("Stack is already full.");
            yield break;
        }

        List<GameObject> breadsToCollect = _breadFac.GetBreadsFromFac(spaceLeft);

        foreach (GameObject bread in breadsToCollect)
        {
            if (_stackedBreads.Count >= _maxStackSize)
            {
                Debug.LogWarning("Stack is full. Stopping collection.");
                yield break;
            }

            Transform targetPosition = _stackPositions[_stackedBreads.Count];
            GameObject stackBread = targetPosition.GetChild(0).gameObject;

            bool isBreadAddedToStack = false;
            bread.transform
                .DOMove(targetPosition.position, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    stackBread.SetActive(true);
                    _stackedBreads.Push(stackBread);
                    _breadFac.ReturnBreadToPool(bread);
                    isBreadAddedToStack = true;

                    // 빵 상태 변화 이벤트 호출
                    OnBreadStateChanged?.Invoke(_stackedBreads.Count > 0);
                });

            _playerAnimator.SetTrigger("Stack_Idle");

            yield return new WaitUntil(() => isBreadAddedToStack);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void RemoveBreadFromStack()
    {
        if (_stackedBreads.Count > 0)
        {
            GameObject bread = _stackedBreads.Pop();
            bread.SetActive(false);

            // 빵 상태 변화 이벤트 호출
            OnBreadStateChanged?.Invoke(_stackedBreads.Count > 0);
        }
    }
}
