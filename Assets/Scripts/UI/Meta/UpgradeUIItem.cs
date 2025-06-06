using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChaosCosmos.Gameplay.Progression;
using ChaosCosmos.Services.Progression;
using ChaosCosmos.Services.RemoteConfig;
using ChaosCosmos.Core.Services;
using System;
using ChaosCosmos.Core.Constants; // Per RemoteConfigKeyPatterns

namespace ChaosCosmos.UI.Meta
{
    public class UpgradeUIItem : MonoBehaviour
    {
        [Header("UI References")]
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI costText;
        public Button purchaseButton;
        public TextMeshProUGUI purchaseButtonText; // Testo del bottone, es. "Acquista" o "Max"

        private UpgradeData _upgradeData;
        private IProgressService _progressService;
        private IRemoteConfigService _remoteConfigService;
        private Action _onUpgradePurchasedCallback;

        public void Setup(UpgradeData upgradeData, IProgressService progressService, IRemoteConfigService remoteConfigService, Action onUpgradePurchased)
        {
            _upgradeData = upgradeData;
            _progressService = progressService;
            _remoteConfigService = remoteConfigService;
            _onUpgradePurchasedCallback = onUpgradePurchased;

            if (_upgradeData == null)
            {
                Debug.LogError("UpgradeUIItem: UpgradeData è nullo in Setup.");
                gameObject.SetActive(false);
                return;
            }
            if (_progressService == null)
            {
                Debug.LogError($"UpgradeUIItem ({_upgradeData.name}): IProgressService è nullo in Setup.");
                gameObject.SetActive(false);
                return;
            }
            // _remoteConfigService può essere null, GetActualCost() lo gestisce.

            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveAllListeners(); // Rimuovi vecchi listener
                purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
            }
            else
            {
                Debug.LogError($"UpgradeUIItem ({_upgradeData.name}): PurchaseButton non assegnato!");
            }

            UpdateUI();
        }

        private int GetActualCost()
        {
            if (_upgradeData == null) return int.MaxValue;

            int defaultCost = _upgradeData.xpCost;
            if (_remoteConfigService != null && _remoteConfigService.IsReady)
            {
                // Assicurati che upgradeID sia valido per la chiave
                if (string.IsNullOrEmpty(_upgradeData.upgradeID))
                {
                    Debug.LogWarning($"UpgradeUIItem ({_upgradeData.name}): upgradeID è nullo o vuoto, non posso ottenere costo da RC.");
                    return defaultCost;
                }
                string remoteConfigKey = RemoteConfigKeyPatterns.GetUpgradeXpCostKey(_upgradeData.upgradeID);
                return _remoteConfigService.GetInt(remoteConfigKey, defaultCost);
            }
            return defaultCost;
        }

        public void UpdateUI()
        {
            if (_upgradeData == null || _progressService == null)
            {
                // Debug.LogWarning($"UpgradeUIItem: Impossibile aggiornare UI, UpgradeData o ProgressService nulli.");
                // Potrebbe essere chiamato prima che Setup sia completo o se l'oggetto viene disattivato.
                return;
            }

            if (nameText != null) nameText.text = _upgradeData.upgradeName;
            if (descriptionText != null) descriptionText.text = _upgradeData.description;
            if (iconImage != null)
            {
                iconImage.sprite = _upgradeData.icon;
                iconImage.enabled = (_upgradeData.icon != null);
            }

            int currentLevel = _progressService.GetUpgradeLevel(_upgradeData.upgradeID);
            int maxLevel = _upgradeData.maxLevel;
            if (levelText != null) levelText.text = $"Livello: {currentLevel} / {maxLevel}";

            int actualCost = GetActualCost();

            if (currentLevel >= maxLevel)
            {
                if (costText != null) costText.text = "MAX LIVELLO";
                if (purchaseButtonText != null) purchaseButtonText.text = "Max";
                if (purchaseButton != null) purchaseButton.interactable = false;
            }
            else
            {
                if (costText != null) costText.text = $"Costo: {actualCost} XP";
                if (purchaseButtonText != null) purchaseButtonText.text = "Acquista";
                if (purchaseButton != null) purchaseButton.interactable = _progressService.CanAffordUpgrade(_upgradeData);
            }
        }

        private void OnPurchaseButtonClicked()
        {
            if (_progressService != null && _upgradeData != null)
            {
                bool success = _progressService.PurchaseUpgrade(_upgradeData);
                if (success)
                {
                    Debug.Log($"Upgrade '{_upgradeData.upgradeName}' acquistato con successo via UI.");
                    // UpdateUI(); // L'aggiornamento di questo item specifico è già fatto da RefreshAllUI nel panel
                    _onUpgradePurchasedCallback?.Invoke();
                }
                else
                {
                    Debug.LogWarning($"Acquisto fallito per '{_upgradeData.upgradeName}' via UI. (XP Insufficienti, Max Livello o Prerequisiti mancanti)");
                    // Potrebbe essere utile aggiornare la UI qui per riflettere lo stato (es. pulsante disabilitato)
                    // se CanAffordUpgrade è cambiato a causa di XP spesi altrove, ma il callback lo farà.
                    UpdateUI(); // Aggiorna comunque per riflettere lo stato (es. se XP sono cambiati)
                }
            }
        }
    }
}
