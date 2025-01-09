using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening; // DOTween ���ӽ����̽� �߰�

[RequireComponent(typeof(CharacterController))] // ĳ���� ��Ʈ�ѷ� �ʼ�
public class PlayerInteraction : MonoBehaviour
{
    private CharacterController _characterController; // ĳ���� ��Ʈ�ѷ�
    public Transform[] _stackPositions; // �� ���� ��ġ �迭 (�ִ� 8ĭ)
    public Animator _playerAnimator; // �÷��̾� �ִϸ�����
    public int _maxStackSize = 8; // �ִ� ���� ũ��
    public Stack<GameObject> _stackedBreads = new Stack<GameObject>(); // ���� ���õ� ���� (FILO)

    private BreadFac _breadFac; // BreadFac ����
    private bool _isOnOvenPlane = false; // OvenPlane ���� �ִ��� ����

    // �� ���� ��ȭ �̺�Ʈ
    public event System.Action<bool> OnBreadStateChanged;

    private void Awake()
    {
        // ĳ���� ��Ʈ�ѷ� �ʱ�ȭ
        _characterController = GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OvenPlane"))
        {
            _isOnOvenPlane = true; // OvenPlane ���� ����
            Debug.Log("Player entered OvenPlane.");

            // OvenPlane�� BreadFac ã��
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
            _isOnOvenPlane = false; // OvenPlane���� ���
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

                    // �� ���� ��ȭ �̺�Ʈ ȣ��
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

            // �� ���� ��ȭ �̺�Ʈ ȣ��
            OnBreadStateChanged?.Invoke(_stackedBreads.Count > 0);
        }
    }
}
