using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.Progression;
using ChaosCosmos.Core.Constants;

namespace ChaosCosmos.Services.PassSystem
{
    public class PassManager : IPassService
    {
        public PassSeasonData CurrentSeason { get; private set; }
        public int CurrentPassXP { get; private set; }
        public int CurrentPassTier { get; private set; }
        public bool HasPremiumPass { get; private set; }

        private IProgressService _progressService;

        private string GetXPKey() => $"{PlayerPrefsKeys.PASS_SYSTEM_PREFIX}{CurrentSeason?.seasonID}_XP";
        private string GetPremiumKey() => $"{PlayerPrefsKeys.PASS_SYSTEM_PREFIX}{CurrentSeason?.seasonID}_HasPremium";
        private string GetClaimedRewardKey(int tier, bool isPremium, string rewardId) =>
            $"{PlayerPrefsKeys.PASS_SYSTEM_PREFIX}{CurrentSeason?.seasonID}_Tier{tier}_{(isPremium ? "P" : "F")}_{rewardId}_Claimed";

        private Dictionary<int, long> _cumulativeXpPerTier = new Dictionary<int, long>();

        public PassManager(IProgressService progressService = null)
        {
            _progressService = progressService;
            Debug.Log("PassManager: Creato. In attesa di Initialize().");
        }

        public void Initialize(PassSeasonData seasonData)
        {
            CurrentSeason = seasonData;
            if (CurrentSeason == null || CurrentSeason.tiers == null)
            {
                Debug.LogError("PassManager: CurrentSeason o i suoi tier sono null durante Initialize!");
                return;
            }

            _cumulativeXpPerTier.Clear();
            long cumulativeXp = 0;
            foreach (var tier in CurrentSeason.tiers.OrderBy(t => t.tierLevel)) // var è ok
            {
                cumulativeXp += tier.xpToUnlockThisTier;
                _cumulativeXpPerTier[tier.tierLevel] = cumulativeXp;
            }

            LoadPassProgress();
            UpdatePassTier();
            Debug.Log($"PassManager: Inizializzato con la stagione '{CurrentSeason.seasonName}'. XP Pass: {CurrentPassXP}, Tier: {CurrentPassTier}, Premium: {HasPremiumPass}");
        }

        public void AddPassXP(int amount)
        {
            if (amount <= 0 || CurrentSeason == null)
            { // Added braces
                return;
            }
            CurrentPassXP += amount;
            UpdatePassTier();
            Debug.Log($"PassManager: XP Pass aggiunti: {amount}. XP Pass Totali: {CurrentPassXP}. Nuovo Tier Calcolato: {CurrentPassTier}");
            SavePassProgress();
        }

        private void UpdatePassTier()
        {
            if (CurrentSeason == null || _cumulativeXpPerTier.Count == 0)
            {
                CurrentPassTier = 0;
                return;
            }
            int newTier = 0;
            foreach (var tierEntry in _cumulativeXpPerTier.OrderBy(kvp => kvp.Value)) // var è ok
            {
                if (CurrentPassXP >= tierEntry.Value)
                {
                    newTier = tierEntry.Key;
                }
                else
                {
                    break;
                }
            }
            CurrentPassTier = newTier;
        }

        public bool IsTierUnlocked(int tierLevel) => CurrentPassTier >= tierLevel;

        public bool IsRewardClaimed(int tierLevel, bool isPremiumReward, RewardData reward)
        {
            if (reward == null || string.IsNullOrEmpty(reward.rewardID) || CurrentSeason == null)
            { // Added braces
                return false;
            }
            return PlayerPrefs.GetInt(GetClaimedRewardKey(tierLevel, isPremiumReward, reward.rewardID), 0) == 1;
        }

        public bool ClaimReward(int tierLevel, bool isPremiumReward, RewardData rewardToClaim)
        {
            if (CurrentSeason == null || rewardToClaim == null || string.IsNullOrEmpty(rewardToClaim.rewardID))
            {
                Debug.LogError("PassManager: Stagione o ricompensa (o ID ricompensa) non valida per il claim.");
                return false;
            }
            if (!IsTierUnlocked(tierLevel))
            {
                Debug.LogWarning($"PassManager: Tentativo di claim ricompensa per tier {tierLevel} non ancora sbloccato. Tier attuale: {CurrentPassTier}");
                return false;
            }
            if (isPremiumReward && !HasPremiumPass)
            {
                Debug.LogWarning($"PassManager: Tentativo di claim ricompensa premium per tier {tierLevel} senza avere il Pass Premium.");
                return false;
            }
            if (IsRewardClaimed(tierLevel, isPremiumReward, rewardToClaim))
            {
                Debug.LogWarning($"PassManager: Ricompensa '{rewardToClaim.displayName}' del tier {tierLevel} (Premium: {isPremiumReward}) già richiesta.");
                return false;
            }

            bool rewardGrantedSuccessfully = false;
            switch (rewardToClaim.type)
            {
                case RewardType.XPCurrency:
                    if (_progressService != null)
                    {
                        _progressService.AddXP(rewardToClaim.amount);
                        Debug.Log($"PassManager: Ricompensa '{rewardToClaim.displayName}' (XP: {rewardToClaim.amount}) erogata tramite ProgressService.");
                        rewardGrantedSuccessfully = true;
                    }
                    else
                    {
                        Debug.LogError("PassManager: _progressService è nullo. Impossibile erogare ricompensa XP.");
                    }
                    break;
                case RewardType.SpecificUpgrade:
                    if (_progressService != null && rewardToClaim.upgradeToGrant != null)
                    {
                        rewardGrantedSuccessfully = _progressService.GrantFreeUpgrade(rewardToClaim.upgradeToGrant);
                        if (rewardGrantedSuccessfully)
                        {
                            Debug.Log($"PassManager: Ricompensa '{rewardToClaim.displayName}' (Upgrade: {rewardToClaim.upgradeToGrant.upgradeName}) erogata tramite ProgressService.");
                        }
                        else
                        {
                            Debug.LogWarning($"PassManager: Fallimento erogazione ricompensa Upgrade '{rewardToClaim.upgradeToGrant.upgradeName}'. Controllare log di ProgressManager.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"PassManager: _progressService o upgradeToGrant nullo per ricompensa SpecificUpgrade '{rewardToClaim.displayName}'.");
                    }
                    break;
                case RewardType.SoftCurrency:
                    Debug.Log($"PassManager: Ricompensa '{rewardToClaim.displayName}' (SoftCurrency: {rewardToClaim.amount}) richiesta. (TODO: Integrare CurrencyManager)");
                    rewardGrantedSuccessfully = true;
                    break;
                case RewardType.HardCurrency:
                    Debug.Log($"PassManager: Ricompensa '{rewardToClaim.displayName}' (HardCurrency: {rewardToClaim.amount}) richiesta. (TODO: Integrare CurrencyManager)");
                    rewardGrantedSuccessfully = true;
                    break;
                case RewardType.Skin:
                    Debug.Log($"PassManager: Ricompensa '{rewardToClaim.displayName}' (Skin) richiesta. (TODO: Integrare Inventory/SkinManager)");
                    rewardGrantedSuccessfully = true;
                    break;
                default:
                    Debug.LogWarning($"PassManager: Tipo di ricompensa non gestito: {rewardToClaim.type} per '{rewardToClaim.displayName}'.");
                    rewardGrantedSuccessfully = true;
                    break;
            }

            if (rewardGrantedSuccessfully)
            {
                PlayerPrefs.SetInt(GetClaimedRewardKey(tierLevel, isPremiumReward, rewardToClaim.rewardID), 1);
                SavePassProgress();
                Debug.Log($"PassManager: Stato claim per '{rewardToClaim.displayName}' del tier {tierLevel} (Premium: {isPremiumReward}) salvato.");
                return true;
            }
            else
            {
                Debug.LogError($"PassManager: Erogazione effettiva della ricompensa '{rewardToClaim.displayName}' fallita. Il claim non verrà registrato.");
                return false;
            }
        }

        public void PurchasePremiumPass()
        {
            if (CurrentSeason == null)
            { // Added braces
                return;
            }
            HasPremiumPass = true;
            Debug.Log("PassManager: Pass Premium acquistato!");
            SavePassProgress();
        }

        public void SavePassProgress()
        {
            if (CurrentSeason == null)
            { // Added braces
                return;
            }
            PlayerPrefs.SetInt(GetXPKey(), CurrentPassXP);
            PlayerPrefs.SetInt(GetPremiumKey(), HasPremiumPass ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void LoadPassProgress()
        {
            if (CurrentSeason == null)
            {
                Debug.LogWarning("PassManager.LoadPassProgress: CurrentSeason è null. Impossibile caricare.");
                return;
            }
            CurrentPassXP = PlayerPrefs.GetInt(GetXPKey(), 0);
            HasPremiumPass = PlayerPrefs.GetInt(GetPremiumKey(), 0) == 1;
        }

        public void ResetPassProgress()
        {
            if (CurrentSeason == null)
            { // Added braces
                return;
            }
            PlayerPrefs.DeleteKey(GetXPKey());
            PlayerPrefs.DeleteKey(GetPremiumKey());

            if (CurrentSeason.tiers != null)
            {
                foreach(var tier in CurrentSeason.tiers) // var è ok
                {
                    if (tier.freeRewards != null)
                    {
                        foreach(var reward in tier.freeRewards) // var è ok
                        {
                            if(reward != null && !string.IsNullOrEmpty(reward.rewardID))
                            { // Added braces
                                PlayerPrefs.DeleteKey(GetClaimedRewardKey(tier.tierLevel, false, reward.rewardID));
                            }
                        }
                    }
                    if (tier.premiumRewards != null)
                    {
                        foreach(var reward in tier.premiumRewards) // var è ok
                        {
                            if(reward != null && !string.IsNullOrEmpty(reward.rewardID))
                            { // Added braces
                                PlayerPrefs.DeleteKey(GetClaimedRewardKey(tier.tierLevel, true, reward.rewardID));
                            }
                        }
                    }
                }
            }

            CurrentPassXP = 0;
            HasPremiumPass = false;
            PlayerPrefs.Save();

            PassSeasonData tempSeason = CurrentSeason;
            CurrentSeason = null;
            Initialize(tempSeason);
            Debug.Log("PassManager: Progresso Pass resettato.");
        }
    }
}
