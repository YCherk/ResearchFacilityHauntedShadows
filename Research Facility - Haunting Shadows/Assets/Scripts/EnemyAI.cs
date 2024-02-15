using JetBrains.Annotations; // A set of annotations for improving code analysis.
using UnityEngine; // Unity's core namespace.
using UnityEngine.AI; // Namespace for the navigation and pathfinding system.
using System.Collections; // For using coroutines.
using UnityEngine.UI; // For using UI components like Text and Slider.

public class EnemyAI : MonoBehaviour
{
    // Variables for controlling enemy behavior:
    public float timeToOpenDoor = 10f; // How long the enemy waits before attempting to open a door.
    private float doorTimer = 0f; // Timer to track the time before opening a door.

    private float timeSinceLastSeen = 0f; // Tracks how long since the enemy last saw the player.
    public float timeToGiveUpChase = 5f; // After this time without seeing the player, the enemy gives up the chase.

    // Patrol behavior variables:
    public float patrolRadius = 10.0f; // How far from its starting point the enemy can patrol.
    public float patrolTimer = 5.0f; // How often the enemy changes its patrol destination.
    public float moveSpeed = 2.0f; // Normal moving speed.
    public float chaseSpeed = 4.0f; // Speed when chasing the player.
    public Transform player; // Reference to the player's position.
    public float attackDistance = 2.0f; // The distance within which the enemy can attack the player.
    public float sightDistance = 15.0f; // How far the enemy can see ahead.
    public float backSightDistance = 5.0f; // How far the enemy can see behind.
    public float fieldOfView = 60.0f; // The angle for the enemy's forward field of view.
    public float backFieldOfView = 90.0f; // The angle for the enemy's backward field of view.

    // UI elements for displaying warnings and jump scares:
    public Text warningText; // Text component for displaying warnings to the player.
    public CanvasGroup warningTextCanvasGroup; // Canvas group for fading the warning text.
    public float fadeDuration = 0.5f; // Duration of the fade effect.

    public GameObject jumpscareCanvas; // Canvas for jump scare effects.
    private float chaseDuration = 0f; // How long the enemy has been chasing the player.
    private bool isJumpscareTriggered = false; // Flag to track if a jump scare has been triggered.
    public AudioSource jumpScare; // Audio source for playing jump scare sounds.
    public CanvasGroup jumpscareCanvasGroup; // Canvas group for controlling jump scare visuals.
    public float jumpscareChance = 0.3f; // Probability of triggering a jump scare.
    private Coroutine jumpscareCoroutine = null; // Reference to the jump scare coroutine.

    // Audio sources for different enemy actions:
    public AudioSource walkingAudioSource; // Plays while the enemy is walking.
    public AudioSource runningAudioSource; // Plays while the enemy is running.
    public AudioSource attackAudioSource; // Plays when the enemy attacks.
    public AudioSource knockOnDoor; // Plays when the enemy knocks on a door.

    public AudioClip[] audioClips; // An array of audio clips that can be played.
    public float audioPlayChance = 0.5f; // Chance of playing an audio clip.
    private bool isAudioPlaying = false; // Flag to prevent multiple audio clips from playing simultaneously.

    // Variables for microphone input to detect player noises:
    public float sensitivity = 100; // Sensitivity to microphone input.
    public float loudnessThreshold = 10; // The loudness level that triggers the enemy's response.
    public Slider volumeSlider; // UI slider to display microphone volume.
    private AudioClip microphoneInput; // The audio clip that records microphone input.
    private bool isMicrophoneInitialized = false; // Flag to check if the microphone has been initialized.
    public int sampleWindow = 64; // The size of the sample window for analyzing audio data.
    public Text dialogueScreamText; // Text component for displaying scream-related dialogues.

    private float timer; // General purpose timer.
    private NavMeshAgent agent; // The NavMesh agent component for pathfinding.
    private Animator animator; // Animator component for controlling animations.
    private bool isChasingPlayer = false; // Flag to indicate if the enemy is currently chasing the player.
    private bool isSearchingForPlayer = false; // Flag to indicate if the enemy is searching for the player.
    private Vector3 lastKnownPlayerPosition; // The last known position of the player.

    public float minFlickerDuration = 0.1f; // Minimum duration of flickering effect.
    public float maxFlickerDuration = 0.5f; // Maximum duration of flickering effect.
    private Coroutine flickerCoroutine = null; // Reference to the flicker effect coroutine.

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component attached to this enemy.
        animator = GetComponent<Animator>(); // Get the Animator component attached to this enemy.
        timer = patrolTimer; // Initialize the patrol timer.
        agent.speed = moveSpeed; // Set the agent's speed to the moveSpeed variable.
        agent.updateRotation = true; // Allow the agent to update its rotation to match the direction it's moving.
        InitializeMicrophone(); // Call the method to initialize microphone input.
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Calculate the distance to the player.
        bool canSeePlayer = CanSeePlayer(distanceToPlayer); // Check if the enemy can see the player based on distance and field of view.
        bool canSeePlayerFromBehind = CanSeePlayerFromBehind(distanceToPlayer); // Check if the enemy can see the player from behind.

        // Set animation states based on the enemy's state:
        animator.SetBool("WalkForward", true); // Always walk forward by default.
        animator.SetBool("isRunning", false); // Not running by default.
        animator.SetBool("IsAttack", false); // Not attacking by default.
        animator.SetBool("isIdle", false); // Not idle by default.

        if (isMicrophoneInitialized)
        {
            int micPosition = Microphone.GetPosition(null) - sampleWindow; // Calculate the position in the audio clip.
            if (micPosition < 0) return; // If the position is negative, exit the method.

            float micLoudness = GetAveragedVolume(micPosition, microphoneInput) * sensitivity; // Calculate the loudness of the microphone input.
            volumeSlider.value = Mathf.Clamp(micLoudness, 0, volumeSlider.maxValue); // Update the volume slider with the loudness value.

            if (micLoudness > loudnessThreshold) // If the loudness exceeds the threshold:
            {
                StartCoroutine(DisplayWarning()); // Start the coroutine to display a warning message.
                TurnTowardsPlayer(); // Turn the enemy towards the player.
                isChasingPlayer = true; // Set the chasing flag to true.
                isSearchingForPlayer = false; // Set the searching flag to false.
                lastKnownPlayerPosition = player.position; // Update the last known position of the player.
                agent.SetDestination(player.position); // Set the NavMeshAgent's destination to the player's position.
                animator.SetBool("isRunning", true); // Set the running animation.
                PlayRandomAudioClip(); // Play a random audio clip.
            }
        }

        if (canSeePlayer || canSeePlayerFromBehind) // If the enemy can see the player:
        {
            TurnTowardsPlayer(); // Turn the enemy towards the player.
            isChasingPlayer = true; // Set the chasing flag to true.
            isSearchingForPlayer = false; // Set the searching flag to false.
            lastKnownPlayerPosition = player.position; // Update the last known position of the player.
            agent.SetDestination(player.position); // Set the NavMeshAgent's destination to the player's position.
            animator.SetBool("isRunning", true); // Set the running animation.
            ManageFlickering(); // Start the flickering effect if not already started.

            chaseDuration += Time.deltaTime; // Increment the chase duration timer.

            if (chaseDuration > 15f && !isJumpscareTriggered && Random.value < jumpscareChance) // If the chase has lasted long enough and the conditions for a jump scare are met:
            {
                isJumpscareTriggered = true; // Set the jump scare flag to true.
                jumpscareCoroutine = StartCoroutine(TriggerJumpscare()); // Start the jump scare coroutine.
            }

            if (distanceToPlayer <= attackDistance) // If the enemy is within attack distance:
            {
                animator.SetBool("IsAttack", true); // Set the attack animation.
                PlayRandomAudioClip(); // Play a random audio clip.
            }
            else // If the enemy is not within attack distance:
            {
                animator.SetBool("IsAttack", false); // Reset the attack animation.
            }
        }
        else if (isChasingPlayer) // If the enemy is currently chasing the player but can no longer see them:
        {
            timeSinceLastSeen += Time.deltaTime; // Increment the time since last seen timer.
            if (timeSinceLastSeen > timeToGiveUpChase) // If enough time has passed without seeing the player:
            {
                isChasingPlayer = false; // Reset the chasing flag.
                isSearchingForPlayer = true; // Set the searching flag to true.
                StopFlickering(); // Stop the flickering effect.
                lastKnownPlayerPosition = player.position; // Update the last known position of the player.
                agent.SetDestination(lastKnownPlayerPosition); // Set the NavMeshAgent's destination to the last known player position.
            }
        }
        else if (isSearchingForPlayer) // If the enemy is searching for the player:
        {
            doorTimer += Time.deltaTime; // Increment the door timer.
            if (doorTimer >= timeToOpenDoor) // If enough time has passed to attempt opening a door:
            {
                CheckAndOpenDoor(); // Check for and attempt to open doors.
                doorTimer = 0f; // Reset the door timer.
            }

            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f) // If the enemy has reached the last known player position:
            {
                isSearchingForPlayer = false; // Reset the searching flag.
                Patrol(); // Resume patrolling.
            }
            else // If the enemy is on its way to the last known player position:
            {
                animator.SetBool("isRunning", true); // Set the running animation.
            }
        }
        else // If the enemy is not chasing, attacking, or searching for the player:
        {
            Patrol(); // Resume patrolling.
        }

        HandleAudioPlayback(); // Handle audio playback based on the enemy's state.
    }

    private void CheckAndOpenDoor()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f); // Perform a sphere overlap check to find nearby doors.
        foreach (var hitCollider in hitColliders) // Iterate through all colliders found by the overlap check.
        {
            if (hitCollider.CompareTag("Door")) // Check if the collider belongs to a door.
            {
                DoorController door = hitCollider.GetComponent<DoorController>(); // Get the DoorController component from the door.
                if (door != null) // If the door has a DoorController component:
                {
                    door.ForceOpenDoor(); // Call the method to force the door open.
                }
            }
        }
    }

    public void SetDifficultyParameters(float newChaseSpeed, float newSightDistance, float newAttackDistance, float newFieldOfView, float newSensitivity, float newLoudnessThreshold)
    {
        // This method allows external scripts to adjust the enemy's difficulty parameters dynamically.
        chaseSpeed = newChaseSpeed; // Update the chase speed.
        sightDistance = newSightDistance; // Update the sight distance.
        attackDistance = newAttackDistance; // Update the attack distance.
        fieldOfView = newFieldOfView; // Update the field of view.
        sensitivity = newSensitivity; // Update the microphone sensitivity.
        loudnessThreshold = newLoudnessThreshold; // Update the loudness threshold.
    }

    private void ManageFlickering()
    {
        if (flickerCoroutine == null) // If the flicker coroutine is not already running:
        {
            flickerCoroutine = StartCoroutine(Flicker()); // Start the flicker coroutine.
        }
    }

    private void StopFlickering()
    {
        if (flickerCoroutine != null) // If the flicker coroutine is running:
        {
            StopCoroutine(flickerCoroutine); // Stop the flicker coroutine.
            flickerCoroutine = null; // Reset the coroutine reference.
            SetEnemyVisibility(true); // Make the enemy fully visible.
        }
    }

    IEnumerator Flicker()
    {
        while (true) // Loop indefinitely:
        {
            float flickerTime = Random.Range(minFlickerDuration, maxFlickerDuration); // Randomly choose a duration for the flicker.

            SetEnemyVisibility(!IsEnemyVisible()); // Toggle the enemy's visibility.
            agent.speed = (float)(IsEnemyVisible() ? chaseSpeed : chaseSpeed * 1.5); // Adjust the enemy's speed based on visibility.

            yield return new WaitForSeconds(flickerTime); // Wait for the duration of the flicker before toggling visibility again.
        }
    }

    private void SetEnemyVisibility(bool isVisible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(); // Get all Renderer components attached to the enemy and its children.
        foreach (var renderer in renderers) // Iterate through each Renderer:
        {
            renderer.enabled = isVisible; // Set the renderer's visibility based on the isVisible parameter.
        }
    }

    private bool IsEnemyVisible()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(); // Get all Renderer components attached to the enemy and its children.
        if (renderers.Length > 0) // If there are any Renderer components:
        {
            return renderers[0].enabled; // Return the visibility state of the first Renderer.
        }
        return true; // Default to true if no Renderers are found.
    }

    IEnumerator TriggerJumpscare()
    {
        isJumpscareTriggered = true; // Set the jump scare flag to true.
        jumpScare.Play(); // Play the jump scare audio.
        jumpscareCanvas.SetActive(true); // Activate the jump scare canvas.

        float flashDuration = 0.1f; // Set the duration of each flash.
        int numberOfFlashes = 3; // Set the total number of flashes.

        for (int i = 0; i < numberOfFlashes; i++) // Loop through each flash:
        {
            jumpscareCanvasGroup.alpha = 1; // Set the canvas group's alpha to fully opaque for the flash.
            yield return new WaitForSeconds(flashDuration); // Wait for the duration of the flash.

            jumpscareCanvasGroup.alpha = 0; // Set the canvas group's alpha to fully transparent after the flash.
            yield return new WaitForSeconds(flashDuration); // Wait for the duration between flashes.
        }

        jumpscareCanvasGroup.alpha = 1; // Set the canvas group's alpha to fully opaque one last time.

        jumpscareCanvas.SetActive(false); // Deactivate the jump scare canvas.
        yield return new WaitForSeconds(1f); // Wait for a moment after the jump scare.

        jumpscareCoroutine = null; // Reset the coroutine reference.
    }

    void PlayRandomAudioClip()
    {
        if (audioClips.Length > 0 && !isAudioPlaying) // If there are audio clips available and audio is not currently playing:
        {
            int clipIndex = Random.Range(0, audioClips.Length); // Randomly select an audio clip index.
            AudioClip clipToPlay = audioClips[clipIndex]; // Get the selected audio clip.
            AudioSource.PlayClipAtPoint(clipToPlay, transform.position); // Play the selected audio clip at the enemy's position.
            isAudioPlaying = true; // Set the audio playing flag to true.

            StartCoroutine(ResetAudioPlayingFlag(clipToPlay.length)); // Start a coroutine to reset the audio playing flag after the clip finishes playing.
        }
    }

    IEnumerator ResetAudioPlayingFlag(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay.
        isAudioPlaying = false; // Reset the audio playing flag to false.
    }

    IEnumerator DisplayWarning()
    {
        warningText.text = "It heard you"; // Set the warning text.
        warningText.gameObject.SetActive(true); // Activate the warning text GameObject.

        float elapsedTime = 0; // Initialize elapsed time for the fade effect.
        while (elapsedTime < fadeDuration) // For the duration of the fade effect:
        {
            warningTextCanvasGroup.alpha = elapsedTime / fadeDuration; // Calculate and set the canvas group's alpha based on elapsed time.
            elapsedTime += Time.deltaTime; // Increment elapsed time by the time since the last frame.
            yield return null; // Wait for the next frame.
        }
        warningTextCanvasGroup.alpha = 1; // Ensure the canvas group's alpha is fully opaque.

        yield return new WaitForSeconds(2); // Wait for 2 seconds with the warning visible.

        elapsedTime = 0; // Reset elapsed time for the fade out effect.
        while (elapsedTime < fadeDuration) // For the duration of the fade out effect:
        {
            warningTextCanvasGroup.alpha = 1 - (elapsedTime / fadeDuration); // Calculate and set the canvas group's alpha for the fade out.
            elapsedTime += Time.deltaTime; // Increment elapsed time by the time since the last frame.
            yield return null; // Wait for the next frame.
        }
        warningTextCanvasGroup.alpha = 0; // Ensure the canvas group's alpha is fully transparent.

        warningText.gameObject.SetActive(false); // Deactivate the warning text GameObject.
    }

    void InitializeMicrophone()
    {
        int sampleRate = 44100; // Set the standard sampling rate for the microphone.
        int recordingLength = 10; // Set the length of the recording in seconds.
        microphoneInput = Microphone.Start(null, true, recordingLength, sampleRate); // Start the microphone with the specified settings.
        isMicrophoneInitialized = true; // Set the microphone initialized flag to true.
    }

    public float GetAveragedVolume(int clipPosition, AudioClip clip)
    {
        int startPosition = clipPosition - sampleWindow; // Calculate the starting position for the sample window.

        if (startPosition < 0) return 0; // If the starting position is negative, return 0.

        float[] waveData = new float[sampleWindow]; // Create an array to store the wave data.

        clip.GetData(waveData, startPosition); // Get the wave data from the audio clip.

        float totalLoudness = 0; // Initialize the total loudness.
        foreach (var sample in waveData) // For each sample in the wave data:
        {
            totalLoudness += Mathf.Abs(sample); // Add the absolute value of the sample to the total loudness.
        }

        return totalLoudness / sampleWindow; // Return the average loudness.
    }

    void HandleAudioPlayback()
    {
        if (IsWalking() && !walkingAudioSource.isPlaying) // If the enemy is walking and the walking audio is not already playing:
        {
            walkingAudioSource.Play(); // Play the walking audio.
            runningAudioSource.Stop(); // Stop the running audio.
            attackAudioSource.Stop(); // Stop the attack audio.
        }
        else if (isChasingPlayer && !runningAudioSource.isPlaying) // If the enemy is chasing the player and the running audio is not already playing:
        {
            runningAudioSource.Play(); // Play the running audio.
            walkingAudioSource.Stop(); // Stop the walking audio.
            attackAudioSource.Stop(); // Stop the attack audio.
        }
        else if (IsIdle()) // If the enemy is idle:
        {
            walkingAudioSource.Stop(); // Stop the walking audio.
            runningAudioSource.Stop(); // Stop the running audio.
            attackAudioSource.Stop(); // Stop the attack audio.
        }
    }

    bool IsWalking()
    {
        // Check if the enemy is moving but not chasing or searching for the player.
        return !isChasingPlayer && !isSearchingForPlayer && agent.velocity.magnitude > 0.01f;
    }

    bool IsIdle()
    {
        // Check if the enemy is not moving.
        return agent.velocity.magnitude < 0.01f;
    }

    public void AttackPlayer()
    {
        // If the enemy is within attack distance of the player, inflict damage on the player.
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(30); // Call the TakeDamage method on the player's health component.
        }
    }

    bool CanSeePlayer(float distanceToPlayer)
    {
        // Check if the player is within the enemy's forward sight distance and field of view.
        if (distanceToPlayer <= sightDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized; // Calculate the direction to the player.
            float angle = Vector3.Angle(transform.forward, directionToPlayer); // Calculate the angle between the enemy's forward direction and the direction to the player.
            if (angle <= fieldOfView / 2) // If the angle is within the enemy's field of view:
            {
                RaycastHit hit; // Variable to store hit information.
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, sightDistance)) // Perform a raycast towards the player.
                {
                    return hit.transform == player; // Return true if the raycast hits the player.
                }
            }
        }
        return false; // Return false if the player is not within sight.
    }

    bool CanSeePlayerFromBehind(float distanceToPlayer)
    {
        // Check if the player is within the enemy's backward sight distance and field of view.
        if (distanceToPlayer <= backSightDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized; // Calculate the direction to the player.
            float angle = Vector3.Angle(-transform.forward, directionToPlayer); // Calculate the angle between the enemy's backward direction and the direction to the player.
            if (angle <= backFieldOfView / 2) // If the angle is within the enemy's backward field of view:
            {
                RaycastHit hit; // Variable to store hit information.
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, backSightDistance)) // Perform a raycast towards the player.
                {
                    return hit.transform == player; // Return true if the raycast hits the player.
                }
            }
        }
        return false; // Return false if the player is not within backward sight.
    }

    void Patrol()
    {
        // Method to handle patrolling behavior.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) // If the agent is not calculating a path and has reached its destination:
        {
            if (agent.velocity.sqrMagnitude == 0f) // If the agent has stopped moving:
            {
                timer += Time.deltaTime; // Increment the patrol timer.
            }

            if (timer >= patrolTimer) // If the patrol timer exceeds the set duration:
            {
                Vector3 newPos = RandomNavSphere(transform.position, patrolRadius, -1); // Calculate a new patrol destination.
                agent.SetDestination(newPos); // Set the agent's destination to the new position.
                timer = 0; // Reset the patrol timer.
            }
        }
        else // If the agent is moving towards a destination:
        {
            timer = 0; // Reset the patrol timer.
        }

        animator.SetBool("WalkForward", agent.velocity.magnitude > 0.1f); // Set the walking forward animation based on the agent's velocity.
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        // Static method to calculate a random patrol destination within a specified radius.
        Vector3 randDirection = Random.insideUnitSphere * dist; // Generate a random direction.

        for (int i = 0; i < 30; i++) // Attempt multiple times to find a valid position:
        {
            Vector3 randomPos = origin + randDirection; // Calculate a potential position.
            NavMeshHit navHit; // Variable to store NavMesh hit information.
            if (NavMesh.SamplePosition(randomPos, out navHit, dist, layermask)) // Sample the NavMesh to find a nearby valid position.
            {
                return navHit.position; // Return the valid position.
            }
        }
        return origin; // Return the original position if no valid position is found.
    }

    private void TurnTowardsPlayer()
    {
        // Method to turn the enemy towards the player smoothly.
        if (!isChasingPlayer || Vector3.Distance(transform.position, player.position) <= attackDistance) return; // Exit if not chasing or too close to the player.

        Vector3 directionToPlayer = (player.position - transform.position).normalized; // Calculate the direction to the player.
        directionToPlayer.y = 0; // Eliminate any vertical component to ensure rotation only on the horizontal plane.

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer); // Calculate the target rotation towards the player.
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 100f); // Smoothly rotate towards the target rotation.
    }
}
