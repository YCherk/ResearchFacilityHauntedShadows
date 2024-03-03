using UnityEngine;

public class Key : MonoBehaviour
{
    private bool playerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UIManager.Instance.ShowKeyPrompt("Press [E] to collect"); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UIManager.Instance.HideKeyPrompt(); 
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            KeyManager.Instance.AddKey(); 
            UIManager.Instance.HideKeyPrompt(); // hide the prompt once the key is collected
            Destroy(gameObject); // remove the key object from the scene
        }
    }
}
