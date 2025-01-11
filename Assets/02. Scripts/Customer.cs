// Customer.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using DG.Tweening;
using System.Collections.Generic;

public class Customer : MonoBehaviour
{
    public int DesiredBreadCount { get; private set; } // 손님이 원하는 빵 갯수
    public int CollectedBreadCount { get; private set; } // 손님이 수집한 빵 갯수

    [SerializeField] private Transform[] _stackPositions; // 빵 스택 위치
    private Stack<GameObject> _collectedBreads = new Stack<GameObject>(); // 수집한 빵
    public List<GameObject> CollectedBreads { get; private set; } = new List<GameObject>();
    private Animator _animator; // 애니메이터
    private Coroutine _currentMoveCoroutine; // 현재 이동 코루틴
    public bool IsPaymentComplete { get; private set; } = false;
    public Transform InitialBreadPosition { get; set; }

    public GameObject _paperBag;
    public event System.Action OnPaymentCompleted;
    private bool _isPaidFlag;
    private bool _isProcessFlag = false;
    public bool IsPaid
    {

        get => _isPaidFlag;
        set
        {
            if(_isPaidFlag != value)
            {
                _isPaidFlag = value;
                // 값 변경시 이벤트 호출하자
                if(_isPaidFlag) OnPaymentCompleted?.Invoke(); 
            }
        }
    }
    public bool _payFlag = false;

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
        _isPaidFlag = false;
        _paperBag.SetActive(false);
        _isPaidFlag = false;
    }

    public void MoveTo(Transform target, System.Action onReachTarget)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

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

    public void StartCollecting(StallManager stallManager, Transform stallTransform, System.Action onCollectionComplete)
    {
        stallManager.RequestBreadCollection(() =>
        {
            StartCoroutine(StartCollectingCoroutine(stallManager, stallTransform, onCollectionComplete));
        });
    }

    public IEnumerator StartCollectingCoroutine(StallManager stallManager, Transform stallTransform, System.Action onCollectionComplete)
    {
        // Stall 바라보기 설정
        transform.LookAt(stallTransform.position);
        UpdateAnimatorState(false, CollectedBreadCount > 0); // 이동 중이 아님, 현재 빵 상태 반영

        while (CollectedBreadCount < DesiredBreadCount)
        {
            GameObject breadToCollect = stallManager.GetAvailableBread();
            if (breadToCollect == null)
            {
                yield return new WaitForSeconds(1f); // 빵이 없으면 대기
                continue;
            }

            // 스택 추가 완료를 확인하기 위한 플래그
            bool isBreadAddedToStack = false;

            // 스택 포지션 할당
            Transform targetPosition = _stackPositions[CollectedBreadCount];

            breadToCollect.transform
                .DOMove(transform.position, 0.5f) // 손님에게 먼저 이동
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // 빵을 스택에 추가
                    _collectedBreads.Push(breadToCollect);
                    breadToCollect.SetActive(false);

                    // CollectedBreadCount를 기반으로 StackPosition의 빵 활성화
                    GameObject stackBread = targetPosition.GetChild(0).gameObject;
                    stackBread.SetActive(true);

                    // CollectedBreadCount 증가
                    CollectedBreadCount++;

                    // StallManager에서 빵 점유 해제
                    stallManager.ReleaseBread(breadToCollect);

                    // 추가 완료 플래그 설정
                    isBreadAddedToStack = true;

                    // 애니메이터 상태 갱신
                    UpdateAnimatorState(false, CollectedBreadCount > 0); // 이동 중이 아님, 빵이 있음
                });

            // 스택 추가 완료 대기
            yield return new WaitUntil(() => isBreadAddedToStack);
            yield return new WaitForSeconds(0.15f); // 간격 추가
        }

        UpdateAnimatorState(true, CollectedBreadCount > 0); // 이동 중, 빵이 있음
        onCollectionComplete?.Invoke(); // 다음 waypoint 이동 호출
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

    public void CompletePayment()
    {
        IsPaymentComplete = true; // 결제 완료 상태 설정

        // 애니메이션 상태 변경
        UpdateAnimatorState(false, true);
    }

    private void UpdateAnimatorState(bool isMoving, bool hasBread)
    {
        _animator.SetBool("IsMoving", isMoving);
        _animator.SetBool("HasBread", hasBread);
    }

    private IEnumerator ProcessPayment(POSManager posManager)
    {
        if (posManager == null || _isPaidFlag) yield break;

        // PaperBag 활성화
        yield return new WaitForSeconds(1f);

        GameObject paperBag = posManager.GetOrActivatePaperBag(); // POSManager에서 PaperBag 가져오기
        Transform paperBagTransform = paperBag.transform;

        // 빵 이동
        foreach (Transform stackPosition in _stackPositions)
        {
            if (stackPosition.childCount > 0)
            {
                GameObject bread = stackPosition.GetChild(0).gameObject;
                if (bread.activeSelf)
                {
                    bread.transform.DOMove(paperBagTransform.position, 0.5f).OnComplete(() =>
                    {
                        bread.SetActive(false); // 빵 비활성화
                    });
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        paperBag.transform.DOMove(_paperBag.transform.position, 0.5f).OnComplete(() =>
        {
            paperBag.SetActive(false);
            _paperBag.SetActive(true);
        });

        // Default_Idle 애니메이션 전환
        UpdateAnimatorState(false, true);

        // 결제 완료 상태 설정
        IsPaid = true;
        posManager._isProcessing = false;
        CompletePayment();

        // 애니메이션을 Stack_Walk로 전환하며 이동 준비
        yield return new WaitForSeconds(1f);

        UpdateAnimatorState(true, true);

        // POSManager에 결제 완료 알림
        posManager.CompletePayment(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("POS"))
        {
            GameObject pos = other.gameObject;
            _payFlag = pos.GetComponent<POSManager>()._isPlayerFlag;

            // 플레이어 없이 기다리는 중
            if (!_payFlag)
            {
                UpdateAnimatorState(false, true);
            }

            // 플레이어가 왔고 계산중
            else if (_payFlag && !_isPaidFlag && !_isProcessFlag)
            {
                _isProcessFlag = true;
                StartCoroutine(ProcessPayment(pos.GetComponent<POSManager>()));
            }

            else if (_payFlag && _isPaidFlag)
            {
                _isPaidFlag = false;

            }
        }
    }
}