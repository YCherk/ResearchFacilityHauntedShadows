using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    // Defines the different levels of game difficulty
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    [Header("Difficulty Settings")]
    // Current difficulty setting of the game
    public DifficultyLevel currentDifficulty = DifficultyLevel.Medium;

    [Header("Key Settings")]
    // Number of keys required to complete the game on Easy difficulty
    public int easyKeys = 3;
    // Number of keys required to complete the game on Medium difficulty
    public int mediumKeys = 5;
    // Number of keys required to complete the game on Hard difficulty
    public int hardKeys = 7;

    [Header("Enemy AI Settings")]
    // Reference to the EnemyAI script to adjust AI behavior based on difficulty
    public EnemyAI enemyAI;

    [Header("Easy Difficulty Settings")]
    // Movement speed of the enemy on Easy difficulty
    public float easyChaseSpeed = 3.0f;
    // Distance at which the enemy can spot the player on Easy difficulty
    public float easySightDistance = 10f;
    // Distance within which the enemy can attack the player on Easy difficulty
    public float easyAttackDistance = 1.5f;
    // Field of view angle of the enemy on Easy difficulty
    public float easyFieldOfView = 50f;
    // Sensitivity to player actions (e.g., noise) on Easy difficulty
    public float easySensitivity = 80f;
    // Threshold of player-made noise for the enemy to react on Easy difficulty
    public float easyLoudnessThreshold = 8f;

    [Header("Medium Difficulty Settings")]
    // Adjusted values for Medium difficulty, making the game more challenging than Easy
    public float mediumChaseSpeed = 4.0f;
    public float mediumSightDistance = 15f;
    public float mediumAttackDistance = 2.0f;
    public float mediumFieldOfView = 60f;
    public float mediumSensitivity = 100f;
    public float mediumLoudnessThreshold = 10f;

    [Header("Hard Difficulty Settings")]
    // Adjusted values for Hard difficulty, making the game significantly more challenging
    public float hardChaseSpeed = 5.0f;
    public float hardSightDistance = 20f;
    public float hardAttackDistance = 2.5f;
    public float hardFieldOfView = 70f;
    public float hardSensitivity = 120f;
    public float hardLoudnessThreshold = 12f;

    // On start, load the selected difficulty from player preferences and apply settings
    void Start()
    {
        currentDifficulty = (DifficultyLevel)PlayerPrefs.GetInt("SelectedDifficulty", (int)DifficultyLevel.Medium);
        ApplyDifficultySettings();
    }

    // Applies the difficulty settings to the game based on the selected difficulty level
    void ApplyDifficultySettings()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                // Apply Easy difficulty settings
                KeyManager.Instance.totalKeys = easyKeys;
                enemyAI.chaseSpeed = easyChaseSpeed;
                enemyAI.sightDistance = easySightDistance;
                enemyAI.attackDistance = easyAttackDistance;
                enemyAI.fieldOfView = easyFieldOfView;
                enemyAI.sensitivity = easySensitivity;
                enemyAI.loudnessThreshold = easyLoudnessThreshold;
                break;

            case DifficultyLevel.Medium:
                // Apply Medium difficulty settings
                KeyManager.Instance.totalKeys = mediumKeys;
                enemyAI.chaseSpeed = mediumChaseSpeed;
                enemyAI.sightDistance = mediumSightDistance;
                enemyAI.attackDistance = mediumAttackDistance;
                enemyAI.fieldOfView = mediumFieldOfView;
                enemyAI.sensitivity = mediumSensitivity;
                enemyAI.loudnessThreshold = mediumLoudnessThreshold;
                break;

            case DifficultyLevel.Hard:
                // Apply Hard difficulty settings
                KeyManager.Instance.totalKeys = hardKeys;
                enemyAI.chaseSpeed = hardChaseSpeed;
                enemyAI.sightDistance = hardSightDistance;
                enemyAI.attackDistance = hardAttackDistance;
                enemyAI.fieldOfView = hardFieldOfView;
                enemyAI.sensitivity = hardSensitivity;
                enemyAI.loudnessThreshold = hardLoudnessThreshold;
                break;
        }
    }
}
