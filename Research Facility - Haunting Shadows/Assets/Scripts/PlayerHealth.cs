// These lines include necessary tools from Unity to work with UI elements and scenes.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// This is a blueprint for a player's health system.
public class PlayerHealth : MonoBehaviour
{
    // Public variables can be changed in Unity's editor.
    public int maxHealth = 100; // The maximum health a player can have.
    private int currentHealth; // The current health of the player, not visible in the editor.
    public Slider healthBarSlider; // A visual slider representing the player's health.
    public RawImage bloodSplatterRawImage; // An image for blood splatter effect.
    public Image screenTintPanel; // A full-screen image for changing screen color.
    public ScreenFader screenFader; // A component for fading the screen to black.
    public AudioClip[] damageAudioClips; // Sounds to play when the player is hurt.
    private int currentAudioClipIndex = 0; // To keep track of which audio clip to play next.

    // Variables for controlling visual effects.
    public float splatterDuration = 0.2f; // How long the blood splatter is visible.
    public float pulseDuration = 1f; // How long each pulse of the screen tint lasts.
    public float maxPulseAlpha = 0.4f; // The deepest color the pulse can reach.
    public int healthThresholdForPulse = 30; // Health level at which the screen starts pulsing.
    public float tintIntensity = 0.5f; // How strong the screen tint is.
    public AudioSource damageAudioSource; // An audio source for playing damage sounds.
    public AudioSource stabSound; // An audio source for playing a stabbing sound.

    private bool isPulsing = false; // Keeps track if the screen is currently pulsing.

    // This method runs once when the game starts.
    void Start()
    {
        currentHealth = maxHealth; // Set current health to maximum at start.
        InitializeHealthBar(); // Setup the health bar.
        bloodSplatterRawImage.color = new Color(1, 1, 1, 0); // Make blood splatter invisible.
        screenTintPanel.enabled = false; // Turn off screen tint at the start.
    }

    // This method is called to reduce the player's health.
    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // Subtract damage from current health.
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Make sure health is within bounds.
        UpdateHealthBar(); // Update the health bar UI.

        // Play a damage sound if available.
        if (damageAudioSource != null && damageAudioClips.Length > 0)
        {
            damageAudioSource.clip = damageAudioClips[currentAudioClipIndex];
            damageAudioSource.Play();
            currentAudioClipIndex = (currentAudioClipIndex + 1) % damageAudioClips.Length; // Move to the next audio clip.
        }

        // Play a stab sound if it's not already playing.
        if (stabSound != null && !stabSound.isPlaying)
        {
            stabSound.Play();
        }

        // Make sure the screen tint is visible when taking damage.
        if (screenTintPanel != null && !screenTintPanel.enabled)
        {
            screenTintPanel.enabled = true;
        }

        UpdateScreenTintAndSplatter(); // Update the screen tint and splatter effects.

        StopCoroutine("ShowBloodSplatter"); // Stop any ongoing blood splatter effect.
        StartCoroutine(ShowBloodSplatter()); // Start a new blood splatter effect.

        // Start or stop the screen pulsing effect based on current health.
        if (currentHealth <= healthThresholdForPulse && !isPulsing)
        {
            StartCoroutine(PulseScreenRed());
        }
        else if (currentHealth > healthThresholdForPulse && isPulsing)
        {
            StopCoroutine("PulseScreenRed");
            isPulsing = false;
        }

        // If health drops to 0, trigger the die method.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // This method increases the player's health.
    public void Heal(int amount)
    {
        currentHealth += amount; // Add the healing amount to current health.
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't exceed max.
        UpdateHealthBar(); // Update the health bar.

        // Turn off screen tint when healed.
        if (screenTintPanel != null)
        {
            screenTintPanel.enabled = false;
        }

        // Stop the pulsing effect if health goes above the threshold.
        if (currentHealth > healthThresholdForPulse && isPulsing)
        {
            StopCoroutine("PulseScreenRed");
            isPulsing = false;
        }
    }

    // Sets up the health bar UI.
    private void InitializeHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth; // The maximum value of the slider.
            healthBarSlider.value = currentHealth; // The current value of the slider.
        }
    }

    // Updates the health bar UI to reflect the current health.
    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth; // Update the slider's value.
        }
    }

    // Adjusts the screen tint and blood splatter based on current health.
    private void UpdateScreenTintAndSplatter()
    {
        float healthPercent = (float)currentHealth / maxHealth; // Calculate health percentage.
        float alpha = (1 - healthPercent) * tintIntensity; // Calculate transparency for effects.
        if (screenTintPanel.enabled)
        {
            screenTintPanel.color = new Color(1, 0, 0, alpha); // Update screen tint color.
        }
        bloodSplatterRawImage.color = new Color(1, 1, 1, alpha); // Update blood splatter transparency.
    }

    // Shows the blood splatter effect for a short time.
    IEnumerator ShowBloodSplatter()
    {
        bloodSplatterRawImage.color = new Color(1, 1, 1, 1); // Make splatter fully visible.
        yield return new WaitForSeconds(splatterDuration); // Wait for the duration of the splatter.
        bloodSplatterRawImage.color = new Color(1, 1, 1, 0); // Make splatter invisible again.
    }

    // Creates a pulsing red effect on the screen based on the player's health.
    IEnumerator PulseScreenRed()
    {
        isPulsing = true;
        while (isPulsing) // Keep pulsing while the condition is true.
        {
            float startAlpha = screenTintPanel.color.a; // Starting transparency.
            // Fade in the red tint.
            for (float t = 0; t <= pulseDuration; t += Time.deltaTime)
            {
                float normalizedTime = t / pulseDuration; // Calculate the progress of the pulse.
                screenTintPanel.color = new Color(1, 0, 0, Mathf.Lerp(startAlpha, maxPulseAlpha, normalizedTime)); // Update the color.
                yield return null; // Wait for the next frame.
            }
            // Fade out the red tint.
            for (float t = 0; t <= pulseDuration; t += Time.deltaTime)
            {
                float normalizedTime = t / pulseDuration; // Calculate the progress of the pulse.
                screenTintPanel.color = new Color(1, 0, 0, Mathf.Lerp(maxPulseAlpha, startAlpha, normalizedTime)); // Update the color.
                yield return null; // Wait for the next frame.
            }
        }
    }

    // Handles what happens when the player's health reaches 0.
    private void Die()
    {
        StopCoroutine("PulseScreenRed"); // Stop the pulsing effect.
        isPulsing = false; // Indicate that pulsing has stopped.
        screenFader.FadeToGameover(); // Trigger a transition to the game over state.
    }
}
