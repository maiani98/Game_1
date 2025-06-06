using UnityEngine;
using UnityEngine.UI; // Per Button
using TMPro;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.GameManagement;
using ChaosCosmos.Gameplay.GameLogic;
using UnityEngine.SceneManagement;
using ChaosCosmos.Core.Constants; // Per SceneNames

namespace ChaosCosmos.UI.Meta
{
    public class ResultsPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panelRoot; // Il GameObject radice del pannello
        public TextMeshProUGUI finalMassText;
        public TextMeshProUGUI xpEarnedText;
        public TextMeshProUGUI timePlayedText; // Aggiunto per mostrare il tempo
        public Button backToMenuButton;

        private IGameManagerService _gameManagerService;
        // private IProgressService _progressService; // Non strettamente necessario qui se GameManagerService già aggiunge XP

        void Start()
        {
            if (!ServiceLocator.IsRegistered<IGameManagerService>())
            {
                Debug.LogError("ResultsPanelUI: IGameManagerService non registrato! Il pannello non funzionerà.");
                if (panelRoot != null) panelRoot.SetActive(false);
                enabled = false;
                return;
            }
            _gameManagerService = ServiceLocator.Get<IGameManagerService>();

            // Non è necessario _progressService se ci fidiamo che GameManagerService abbia già aggiornato XP.
            // if (ServiceLocator.IsRegistered<IProgressService>())
            // {
            //     _progressService = ServiceLocator.Get<IProgressService>();
            // }

            if (_gameManagerService != null)
            {
                _gameManagerService.OnGameOver += HandleGameOver;
            }

            if (backToMenuButton != null)
            {
                backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
            }
            else
            {
                Debug.LogError("ResultsPanelUI: BackToMenuButton non assegnato!");
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false); // Nascondi all'inizio
            }
            else
            {
                Debug.LogError("ResultsPanelUI: PanelRoot non assegnato!");
            }
        }

        void OnDestroy()
        {
            if (_gameManagerService != null)
            {
                _gameManagerService.OnGameOver -= HandleGameOver;
            }
            if (backToMenuButton != null)
            {
                 backToMenuButton.onClick.RemoveAllListeners();
            }
        }

        private void HandleGameOver(GameResultData results)
        {
            if (panelRoot == null) return;

            panelRoot.SetActive(true);

            if (finalMassText != null)
            {
                finalMassText.text = $"Massa Finale: {results.finalMass:F1}";
            }
            if (xpEarnedText != null)
            {
                xpEarnedText.text = $"XP Guadagnati: {results.xpEarned}";
            }
            if (timePlayedText != null)
            {
                int minutes = Mathf.FloorToInt(results.timePlayed / 60F);
                int seconds = Mathf.FloorToInt(results.timePlayed % 60F);
                timePlayedText.text = $"Tempo Giocato: {minutes:00}:{seconds:00}";
            }

            // Qui si potrebbe anche aggiornare dinamicamente l'XP totale del giocatore
            // se ProgressService avesse un evento OnXPChanged a cui iscriversi.
            // O se ResultsPanelUI avesse un riferimento a PlayerXPText dell'HUD per aggiornarlo.
        }

        private void OnBackToMenuClicked()
        {
            // Assumiamo che tornare al menu resetti lo stato del GameManager a Pregame
            // o che la LobbyScene lo faccia.
            // Se GameManagerService dovesse resettare il suo stato qui:
            // _gameManagerService?.ChangeGameState(GameState.Pregame); // Metodo ipotetico o reset esplicito

            // Disattiva il pannello prima di cambiare scena
            if (panelRoot != null) panelRoot.SetActive(false);

            // Carica la scena della Lobby
            SceneManager.LoadScene(SceneNames.MAIN_MENU_SCENE); // Aggiornato a MAIN_MENU_SCENE
        }
    }
}
