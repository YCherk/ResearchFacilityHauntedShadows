using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    public enum PickupType { Battery, Medkit }
    public PickupType pickupType; // Define the type of pickup this instance represents

    public Text pickupPrompt; // Reference to the UI Text for the pickup prompt
    private bool isPlayerInRange = false; // Tracks if the player is in range of the item
    public Text collectText; // Text for displaying the collection message

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
            HandleItemPickup();
        }
    }

    private void HandleItemPickup()
    {
        switch (pickupType)
        {
            case PickupType.Battery:
                PickUpBattery();
                break;
            case PickupType.Medkit:
                PickUpMedkit();
                break;
        }

        isPlayerInRange = false;
        UpdatePickupPrompt(); // Update the prompt after picking up the item
    }

    private void PickUpBattery()
    {
        FlashlightController flashlightController = FindObjectOfType<FlashlightController>();
        if (flashlightController != null)
        {
            flashlightController.PickupBattery();
            UIManager.Instance.ShowMessage("Battery Collected", 2);
            Destroy(gameObject); // Destroy the battery object
        }
    }

    private void PickUpMedkit()
    {
        PlayerHealth playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(playerHealth.maxHealth); // Fully heal the player
            UIManager.Instance.ShowMessage("Medkit Collected", 2);
            Destroy(gameObject); // Destroy the medkit
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            UpdatePickupPrompt(); // Show the prompt when the player is in range
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            UpdatePickupPrompt(); // Hide the prompt when the player leaves the range
        }
    }

    private void UpdatePickupPrompt()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.text = isPlayerInRange ? "Press [E] to collect" : "";
        }
    }
}
