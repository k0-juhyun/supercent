using UnityEngine;

public class BreadController : MonoBehaviour
{
    private Customer _owner;

    public bool CanAcquire(Customer newOwner)
    {
        return _owner == null; // �̹� �����ڰ� ���� ���� ȹ�� ����
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
        _owner = null; // ���� ���� ����
    }
}
