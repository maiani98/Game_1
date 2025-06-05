using System;
using ChaosCosmos.Core.Services;

namespace ChaosCosmos.Services.Ads
{
    public enum AdCompletionStatus { Completed, Failed, Skipped }

    public interface IAdsService : IService
    {
        bool IsInitialized { get; }
        void Initialize();

        bool IsRewardedVideoReady();
        void ShowRewardedVideo(Action<AdCompletionStatus> onAdCompleted);

        bool IsInterstitialAdReady();
        void ShowInterstitialAd(Action<bool> onAdClosed); // true se mostrato con successo

        // Per ora non implementiamo banner, ma l'interfaccia può prevederli
        // bool IsBannerAdLoaded();
        // void ShowBannerAd();
        // void HideBannerAd();
    }
}
