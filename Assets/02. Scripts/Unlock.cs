using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unlock : MonoBehaviour
{
    public ParticleSystem _particleSystem;
    [SerializeField] private GameObject _Locekd;
    [SerializeField] private GameObject _unLocekd;
    [SerializeField] private GameObject _left;

    private bool _locked = true;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && _locked)
        {
            int _money = other.GetComponent<Money>()._currentMoney;

            if(_money > 30)
            {
                _locked = false;
                _particleSystem.Play();
                _unLocekd.SetActive(true);
                _left.SetActive(true);
                _Locekd.SetActive(false);
            }
        }
    }
}
