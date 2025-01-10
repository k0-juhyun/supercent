using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class Customer : MonoBehaviour
{
    public int DesiredBreadCount { get; private set; } // �մ��� ���ϴ� �� ����
    public int CollectedBreadCount { get; private set; } // �մ��� ������ �� ����

    [SerializeField] private Transform[] _stackPositions; // �� ���� ��ġ
    private Stack<GameObject> _collectedBreads = new Stack<GameObject>(); // ������ ��
    private Animator _animator; // �ִϸ�����
    private bool _isCollectingBreads = false; // �� ���� ������ ����
    private Coroutine _currentMoveCoroutine; // ���� �̵� �ڷ�ƾ

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
            transform.LookAt(target.position); // �̵� �������� �ٶ󺸰� ����
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
                yield return new WaitForSeconds(1f); // ���� ������ ���
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

                    stallManager.ReleaseBread(breadToCollect); // �� ���� ����
                    UpdateAnimatorState(false, CollectedBreadCount > 0);
                });

            yield return new WaitUntil(() => stackBread.activeSelf);
            yield return new WaitForSeconds(0.15f); // ���� �߰�
        }

        _isCollectingBreads = false;

        UpdateAnimatorState(true, CollectedBreadCount > 0);
    }



    private GameObject FindActiveBreadInStall(Transform stallPlane)
    {
        Debug.Log("Ȱ��ȭ�� �� ã��");
        for (int i = 0; i < stallPlane.childCount; i++)
        {
            Transform stallPos = stallPlane.GetChild(i);
            if (stallPos.childCount > 0)
            {
                Debug.Log("���Ǵ뿡 ������");
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
                yield return new WaitForSeconds(1f); // ����ϸ鼭 �ݺ�
                continue;
            }

            // �� ���� ó��
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
            yield return new WaitForSeconds(0.15f); // ���� �߰�
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
