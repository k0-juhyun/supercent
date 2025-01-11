// Customer.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using DG.Tweening;
using System.Collections.Generic;

public class Customer : MonoBehaviour
{
    public int DesiredBreadCount { get; private set; } // �մ��� ���ϴ� �� ����
    public int CollectedBreadCount { get; private set; } // �մ��� ������ �� ����

    [SerializeField] private Transform[] _stackPositions; // �� ���� ��ġ
    private Stack<GameObject> _collectedBreads = new Stack<GameObject>(); // ������ ��
    public List<GameObject> CollectedBreads { get; private set; } = new List<GameObject>();
    private Animator _animator; // �ִϸ�����
    private Coroutine _currentMoveCoroutine; // ���� �̵� �ڷ�ƾ
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
                // �� ����� �̺�Ʈ ȣ������
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
            transform.LookAt(target.position); // �̵� �������� �ٶ󺸰� ����
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
        // Stall �ٶ󺸱� ����
        transform.LookAt(stallTransform.position);
        UpdateAnimatorState(false, CollectedBreadCount > 0); // �̵� ���� �ƴ�, ���� �� ���� �ݿ�

        while (CollectedBreadCount < DesiredBreadCount)
        {
            GameObject breadToCollect = stallManager.GetAvailableBread();
            if (breadToCollect == null)
            {
                yield return new WaitForSeconds(1f); // ���� ������ ���
                continue;
            }

            // ���� �߰� �ϷḦ Ȯ���ϱ� ���� �÷���
            bool isBreadAddedToStack = false;

            // ���� ������ �Ҵ�
            Transform targetPosition = _stackPositions[CollectedBreadCount];

            breadToCollect.transform
                .DOMove(transform.position, 0.5f) // �մԿ��� ���� �̵�
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // ���� ���ÿ� �߰�
                    _collectedBreads.Push(breadToCollect);
                    breadToCollect.SetActive(false);

                    // CollectedBreadCount�� ������� StackPosition�� �� Ȱ��ȭ
                    GameObject stackBread = targetPosition.GetChild(0).gameObject;
                    stackBread.SetActive(true);

                    // CollectedBreadCount ����
                    CollectedBreadCount++;

                    // StallManager���� �� ���� ����
                    stallManager.ReleaseBread(breadToCollect);

                    // �߰� �Ϸ� �÷��� ����
                    isBreadAddedToStack = true;

                    // �ִϸ����� ���� ����
                    UpdateAnimatorState(false, CollectedBreadCount > 0); // �̵� ���� �ƴ�, ���� ����
                });

            // ���� �߰� �Ϸ� ���
            yield return new WaitUntil(() => isBreadAddedToStack);
            yield return new WaitForSeconds(0.15f); // ���� �߰�
        }

        UpdateAnimatorState(true, CollectedBreadCount > 0); // �̵� ��, ���� ����
        onCollectionComplete?.Invoke(); // ���� waypoint �̵� ȣ��
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
        IsPaymentComplete = true; // ���� �Ϸ� ���� ����

        // �ִϸ��̼� ���� ����
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

        // PaperBag Ȱ��ȭ
        yield return new WaitForSeconds(1f);

        GameObject paperBag = posManager.GetOrActivatePaperBag(); // POSManager���� PaperBag ��������
        Transform paperBagTransform = paperBag.transform;

        // �� �̵�
        foreach (Transform stackPosition in _stackPositions)
        {
            if (stackPosition.childCount > 0)
            {
                GameObject bread = stackPosition.GetChild(0).gameObject;
                if (bread.activeSelf)
                {
                    bread.transform.DOMove(paperBagTransform.position, 0.5f).OnComplete(() =>
                    {
                        bread.SetActive(false); // �� ��Ȱ��ȭ
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

        // Default_Idle �ִϸ��̼� ��ȯ
        UpdateAnimatorState(false, true);

        // ���� �Ϸ� ���� ����
        IsPaid = true;
        posManager._isProcessing = false;
        CompletePayment();

        // �ִϸ��̼��� Stack_Walk�� ��ȯ�ϸ� �̵� �غ�
        yield return new WaitForSeconds(1f);

        UpdateAnimatorState(true, true);

        // POSManager�� ���� �Ϸ� �˸�
        posManager.CompletePayment(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("POS"))
        {
            GameObject pos = other.gameObject;
            _payFlag = pos.GetComponent<POSManager>()._isPlayerFlag;

            // �÷��̾� ���� ��ٸ��� ��
            if (!_payFlag)
            {
                UpdateAnimatorState(false, true);
            }

            // �÷��̾ �԰� �����
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