using System;
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace ChaosCosmos.Services.Ads
{
    /// <summary>
    /// Implementation of <see cref="IAdsService"/> based on Unity Ads.
    /// Requires the "Advertisement" package to be installed via the
    /// Unity Package Manager.
    /// </summary>
public class UnityAdsService : IAdsService, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private const string RewardedPlacement = "rewardedVideo";
        private const string InterstitialPlacement = "interstitial";

        private readonly string _androidGameId;
        private readonly string _iOSGameId;
        private readonly bool _testMode;
        private string _gameId;

        private bool _rewardedReady;
        private bool _interstitialReady;

        public bool IsInitialized { get; private set; }

        public UnityAdsService(string androidGameId, string iosGameId, bool testMode = true)
        {
            _androidGameId = androidGameId;
            _iOSGameId = iosGameId;
            _testMode = testMode;
        }

        public void Initialize()
        {
#if UNITY_IOS
            _gameId = _iOSGameId;
#elif UNITY_ANDROID
            _gameId = _androidGameId;
#else
            Debug.LogWarning("UnityAdsService: Unsupported platform for ads.");
            return;
#endif
            if (Advertisement.isInitialized)
            {
                IsInitialized = true;
                LoadRewardedVideo();
                LoadInterstitialAd();
            }
            else
            {
                Advertisement.Initialize(_gameId, _testMode, this);
            }
        }

        public bool IsRewardedVideoReady() => _rewardedReady && Advertisement.isInitialized;

        public void ShowRewardedVideo(Action<AdCompletionStatus> onAdCompleted)
        {
            if (!IsRewardedVideoReady())
            {
                onAdCompleted?.Invoke(AdCompletionStatus.Failed);
                return;
            }
            Advertisement.Show(RewardedPlacement, this);
            _rewardedCallback = onAdCompleted;
        }

        public bool IsInterstitialAdReady() => _interstitialReady && Advertisement.isInitialized;

        public void ShowInterstitialAd(Action<bool> onAdClosed)
        {
            if (!IsInterstitialAdReady())
            {
                onAdClosed?.Invoke(false);
                return;
            }
            Advertisement.Show(InterstitialPlacement, this);
            _interstitialCallback = onAdClosed;
        }

        private void LoadRewardedVideo()
        {
            Advertisement.Load(RewardedPlacement, this);
        }

        private void LoadInterstitialAd()
        {
            Advertisement.Load(InterstitialPlacement, this);
        }

        private Action<AdCompletionStatus> _rewardedCallback;
        private Action<bool> _interstitialCallback;

        // Initialization callbacks
        public void OnInitializationComplete()
        {
            Debug.Log("UnityAdsService: initialization complete");
            IsInitialized = true;
            LoadRewardedVideo();
            LoadInterstitialAd();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"UnityAds initialization failed: {error} - {message}");
            IsInitialized = false;
        }

        // Load callbacks
        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId == RewardedPlacement)
                _rewardedReady = true;
            if (placementId == InterstitialPlacement)
                _interstitialReady = true;
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogWarning($"UnityAds failed to load {placementId}: {error} - {message}");
            if (placementId == RewardedPlacement)
                _rewardedReady = false;
            if (placementId == InterstitialPlacement)
                _interstitialReady = false;
        }

        // Show callbacks
        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogError($"UnityAds show failed: {placementId} {error} - {message}");
            if (placementId == RewardedPlacement)
                _rewardedCallback?.Invoke(AdCompletionStatus.Failed);
            else if (placementId == InterstitialPlacement)
                _interstitialCallback?.Invoke(false);
        }

        public void OnUnityAdsShowStart(string placementId) { }

        public void OnUnityAdsShowClick(string placementId) { }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (placementId == RewardedPlacement)
            {
                var status = showCompletionState == UnityAdsShowCompletionState.COMPLETED ? AdCompletionStatus.Completed : AdCompletionStatus.Skipped;
                _rewardedCallback?.Invoke(status);
                _rewardedReady = false;
                LoadRewardedVideo();
            }
            else if (placementId == InterstitialPlacement)
            {
                _interstitialCallback?.Invoke(true);
                _interstitialReady = false;
                LoadInterstitialAd();
            }
        }
    }
#else
    /// <summary>
    /// Fallback implementation used when the Unity Ads package is missing.
    /// It logs warnings but allows the project to compile.
    /// </summary>
    public class UnityAdsService : IAdsService
    {
        public bool IsInitialized => false;

        public UnityAdsService(string androidGameId, string iosGameId, bool testMode = true) { }

        public void Initialize()
        {
            Debug.LogWarning("UnityAdsService: Unity Ads package not installed.");
        }

        public bool IsRewardedVideoReady() => false;

        public void ShowRewardedVideo(Action<AdCompletionStatus> onAdCompleted)
        {
            Debug.LogWarning("UnityAdsService: Rewarded video not available.");
            onAdCompleted?.Invoke(AdCompletionStatus.Failed);
        }

        public bool IsInterstitialAdReady() => false;

        public void ShowInterstitialAd(Action<bool> onAdClosed)
        {
            Debug.LogWarning("UnityAdsService: Interstitial ad not available.");
            onAdClosed?.Invoke(false);
        }
    }
#endif
}
