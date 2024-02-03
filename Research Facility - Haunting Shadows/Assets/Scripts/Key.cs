using UnityEngine;

public class Key : MonoBehaviour
{
    private bool playerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UIManager.Instance.ShowKeyPrompt("Press [E] to collect"); // Use the specific method for key prompts
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UIManager.Instance.HideKeyPrompt(); // Specifically hide the key prompt
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            KeyManager.Instance.AddKey(); // Assuming this method manages the collected keys
            UIManager.Instance.HideKeyPrompt(); // Hide the prompt once the key is collected
            Destroy(gameObject); // Remove the key object from the scene
        }
    }
}
