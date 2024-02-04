using UnityEngine;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    public Light flashlight;
    public Text flashlightMessage;
    public Slider flashlightBatteryBar;

    private bool isFlashlightOn = false;
    private bool canUseFlashlight = true;
    private float flashlightTimer = 0;
    private float flashlightDuration = 30f;
    private float messageTimer = 0; // Separate timer for message display
    private bool showMessage = false;
    public AudioSource flashlightSound;
    private bool isFlickering = false;
    private float flickerDuration = 0.1f;
    private float nextFlickerTime = 0f;
    private float minFlickerIntensity = 0.5f; // Minimum intensity during flicker
    private float maxFlickerIntensity = 1f; // Maximum intensity (original intensity)

    void Start()
    {
        flashlightMessage.text = "";
        flashlightBatteryBar.maxValue = flashlightDuration;
        flashlightBatteryBar.value = flashlightBatteryBar.maxValue;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && canUseFlashlight)
        {
            ToggleFlashlight();
            flashlightSound.Play();
        }

        if (isFlashlightOn)
        {
            flashlightTimer += Time.deltaTime;
            flashlightBatteryBar.value = flashlightBatteryBar.maxValue - flashlightTimer;

            // Check if the battery life is less than or equal to 10 seconds
            if (flashlightTimer >= flashlightDuration - 10 && flashlightTimer < flashlightDuration)
            {
                if (!isFlickering)
                {
                    isFlickering = true; // Start flickering
                }

                // Flickering logic
                if (Time.time >= nextFlickerTime)
                {
                    FlickerLight();
                    nextFlickerTime = Time.time + flickerDuration;
                }
            }
            else if (flashlightTimer >= flashlightDuration)
            {
                TurnOffFlashlight();
                ShowMessage("Damn, flashlight is out, I need to find some batteries.");
                canUseFlashlight = false;
            }
        }


        if (showMessage)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer >= 3f) // Clears the message after 3 seconds
            {
                flashlightMessage.text = "";
                showMessage = false;
                messageTimer = 0; // Reset message timer
            }
        }
    }
    void FlickerLight()
    {
        if (!isFlickering) return; // Only flicker when isFlickering is true

        flashlight.intensity = Random.Range(minFlickerIntensity, maxFlickerIntensity);
    }

    void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        flashlight.enabled = isFlashlightOn;
        // Reset flickering to false every time the flashlight is toggled to avoid it starting in a flicker state if turned off and on again quickly
        isFlickering = false;
    }

    void TurnOffFlashlight()
    {
        isFlashlightOn = false;
        flashlight.enabled = false;
        isFlickering = false; // Stop flickering when the flashlight is turned off
    }

    public void PickupBattery()
    {
        flashlightTimer = 0;
        flashlightBatteryBar.value = flashlightBatteryBar.maxValue;
        canUseFlashlight = true;
        isFlickering = false; // Make sure to reset flickering when a battery is picked up
        ShowMessage("Battery Collected");
    }

    private void ShowMessage(string message)
    {
        flashlightMessage.text = message;
        showMessage = true;
        messageTimer = 0; // Reset message timer
    }
}
