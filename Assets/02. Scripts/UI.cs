using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    [SerializeField] private TMP_Text _moneyText; // TMP_Text�� ����� Money ǥ�� UI
    private int _currentMoney = 0; // ���� Money ��

    // Money ���� ������Ʈ�ϰ� TMP_Text�� ����
    public void UpdateMoney(int newMoney)
    {
        _currentMoney = newMoney;
        RefreshMoneyText();
    }

    // TMP_Text�� ����
    private void RefreshMoneyText()
    {
        if (_moneyText != null)
        {
            _moneyText.text = $"{_currentMoney}";
        }
        else
        {
            Debug.LogWarning("����ȵ�");
        }
    }

    // ���� Money ���� ��ȯ
    public int GetCurrentMoney()
    {
        return _currentMoney;
    }
}
