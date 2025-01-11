using UnityEngine;

public class BreadController : MonoBehaviour
{
    private Customer _owner;

    public bool CanAcquire(Customer newOwner)
    {
        return _owner == null; // 이미 소유자가 없을 때만 획득 가능
    }

    public void Acquire(Customer newOwner)
    {
        if (_owner == null)
        {
            _owner = newOwner;
        }
    }

    public void Release()
    {
        _owner = null; // 소유 상태 해제
    }
}
