using UnityEngine;

public class DoorController : MonoBehaviour
{
    Animator animator;
    bool isOpen = false;
    public AudioSource DoorSound;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
        DoorSound.Play();
    }

    public void ForceOpenDoor()
    {
        isOpen = true; // Set the door state to open
        animator.SetBool("isOpen", isOpen); // Trigger the door open animation
        DoorSound.Play();
    }
}
