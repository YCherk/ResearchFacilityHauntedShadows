using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class DifficultySettingHandler : MonoBehaviour
{
    public TMP_Dropdown difficultyDropdown; // Change to TMP_Dropdown type

    void Start()
    {
        // Load the previously selected difficulty, defaulting to the first option if none was previously selected
        difficultyDropdown.value = PlayerPrefs.GetInt("SelectedDifficulty", 0);
    }

    public void OnDifficultyChanged()
    {
        // Save the selected difficulty
        PlayerPrefs.SetInt("SelectedDifficulty", difficultyDropdown.value);
        PlayerPrefs.Save();
    }
}
