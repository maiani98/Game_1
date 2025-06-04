using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Se si usa Unity UI per il bottone
using ChaosCosmos.Core.Services; // Per ServiceLocator
using ChaosCosmos.Services.Analytics; // Per IAnalyticsService
using System.Collections.Generic; // Per Dictionary

namespace ChaosCosmos.UI
{
    public class LobbyManager : MonoBehaviour
    {
        // Nome della scena di gameplay da caricare
        public string gameplaySceneName = "PlanetTestScene"; // Default, cambialo se la scena di gioco ha un altro nome

        // Riferimento al bottone "Start Game" se si usa Unity UI
        // public Button startGameButton;

        void Start()
        {
            // Se si usa un bottone da codice:
            // if (startGameButton != null)
            // {
            //     startGameButton.onClick.AddListener(StartGame);
            // }
            // else
            // {
            //     Debug.LogWarning("LobbyManager: StartGameButton non assegnato.");
            // }
            Debug.Log("LobbyManager: Scena Lobby caricata.");
        }

        public void StartGame() // Questo metodo può essere chiamato da un evento OnClick di un bottone UI
        {
            if (string.IsNullOrEmpty(gameplaySceneName))
            {
                Debug.LogError("LobbyManager: gameplaySceneName non è impostato!");
                return;
            }

            IAnalyticsService analytics = ServiceLocator.Get<IAnalyticsService>();
            if (analytics != null)
            {
                analytics.TrackEvent("GameStarted", new Dictionary<string, object>
                {
                    { "level_name", gameplaySceneName }, // Esempio di parametro
                    { "start_timestamp", System.DateTime.UtcNow.ToString("o") }
                });
            }

            Debug.Log($"LobbyManager: Caricamento scena di gioco: {gameplaySceneName}");
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
