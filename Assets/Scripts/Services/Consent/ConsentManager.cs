using UnityEngine;
using System;
using System.Collections;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Core.Utils;

namespace ChaosCosmos.Services.Consent
{
    public class ConsentManager : IConsentService
    {
        public bool IsInitialized { get; private set; }

        public GDPRConsentStatus UserGDPRConsent { get; private set; }
        public TrackingAuthorizationStatus UserATTStatus { get; private set; }

        private Action _onFlowCompletedCallback;

        public ConsentManager()
        {
            LoadConsentStatus();
            Debug.Log($"ConsentManager: Costruito. GDPR Salvo: {UserGDPRConsent}, ATT Salvo: {UserATTStatus}");
        }

        public void RequestConsentFlow(Action onFlowCompleted)
        {
            _onFlowCompletedCallback = onFlowCompleted;
            IsInitialized = false;
            Debug.Log("ConsentManager: Avvio flusso richiesta consensi (simulato)...");
            StartSimulatedGDPRFlow();
        }

        private void StartSimulatedGDPRFlow()
        {
            if (UserGDPRConsent == GDPRConsentStatus.Unknown)
            {
                Debug.Log("[SIMULAZIONE POPUP GDPR] L'utente deve fare una scelta per il GDPR.");
                SimulateDelayedGDPRChoice(GDPRConsentStatus.Given, 0.1f); // Delay breve per test
            }
            else
            {
                Debug.Log($"ConsentManager: Scelta GDPR già effettuata: {UserGDPRConsent}. Procedo.");
                ContinueConsentFlowAfterGDPR();
            }
        }

        public void ForceSetGDPRConsent(GDPRConsentStatus status)
        {
            UserGDPRConsent = status;
            PlayerPrefs.SetInt(PlayerPrefsKeys.GDPR_CONSENT_KEY, (int)UserGDPRConsent);
            PlayerPrefs.Save();
            Debug.Log($"ConsentManager: Scelta GDPR impostata forzatamente a: {UserGDPRConsent}");
        }

        private void SimulateDelayedGDPRChoice(GDPRConsentStatus statusToSet, float delay)
        {
            GameObject delayRunnerGO = new GameObject($"GDPRChoiceDelayRunner_{Guid.NewGuid()}");
            DelayExecutor executor = delayRunnerGO.AddComponent<DelayExecutor>();
            executor.ExecuteAfterDelay(() => {
                UserGDPRConsent = statusToSet;
                PlayerPrefs.SetInt(PlayerPrefsKeys.GDPR_CONSENT_KEY, (int)UserGDPRConsent);
                PlayerPrefs.Save();
                Debug.Log($"ConsentManager: Scelta GDPR simulata e salvata: {UserGDPRConsent}");
                ContinueConsentFlowAfterGDPR();
            }, delay, () => { if(delayRunnerGO != null) UnityEngine.Object.Destroy(delayRunnerGO); });
        }

        private void ContinueConsentFlowAfterGDPR()
        {
            bool isEditorTestATT = false;
            #if UNITY_IOS && !UNITY_EDITOR
            bool isIOSDeviceForATT = true;
            Debug.Log("ConsentManager: Piattaforma iOS rilevata per ATT.");
            #elif UNITY_EDITOR
            isEditorTestATT = true; // Flag per log specifico in editor
            bool isIOSDeviceForATT = true; // FORZA TRUE PER TESTARE FLUSSO ATT IN EDITOR
            #else
            bool isIOSDeviceForATT = false;
            Debug.Log("ConsentManager: Non è una piattaforma iOS, ATT flow non applicabile.");
            #endif

            if (isEditorTestATT && isIOSDeviceForATT) Debug.LogWarning("ConsentManager: [EDITOR ONLY] Flusso ATT forzato per test. Su device reale, dipende da versione iOS.");


            if (isIOSDeviceForATT && UserATTStatus == TrackingAuthorizationStatus.NotDetermined)
            {
                StartSimulatedATTFlow();
            }
            else
            {
                if(isIOSDeviceForATT) Debug.Log($"ConsentManager: Scelta ATT già effettuata ({UserATTStatus}) o non richiesta.");
                FinalizeConsentFlow();
            }
        }

        private void StartSimulatedATTFlow()
        {
            Debug.Log("[SIMULAZIONE POPUP ATT] L'utente deve fare una scelta per App Tracking Transparency (iOS).");
            SimulateDelayedATTChoice(TrackingAuthorizationStatus.Authorized, 0.1f); // Delay breve
        }

        public void ForceSetATTStatus(TrackingAuthorizationStatus status)
        {
            UserATTStatus = status;
            PlayerPrefs.SetInt(PlayerPrefsKeys.ATT_STATUS_KEY, (int)UserATTStatus);
            PlayerPrefs.Save();
            Debug.Log($"ConsentManager: Scelta ATT impostata forzatamente a: {UserATTStatus}");
        }

        private void SimulateDelayedATTChoice(TrackingAuthorizationStatus status, float delay)
        {
            GameObject delayRunnerGO = new GameObject($"ATTChoiceDelayRunner_{Guid.NewGuid()}");
            DelayExecutor executor = delayRunnerGO.AddComponent<DelayExecutor>();
            executor.ExecuteAfterDelay(() => {
                UserATTStatus = status;
                PlayerPrefs.SetInt(PlayerPrefsKeys.ATT_STATUS_KEY, (int)UserATTStatus);
                PlayerPrefs.Save();
                Debug.Log($"ConsentManager: Scelta ATT simulata e salvata: {UserATTStatus}");
                FinalizeConsentFlow();
            }, delay, () => { if(delayRunnerGO != null) UnityEngine.Object.Destroy(delayRunnerGO); });
        }

        private void FinalizeConsentFlow()
        {
            IsInitialized = true;
            Debug.Log($"ConsentManager: Flusso richiesta consensi completato. Stato finale - GDPR: {UserGDPRConsent}, ATT: {UserATTStatus}");
            _onFlowCompletedCallback?.Invoke();
            _onFlowCompletedCallback = null;
        }

        public void LoadConsentStatus()
        {
            UserGDPRConsent = (GDPRConsentStatus)PlayerPrefs.GetInt(PlayerPrefsKeys.GDPR_CONSENT_KEY, (int)GDPRConsentStatus.Unknown);
            UserATTStatus = (TrackingAuthorizationStatus)PlayerPrefs.GetInt(PlayerPrefsKeys.ATT_STATUS_KEY, (int)TrackingAuthorizationStatus.NotDetermined);
        }

        public bool HasGivenGDPRConsentForAnalytics() => UserGDPRConsent == GDPRConsentStatus.Given;
        public bool HasGivenGDPRConsentForPersonalizedAds() => UserGDPRConsent == GDPRConsentStatus.Given;
        public bool CanTrackDeviceForAds()
        {
            #if UNITY_IOS
            return HasGivenGDPRConsentForPersonalizedAds() && UserATTStatus == TrackingAuthorizationStatus.Authorized;
            #else
            return HasGivenGDPRConsentForPersonalizedAds();
            #endif
        }

        public void ResetConsentStatus()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.GDPR_CONSENT_KEY);
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.ATT_STATUS_KEY);
            UserGDPRConsent = GDPRConsentStatus.Unknown;
            UserATTStatus = TrackingAuthorizationStatus.NotDetermined;
            PlayerPrefs.Save();
            IsInitialized = true;
            Debug.Log("ConsentManager: Stato consensi resettato a Unknown/NotDetermined e PlayerPrefs cancellati.");
        }
    }
}
