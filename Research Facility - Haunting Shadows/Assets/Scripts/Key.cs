using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    public Text pickupPrompt; // Reference to the UI Text for the pickup prompt

    private void Start()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.text = ""; // Initially hide the prompt
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (pickupPrompt != null)
            {
                pickupPrompt.text = "Press [E] to collect"; // Show the prompt
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            KeyManager.Instance.AddKey();
            if (pickupPrompt != null)
            {
                pickupPrompt.text = ""; // Hide the prompt
            }
            Destroy(gameObject); // Destroy the key object
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (pickupPrompt != null)
            {
                pickupPrompt.text = ""; // Hide the prompt
            }
        }
    }
}
