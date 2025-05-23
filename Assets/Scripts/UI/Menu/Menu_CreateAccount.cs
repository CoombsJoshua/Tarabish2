using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Core;

namespace Tarabish.UI.Menu
{
    public class Menu_CreateAccount : Menu_
    {
        [System.Serializable]
        public class ButtonReferences
        {
            public Button Start; // Button to submit the name
        }

        public TMP_InputField NameInputField; // Input field for entering the name
        public TextMeshProUGUI FeedbackText; // Text field to display feedback to the player
        public ButtonReferences Buttons = new ButtonReferences();

        public override MenuType MenuType => MenuType.CreateAccount;

        private void OnEnable()
        {
            // Attach a listener to the Start button to handle setting the player name
            Buttons.Start.onClick.AddListener(async () => await SetPlayerNameAsync());
        }

        private async Task SetPlayerNameAsync()
        {
            string playerName = NameInputField.text.Trim();

            // Validate the input
            if (string.IsNullOrEmpty(playerName))
            {
                SetFeedback("Player name cannot be empty!", true);
                return;
            }

            if (playerName.Length < 3 || playerName.Length > 20)
            {
                SetFeedback("Name must be between 3 and 20 characters!", true);
                return;
            }

            try
            {
                // Set the player name using Unity Services
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                Debug.Log($"Player name set to: {playerName}");

                // Provide success feedback
                SetFeedback("Player name set successfully!", false);

                // Switch to the Title menu after successfully setting the name
                CanvasManager.Instance.SwitchCanvas(MenuType.Title);
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError("Failed to set player name.");
                Debug.LogException(ex);
                SetFeedback("Error: Unable to set player name.", true);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError("Request failed while setting player name.");
                Debug.LogException(ex);
                SetFeedback("Error: Request failed. Please try again.", true);
            }
        }

        private void SetFeedback(string message, bool isError)
        {
            if (FeedbackText != null)
            {
                FeedbackText.text = message;
                FeedbackText.color = isError ? Color.red : Color.green;
            }
        }

        private void OnDisable()
        {
            // Clean up the listener when the menu is disabled
            Buttons.Start.onClick.RemoveAllListeners();
        }
    }
}
