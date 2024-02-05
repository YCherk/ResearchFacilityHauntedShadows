using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class KeyAndLanternSpawner : MonoBehaviour
{
    public GameObject keyPrefab;
    public GameObject lanternPrefab;
    public float searchRadius = 10f; // Radius for searching a valid NavMesh point
    public int maxAttemptsPerKey = 1000;
    public float lanternSpawnOffset = 1f;
    public Vector2 searchAreaSize = new Vector2(50f, 50f); // Size of the area in which to search for spawn points
    public LayerMask groundLayer; // Layer mask to identify the ground layer
    public float maxGroundCheckDistance = 10f; // Maximum distance to check for ground

    private DifficultyManager difficultyManager;
    private List<Vector3> spawnPoints = new List<Vector3>();

    void Start()
    {
        difficultyManager = FindObjectOfType<DifficultyManager>();
        if (difficultyManager == null)
        {
            Debug.LogError("DifficultyManager not found in the scene.");
            return;
        }

        int keyCount = GetKeyCountBasedOnDifficulty();
        SpawnKeysAndLanterns(keyCount);
    }

    int GetKeyCountBasedOnDifficulty()
    {
        switch (difficultyManager.currentDifficulty)
        {
            case DifficultyManager.DifficultyLevel.Easy:
                return difficultyManager.easyKeys;
            case DifficultyManager.DifficultyLevel.Medium:
                return difficultyManager.mediumKeys;
            case DifficultyManager.DifficultyLevel.Hard:
                return difficultyManager.hardKeys;
            default:
                return 0;
        }
    }

    void SpawnKeysAndLanterns(int keyCount)
    {
        for (int i = 0; i < keyCount; i++)
        {
            Vector3 keyPosition = TrySpawnKey();
            if (keyPosition != Vector3.zero)
            {
                SpawnLanternNearKey(keyPosition);
            }
        }
    }

    Vector3 TrySpawnKey()
    {
        for (int attempts = 0; attempts < maxAttemptsPerKey; attempts++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-searchAreaSize.x / 2, searchAreaSize.x / 2),
                0,
                Random.Range(-searchAreaSize.y / 2, searchAreaSize.y / 2)
            ) + transform.position;

            // Adjust vertical position to be at ground level
            if (Physics.Raycast(randomPoint + Vector3.up * maxGroundCheckDistance, Vector3.down, out RaycastHit hitInfo, maxGroundCheckDistance * 2, groundLayer))
            {
                randomPoint.y = hitInfo.point.y;
            }
            else
            {
                continue; // Skip this location if no ground was found below
            }

            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(randomPoint, out navMeshHit, searchRadius, NavMesh.AllAreas))
            {
                Vector3 spawnPoint = navMeshHit.position;

                if (!IsPointTooCloseToOthers(spawnPoint))
                {
                    spawnPoints.Add(spawnPoint);
                    Instantiate(keyPrefab, spawnPoint, Quaternion.identity);
                    return spawnPoint;
                }
            }
        }

        Debug.LogWarning("Failed to spawn key after " + maxAttemptsPerKey + " attempts.");
        return Vector3.zero;
    }

    void SpawnLanternNearKey(Vector3 keyPosition)
    {
        Vector3 offset = Random.insideUnitSphere * lanternSpawnOffset;
        offset.y = 0;
        Vector3 lanternPosition = keyPosition + offset;

        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(lanternPosition, out navMeshHit, lanternSpawnOffset, NavMesh.AllAreas))
        {
            Instantiate(lanternPrefab, navMeshHit.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Failed to place lantern near key.");
        }
    }

    private bool IsPointTooCloseToOthers(Vector3 point)
    {
        foreach (var otherPoint in spawnPoints)
        {
            if (Vector3.Distance(point, otherPoint) < searchRadius)
            {
                return true;
            }
        }
        return false;
    }
}
