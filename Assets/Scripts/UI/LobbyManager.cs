using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Se si usa Unity UI per il bottone
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.Analytics;
using System.Collections.Generic;
using ChaosCosmos.Core.Constants; // Aggiunto using per SceneNames

namespace ChaosCosmos.UI
{
    public class LobbyManager : MonoBehaviour
    {
        // Nome della scena di gameplay da caricare
        public string gameplaySceneName = SceneNames.GAMEPLAY_SCENE_PLACEHOLDER;

        // Riferimento al bottone "Start Game" se si usa Unity UI
        // public Button startGameButton;

        void Start()
        {
            Debug.Log("LobbyManager: Scena Lobby caricata.");
        }

        public void StartGame()
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
                    { "level_name", gameplaySceneName },
                    { "start_timestamp", System.DateTime.UtcNow.ToString("o") }
                });
            }

            Debug.Log($"LobbyManager: Caricamento scena di gioco: {gameplaySceneName}");
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
