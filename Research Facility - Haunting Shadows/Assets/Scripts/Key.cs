using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    public Text pickupPrompt; // Reference to the UI Text for the pickup prompt
    private bool playerInRange = false; // Flag to check if the player is in range

    private void Start()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.text = ""; // Initially hide the prompt
        }
    }

    private void Update()
    {
        // Check if the player is in range and the E key is pressed
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            KeyManager.Instance.AddKey();
            if (pickupPrompt != null)
            {
                pickupPrompt.text = ""; // Hide the prompt
            }
            Destroy(gameObject); // Destroy the key object
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true; // Set flag to true
            if (pickupPrompt != null)
            {
                pickupPrompt.text = "Press [E] to collect"; // Show the prompt
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false; // Set flag to false
            if (pickupPrompt != null)
            {
                pickupPrompt.text = ""; // Hide the prompt
            }
        }
    }
}
