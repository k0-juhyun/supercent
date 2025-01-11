using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoneyStack : MonoBehaviour
{
    private Queue<GameObject> _moneyQueue = new Queue<GameObject>(); // Money ������Ʈ ť
    [SerializeField] private GameObject _moneyFallPrefab; // MoneyFall ������
    [SerializeField] private Transform _moneyFallPoolTransform; // MoneyFall ������Ʈ Ǯ�� �θ� Transform
    private List<GameObject> _moneyFallPool = new List<GameObject>(); // MoneyFall ������Ʈ Ǯ ����Ʈ
    public UI _ui;
    public Money _money;

    public int MoneyPerStack = 7; // ���ô� �ݾ�
    private int _totalMoney = 0; // ���� �ݾ�

    private void Awake()
    {
        // �ڽ� ������Ʈ�� ť�� �߰� (�ʱ⿡�� ��� ��Ȱ��ȭ)
        foreach (Transform child in transform)
        {
            _moneyQueue.Enqueue(child.gameObject);
            child.gameObject.SetActive(false);
        }

        // MoneyFall ������Ʈ Ǯ �ʱ�ȭ
        for (int i = 0; i < 10; i++) // Ǯ ũ�� ����
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
            _totalMoney += MoneyPerStack; // ���ô� �ݾ� ����
        }
        else
        {
            Debug.LogWarning("�� �̻� Ȱ��ȭ�� MoneyStack�� �����ϴ�.");
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

        // Ǯ�� ��� ������ ������Ʈ�� ������ ���� ����
        GameObject newMoneyFall = Instantiate(_moneyFallPrefab, _moneyFallPoolTransform);
        newMoneyFall.SetActive(false);
        _moneyFallPool.Add(newMoneyFall);
        return newMoneyFall;
    }

    private void TransferMoneyToPlayer(Transform playerTransform)
    {
        if (_moneyQueue.Count <= 0 && _totalMoney <= 0)
        {
            Debug.Log("Ȱ��ȭ�� MoneyStack�� ���ų� ���� �ݾ��� �����ϴ�.");
            return; // MoneyStack�� ������ ����
        }

        StartCoroutine(TransferMoneyCoroutine(playerTransform));
    }

    private IEnumerator TransferMoneyCoroutine(Transform playerTransform)
    {
        while (_moneyQueue.Count > 0)
        {
            GameObject activeMoney = _moneyQueue.Peek();

            // Ȱ��ȭ�� MoneyStack�� ������ �ߴ�
            if (activeMoney == null || !activeMoney.activeSelf)
            {
                Debug.LogWarning("ť�� Ȱ��ȭ�� MoneyStack�� �����ϴ�.");
                break;
            }

            // MoneyStack ��Ȱ��ȭ
            activeMoney.SetActive(false);
            _moneyQueue.Dequeue();

            // MoneyFall �̵� ó��
            GameObject moneyFall = GetMoneyFallFromPool();
            moneyFall.transform.position = activeMoney.transform.position;
            moneyFall.SetActive(true);

            // MoneyFall�� �ڽ� ������Ʈ(Fall)�� Ȱ��ȭ
            Transform fallChild = moneyFall.transform.Find("Fall");
            if (fallChild != null)
            {
                fallChild.gameObject.SetActive(true);
            }

            // MoneyFall�� Player�� �̵�
            moneyFall.transform.DOMove(playerTransform.position, 0.5f).OnComplete(() =>
            {
                // MoneyFall ��Ȱ��ȭ �� Fall ��Ȱ��ȭ
                if (fallChild != null)
                {
                    fallChild.gameObject.SetActive(false);
                }
                moneyFall.SetActive(false);
            });

            yield return new WaitForSeconds(0.1f); // ���� ���
        }

        // ������ �ݾ� ����
        if (_totalMoney > 0)
        {
            Debug.Log($"Player�� {_totalMoney}���� ȹ���߽��ϴ�.");
            _money._currentMoney += _totalMoney;
            _ui.UpdateMoney(_money._currentMoney);
            _totalMoney = 0; // ���� �ݾ� �ʱ�ȭ
        }

        // ��� MoneyStack ��Ȱ��ȭ
        ResetStack();
    }

    // MoneyStack�� �ʱ�ȭ�Ͽ� ��� �ڽ� ������Ʈ�� ��Ȱ��ȭ�ϰ� ť�� �ٽ� ����
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

        _totalMoney = 0; // ���� �ݾ� �ʱ�ȭ
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player�� MoneyStack�� �����մϴ�.");
            TransferMoneyToPlayer(other.transform);
        }
    }
}
