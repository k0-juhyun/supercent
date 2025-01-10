using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class Customer : MonoBehaviour
{
    public int DesiredBreadCount { get; private set; } // 손님이 원하는 빵 갯수
    public int CollectedBreadCount { get; private set; } // 손님이 수집한 빵 갯수

    [SerializeField] private Transform[] _stackPositions; // 빵 스택 위치
    private Stack<GameObject> _collectedBreads = new Stack<GameObject>(); // 수집한 빵
    private Animator _animator; // 애니메이터
    private bool _isCollectingBreads = false; // 빵 수집 중인지 여부
    private Coroutine _currentMoveCoroutine; // 현재 이동 코루틴

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Initialize(int breadCount)
    {
        DesiredBreadCount = breadCount;
        CollectedBreadCount = 0;
        _collectedBreads.Clear();
        ResetStackPositions();
        UpdateAnimatorState(false, false);
    }

    public void MoveTo(Transform target, System.Action onReachTarget)
    {
        if (_currentMoveCoroutine != null)
        {
            StopCoroutine(_currentMoveCoroutine);
        }
        _currentMoveCoroutine = StartCoroutine(MoveToTarget(target, onReachTarget));
    }

    private IEnumerator MoveToTarget(Transform target, System.Action onReachTarget)
    {
        UpdateAnimatorState(true, CollectedBreadCount > 0);

        while (Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * 2f);
            transform.LookAt(target.position); // 이동 방향으로 바라보게 설정
            yield return null;
        }

        UpdateAnimatorState(false, CollectedBreadCount > 0);

        onReachTarget?.Invoke();
    }

    public IEnumerator StartCollectingCoroutine(StallManager stallManager)
    {
        _isCollectingBreads = true;

        while (CollectedBreadCount < DesiredBreadCount)
        {
            GameObject breadToCollect = stallManager.GetAvailableBread();
            if (breadToCollect == null)
            {
                yield return new WaitForSeconds(1f); // 빵이 없으면 대기
                continue;
            }

            Transform targetPosition = _stackPositions[CollectedBreadCount];
            GameObject stackBread = targetPosition.GetChild(0).gameObject;

            breadToCollect.transform
                .DOMove(targetPosition.position, 1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    breadToCollect.SetActive(false);
                    stackBread.SetActive(true);
                    _collectedBreads.Push(stackBread);
                    CollectedBreadCount++;

                    stallManager.ReleaseBread(breadToCollect); // 빵 점유 해제
                    UpdateAnimatorState(false, CollectedBreadCount > 0);
                });

            yield return new WaitUntil(() => stackBread.activeSelf);
            yield return new WaitForSeconds(0.15f); // 간격 추가
        }

        _isCollectingBreads = false;

        UpdateAnimatorState(true, CollectedBreadCount > 0);
    }



    private GameObject FindActiveBreadInStall(Transform stallPlane)
    {
        Debug.Log("활성화된 빵 찾기");
        for (int i = 0; i < stallPlane.childCount; i++)
        {
            Transform stallPos = stallPlane.GetChild(i);
            if (stallPos.childCount > 0)
            {
                Debug.Log("가판대에 빵있음");
                GameObject bread = stallPos.GetChild(0).gameObject;
                if (bread.activeSelf)
                {
                    return bread;
                }
            }
        }
        return null;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("StallPlane") && !_isCollectingBreads && CollectedBreadCount < DesiredBreadCount)
        {
            StallManager stallManager = other.GetComponent<StallManager>();
            if (stallManager != null)
            {
                StartCoroutine(StartCollectingCoroutine(stallManager));
            }
        }
    }


    private IEnumerator CollectBreadFromStall(Transform stallPlane)
    {
        _isCollectingBreads = true;

        while (CollectedBreadCount < DesiredBreadCount)
        {
            GameObject breadToCollect = FindActiveBreadInStall(stallPlane);
            if (breadToCollect == null)
            {
                yield return new WaitForSeconds(1f); // 대기하면서 반복
                continue;
            }

            // 빵 수집 처리
            Transform targetPosition = _stackPositions[CollectedBreadCount];
            GameObject stackBread = targetPosition.GetChild(0).gameObject;

            breadToCollect.transform
                .DOMove(targetPosition.position, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    breadToCollect.SetActive(false);
                    stackBread.SetActive(true);
                    _collectedBreads.Push(stackBread);
                    CollectedBreadCount++;

                    UpdateAnimatorState(false, CollectedBreadCount > 0);
                });

            yield return new WaitUntil(() => stackBread.activeSelf);
            yield return new WaitForSeconds(0.15f); // 간격 추가
        }

        _isCollectingBreads = false;

        UpdateAnimatorState(true, CollectedBreadCount > 0);
    }

    private void ResetStackPositions()
    {
        foreach (Transform stackPosition in _stackPositions)
        {
            if (stackPosition.childCount > 0)
            {
                stackPosition.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    private void UpdateAnimatorState(bool isMoving, bool hasBread)
    {
        _animator.SetBool("IsMoving", isMoving);
        _animator.SetBool("HasBread", hasBread);
    }
}
