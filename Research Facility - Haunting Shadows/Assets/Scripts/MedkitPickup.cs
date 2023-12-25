using UnityEngine;
using UnityEngine.UI;

public class MedkitPickup : MonoBehaviour
{
    public Text pickupPrompt; // Reference to the UI Text for the pickup prompt
    private bool isPlayerInRange = false; // Tracks if the player is in range of the medkit

    private void Start()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.text = ""; // Initially hide the prompt
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PlayerHealth playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(playerHealth.maxHealth); // Fully heal the player
                ClearPickupPrompt(); // Clear the pickup prompt
                Destroy(gameObject); // Destroy the medkit
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
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
            ClearPickupPrompt(); // Clear the pickup prompt
        }
    }

    private void ClearPickupPrompt()
    {
        isPlayerInRange = false;
        if (pickupPrompt != null)
        {
            pickupPrompt.text = ""; // Clear the prompt text
        }
    }
}
