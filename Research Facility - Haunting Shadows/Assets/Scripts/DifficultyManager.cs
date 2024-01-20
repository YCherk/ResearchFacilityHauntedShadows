using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    [Header("Difficulty Settings")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Medium;

    [Header("Key Settings")]
    public int easyKeys = 3;
    public int mediumKeys = 5;
    public int hardKeys = 7;

    [Header("Enemy AI Settings")]
    public EnemyAI enemyAI;

    [Header("Easy Difficulty Settings")]
    public float easyChaseSpeed = 3.0f;
    public float easySightDistance = 10f;
    public float easyAttackDistance = 1.5f;
    public float easyFieldOfView = 50f;
    public float easySensitivity = 80f;
    public float easyLoudnessThreshold = 8f;

    [Header("Medium Difficulty Settings")]
    public float mediumChaseSpeed = 4.0f;
    public float mediumSightDistance = 15f;
    public float mediumAttackDistance = 2.0f;
    public float mediumFieldOfView = 60f;
    public float mediumSensitivity = 100f;
    public float mediumLoudnessThreshold = 10f;

    [Header("Hard Difficulty Settings")]
    public float hardChaseSpeed = 5.0f;
    public float hardSightDistance = 20f;
    public float hardAttackDistance = 2.5f;
    public float hardFieldOfView = 70f;
    public float hardSensitivity = 120f;
    public float hardLoudnessThreshold = 12f;

    void Start()
    {
        currentDifficulty = (DifficultyLevel)PlayerPrefs.GetInt("SelectedDifficulty", (int)DifficultyLevel.Medium);
        ApplyDifficultySettings();
    }

    void ApplyDifficultySettings()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                KeyManager.Instance.totalKeys = easyKeys;
                enemyAI.chaseSpeed = easyChaseSpeed;
                enemyAI.sightDistance = easySightDistance;
                enemyAI.attackDistance = easyAttackDistance;
                enemyAI.fieldOfView = easyFieldOfView;
                enemyAI.sensitivity = easySensitivity;
                enemyAI.loudnessThreshold = easyLoudnessThreshold;
                break;

            case DifficultyLevel.Medium:
                KeyManager.Instance.totalKeys = mediumKeys;
                enemyAI.chaseSpeed = mediumChaseSpeed;
                enemyAI.sightDistance = mediumSightDistance;
                enemyAI.attackDistance = mediumAttackDistance;
                enemyAI.fieldOfView = mediumFieldOfView;
                enemyAI.sensitivity = mediumSensitivity;
                enemyAI.loudnessThreshold = mediumLoudnessThreshold;
                break;

            case DifficultyLevel.Hard:
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
