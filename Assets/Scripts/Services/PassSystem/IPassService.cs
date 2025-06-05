using ChaosCosmos.Core.Services;
using ChaosCosmos.Gameplay.PassSystem; // Per PassSeasonData, PassTierData
using System.Collections.Generic;

namespace ChaosCosmos.Services.PassSystem
{
    public interface IPassService : IService
    {
        PassSeasonData CurrentSeason { get; }
        int CurrentPassXP { get; } // XP accumulati per il pass corrente
        int CurrentPassTier { get; } // Ultimo tier sbloccato (basato su XP)
        bool HasPremiumPass { get; }

        void Initialize(PassSeasonData seasonData); // Per caricare la stagione corrente
        void AddPassXP(int amount); // Potrebbe essere chiamato da ProgressManager o direttamente
        bool IsTierUnlocked(int tierLevel);
        bool IsRewardClaimed(int tierLevel, bool isPremiumReward, RewardData reward);
        bool ClaimReward(int tierLevel, bool isPremiumReward, RewardData rewardToClaim);
        void PurchasePremiumPass(); // Simula l'acquisto

        void SavePassProgress();
        void LoadPassProgress();
        void ResetPassProgress();
    }
}
