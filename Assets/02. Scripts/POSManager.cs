using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class POSManager : MonoBehaviour
{
    public Transform _queueStart; // ���� ���� ��ġ
    public Transform _posPoint; // POS ��ġ
    public GameObject _paperBagPrefab; // PaperBag ������
    public Money _moneyManager;
    public MoneyStack _moneyStack; // MoneyStack ���� ��ũ��Ʈ
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
        _isProcessing = false; // ��� �Ϸ� �� ó�� ���� ���·� ����
        _isPlayerFlag = false; // Player�� ���� �غ�

        // MoneyStack ������Ʈ
        if (_moneyStack != null)
        {
            _moneyStack.ActivateNextMoney();
        }

        // UI ������Ʈ
        if (_ui != null && _moneyManager != null)
        {
            _ui.UpdateMoney(_moneyManager._currentMoney);
        }

        // ���� �մ� ��� ó�� ����
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

            // �ӽ� Transform ����
            Transform tempTransform = CreateTemporaryTransform(queuePosition);

            customer.MoveTo(tempTransform, null); // �� ��ġ�� �̵�
            i++;
        }

        if (!_isProcessing) ProcessNextCustomer(); // ��� ���� �ƴϸ� ���� �մ� ó��
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
            // POS���� ���� ����
            StartCoroutine(HandlePayment(currentCustomer));
        });

        ArrangeQueue(); // ������ �� ����
    }

    private IEnumerator HandlePayment(Customer customer)
    {
        while (!_isPlayerFlag) // �÷��̾� ���
        {
            yield return null;
        }

        // ���� ���� ����
        Debug.Log("���� ��...");
    }
}
