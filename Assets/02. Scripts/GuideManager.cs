using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Guide
{
    Oven,
    Stall,
    Pos,
    MoneyStack,
    Locked,
    Left
}

public class GuideManager : MonoBehaviour
{
    public Guide _guide = Guide.Oven; // �ʱ� ���¸� Oven���� ����
    [SerializeField] private Transform[] _overPointTransform; // �� ���̵� ����Ʈ �迭
    [SerializeField] private GameObject _playerPoint; // Player�� ���� ������ ����Ű�� ������Ʈ
    [SerializeField] private GameObject _overPoint; // OverPoint ������Ʈ

    private void Start()
    {
        UpdateGuidePoints(); // �ʱ� ���̵� ����Ʈ ����
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("OvenPlane") && _guide == Guide.Oven)
        {
            AdvanceGuide();
        }
        else if (other.gameObject.CompareTag("StallPlane") && _guide == Guide.Stall)
        {
            AdvanceGuide();
        }
        else if (other.gameObject.CompareTag("POS") && _guide == Guide.Pos)
        {
            AdvanceGuide();
        }
        else if (other.gameObject.CompareTag("MoneyStack") && _guide == Guide.MoneyStack)
        {
            AdvanceGuide();
        }
        else if (other.gameObject.CompareTag("Locked") && _guide == Guide.Locked)
        {
            AdvanceGuide();
        }
        else if (other.gameObject.CompareTag("Left") && _guide == Guide.Left)
        {
            Debug.Log("Guide �Ϸ�!");
        }
    }

    private void AdvanceGuide()
    {
        // ���̵� ���¸� ���� ������ ��ȯ
        if (_guide < Guide.Left)
        {
            _guide++;
            UpdateGuidePoints();
        }
    }

    private void UpdateGuidePoints()
    {
        // ���� ���̵� ���¿� ���� PlayerPoint�� OverPoint ������ ������Ʈ
        switch (_guide)
        {
            case Guide.Oven:
                SetGuidePoints(_overPointTransform[0].position, _overPointTransform[0].position);
                Debug.Log("Oven ���̵带 ����Ŵ");
                break;
            case Guide.Stall:
                SetGuidePoints(_overPointTransform[1].position, _overPointTransform[1].position);
                Debug.Log("Basket ���̵带 ����Ŵ");
                break;
            case Guide.Pos:
                SetGuidePoints(_overPointTransform[2].position, _overPointTransform[2].position);
                Debug.Log("Pos ���̵带 ����Ŵ");
                break;
            case Guide.MoneyStack:
                SetGuidePoints(_overPointTransform[3].position, _overPointTransform[3].position);
                Debug.Log("MoneyStack ���̵带 ����Ŵ");
                break;
            case Guide.Locked:
                SetGuidePoints(_overPointTransform[4].position, _overPointTransform[4].position);
                Debug.Log("Unlock ���̵带 ����Ŵ");
                break;
            case Guide.Left:
                SetGuidePoints(_overPointTransform[5].position, _overPointTransform[5].position);
                Debug.Log("Left ���̵带 ����Ŵ");
                break;
            default:
                Debug.LogWarning("Unknown Guide State");
                break;
        }
    }

    private void SetGuidePoints(Vector3 playerPointDirection, Vector3 overPointPosition)
    {
        if (_playerPoint != null)
        {
            // PlayerPoint�� ���� ��ġ�� ���ϰ� ȸ��
            Vector3 direction = (playerPointDirection - _playerPoint.transform.position).normalized;
            direction.y = 0.1f;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // x�� ȸ���� 90���� ����
            _playerPoint.transform.rotation = Quaternion.Euler(90f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        }


        if (_overPoint != null)
        {
            // OverPoint ��ġ ������Ʈ
            _overPoint.transform.position = overPointPosition;
        }
    }
}
