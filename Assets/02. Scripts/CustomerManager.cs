using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Data.Common;

public class CustomerManager : MonoBehaviour
{
    [SerializeField] private GameObject _customerPrefab;
    [SerializeField] private Transform _customerSpawnPoint;
    [SerializeField] private Transform[] _wayPoints;
    [SerializeField] private Transform[] _wayPoint2SubPoints;
    [SerializeField] private Transform _outPoints;
    [SerializeField] private Transform _waitingPoints;
    [SerializeField] private Transform _table;
    [SerializeField] private Transform _stallPlane; // StallPlane �߰�

    private Queue<GameObject> _customerPool = new Queue<GameObject>();
    private Queue<GameObject> _tableQueue = new Queue<GameObject>(); // ���̺� ��⿭
    private Dictionary<Transform, bool> _wayPoint2Occupancy = new Dictionary<Transform, bool>();
    private Dictionary<Transform, bool> _wayPoint4Occupancy = new Dictionary<Transform, bool>();
    private bool _isTableOccupied = false;

    public float _spawnDelay;

    private void Start()
    {
        foreach (Transform subPoint in _wayPoint2SubPoints)
        {
            _wayPoint2Occupancy[subPoint] = false;
        }

        InitializeCustomerPool(10);
        StartCoroutine(SpawnCustomers());
    }

    private void InitializeCustomerPool(int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject customer = Instantiate(_customerPrefab);
            customer.transform.SetParent(_customerSpawnPoint);
            customer.SetActive(false);
            _customerPool.Enqueue(customer);
        }
    }

    private IEnumerator SpawnCustomers()
    {
        while (true)
        {
            if (_customerPool.Count > 0)
            {
                GameObject customer = _customerPool.Dequeue();
                customer.SetActive(true);
                customer.transform.position = _customerSpawnPoint.position;

                Customer customerScript = customer.GetComponent<Customer>();
                int breadCount = Random.Range(1, 6);
                customerScript.Initialize(breadCount);

                AssignWayPoints(customer);
            }

            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    private void AssignWayPoints(GameObject customer)
    {
        Customer customerScript = customer.GetComponent<Customer>();

        customerScript.MoveTo(_wayPoints[0], () =>
        {
            customerScript.MoveTo(_wayPoints[1], () =>
            {
                AssignWayPoint2(customer);
            });
        });
    }

    private void AssignWayPoint2(GameObject customer)
    {
        foreach (var subPoint in _wayPoint2SubPoints)
        {
            if (!_wayPoint2Occupancy[subPoint])
            {
                _wayPoint2Occupancy[subPoint] = true;
                TraverseWayPoint2(customer, subPoint, () =>
                {
                    Debug.Log("���Ǵ� ����");
                    StartCoroutine(HandleBreadCollection(customer, _stallPlane, subPoint));
                });
                return;
            }
        }

        Debug.Log("��� WayPoint2 ���� ����Ʈ�� ���� ��");
    }

    private void TraverseWayPoint2(GameObject customer, Transform targetSubPoint, System.Action onComplete)
    {
        List<Transform> path = new List<Transform> { _wayPoints[1] };

        for (int i = _wayPoint2SubPoints.Length - 1; i >= 0; i--)
        {
            path.Add(_wayPoint2SubPoints[i]);
            if (_wayPoint2SubPoints[i] == targetSubPoint)
                break;
        }

        StartCoroutine(FollowPath(customer, path, () =>
        {
            onComplete?.Invoke();
        }));
    }

    private IEnumerator FollowPath(GameObject customer, List<Transform> path, System.Action onComplete)
    {
        foreach (var point in path)
        {
            bool reached = false;
            Customer customerScript = customer.GetComponent<Customer>();
            customerScript.MoveTo(point, () => reached = true);
            yield return new WaitUntil(() => reached);
        }

        onComplete?.Invoke();
    }

    private IEnumerator HandleBreadCollection(GameObject customer, Transform stallPlane, Transform subPoint)
    {
        Customer customerScript = customer.GetComponent<Customer>();
        StallManager stallManager = stallPlane.GetComponent<StallManager>();

        if (stallManager == null)
        {
            Debug.LogWarning("StallManager�� ã�� �� �����ϴ�.");
            yield break;
        }

        // StallPlane�� Transform�� �Բ� ����
        yield return StartCoroutine(customerScript.StartCollectingCoroutine(stallManager, stallPlane, () =>
        {
            _wayPoint2Occupancy[subPoint] = false;
            DecideCustomerType(customer);
        }));
    }

    private void DecideCustomerType(GameObject customer)
    {
        Customer customerScript = customer.GetComponent<Customer>();

        if (!_isTableOccupied && Random.value > 0.9f)
        {
            Debug.Log("�԰�");
            AssignTable(customer);

            // ���� �Ϸ� �� AssignTable�� �ٽ� ȣ��
            customerScript.OnPaymentCompleted += () =>
            {
                AssignTable(customer);
            };
        }
        else
        {
            Debug.Log("����");
            AssignWayPoint4(customer);

            // ���� �Ϸ� �� AssignWayPoint4�� �ٽ� ȣ��
            customerScript.OnPaymentCompleted += () =>
            {
                AssignWayPoint4(customer);
            };
        }
    }

    private void AssignWayPoint4(GameObject customer)
    {
        Customer customerScript = customer.GetComponent<Customer>();

        // WayPoint4 ���� �� ���� ���ο� ���� ����
        customerScript.MoveTo(_wayPoints[3], () =>
        {
            Debug.Log("�մ� WayPoint4 ����");
            if (customerScript.IsPaid)
            {
                Debug.Log("�մ� ���� �Ϸ�. ������ ���ô�");
                customerScript.MoveTo(_outPoints, () =>
                {
                    ReturnCustomerToPool(customer);
                });
            }
            else
            {
                Debug.Log("�մ� ���� �̿Ϸ�. ��� ���� ����");
            }
        });
    }

    private void AssignTable(GameObject customer)
    {
        Customer customerScript = customer.GetComponent<Customer>();

        if (!_isTableOccupied)
        {
            customerScript.MoveTo(_wayPoints[4], () =>
            {
                customerScript.MoveTo(_table, () =>
                {
                    _isTableOccupied = true;

                    if (_tableQueue.Count > 0)
                    {
                        GameObject nextCustomer = _tableQueue.Dequeue();
                        AssignTable(nextCustomer); // ��⿭�� ���� �� ó��
                    }
                    else
                    {
                        ReturnCustomerToPool(customer);
                    }
                });
            });
        }
        else
        {
            Debug.Log("���̺��� ���� á���ϴ�. ���� ���ϴ�.");
            _tableQueue.Enqueue(customer);
            ArrangeTableQueue(); // ��⿭ ����
        }
    }

    private void ArrangeTableQueue()
    {
        int i = 0;
        foreach (GameObject queuedCustomer in _tableQueue)
        {
            Customer customerScript = queuedCustomer.GetComponent<Customer>();
            Vector3 queuePosition = _waitingPoints.transform.position;
            Transform tempTransform = CreateTemporaryTransform(queuePosition);

            customerScript.MoveTo(tempTransform, null); // �� ��ġ�� �̵�
            i++;
        }
    }

    private Transform CreateTemporaryTransform(Vector3 position)
    {
        GameObject tempObject = new GameObject("TempTableQueuePosition");
        tempObject.transform.position = position;
        return tempObject.transform;
    }

    private void ReturnCustomerToPool(GameObject customer)
    {
        customer.SetActive(false);
        customer.transform.position = _customerSpawnPoint.position;
        _customerPool.Enqueue(customer);
    }
}
