using UnityEngine;
using TMPro;
using System.Collections.Generic;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Services.PassSystem;
using System;
using System.Linq; // Per OrderBy su tier.xpToUnlockThisTier, anche se non strettamente necessario qui

namespace ChaosCosmos.UI.Meta
{
    public class PassTierUIItem : MonoBehaviour
    {
        [Header("UI References - Header")]
        public TextMeshProUGUI tierLevelText;
        public TextMeshProUGUI tierXPText;
        public Image tierStatusImage;

        [Header("UI References - Rewards")]
        public RectTransform freeRewardsContainer;
        public RectTransform premiumRewardsContainer;
        public GameObject passRewardItemPrefab;

        [Header("Visuals (Opzionale)")]
        public Image premiumLockOverlay;

        private PassTierData _tierData;
        private IPassService _passService;
        private Action<int, bool, RewardData> _onClaimRewardClickedCallback;

        private List<PassRewardUIItem> _instantiatedRewardItems = new List<PassRewardUIItem>();

        public void Setup(PassTierData tierData, IPassService passService, Action<int, bool, RewardData> onClaimRewardClicked)
        {
            _tierData = tierData;
            _passService = passService;
            _onClaimRewardClickedCallback = onClaimRewardClicked;

            if (_tierData == null)
            {
                Debug.LogError("PassTierUIItem: PassTierData è nullo in Setup.");
                gameObject.SetActive(false);
                return;
            }
             if (_passService == null)
            {
                Debug.LogError($"PassTierUIItem (Tier {_tierData.tierLevel}): IPassService è nullo in Setup.");
                gameObject.SetActive(false);
                return;
            }
            if (passRewardItemPrefab == null)
            {
                Debug.LogError($"PassTierUIItem (Tier {_tierData.tierLevel}): PassRewardItemPrefab non assegnato.");
                gameObject.SetActive(false);
                return;
            }

            PopulateUI();
        }

        public void PopulateUI()
        {
            if (_tierData == null || _passService == null)
            {
                // Debug.LogWarning("PassTierUIItem.PopulateUI: Dati o servizio mancanti.");
                return;
            }

            if (tierLevelText != null)
            {
                tierLevelText.text = $"Tier {_tierData.tierLevel}";
            }

            bool tierUnlocked = _passService.IsTierUnlocked(_tierData.tierLevel);

            if (tierXPText != null)
            {
                if (tierUnlocked)
                {
                    tierXPText.text = "Sbloccato";
                }
                else
                {
                    // Trovare l'XP cumulativo per questo tier e quello precedente per mostrare progresso
                    // Questo richiede accesso alla logica di _cumulativeXpPerTier di PassManager
                    // Per ora, mostriamo solo l'XP richiesto per questo specifico tier.
                    // Una soluzione migliore sarebbe PassManager che espone XP/XPMaxPerTier(level)
                    long xpForThisTierUnlock = GetCumulativeXpForTier(_tierData.tierLevel);
                    long previousTierUnlockXp = _tierData.tierLevel > 1 ? GetCumulativeXpForTier(_tierData.tierLevel - 1) : 0;
                    long currentPassXp = _passService.CurrentPassXP;

                    if (xpForThisTierUnlock > previousTierUnlockXp) // Evita divisione per zero se xpToUnlockThisTier è 0
                    {
                        long progressInTier = Mathf.Max(0, currentPassXp - previousTierUnlockXp);
                        long neededForTier = xpForThisTierUnlock - previousTierUnlockXp;
                        tierXPText.text = $"{progressInTier} / {neededForTier} XP";
                    } else {
                         tierXPText.text = $"{_tierData.xpToUnlockThisTier} XP"; // Fallback
                    }
                }
            }

            if (tierStatusImage != null)
            {
                // TODO: Aggiorna sprite bloccato/sbloccato in base a tierUnlocked
                // Esempio: tierStatusImage.sprite = tierUnlocked ? _unlockedSprite : _lockedSprite;
            }

            foreach (var item in _instantiatedRewardItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _instantiatedRewardItems.Clear();

            if (freeRewardsContainer != null)
            {
                foreach (var rewardData in _tierData.freeRewards)
                {
                    InstantiateRewardItem(rewardData, freeRewardsContainer, false, tierUnlocked);
                }
            }

            if (premiumRewardsContainer != null)
            {
                foreach (var rewardData in _tierData.premiumRewards)
                {
                    InstantiateRewardItem(rewardData, premiumRewardsContainer, true, tierUnlocked);
                }
                if (premiumLockOverlay != null)
                {
                    premiumLockOverlay.SetActive(!_passService.HasPremiumPass && _tierData.premiumRewards.Any());
                    // Mostra il lucchetto solo se ci sono ricompense premium e l'utente non ha il pass.
                    // Potrebbe essere anche per tier non sbloccato.
                }
            }
            RefreshRewardStates(); // Assicura che gli stati dei pulsanti siano corretti
        }

        // Helper per ottenere XP cumulativi (duplicazione semplificata da PassManager per UI)
        private long GetCumulativeXpForTier(int tierLevel)
        {
            if (_passService == null || _passService.CurrentSeason == null || _passService.CurrentSeason.tiers == null) return 0;

            long cumulative = 0;
            foreach(var tier in _passService.CurrentSeason.tiers.OrderBy(t => t.tierLevel))
            {
                if (tier.tierLevel <= tierLevel)
                {
                    cumulative += tier.xpToUnlockThisTier;
                }
                if (tier.tierLevel == tierLevel) break;
            }
            return cumulative;
        }


        private void InstantiateRewardItem(RewardData rewardData, RectTransform parent, bool isPremium, bool isTierUnlocked)
        {
            if (rewardData == null)
            {
                return;
            }
            if (passRewardItemPrefab == null)
            {
                Debug.LogError($"PassTierUIItem (Tier {_tierData.tierLevel}): PassRewardItemPrefab non assegnato, impossibile istanziare ricompensa '{rewardData.displayName}'.");
                return;
            }

            GameObject itemGO = Instantiate(passRewardItemPrefab, parent);
            PassRewardUIItem uiItem = itemGO.GetComponent<PassRewardUIItem>();
            if (uiItem != null)
            {
                bool isClaimed = _passService.IsRewardClaimed(_tierData.tierLevel, isPremium, rewardData);
                uiItem.Setup(rewardData, _tierData.tierLevel, isPremium,
                             isTierUnlocked, isClaimed, _passService.HasPremiumPass,
                             _onClaimRewardClickedCallback);
                _instantiatedRewardItems.Add(uiItem);
            }
            else
            {
                Debug.LogError($"PassTierUIItem (Tier {_tierData.tierLevel}): Prefab ricompensa '{passRewardItemPrefab.name}' non ha il componente PassRewardUIItem!");
                Destroy(itemGO);
            }
        }

        public void RefreshRewardStates()
        {
            if (_tierData == null || _passService == null || _instantiatedRewardItems == null) return;

            bool tierUnlocked = _passService.IsTierUnlocked(_tierData.tierLevel);
            bool hasPremium = _passService.HasPremiumPass;

            foreach(var uiItem in _instantiatedRewardItems)
            {
                if (uiItem != null)
                {
                    RewardData rd = uiItem.GetRewardDataForRefresh(); // Assumendo che questo metodo esista
                    if (rd != null)
                    {
                        bool isClaimed = _passService.IsRewardClaimed(_tierData.tierLevel, uiItem.IsPremiumForRefresh(), rd);
                        uiItem.UpdateState(tierUnlocked, isClaimed, hasPremium);
                    }
                }
            }
            if (premiumLockOverlay != null)
            {
                // Mostra il lucchetto solo se ci sono ricompense premium e l'utente non ha il pass.
                // Non importa se il tier è sbloccato o meno per il lucchetto generale del premium.
                premiumLockOverlay.SetActive(!hasPremium && _tierData.premiumRewards.Any(r => r != null));
            }
        }
    }
}
