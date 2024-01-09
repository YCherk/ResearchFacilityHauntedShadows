using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class BatteryPickup : MonoBehaviour
{
    public Text pickupPrompt; // Reference to the UI Text for the pickup prompt
    private bool playerInRange = false; // Flag to check if the player is in range
    public Text collectText;

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
            // Attempt to get the FlashlightController component from the player
            FlashlightController flashlightController = FindObjectOfType<FlashlightController>();
            if (flashlightController != null)
            {
                flashlightController.PickupBattery();
                if (pickupPrompt != null)
                {
                    pickupPrompt.text = ""; // Hide the prompt
                }
                Destroy(gameObject); // Destroy the battery object
                StartCoroutine(DisplayDialogue("Battery Collected", 2));
            }
        }
    }
    private IEnumerator DisplayDialogue(string message, float duration)
    {
        collectText.text = message;
        collectText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        collectText.gameObject.SetActive(false);
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
