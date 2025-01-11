using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoneyStack : MonoBehaviour
{
    private Queue<GameObject> _moneyQueue = new Queue<GameObject>(); // Money 오브젝트 큐
    [SerializeField] private GameObject _moneyFallPrefab; // MoneyFall 프리팹
    [SerializeField] private Transform _moneyFallPoolTransform; // MoneyFall 오브젝트 풀의 부모 Transform
    private List<GameObject> _moneyFallPool = new List<GameObject>(); // MoneyFall 오브젝트 풀 리스트
    public UI _ui;
    public Money _money;

    public int MoneyPerStack = 7; // 스택당 금액
    private int _totalMoney = 0; // 누적 금액

    private void Awake()
    {
        // 자식 오브젝트를 큐에 추가 (초기에는 모두 비활성화)
        foreach (Transform child in transform)
        {
            _moneyQueue.Enqueue(child.gameObject);
            child.gameObject.SetActive(false);
        }

        // MoneyFall 오브젝트 풀 초기화
        for (int i = 0; i < 10; i++) // 풀 크기 설정
        {
            GameObject moneyFall = Instantiate(_moneyFallPrefab, _moneyFallPoolTransform);
            moneyFall.SetActive(false);
            _moneyFallPool.Add(moneyFall);
        }
    }

    public void ActivateNextMoney()
    {
        if (_moneyQueue.Count > 0)
        {
            GameObject nextMoney = _moneyQueue.Dequeue();
            nextMoney.SetActive(true);
            _totalMoney += MoneyPerStack; // 스택당 금액 누적
        }
        else
        {
            Debug.LogWarning("더 이상 활성화할 MoneyStack이 없습니다.");
        }
    }

    private GameObject GetMoneyFallFromPool()
    {
        foreach (GameObject moneyFall in _moneyFallPool)
        {
            if (!moneyFall.activeSelf)
            {
                return moneyFall;
            }
        }

        // 풀에 사용 가능한 오브젝트가 없으면 새로 생성
        GameObject newMoneyFall = Instantiate(_moneyFallPrefab, _moneyFallPoolTransform);
        newMoneyFall.SetActive(false);
        _moneyFallPool.Add(newMoneyFall);
        return newMoneyFall;
    }

    private void TransferMoneyToPlayer(Transform playerTransform)
    {
        if (_moneyQueue.Count <= 0 && _totalMoney <= 0)
        {
            Debug.Log("활성화된 MoneyStack이 없거나 누적 금액이 없습니다.");
            return; // MoneyStack이 없으면 종료
        }

        StartCoroutine(TransferMoneyCoroutine(playerTransform));
    }

    private IEnumerator TransferMoneyCoroutine(Transform playerTransform)
    {
        while (_moneyQueue.Count > 0)
        {
            GameObject activeMoney = _moneyQueue.Peek();

            // 활성화된 MoneyStack이 없으면 중단
            if (activeMoney == null || !activeMoney.activeSelf)
            {
                Debug.LogWarning("큐에 활성화된 MoneyStack이 없습니다.");
                break;
            }

            // MoneyStack 비활성화
            activeMoney.SetActive(false);
            _moneyQueue.Dequeue();

            // MoneyFall 이동 처리
            GameObject moneyFall = GetMoneyFallFromPool();
            moneyFall.transform.position = activeMoney.transform.position;
            moneyFall.SetActive(true);

            // MoneyFall의 자식 오브젝트(Fall)를 활성화
            Transform fallChild = moneyFall.transform.Find("Fall");
            if (fallChild != null)
            {
                fallChild.gameObject.SetActive(true);
            }

            // MoneyFall을 Player로 이동
            moneyFall.transform.DOMove(playerTransform.position, 0.5f).OnComplete(() =>
            {
                // MoneyFall 비활성화 및 Fall 비활성화
                if (fallChild != null)
                {
                    fallChild.gameObject.SetActive(false);
                }
                moneyFall.SetActive(false);
            });

            yield return new WaitForSeconds(0.1f); // 간격 대기
        }

        // 누적된 금액 전달
        if (_totalMoney > 0)
        {
            Debug.Log($"Player가 {_totalMoney}원을 획득했습니다.");
            _money._currentMoney += _totalMoney;
            _ui.UpdateMoney(_money._currentMoney);
            _totalMoney = 0; // 누적 금액 초기화
        }

        // 모든 MoneyStack 비활성화
        ResetStack();
    }

    // MoneyStack을 초기화하여 모든 자식 오브젝트를 비활성화하고 큐를 다시 설정
    public void ResetStack()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        _moneyQueue.Clear();

        foreach (Transform child in transform)
        {
            _moneyQueue.Enqueue(child.gameObject);
        }

        _totalMoney = 0; // 누적 금액 초기화
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player가 MoneyStack을 수집합니다.");
            TransferMoneyToPlayer(other.transform);
        }
    }
}
