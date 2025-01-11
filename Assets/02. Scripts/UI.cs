using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    [SerializeField] private TMP_Text _moneyText; // TMP_Text로 연결된 Money 표시 UI
    private int _currentMoney = 0; // 현재 Money 값

    // Money 값을 업데이트하고 TMP_Text를 갱신
    public void UpdateMoney(int newMoney)
    {
        _currentMoney = newMoney;
        RefreshMoneyText();
    }

    // TMP_Text를 갱신
    private void RefreshMoneyText()
    {
        if (_moneyText != null)
        {
            _moneyText.text = $"{_currentMoney}";
        }
        else
        {
            Debug.LogWarning("연결안됨");
        }
    }

    // 현재 Money 값을 반환
    public int GetCurrentMoney()
    {
        return _currentMoney;
    }
}
