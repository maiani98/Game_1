using UnityEngine;
using System;
using System.Collections;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.IAP;
using ChaosCosmos.Services.Consent;
using ChaosCosmos.Core.Utils;

namespace ChaosCosmos.Services.Ads
{
    public class AdsFacade : IAdsService
    {
        public bool IsInitialized { get; private set; }
        private IIAPService _iapService;
        private IConsentService _consentService;
        private bool _adsRemoved = false;

        private float _rewardedVideoLoadTime = 2f;
        private float _interstitialLoadTime = 1.5f;
        private bool _isRewardedVideoLoading = false;
        private bool _isInterstitialLoading = false;
        private bool _rewardedVideoReady = false;
        private bool _interstitialReady = false;

        public AdsFacade(IIAPService iapService)
        {
            _iapService = iapService; // Non lanciare ArgumentNullException qui, Bootstrapper potrebbe passare null se IAP fallisce l'init
            if (_iapService == null)
            {
                Debug.LogError("AdsFacade Constructor: IIAPService è nullo. La funzionalità 'Remove Ads' non sarà disponibile.");
            }
        }

        private void EnsureConsentService()
        {
            if (_consentService == null && ServiceLocator.IsRegistered<IConsentService>())
            {
                _consentService = ServiceLocator.Get<IConsentService>();
                 if (_consentService == null) Debug.LogError("AdsFacade: Impossibile recuperare IConsentService da ServiceLocator.");
            }
        }

        public void Initialize()
        {
            EnsureConsentService();
            Debug.Log("AdsFacade: Inizializzazione...");

            if (_iapService != null)
            {
                if (!_iapService.IsInitialized)
                {
                    Debug.LogWarning("AdsFacade Initialize: IIAPService fornito ma non ancora inizializzato. Lo stato 'Remove Ads' potrebbe non essere accurato fino a quando IAP non sarà pronto.");
                }
                // Lo stato _adsRemoved verrà aggiornato in UpdateAdsRemovedStatus()
            }
            else
            {
                Debug.LogWarning("AdsFacade Initialize: IIAPService non disponibile. La funzionalità 'Remove Ads' sarà disattivata.");
            }

            UpdateAdsRemovedStatus();
            IsInitialized = true; // Marcato come inizializzato anche se alcune dipendenze non sono ottimali.
                                  // La logica interna gestirà lo stato di queste dipendenze.
            Debug.Log($"AdsFacade: Inizializzato. Annunci rimossi? {_adsRemoved}");


            if (!_adsRemoved)
            {
                LoadRewardedVideo();
                LoadInterstitialAd();
            }
        }

        private void UpdateAdsRemovedStatus()
        {
            if (_iapService != null && _iapService.IsInitialized)
            {
                _adsRemoved = _iapService.HasUserPurchased(IAPFacade.ProductID_RemoveAds);
            }
            else
            {
                // Se IAP non è pronto, assumiamo che gli annunci non siano rimossi per sicurezza.
                _adsRemoved = false;
            }
            // Debug.Log($"AdsFacade: Stato 'Remove Ads' aggiornato: {_adsRemoved}"); // Log frequente, meglio solo al cambio o init
        }

        private void LoadRewardedVideo()
        {
            if (!IsInitialized) { Debug.LogWarning("AdsFacade: LoadRewardedVideo chiamato ma servizio non inizializzato."); return; }
            if (_isRewardedVideoLoading || _rewardedVideoReady) return;
            _isRewardedVideoLoading = true;
            Debug.Log("AdsFacade: Caricamento Rewarded Video (simulato)...");
            InvokeAction(() => {
                _rewardedVideoReady = true;
                _isRewardedVideoLoading = false;
                Debug.Log("AdsFacade: Rewarded Video pronto (simulato).");
            }, _rewardedVideoLoadTime);
        }
        public bool IsRewardedVideoReady() => _rewardedVideoReady;

        public void ShowRewardedVideo(Action<AdCompletionStatus> onAdCompleted)
        {
            if (!IsInitialized)
            {
                Debug.LogError("AdsFacade.ShowRewardedVideo: Servizio non inizializzato.");
                onAdCompleted?.Invoke(AdCompletionStatus.Failed);
                return;
            }
             if (onAdCompleted == null)
            {
                Debug.LogWarning("AdsFacade.ShowRewardedVideo: onAdCompleted callback è nullo.");
            }

            if (IsRewardedVideoReady())
            {
                Debug.Log("AdsFacade: Mostrando Rewarded Video (simulato)...");
                _rewardedVideoReady = false;
                InvokeAction(() => {
                    Debug.Log("AdsFacade: Rewarded Video completato (simulato).");
                    onAdCompleted?.Invoke(AdCompletionStatus.Completed);
                    LoadRewardedVideo();
                }, 3f);
            }
            else
            {
                Debug.LogWarning("AdsFacade: Rewarded Video non pronto.");
                onAdCompleted?.Invoke(AdCompletionStatus.Failed);
                LoadRewardedVideo();
            }
        }

        private void LoadInterstitialAd()
        {
            if (!IsInitialized) { Debug.LogWarning("AdsFacade: LoadInterstitialAd chiamato ma servizio non inizializzato."); return; }
            UpdateAdsRemovedStatus();
            if (_adsRemoved)
            {
                Debug.Log("AdsFacade: Annunci rimossi, caricamento Interstitial saltato.");
                return;
            }

            EnsureConsentService();
            if (_consentService == null || !_consentService.IsInitialized || !_consentService.HasGivenGDPRConsentForPersonalizedAds())
            {
                Debug.LogWarning("AdsFacade: Consenso per annunci personalizzati non dato o servizio di consenso non pronto. Caricamento Interstitial Ad saltato.");
                _interstitialReady = false;
                return;
            }

            if (_isInterstitialLoading || _interstitialReady) return;
            _isInterstitialLoading = true;
            Debug.Log("AdsFacade: Caricamento Interstitial Ad (simulato)...");
            InvokeAction(() => {
                _interstitialReady = true;
                _isInterstitialLoading = false;
                Debug.Log("AdsFacade: Interstitial Ad pronto (simulato).");
            }, _interstitialLoadTime);
        }
        public bool IsInterstitialAdReady()
        {
            if (!IsInitialized) return false;
            UpdateAdsRemovedStatus();
            if (_adsRemoved) return false;

            EnsureConsentService();
            if (_consentService == null || !_consentService.IsInitialized || !_consentService.HasGivenGDPRConsentForPersonalizedAds())
            {
                return false;
            }
            return _interstitialReady;
        }

        public void ShowInterstitialAd(Action<bool> onAdClosed)
        {
            if (!IsInitialized)
            {
                Debug.LogError("AdsFacade.ShowInterstitialAd: Servizio non inizializzato.");
                onAdClosed?.Invoke(false);
                return;
            }
            if (onAdClosed == null)
            {
                Debug.LogWarning("AdsFacade.ShowInterstitialAd: onAdClosed callback è nullo.");
            }

            UpdateAdsRemovedStatus();
            if (_adsRemoved)
            {
                Debug.Log("AdsFacade: Annunci rimossi, Interstitial non mostrato.");
                onAdClosed?.Invoke(false);
                return;
            }

            EnsureConsentService();
            if (_consentService == null || !_consentService.IsInitialized || !_consentService.HasGivenGDPRConsentForPersonalizedAds())
            {
                 Debug.LogWarning("AdsFacade: Consenso per annunci personalizzati non dato o servizio di consenso non pronto. Interstitial Ad non mostrato.");
                 onAdClosed?.Invoke(false);
                 return;
            }

            if (IsInterstitialAdReady())
            {
                Debug.Log("AdsFacade: Mostrando Interstitial Ad (simulato)...");
                _interstitialReady = false;
                InvokeAction(() => {
                    Debug.Log("AdsFacade: Interstitial Ad chiuso (simulato).");
                    onAdClosed?.Invoke(true);
                    LoadInterstitialAd();
                }, 1.5f);
            }
            else
            {
                Debug.LogWarning("AdsFacade: Interstitial Ad non pronto (o consenso mancante).");
                onAdClosed?.Invoke(false);
                LoadInterstitialAd();
            }
        }

        private static void InvokeAction(Action action, float delay)
        {
            if (Application.isPlaying)
            {
                GameObject delayRunnerGO = new GameObject($"ActionDelayRunner_{Guid.NewGuid()}");
                DelayExecutor executor = delayRunnerGO.AddComponent<DelayExecutor>();
                executor.ExecuteAfterDelay(action, delay, () => { if(delayRunnerGO != null) UnityEngine.Object.Destroy(delayRunnerGO); });
            } else {
                Debug.LogWarning("AdsFacade.InvokeAction: Esecuzione immediata fuori Play Mode (nessun ritardo).");
                action?.Invoke();
            }
        }
    }
}
