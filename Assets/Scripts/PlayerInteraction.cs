using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CharacterController))]
public class PlayerInteraction : MonoBehaviour
{
    private CharacterController _characterController;
    public Transform[] _stackPositions;
    public Animator _playerAnimator;
    public int _maxStackSize = 8;
    public Stack<GameObject> _stackedBreads = new Stack<GameObject>();

    private BreadFac _breadFac;
    private bool _isOnOvenPlane = false;

    public event System.Action<bool> OnBreadStateChanged;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OvenPlane"))
        {
            _isOnOvenPlane = true;
            _breadFac = other.GetComponentInChildren<BreadFac>();

            if (_breadFac != null)
            {
                StartCoroutine(CollectBreadFromFac());
            }
        }
        else if (other.CompareTag("StallPlane"))
        {
            if (IsStallFull(other.transform))
            {
                Debug.LogWarning("Stall is full. Cannot place more bread.");
                return; // Stall이 가득 찬 경우 배치 중단
            }

            StartCoroutine(PlaceBreadInStall(other.transform));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OvenPlane"))
        {
            _isOnOvenPlane = false;
        }
    }

    private IEnumerator CollectBreadFromFac()
    {
        if (_breadFac == null) yield break;

        int spaceLeft = _maxStackSize - _stackedBreads.Count;
        if (spaceLeft <= 0) yield break;

        List<GameObject> breadsToCollect = _breadFac.GetBreadsFromFac(spaceLeft);

        foreach (GameObject bread in breadsToCollect)
        {
            if (_stackedBreads.Count >= _maxStackSize) yield break;

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

                    OnBreadStateChanged?.Invoke(_stackedBreads.Count > 0);
                    UpdateAnimatorState();
                });

            yield return new WaitUntil(() => isBreadAddedToStack);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator PlaceBreadInStall(Transform stallPlane)
    {
        for (int i = 0; i < stallPlane.childCount && _stackedBreads.Count > 0; i++)
        {
            Transform stallPos = stallPlane.GetChild(i);
            GameObject stallBread = stallPos.childCount > 0 ? stallPos.GetChild(0).gameObject : null;

            if (stallBread != null && !stallBread.activeSelf)
            {
                GameObject stackBread = _stackedBreads.Pop();
                stackBread.SetActive(false);

                stallBread.transform.position = stackBread.transform.position;
                stallBread.SetActive(true);

                stallBread.transform
                    .DOMove(stallPos.position, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        OnBreadStateChanged?.Invoke(_stackedBreads.Count > 0);
                        UpdateAnimatorState();
                    });

                yield return new WaitForSeconds(0.1f);
            }
        }

        if (_stackedBreads.Count == 0)
        {
            UpdateAnimatorState();
        }
    }

    private bool IsStallFull(Transform stallPlane)
    {
        for (int i = 0; i < stallPlane.childCount; i++)
        {
            Transform stallPos = stallPlane.GetChild(i);

            // 자식이 없거나 자식 빵이 비활성화 상태인 경우 Stall이 가득 차지 않음
            if (stallPos.childCount == 0 || !stallPos.GetChild(0).gameObject.activeSelf)
            {
                return false;
            }
        }

        return true; // 모든 자리가 가득 참
    }

    public void RemoveBreadFromStack()
    {
        if (_stackedBreads.Count > 0)
        {
            GameObject bread = _stackedBreads.Pop();
            bread.SetActive(false);

            OnBreadStateChanged?.Invoke(_stackedBreads.Count > 0);
            UpdateAnimatorState();
        }
    }

    private void UpdateAnimatorState()
    {
        bool hasBread = _stackedBreads.Count > 0;
        _playerAnimator.SetBool("HasBread", hasBread);
    }
}
