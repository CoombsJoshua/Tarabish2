using Tarabish.Definitions;
using Tarabish.Extensions;
using Tarabish.UI.Elements;
using Tarabish.UI.Menu;
using UnityEngine;
using TMPro;
using Tarabish.UI.Playing; // Assuming TMP is used for dropdowns

namespace Tarabish
{
    public class SettingsManager : Singleton<SettingsManager>
    {
        private Menu_Settings _menuSettings => ReferenseManager.Instance.MenuSettings;

        [SerializeField] private TMP_Dropdown DifficultyDropdown; // Reference to the UI Dropdown

        public void Initialize()
        {
            // Update the Target Score
            int.TryParse(_menuSettings.Elements.ScoreToWin.Value, out int scoreToWin);
            RuleDefinitions.TargetScore = scoreToWin;

            // Update the Difficulty
            RuleDefinitions.Difficulty = GetSelectedDifficulty();
            Debug.Log($"Difficulty set to: {RuleDefinitions.Difficulty}");

            this.Log(new LogLineContent()
            {
                Level = ProjectManager.LogLevels.MESSAGE,
                Text = $"Score to win: {scoreToWin}, Difficulty: {RuleDefinitions.Difficulty}",
            });
        }

        private Difficulty GetSelectedDifficulty()
        {
            // Map the dropdown value to the Difficulty enum
            return (Difficulty)DifficultyDropdown.value; // Assumes dropdown values are 0=Beginner, 1=Intermediate, 2=Expert
        }
    }
}
