using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class POSManager : MonoBehaviour
{
    public Transform _queueStart; // 줄의 시작 위치
    public Transform _posPoint; // POS 위치
    public GameObject _paperBagPrefab; // PaperBag 프리팹
    public Money _moneyManager;
    public MoneyStack _moneyStack; // MoneyStack 관리 스크립트
    public UI _ui;

    private Queue<Customer> _customerQueue = new Queue<Customer>();
    public bool _isProcessing = false;
    public bool _isPlayerFlag;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            _isPlayerFlag = player.GetComponent<PlayerInteraction>()._isPosFlag;
        }
        else if (other.CompareTag("Customer"))
        {
            Customer customer = other.GetComponent<Customer>();
            if (!_customerQueue.Contains(customer))
            {
                _customerQueue.Enqueue(customer);
                ArrangeQueue();
            }
        }
    }

    public void CompletePayment(Customer customer)
    {
        _isProcessing = false; // 계산 완료 후 처리 가능 상태로 변경
        _isPlayerFlag = false; // Player가 떠날 준비

        // MoneyStack 업데이트
        if (_moneyStack != null)
        {
            _moneyStack.ActivateNextMoney();
        }

        // UI 업데이트
        if (_ui != null && _moneyManager != null)
        {
            _ui.UpdateMoney(_moneyManager._currentMoney);
        }

        // 다음 손님 계산 처리 시작
        ProcessNextCustomer();
    }


    public GameObject GetOrActivatePaperBag()
    {
        if (_paperBagPrefab != null && !_paperBagPrefab.activeSelf)
        {
            _paperBagPrefab.SetActive(true);
        }
        return _paperBagPrefab;
    }

    private void ArrangeQueue()
    {
        int i = 0;
        foreach (Customer customer in _customerQueue)
        {
            Vector3 queuePosition = _queueStart.position + new Vector3(0, 0, i);

            // 임시 Transform 생성
            Transform tempTransform = CreateTemporaryTransform(queuePosition);

            customer.MoveTo(tempTransform, null); // 줄 위치로 이동
            i++;
        }

        if (!_isProcessing) ProcessNextCustomer(); // 계산 중이 아니면 다음 손님 처리
    }

    private Transform CreateTemporaryTransform(Vector3 position)
    {
        GameObject tempObject = new GameObject("TempQueuePosition");
        tempObject.transform.position = position;
        return tempObject.transform;
    }

    private void ProcessNextCustomer()
    {
        if (_isProcessing || _customerQueue.Count == 0) return;

        _isProcessing = true;
        Customer currentCustomer = _customerQueue.Dequeue();

        currentCustomer.MoveTo(_posPoint, () =>
        {
            // POS에서 결제 진행
            StartCoroutine(HandlePayment(currentCustomer));
        });

        ArrangeQueue(); // 나머지 줄 정리
    }

    private IEnumerator HandlePayment(Customer customer)
    {
        while (!_isPlayerFlag) // 플레이어 대기
        {
            yield return null;
        }

        // 결제 로직 수행
        Debug.Log("결제 중...");
    }
}
