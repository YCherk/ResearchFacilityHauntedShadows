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
        }

        if (isFlashlightOn)
        {
            flashlightTimer += Time.deltaTime;
            flashlightBatteryBar.value = flashlightBatteryBar.maxValue - flashlightTimer;

            if (flashlightTimer >= flashlightBatteryBar.maxValue)
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

    void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        flashlight.enabled = isFlashlightOn;
    }

    void TurnOffFlashlight()
    {
        isFlashlightOn = false;
        flashlight.enabled = false;
    }

    public void PickupBattery()
    {
        flashlightTimer = 0;
        flashlightBatteryBar.value = flashlightBatteryBar.maxValue;
        canUseFlashlight = true;
        ShowMessage("Battery Collected");
    }

    private void ShowMessage(string message)
    {
        flashlightMessage.text = message;
        showMessage = true;
        messageTimer = 0; // Reset message timer
    }
}
