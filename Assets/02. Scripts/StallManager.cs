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
                _stallBreadStatus[bread] = false; // 초기에는 모두 점유되지 않음
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

        // 큐에서 요청 처리
        System.Action currentRequest = _requestQueue.Dequeue();
        currentRequest?.Invoke();

        // 요청 완료 후 다음 요청 처리
        _isProcessingRequest = false;
        ProcessNextRequest();
    }

    public GameObject GetAvailableBread()
    {
        foreach (var kvp in _stallBreadStatus)
        {
            if (!kvp.Value && kvp.Key.gameObject.activeSelf)
            {
                _stallBreadStatus[kvp.Key] = true; // 빵 점유 상태로 변경
                return kvp.Key.gameObject;
            }
        }
        return null; // 사용 가능한 빵 없음
    }

    public void ReleaseBread(GameObject bread)
    {
        Transform breadTransform = bread.transform;
        if (_stallBreadStatus.ContainsKey(breadTransform))
        {
            _stallBreadStatus[breadTransform] = false; // 점유 상태 해제
        }
    }
}
