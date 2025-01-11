using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween ���ӽ����̽� �߰�

public class BreadFac : MonoBehaviour
{
    public GameObject _breadPrefab; // �� ������
    public Transform _spawnPoint; // �� ���� ��ġ
    public Transform _basketPoint; // �ٱ��� ��ġ
    public float _spawnInterval = 1.5f; // �� ���� �ֱ�
    public float _jumpDelay = 0.5f; // �ٱ��Ϸ� �̵� �� ��� �ð�
    public float _jumpDuration = 0.5f; // �ٱ��Ϸ� �̵� �ð�
    public Ease _jumpEase = Ease.OutQuad; // �ٱ��Ϸ� �̵� �� ȿ��

    private Queue<GameObject> _breadPool = new Queue<GameObject>(); // �� Ǯ
    private List<GameObject> _breadsInBasket = new List<GameObject>(); // �ٱ��Ͽ� �ִ� �� ����Ʈ

    private void Start()
    {
        StartCoroutine(SpawnBread());
    }

    private IEnumerator SpawnBread()
    {
        while (true)
        {
            // �� ����
            GameObject bread = GetBreadFromPool();
            bread.transform.position = _spawnPoint.position;
            bread.SetActive(true);

            // �ٱ��Ϸ� �̵� ó��
            MoveToBasket(bread);

            // ���� �� �������� ���
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
        // DOTween�� ����Ͽ� �ٱ��Ϸ� �̵�
        bread.transform
            .DOMove(_basketPoint.position, _jumpDuration)
            .SetEase(_jumpEase)
            .SetDelay(_jumpDelay)
            .OnComplete(() =>
            {
                _breadsInBasket.Add(bread); // �ٱ��� ����Ʈ�� �߰�
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
            _breadsInBasket.Remove(bread); // BreadFac���� ����
        }

        return breadsToCollect;
    }

    public void ReturnBreadToPool(GameObject bread)
    {
        // �� Ǯ�� ��ȯ
        bread.SetActive(false);
        _breadPool.Enqueue(bread);
    }
}
