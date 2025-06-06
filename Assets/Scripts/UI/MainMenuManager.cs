using UnityEngine;
using UnityEngine.SceneManagement;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.GameManagement;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Services.Analytics; // Aggiunto per tracciare evento Play
using System.Collections.Generic;   // Aggiunto per Dictionary

namespace ChaosCosmos.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainButtonsPanel;
        public GameObject upgradesPanel;
        public GameObject battlePassPanel;
        public GameObject settingsPanel;
        // Aggiungere altri pannelli se necessario

        private IGameManagerService _gameManagerService;
        private IAnalyticsService _analyticsService;

        void Start()
        {
            // TODO_MUSIC: Play_Menu_Music_Loop (assicurarsi che parta solo una volta, es. se Bootstrapper non lo fa)

            // Recupera servizi necessari
            if (ServiceLocator.IsRegistered<IGameManagerService>())
            {
                _gameManagerService = ServiceLocator.Get<IGameManagerService>();
            }
            else
            {
                Debug.LogError("MainMenuManager: IGameManagerService non registrato!");
            }

            if (ServiceLocator.IsRegistered<IAnalyticsService>())
            {
                _analyticsService = ServiceLocator.Get<IAnalyticsService>();
            }
            else
            {
                Debug.LogWarning("MainMenuManager: IAnalyticsService non registrato. Eventi UI non saranno tracciati.");
            }

            // Validazione assegnazione pannelli
            if (mainButtonsPanel == null) Debug.LogError("MainMenuManager: MainButtonsPanel non assegnato!");
            if (upgradesPanel == null) Debug.LogWarning("MainMenuManager: UpgradesPanel non assegnato. Funzionalità Upgrade non disponibile.");
            if (battlePassPanel == null) Debug.LogWarning("MainMenuManager: BattlePassPanel non assegnato. Funzionalità BattlePass non disponibile.");
            if (settingsPanel == null) Debug.LogWarning("MainMenuManager: SettingsPanel non assegnato. Funzionalità Impostazioni non disponibile.");


            ShowMainButtonsPanel();
        }

        private void DeactivateAllPanels()
        {
            mainButtonsPanel?.SetActive(false);
            upgradesPanel?.SetActive(false);
            battlePassPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
        }

        public void ShowMainButtonsPanel()
        {
            DeactivateAllPanels();
            if (mainButtonsPanel != null)
            {
                mainButtonsPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("MainMenuManager: MainButtonsPanel è nullo, impossibile mostrarlo!");
            }
        }

        public void OnPlayButtonClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            _analyticsService?.TrackEvent("MainMenu_PlayButtonClicked");

            if (_gameManagerService != null)
            {
                // TODO_MUSIC: Stop_Menu_Music_Loop (o fade out)
                _gameManagerService.StartNewMatch();
                SceneManager.LoadScene(SceneNames.GAMEPLAY_SCENE_PLACEHOLDER);
            }
            else
            {
                Debug.LogError("MainMenuManager: IGameManagerService non disponibile. Impossibile avviare la partita.");
            }
        }

        public void OnUpgradesButtonClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            _analyticsService?.TrackEvent("MainMenu_UpgradesButtonClicked");
            DeactivateAllPanels();
            if (upgradesPanel != null)
            {
                upgradesPanel.SetActive(true);
                UpgradesPanelUI upgradesPanelScript = upgradesPanel.GetComponent<UpgradesPanelUI>();
                upgradesPanelScript?.RefreshAllUI();
            }
            else Debug.LogWarning("MainMenuManager: UpgradesPanel non è assegnato.");
        }

        public void OnBattlePassButtonClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            _analyticsService?.TrackEvent("MainMenu_BattlePassButtonClicked");
            DeactivateAllPanels();
            if (battlePassPanel != null)
            {
                battlePassPanel.SetActive(true);
                BattlePassPanelUI bpPanelScript = battlePassPanel.GetComponent<BattlePassPanelUI>();
                bpPanelScript?.RefreshAllUI();
            }
            else Debug.LogWarning("MainMenuManager: BattlePassPanel non è assegnato.");
        }

        public void OnSettingsButtonClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            _analyticsService?.TrackEvent("MainMenu_SettingsButtonClicked");
            DeactivateAllPanels();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
            else Debug.LogWarning("MainMenuManager: SettingsPanel non è assegnato.");
        }

        public void OnExitButtonClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            _analyticsService?.TrackEvent("MainMenu_ExitButtonClicked");
            Debug.Log("MainMenuManager: OnExitButtonClicked - Uscita dal gioco...");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
