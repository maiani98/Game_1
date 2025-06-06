using UnityEngine;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Core.Services;   // Per ServiceLocator
using ChaosCosmos.Services.Analytics; // Per IAnalyticsService
using ChaosCosmos.Core.Utils;     // Per DelayExecutor
using System;
using System.Collections.Generic;

namespace ChaosCosmos.Services.Tutorial
{
    public class TutorialManagerService : ITutorialService
    {
        public bool IsInitialized { get; private set; }
        private bool _isTutorialFlowActive = false;
        // _sessionCompletedSteps non è più necessario per questo approccio puramente temporizzato

        public void Initialize(Action onInitializedCallback)
        {
            // LoadTutorialStatus() non è necessario qui perché IsTutorialCompleted() legge sempre PlayerPrefs.
            IsInitialized = true;
            Debug.Log($"[TutorialManager] Inizializzato. Tutorial Completato Globalmente? {IsTutorialCompleted()}");
            onInitializedCallback?.Invoke();
        }

        public bool IsTutorialCompleted()
        {
            return PlayerPrefs.GetInt(PlayerPrefsKeys.TUTORIAL_COMPLETED_KEY, 0) == 1;
        }

        public void StartTutorialFlowIfNotCompleted()
        {
            if (IsTutorialCompleted())
            {
                Debug.Log("[TutorialManager] Tutorial già completato globalmente, non avvio il flusso.");
                _isTutorialFlowActive = false;
                return;
            }

            if (_isTutorialFlowActive)
            {
                Debug.Log("[TutorialManager] Flusso tutorial già attivo in questa sessione.");
                return;
            }

            _isTutorialFlowActive = true;
            Debug.Log("[TutorialManager] Avvio Flusso Tutorial Sequenziale Simmerizzato...");
            TriggerNextStep("TUT_STEP_WELCOME_MOVE"); // Inizia con il primo step
        }

        private void TriggerNextStep(string stepID)
        {
            if (!_isTutorialFlowActive)
            {
                // Debug.Log($"[TutorialManager] Flusso interrotto o completato, non triggero step: {stepID}");
                return;
            }

            string message = GetMessageForStep(stepID);
            float delay = GetDelayForStep(stepID);

            Debug.Log($"[TUTORIAL_SIM] STEP ATTIVO: {stepID} - Messaggio: \"{message}\" (Durata simulata: {delay}s)");

            GameObject delayRunnerGO = new GameObject($"TutorialStepDelay_{stepID}");
            DelayExecutor executor = delayRunnerGO.AddComponent<DelayExecutor>();

            // DelayExecutor si auto-distrugge dopo l'esecuzione
            executor.ExecuteAfterDelay(() => {
                if (!_isTutorialFlowActive)
                {
                    // Se nel frattempo il tutorial è stato completato o resettato
                    return;
                }
                Debug.Log($"[TUTORIAL_SIM] STEP COMPLETATO (simulato dopo delay): {stepID}");
                ProcessStepCompletion(stepID);
            }, delay);
        }

        private void ProcessStepCompletion(string completedStepID)
        {
            if (!_isTutorialFlowActive) return; // Sicurezza aggiuntiva

            string nextStepID = null;
            switch (completedStepID)
            {
                case "TUT_STEP_WELCOME_MOVE": nextStepID = "TUT_STEP_INGEST"; break;
                case "TUT_STEP_INGEST": nextStepID = "TUT_STEP_OBJECTIVE"; break;
                case "TUT_STEP_OBJECTIVE": nextStepID = "TUT_STEP_POWERUP_INTRO"; break;
                case "TUT_STEP_POWERUP_INTRO": nextStepID = "TUT_STEP_BOT_INTRO"; break;
                case "TUT_STEP_BOT_INTRO": nextStepID = "TUT_STEP_FINAL"; break;
                case "TUT_STEP_FINAL":
                    MarkTutorialAsFullyCompleted();
                    break;
                default:
                    Debug.LogWarning($"[TutorialManager] Step completato sconosciuto: {completedStepID}. Interrompo flusso.");
                    _isTutorialFlowActive = false;
                    break;
            }

            if (nextStepID != null)
            {
                TriggerNextStep(nextStepID);
            }
        }

        private string GetMessageForStep(string stepID)
        {
            switch(stepID)
            {
                case "TUT_STEP_WELCOME_MOVE": return "Benvenuto in Chaos Cosmos! Trascina sullo schermo per muovere il tuo pianeta.";
                case "TUT_STEP_INGEST": return "Vedi quei frammenti colorati? Ingeriscili per aumentare la tua massa e crescere!";
                case "TUT_STEP_OBJECTIVE": return "L'obiettivo è diventare il pianeta più grande nell'arena prima che scada il tempo. Continua a crescere!";
                case "TUT_STEP_POWERUP_INTRO": return "Attenzione ai Power-Up! Raccoglili per ottenere abilità speciali temporanee.";
                case "TUT_STEP_BOT_INTRO": return "Non sei solo! Altri pianeti (Bot) vagano per il cosmo. Ingerisci quelli più piccoli e stai alla larga da quelli più grandi!";
                case "TUT_STEP_FINAL": return "Ottimo lavoro! Ora sei pronto a dominare il cosmo. Buona fortuna!";
                default: return $"Messaggio tutorial per '{stepID}' non trovato.";
            }
        }

        private float GetDelayForStep(string stepID)
        {
            // Durate simulate per ogni step prima di passare al successivo
            switch(stepID)
            {
                case "TUT_STEP_WELCOME_MOVE": return 5f;  // Tempo per leggere e provare a muoversi
                case "TUT_STEP_INGEST": return 7f;    // Tempo per trovare e ingerire alcuni frammenti
                case "TUT_STEP_OBJECTIVE": return 5f; // Tempo per leggere l'obiettivo
                case "TUT_STEP_POWERUP_INTRO": return 7f; // Tempo per notare/raccogliere un power-up (ipotetico spawn)
                case "TUT_STEP_BOT_INTRO": return 7f; // Tempo per osservare/interagire con un bot (ipotetico spawn)
                case "TUT_STEP_FINAL": return 3f;     // Messaggio finale
                default: return 2f;                  // Default breve per step non definiti
            }
        }

        public void MarkTutorialAsFullyCompleted()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
            _isTutorialFlowActive = false;
            Debug.Log("[TutorialManager] Tutorial Marcato come COMPLETAMENTE COMPLETATO.");

            if (ServiceLocator.IsRegistered<IAnalyticsService>())
            {
                var analytics = ServiceLocator.Get<IAnalyticsService>();
                analytics.TrackEvent("TutorialCompleted");
            }
        }

        public void ResetTutorialStatus()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.Save();
            _isTutorialFlowActive = false;
            Debug.Log("[TutorialManager] Stato tutorial resettato (PlayerPrefs cancellati). Sarà riavviato alla prossima partita.");
        }
    }
}
