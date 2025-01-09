using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �÷��̾ ����ٴ�
public class FollowingCamera : MonoBehaviour
{
    public Transform _player;
    public float _lerpSpeed = 1.0f;
    public Vector3 _offset; // 

    private void LateUpdate()
    {
        if (_player != null)
        {
            Vector3 _camPos = _player.position + _offset;
            Vector3 _lerpPos = Vector3.Lerp(transform.position, _camPos, _lerpSpeed);
            transform.position = _lerpPos;
        }
    }
}
