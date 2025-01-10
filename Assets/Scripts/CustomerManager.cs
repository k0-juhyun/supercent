using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CustomerManager : MonoBehaviour
{
    [SerializeField] private GameObject _customerPrefab;
    [SerializeField] private Transform _customerSpawnPoint;
    [SerializeField] private Transform[] _wayPoints;
    [SerializeField] private Transform[] _wayPoint2SubPoints;
    [SerializeField] private Transform[] _wayPoint4SubPoints;
    [SerializeField] private Transform _table;
    [SerializeField] private Transform _stallPlane; // StallPlane �߰�

    private Queue<GameObject> _customerPool = new Queue<GameObject>();
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

        foreach (Transform subPoint in _wayPoint4SubPoints)
        {
            _wayPoint4Occupancy[subPoint] = false;
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
                    StartCoroutine(HandleBreadCollection(customer, _stallPlane)); // StallPlane ����
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
            StartCoroutine(HandleBreadCollection(customer, targetSubPoint));
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

    private IEnumerator HandleBreadCollection(GameObject customer, Transform subPoint)
    {
        Customer customerScript = customer.GetComponent<Customer>();
        StallManager stallManager = _stallPlane.GetComponent<StallManager>();

        if (stallManager == null)
        {
            Debug.LogWarning("StallManager�� ã�� �� �����ϴ�.");
            yield break;
        }

        // �� ���� ����
        yield return StartCoroutine(customerScript.StartCollectingCoroutine(stallManager));

        // ���� ���� ����
        _wayPoint2Occupancy[subPoint] = false;

        // ���� �ܰ�� ����
        DecideCustomerType(customer);
    }


    private void DecideCustomerType(GameObject customer)
    {
        if (!_isTableOccupied && Random.value > 0.5f)
        {
            AssignTable(customer);
        }
        else
        {
            AssignWayPoint4(customer);
        }
    }

    private void AssignWayPoint4(GameObject customer)
    {
        Customer customerScript = customer.GetComponent<Customer>();
        customerScript.MoveTo(_wayPoints[3], () =>
        {
            TraverseWayPoint4(customer);
        });
    }

    private void TraverseWayPoint4(GameObject customer)
    {
        Customer customerScript = customer.GetComponent<Customer>();

        foreach (var subPoint in _wayPoint4SubPoints)
        {
            if (!_wayPoint4Occupancy[subPoint])
            {
                _wayPoint4Occupancy[subPoint] = true;

                customerScript.MoveTo(subPoint, () =>
                {
                    _wayPoint4Occupancy[subPoint] = false;
                    ReturnCustomerToPool(customer);
                });

                return;
            }
        }

        Debug.Log("��� WayPoint4 ���� ����Ʈ�� ���� á���ϴ�.");
    }

    private void AssignTable(GameObject customer)
    {
        if (!_isTableOccupied)
        {
            _isTableOccupied = true;

            Customer customerScript = customer.GetComponent<Customer>();
            customerScript.MoveTo(_wayPoints[3], () =>
            {
                customerScript.MoveTo(_table, () =>
                {
                    _isTableOccupied = false;
                    ReturnCustomerToPool(customer);
                });
            });
        }
        else
        {
            Debug.Log("���̺��� ���� á���ϴ�.");
        }
    }

    private void ReturnCustomerToPool(GameObject customer)
    {
        customer.SetActive(false);
        customer.transform.position = _customerSpawnPoint.position;
        _customerPool.Enqueue(customer);
    }
}
