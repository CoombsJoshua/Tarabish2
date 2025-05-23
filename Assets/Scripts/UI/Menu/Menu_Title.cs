using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Tarabish.Netcode;
using Unity.Services.Authentication;
using System.Collections;

namespace Tarabish.UI.Menu
{
    public class Menu_Title : Menu_
    {
        public override MenuType MenuType => MenuType.Title;

        public TextMeshProUGUI m_UsernameText, m_UsernameDetailPanelText;
        [SerializeField] private TMP_InputField nameInputField; // Drag your input field here in Unity Inspector
        [SerializeField] private Button changeNameButton; // Drag your button here in Unity Inspector

        public SessionManager Manager;
        public GameObject changeNamePanel;

        // XP and Trophy UI elements
        [SerializeField] private Slider xpBar; // Drag your XP Slider here in the Inspector
        [SerializeField] private TextMeshProUGUI xpLevelText; // Drag your Level TMP Text here
        [SerializeField] private TextMeshProUGUI trophiesText; // Drag your Trophies TMP Text here

        private void OnEnable()
        {
            m_UsernameText.text = AuthenticationService.Instance.PlayerName;
            m_UsernameDetailPanelText.text = AuthenticationService.Instance.PlayerName;
            changeNameButton.onClick.AddListener(() => StartCoroutine(ChangeNameCoroutine()));

            UpdateXPUI();
            UpdateTrophyUI();
        }

        private IEnumerator ChangeNameCoroutine()
        {
            yield return Manager.ChangeNameAsync(nameInputField.text);
            m_UsernameText.text = AuthenticationService.Instance.PlayerName;
            m_UsernameDetailPanelText.text = AuthenticationService.Instance.PlayerName;
            changeNamePanel.SetActive(false);
        }

        public void UpdateXPUI()
        {
            if (Manager.m_LoggedPlayerProfile != null)
            {
                var profile = Manager.m_LoggedPlayerProfile;
                xpBar.maxValue = profile.Level * 100; // Assuming XP required to level up is Level * 100
                xpBar.value = profile.XP;
                xpLevelText.text = $"{profile.Level}";
            }
        }

        public void UpdateTrophyUI()
        {
            if (Manager.m_LoggedPlayerProfile != null)
            {
                trophiesText.text = $"{Manager.m_LoggedPlayerProfile.Trophies}";
            }
        }
    }
}
