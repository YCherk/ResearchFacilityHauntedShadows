// Necessary imports for using Unity's engine, navigation, and collections
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// A class to dynamically spawn keys and lanterns in the game environment
public class KeyAndLanternSpawner : MonoBehaviour
{
    // References to the key and lantern prefabs that will be spawned
    public GameObject keyPrefab;
    public GameObject lanternPrefab;

    // Search radius for finding a valid navigation mesh point near the intended spawn location
    public float searchRadius = 10f;

    // Maximum number of attempts to find a spawn location for each key
    public int maxAttemptsPerKey = 1000;

    // Offset distance for spawning lanterns near the keys to make sure they're not on top of each other
    public float lanternSpawnOffset = 1f;

    // The size of the area (width and length) within which we'll try to spawn keys and lanterns
    public Vector2 searchAreaSize = new Vector2(50f, 50f);

    // Layer mask to identify the ground, helping in positioning objects accurately
    public LayerMask groundLayer;

    // Maximum distance to check downward from a point to ensure it's above ground
    public float maxGroundCheckDistance = 10f;

    // Reference to the game's difficulty manager to adjust spawn rates according to game difficulty
    private DifficultyManager difficultyManager;

    // A list to keep track of all successfully spawned points to prevent overcrowding
    private List<Vector3> spawnPoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        // Attempt to find the DifficultyManager in the scene to access difficulty settings
        difficultyManager = FindObjectOfType<DifficultyManager>();
        if (difficultyManager == null)
        {
            Debug.LogError("DifficultyManager not found in the scene.");
            return; // Stop further execution if DifficultyManager is missing
        }

        // Determine the number of keys to spawn based on the current difficulty level
        int keyCount = GetKeyCountBasedOnDifficulty();

        // Proceed to spawn the calculated number of keys and corresponding lanterns
        SpawnKeysAndLanterns(keyCount);
    }

    // Determines the number of keys to spawn based on the game's difficulty setting
    int GetKeyCountBasedOnDifficulty()
    {
        // Switch between different difficulty levels to set the appropriate number of keys
        switch (difficultyManager.currentDifficulty)
        {
            case DifficultyManager.DifficultyLevel.Easy:
                return difficultyManager.easyKeys; // Lesser keys for an easier game
            case DifficultyManager.DifficultyLevel.Medium:
                return difficultyManager.mediumKeys; // Moderate number of keys
            case DifficultyManager.DifficultyLevel.Hard:
                return difficultyManager.hardKeys; // More keys for a challenging game
            default:
                return 0; // Default to 0 if an unrecognized difficulty level is encountered
        }
    }

    // Handles the spawning of keys and their corresponding lanterns
    void SpawnKeysAndLanterns(int keyCount)
    {
        for (int i = 0; i < keyCount; i++)
        {
            // Try to find a suitable location to spawn a key
            Vector3 keyPosition = TrySpawnKey();
            if (keyPosition != Vector3.zero) // Check if a valid position was found
            {
                // If a key was successfully placed, spawn a lantern nearby
                SpawnLanternNearKey(keyPosition);
            }
        }
    }

    // Attempts to find a suitable location for spawning a key, making multiple attempts if necessary
    Vector3 TrySpawnKey()
    {
        for (int attempts = 0; attempts < maxAttemptsPerKey; attempts++)
        {
            // Generate a random point within the specified search area
            Vector3 randomPoint = new Vector3(
                Random.Range(-searchAreaSize.x / 2, searchAreaSize.x / 2),
                0,
                Random.Range(-searchAreaSize.y / 2, searchAreaSize.y / 2)
            ) + transform.position;

            // Adjust the vertical position of the point to ensure it's grounded
            if (Physics.Raycast(randomPoint + Vector3.up * maxGroundCheckDistance, Vector3.down, out RaycastHit hitInfo, maxGroundCheckDistance * 2, groundLayer))
            {
                randomPoint.y = hitInfo.point.y; // Set the y-coordinate to the ground's level
            }
            else
            {
                continue; // Skip this attempt if no ground was found below the point
            }

            // Try to find a nearby point on the NavMesh for more accurate placement
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(randomPoint, out navMeshHit, searchRadius, NavMesh.AllAreas))
            {
                Vector3 spawnPoint = navMeshHit.position;

                // Ensure the new spawn point isn't too close to previously spawned items
                if (!IsPointTooCloseToOthers(spawnPoint))
                {
                    spawnPoints.Add(spawnPoint); // Add this point to our list of spawn points
                    Instantiate(keyPrefab, spawnPoint, Quaternion.identity); // Spawn the key prefab at the calculated position
                    return spawnPoint; // Return the successful spawn point
                }
            }
        }

        Debug.LogWarning("Failed to spawn key after " + maxAttemptsPerKey + " attempts.");
        return Vector3.zero; // Return a zero vector if unable to find a suitable spawn location
    }

    // Spawns a lantern near a given key's position
    void SpawnLanternNearKey(Vector3 keyPosition)
    {
        // Create a random offset within a sphere to place the lantern near the key
        Vector3 offset = Random.insideUnitSphere * lanternSpawnOffset;
        offset.y = 0; // Ensure the lantern remains on the same vertical level as the key
        Vector3 lanternPosition = keyPosition + offset; // Calculate the lantern's final position

        // Try to adjust the lantern's position to a valid NavMesh location
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(lanternPosition, out navMeshHit, lanternSpawnOffset, NavMesh.AllAreas))
        {
            Instantiate(lanternPrefab, navMeshHit.position, Quaternion.identity); // Spawn the lantern prefab at the adjusted position
        }
        else
        {
            Debug.LogWarning("Failed to place lantern near key."); // Log a warning if unable to place the lantern
        }
    }

    // Checks if a potential spawn point is too close to any previously spawned items
    private bool IsPointTooCloseToOthers(Vector3 point)
    {
        // Loop through all existing spawn points to compare distances
        foreach (var otherPoint in spawnPoints)
        {
            if (Vector3.Distance(point, otherPoint) < searchRadius) // Check if the new point is within the search radius of existing points
            {
                return true; // Return true if too close to an existing point
            }
        }
        return false; // Return false if no conflicts are found
    }
}
