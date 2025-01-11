using System.Collections.Generic;
using UnityEngine;

public class StallManager : MonoBehaviour
{
    private Dictionary<Transform, bool> _stallBreadStatus = new Dictionary<Transform, bool>();
    private Queue<System.Action> _requestQueue = new Queue<System.Action>();
    private bool _isProcessingRequest = false;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.childCount > 0)
            {
                Transform bread = child.GetChild(0);
                _stallBreadStatus[bread] = false; // �ʱ⿡�� ��� �������� ����
            }
        }
    }

    public void RequestBreadCollection(System.Action onRequestProcessed)
    {
        _requestQueue.Enqueue(onRequestProcessed);
        ProcessNextRequest();
    }

    private void ProcessNextRequest()
    {
        if (_isProcessingRequest || _requestQueue.Count == 0) return;

        _isProcessingRequest = true;

        // ť���� ��û ó��
        System.Action currentRequest = _requestQueue.Dequeue();
        currentRequest?.Invoke();

        // ��û �Ϸ� �� ���� ��û ó��
        _isProcessingRequest = false;
        ProcessNextRequest();
    }

    public GameObject GetAvailableBread()
    {
        foreach (var kvp in _stallBreadStatus)
        {
            if (!kvp.Value && kvp.Key.gameObject.activeSelf)
            {
                _stallBreadStatus[kvp.Key] = true; // �� ���� ���·� ����
                return kvp.Key.gameObject;
            }
        }
        return null; // ��� ������ �� ����
    }

    public void ReleaseBread(GameObject bread)
    {
        Transform breadTransform = bread.transform;
        if (_stallBreadStatus.ContainsKey(breadTransform))
        {
            _stallBreadStatus[breadTransform] = false; // ���� ���� ����
        }
    }
}
