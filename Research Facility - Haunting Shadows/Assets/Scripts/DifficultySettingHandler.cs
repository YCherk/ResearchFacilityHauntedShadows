using UnityEngine;
using TMPro; 

public class DifficultySettingHandler : MonoBehaviour
{
    public TMP_Dropdown difficultyDropdown; 

    void Start()
    {
        // Load the previoously selected difficulty, defaulting to the first option if none was previously selected
        difficultyDropdown.value = PlayerPrefs.GetInt("SelectedDifficulty", 0);
    }

    public void OnDifficultyChanged()
    {
        // saves th selected difficulty
        PlayerPrefs.SetInt("SelectedDifficulty", difficultyDropdown.value);
        PlayerPrefs.Save();
    }
}
