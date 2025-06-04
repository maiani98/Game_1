using UnityEngine;
// using UnityEngine.UI; // Per UI.Text standard
using TMPro; // Per TextMeshProUGUI - commentare se non si usa TMP

namespace ChaosCosmos.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI Elements")]
        // Sostituire TextMeshProUGUI con Text se si usa UI.Text standard
        public TextMeshProUGUI massText;
        public TextMeshProUGUI timerText;

        [Header("Player Reference")]
        public string playerTag = "Player"; // Tag per trovare il PlanetController del giocatore

        private ChaosCosmos.Gameplay.PlanetController playerPlanetController;
        private float gameTimer = 0f;

        void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerPlanetController = playerObject.GetComponent<ChaosCosmos.Gameplay.PlanetController>();
            }

            if (playerPlanetController == null)
            {
                Debug.LogError($"HUDController: PlanetController del giocatore (tag '{playerTag}') non trovato!");
            }

            if (massText == null)
            {
                Debug.LogError("HUDController: MassText non assegnato!");
            }
            if (timerText == null)
            {
                Debug.LogError("HUDController: TimerText non assegnato!");
            }

            // Inizializza il testo del timer
            UpdateTimerDisplay();
        }

        void Update()
        {
            // Aggiorna la massa del giocatore
            if (playerPlanetController != null && massText != null)
            {
                // Assumendo che PlanetController abbia GetCurrentMass() o currentMass sia public
                // Fare riferimento alle modifiche suggerite per BotBrain
                float currentMass = playerPlanetController.GetCurrentMass(); // Richiede GetCurrentMass() in PlanetController
                massText.text = $"Massa: {currentMass:F1}"; // Formattato a una cifra decimale
            }
            else if (massText != null && playerPlanetController == null)
            {
                massText.text = "Massa: N/A";
            }

            // Aggiorna il timer di gioco
            if (timerText != null)
            {
                gameTimer += Time.deltaTime;
                UpdateTimerDisplay();
            }
        }

        void UpdateTimerDisplay()
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(gameTimer / 60F);
            int seconds = Mathf.FloorToInt(gameTimer % 60F);
            timerText.text = $"Tempo: {minutes:00}:{seconds:00}";
        }
    }
}
