using UnityEngine;
using TMPro;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Gameplay;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using ChaosCosmos.Core.Services; // Per ServiceLocator
using ChaosCosmos.Services.GameManagement; // Per IGameManagerService
using System; // Per Action<float>

namespace ChaosCosmos.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI massText;
        public TextMeshProUGUI timerText;

        private string _playerTag = GameTags.PLAYER_TAG;

        private PlanetController _playerPlanetController;
        // private float _gameTimer = 0f; // Rimosso, ora gestito da GameManagerService
        private float _timeRemainingDisplay = 0f; // Per visualizzare il tempo rimanente

        [Header("Localization Keys")]
        public LocalizedString massLabelString = new LocalizedString { TableReference = "GameUIText", TableEntryReference = "hud_mass_label" };
        public LocalizedString timeLabelString = new LocalizedString { TableReference = "GameUIText", TableEntryReference = "hud_time_label" };

        private string _cachedMassLabel = "Mass: ";
        private string _cachedTimeLabel = "Time: ";

        private IGameManagerService _gameManagerService;

        void Awake()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(_playerTag);
            if (playerObject != null)
            {
                _playerPlanetController = playerObject.GetComponent<PlanetController>();
            }

            if (_playerPlanetController == null)
            {
                Debug.LogError($"HUDController: PlanetController del giocatore (tag '{_playerTag}') non trovato!");
            }

            if (massText == null) Debug.LogError("HUDController: MassText non assegnato!");
            if (timerText == null) Debug.LogError("HUDController: TimerText non assegnato!");

            // Recupera GameManagerService
            if (ServiceLocator.IsRegistered<IGameManagerService>())
            {
                _gameManagerService = ServiceLocator.Get<IGameManagerService>();
            }
            else
            {
                Debug.LogError("HUDController: IGameManagerService non trovato in ServiceLocator!");
            }
        }

        void OnEnable()
        {
           LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
           if (LocalizationSettings.InitializationOperation.IsDone)
           {
               UpdateLocalizedLabels();
           }
           else
           {
                LocalizationSettings.InitializationOperation.Completed += HandleLocalizationInitialized;
           }

           if (_gameManagerService != null)
           {
               _gameManagerService.OnTimerUpdated += HandleTimerUpdated;
               // Imposta il valore iniziale del timer se il servizio è già pronto
               HandleTimerUpdated(_gameManagerService.TimeRemaining);
           }
        }

        void OnDisable()
        {
           LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
           LocalizationSettings.InitializationOperation.Completed -= HandleLocalizationInitialized; // Assicurati di de-registrarti
           if (_gameManagerService != null)
           {
               _gameManagerService.OnTimerUpdated -= HandleTimerUpdated;
           }
        }

        private void HandleLocalizationInitialized(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<LocalizationSettings> op)
        {
            // Questo viene chiamato solo se l'inizializzazione non era completa in OnEnable
            UpdateLocalizedLabels();
        }


        private void OnLocaleChanged(Locale newLocale)
        {
            Debug.Log($"HUDController: Lingua cambiata a {newLocale.LocaleName}. Aggiorno etichette.");
            UpdateLocalizedLabels();
        }

        private async void UpdateLocalizedLabels() // Reso async per GetLocalizedStringAsync
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                Debug.LogWarning("HUDController: Tentativo di aggiornare etichette ma Localization System non è pronto.");
                return;
            }

            try {
                _cachedMassLabel = await massLabelString.GetLocalizedStringAsync().Task + " ";
            } catch (Exception ex) {
                Debug.LogError($"HUDController: Errore caricamento localized string per massLabel: {ex.Message}");
                _cachedMassLabel = "Mass: "; // Fallback
            }

            try {
                 _cachedTimeLabel = await timeLabelString.GetLocalizedStringAsync().Task + " ";
            } catch (Exception ex) {
                Debug.LogError($"HUDController: Errore caricamento localized string per timeLabel: {ex.Message}");
                _cachedTimeLabel = "Time: "; // Fallback
            }

            UpdateMassDisplay();
            UpdateTimerDisplayInternal();
        }

        void UpdateMassDisplay()
        {
            if (massText == null) return;

            if (_playerPlanetController != null)
            {
                float currentMass = _playerPlanetController.GetCurrentMass();
                massText.text = _cachedMassLabel + $"{currentMass:F1}";
            }
            else
            {
                massText.text = _cachedMassLabel + "N/A";
            }
        }

        private void HandleTimerUpdated(float timeRemaining)
        {
            _timeRemainingDisplay = timeRemaining;
            UpdateTimerDisplayInternal();
        }

        void UpdateTimerDisplayInternal()
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(Mathf.Max(0, _timeRemainingDisplay) / 60F);
            int seconds = Mathf.FloorToInt(Mathf.Max(0, _timeRemainingDisplay) % 60F);
            timerText.text = _cachedTimeLabel + $"{minutes:00}:{seconds:00}";
        }

        void Update()
        {
            // L'aggiornamento della massa è guidato da eventi o da necessità, non per forza ogni frame.
            // Ma se il PlanetController non emette eventi di cambio massa, l'Update qui è un modo per pollarlo.
            if (_playerPlanetController != null && massText != null)
            {
               UpdateMassDisplay();
            }
            else if (massText != null && _playerPlanetController == null)
            {
                massText.text = _cachedMassLabel + "N/A";
            }
            // Il timer viene aggiornato tramite l'evento OnTimerUpdated da GameManagerService
        }
    }
}
