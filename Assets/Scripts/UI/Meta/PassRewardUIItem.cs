using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChaosCosmos.Gameplay.PassSystem;
using System;

namespace ChaosCosmos.UI.Meta
{
    public class PassRewardUIItem : MonoBehaviour
    {
        [Header("UI References")]
        public Image iconImage;
        public TextMeshProUGUI nameOrAmountText;
        public Button claimButton;
        public TextMeshProUGUI claimButtonText; // Testo del bottone, es. "Richiedi", "Bloccato", "Richiesto"
        public GameObject claimedOverlay;

        private RewardData _rewardData;
        private int _tierLevel;
        private bool _isPremiumReward;
        private Action<int, bool, RewardData> _onClaimRewardClicked;

        public void Setup(RewardData rewardData, int tierLevel, bool isPremium,
                          bool isTierUnlocked, bool isRewardClaimed, bool hasPremiumPass,
                          Action<int, bool, RewardData> onClaimRewardClicked)
        {
            _rewardData = rewardData;
            _tierLevel = tierLevel;
            _isPremiumReward = isPremium;
            _onClaimRewardClicked = onClaimRewardClicked;

            if (_rewardData == null)
            {
                Debug.LogWarning("PassRewardUIItem: RewardData è nullo in Setup. Disattivazione item.");
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true); // Assicura sia attivo se i dati sono validi

            if (iconImage != null)
            {
                iconImage.sprite = _rewardData.icon;
                iconImage.enabled = (_rewardData.icon != null);
            }

            if (nameOrAmountText != null)
            {
                string displayText = "";
                if (_rewardData.type == RewardType.XPCurrency ||
                    _rewardData.type == RewardType.SoftCurrency ||
                    _rewardData.type == RewardType.HardCurrency)
                {
                    displayText = $"{_rewardData.amount} {_rewardData.displayName}";
                }
                else
                {
                    displayText = _rewardData.displayName;
                }
                nameOrAmountText.text = displayText;
            }

            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(HandleClaimClicked);
            }
            else
            {
                Debug.LogError($"PassRewardUIItem ({_rewardData.displayName}): ClaimButton non assegnato!");
            }
            UpdateState(isTierUnlocked, isRewardClaimed, hasPremiumPass);
        }

        public void UpdateState(bool isTierUnlocked, bool isRewardClaimed, bool hasPremiumPass)
        {
            if (_rewardData == null)
            {
                // Debug.LogWarning("PassRewardUIItem.UpdateState: _rewardData è nullo.");
                return;
            }

            if (claimedOverlay != null)
            {
                claimedOverlay.SetActive(isRewardClaimed);
            }

            if (claimButton == null) return; // Non possiamo aggiornare il bottone se non c'è

            if (isRewardClaimed)
            {
                if (claimButtonText != null) claimButtonText.text = "Richiesto";
                claimButton.interactable = false;
            }
            else if (!isTierUnlocked)
            {
                if (claimButtonText != null) claimButtonText.text = "Bloccato";
                claimButton.interactable = false;
            }
            else if (_isPremiumReward && !hasPremiumPass)
            {
                if (claimButtonText != null) claimButtonText.text = "Pass Premium";
                claimButton.interactable = false;
            }
            else
            {
                if (claimButtonText != null) claimButtonText.text = "Richiedi";
                claimButton.interactable = true;
            }
        }

        private void HandleClaimClicked()
        {
            if (_rewardData == null)
            {
                Debug.LogError("PassRewardUIItem.HandleClaimClicked: _rewardData è nullo!");
                return;
            }
            _onClaimRewardClicked?.Invoke(_tierLevel, _isPremiumReward, _rewardData);
        }

        // Metodi helper per PassTierUIItem.RefreshRewardStates()
        public bool IsPremiumForRefresh() => _isPremiumReward;
        public RewardData GetRewardDataForRefresh() => _rewardData;
    }
}
