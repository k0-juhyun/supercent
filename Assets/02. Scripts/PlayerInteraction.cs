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
    private bool _isCollectingBreads = false; // 빵 수집 중인지 여부
    private bool _isPlacingBreads = false; // 빵 배치 중인지 여부

    public event System.Action<bool> OnBreadStateChanged;
    public bool _isPosFlag = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("OvenPlane") && !_isCollectingBreads)
        {
            _breadFac = other.GetComponentInChildren<BreadFac>();
            if (_breadFac != null)
            {
                StartCoroutine(CollectBreadFromFac());
            }
        }
        else if (other.CompareTag("StallPlane") && !_isPlacingBreads)
        {
            if (!IsStallFull(other.transform))
            {
                StartCoroutine(PlaceBreadInStall(other.transform));
            }
        }
        else if(other.CompareTag("POS"))
        {
            _isPosFlag = true;
        }
    }

    private IEnumerator CollectBreadFromFac()
    {
        if (_breadFac == null) yield break;

        _isCollectingBreads = true;

        int spaceLeft = _maxStackSize - _stackedBreads.Count;
        if (spaceLeft <= 0)
        {
            _isCollectingBreads = false;
            yield break;
        }

        List<GameObject> breadsToCollect = _breadFac.GetBreadsFromFac(spaceLeft);

        foreach (GameObject bread in breadsToCollect)
        {
            if (_stackedBreads.Count >= _maxStackSize)
            {
                _isCollectingBreads = false;
                yield break;
            }

            Transform targetPosition = _stackPositions[_stackedBreads.Count];
            GameObject stackBread = targetPosition.GetChild(0).gameObject;

            // 스택 추가 완료 기다리기
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

        _isCollectingBreads = false;
    }

    private IEnumerator PlaceBreadInStall(Transform stallPlane)
    {
        _isPlacingBreads = true;

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

        _isPlacingBreads = false;

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

            if (stallPos.childCount == 0 || !stallPos.GetChild(0).gameObject.activeSelf)
            {
                return false;
            }
        }

        return true;
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
