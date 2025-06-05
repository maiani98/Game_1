using UnityEngine;
using TMPro;
using ChaosCosmos.Core.Constants; // Aggiunto per GameTags, non era nel read_files ma è usato
using ChaosCosmos.Gameplay; // Aggiunto per PlanetController, non era nel read_files ma è usato

namespace ChaosCosmos.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI massText;
        public TextMeshProUGUI timerText;

        [Header("Player Reference")]
        private string _playerTag = GameTags.PLAYER_TAG; // Usato internamente

        private PlanetController _playerPlanetController; // Prefixed
        private float _gameTimer = 0f; // Prefixed

        void Start()
        {
            // playerTag è ora privato e usa la costante, non più settabile da Inspector.
            // Se si volesse flessibilità da Inspector, si terrebbe public string playerTag e si userebbe quello.
            GameObject playerObject = GameObject.FindGameObjectWithTag(_playerTag);
            if (playerObject != null)
            {
                _playerPlanetController = playerObject.GetComponent<PlanetController>();
            }

            if (_playerPlanetController == null)
            {
                Debug.LogError($"HUDController: PlanetController del giocatore (tag '{_playerTag}') non trovato!");
            }

            if (massText == null)
            {
                Debug.LogError("HUDController: MassText non assegnato!");
            }
            if (timerText == null)
            {
                Debug.LogError("HUDController: TimerText non assegnato!");
            }

            UpdateTimerDisplay();
        }

        void Update()
        {
            if (_playerPlanetController != null && massText != null)
            {
                float currentMass = _playerPlanetController.GetCurrentMass();
                massText.text = $"Massa: {currentMass:F1}";
            }
            else if (massText != null && _playerPlanetController == null)
            {
                massText.text = "Massa: N/A";
            }

            if (timerText != null)
            {
                _gameTimer += Time.deltaTime;
                UpdateTimerDisplay();
            }
        }

        void UpdateTimerDisplay()
        {
            if (timerText == null)
            { // Aggiunte graffe
                return;
            }

            int minutes = Mathf.FloorToInt(_gameTimer / 60F);
            int seconds = Mathf.FloorToInt(_gameTimer % 60F);
            timerText.text = $"Tempo: {minutes:00}:{seconds:00}";
        }
    }
}
