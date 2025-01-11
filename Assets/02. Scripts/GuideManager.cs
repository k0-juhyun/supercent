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
    public Guide _guide = Guide.Oven; // 초기 상태를 Oven으로 설정
    [SerializeField] private Transform[] _overPointTransform; // 각 가이드 포인트 배열
    [SerializeField] private GameObject _playerPoint; // Player가 향할 방향을 가리키는 오브젝트
    [SerializeField] private GameObject _overPoint; // OverPoint 오브젝트

    private void Start()
    {
        UpdateGuidePoints(); // 초기 가이드 포인트 설정
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
            Debug.Log("Guide 완료!");
        }
    }

    private void AdvanceGuide()
    {
        // 가이드 상태를 다음 순서로 전환
        if (_guide < Guide.Left)
        {
            _guide++;
            UpdateGuidePoints();
        }
    }

    private void UpdateGuidePoints()
    {
        // 현재 가이드 상태에 따라 PlayerPoint와 OverPoint 방향을 업데이트
        switch (_guide)
        {
            case Guide.Oven:
                SetGuidePoints(_overPointTransform[0].position, _overPointTransform[0].position);
                Debug.Log("Oven 가이드를 가리킴");
                break;
            case Guide.Stall:
                SetGuidePoints(_overPointTransform[1].position, _overPointTransform[1].position);
                Debug.Log("Basket 가이드를 가리킴");
                break;
            case Guide.Pos:
                SetGuidePoints(_overPointTransform[2].position, _overPointTransform[2].position);
                Debug.Log("Pos 가이드를 가리킴");
                break;
            case Guide.MoneyStack:
                SetGuidePoints(_overPointTransform[3].position, _overPointTransform[3].position);
                Debug.Log("MoneyStack 가이드를 가리킴");
                break;
            case Guide.Locked:
                SetGuidePoints(_overPointTransform[4].position, _overPointTransform[4].position);
                Debug.Log("Unlock 가이드를 가리킴");
                break;
            case Guide.Left:
                SetGuidePoints(_overPointTransform[5].position, _overPointTransform[5].position);
                Debug.Log("Left 가이드를 가리킴");
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
            // PlayerPoint를 다음 위치를 향하게 회전
            Vector3 direction = (playerPointDirection - _playerPoint.transform.position).normalized;
            direction.y = 0.1f;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // x축 회전을 90도로 고정
            _playerPoint.transform.rotation = Quaternion.Euler(90f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        }


        if (_overPoint != null)
        {
            // OverPoint 위치 업데이트
            _overPoint.transform.position = overPointPosition;
        }
    }
}
