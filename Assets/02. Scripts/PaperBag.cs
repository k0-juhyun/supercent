using UnityEngine;

public class PaperBag : MonoBehaviour
{
    private Animator _animator;

    private void OnEnable()
    {
        _animator = GetComponent<Animator>();
    }

    public void CloseBag()
    {
        _animator.SetTrigger("Close");
    }
}
