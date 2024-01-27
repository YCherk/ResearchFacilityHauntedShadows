using UnityEngine;

public class DoorController : MonoBehaviour
{
    Animator animator;
    bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
    }
}
