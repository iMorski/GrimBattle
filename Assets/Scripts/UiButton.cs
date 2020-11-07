using UnityEngine;

public class UiButton : MonoBehaviour
{
    private Animator Animator;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    public void Push()
    {
        Animator.Play("Button-Push");
    }
}
