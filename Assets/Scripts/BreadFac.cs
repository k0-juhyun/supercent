using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

public class BreadFac : MonoBehaviour
{
    public GameObject _breadPrefab; // 빵 프리팹
    public Transform _spawnPoint; // 빵 생성 위치
    public Transform _basketPoint; // 바구니 위치
    public float _spawnInterval = 1.5f; // 빵 생성 주기
    public float _jumpDelay = 0.5f; // 바구니로 이동 전 대기 시간
    public float _jumpDuration = 0.5f; // 바구니로 이동 시간
    public Ease _jumpEase = Ease.OutQuad; // 바구니로 이동 시 효과

    private Queue<GameObject> _breadPool = new Queue<GameObject>(); // 빵 풀
    private List<GameObject> _breadsInBasket = new List<GameObject>(); // 바구니에 있는 빵 리스트

    private void Start()
    {
        StartCoroutine(SpawnBread());
    }

    private IEnumerator SpawnBread()
    {
        while (true)
        {
            // 빵 생성
            GameObject bread = GetBreadFromPool();
            bread.transform.position = _spawnPoint.position;
            bread.SetActive(true);

            // 바구니로 이동 처리
            MoveToBasket(bread);

            // 다음 빵 생성까지 대기
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private GameObject GetBreadFromPool()
    {
        if (_breadPool.Count > 0)
        {
            return _breadPool.Dequeue();
        }
        return Instantiate(_breadPrefab, transform);
    }

    private void MoveToBasket(GameObject bread)
    {
        // DOTween을 사용하여 바구니로 이동
        bread.transform
            .DOMove(_basketPoint.position, _jumpDuration)
            .SetEase(_jumpEase)
            .SetDelay(_jumpDelay)
            .OnComplete(() =>
            {
                _breadsInBasket.Add(bread); // 바구니 리스트에 추가
            });
    }

    public List<GameObject> GetBreadsFromFac(int count)
    {
        List<GameObject> breadsToCollect = new List<GameObject>();
        int collectedCount = 0;

        foreach (GameObject bread in _breadsInBasket)
        {
            if (collectedCount >= count) break;
            breadsToCollect.Add(bread);
            collectedCount++;
        }

        foreach (GameObject bread in breadsToCollect)
        {
            _breadsInBasket.Remove(bread); // BreadFac에서 제거
        }

        return breadsToCollect;
    }

    public void ReturnBreadToPool(GameObject bread)
    {
        // 빵 풀로 반환
        bread.SetActive(false);
        _breadPool.Enqueue(bread);
    }
}
