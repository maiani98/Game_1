using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Services.PassSystem;
using ChaosCosmos.Services.IAP;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Core.Constants;

namespace ChaosCosmos.UI.Meta
{
    public class BattlePassPanelUI : MonoBehaviour
    {
        [Header("UI References - Header")]
        public TextMeshProUGUI passXPText;
        public TextMeshProUGUI seasonNameText;
        public Button purchasePremiumButton;
        public TextMeshProUGUI purchasePremiumButtonText;

        [Header("UI References - Tiers")]
        public RectTransform tiersContentParent;
        public GameObject passTierItemPrefab;

        private IPassService _passService;
        private IIAPService _iapService;

        private List<PassTierUIItem> _uiTierItems = new List<PassTierUIItem>();
        private bool _isInitialized = false;

        public string premiumPassIAPProductID = "com.chaoscosmos.premiumpass_season1";

        void Start()
        {
            InitializePanel();
        }

        public void InitializePanel()
        {
            if (_isInitialized) return;

            bool gotPassService = TryGetService(out _passService);
            bool gotIAPService = TryGetService(out _iapService);

            if (!gotPassService)
            {
                Debug.LogError("BattlePassPanelUI: IPassService non registrato. Il pannello non può funzionare.");
                gameObject.SetActive(false);
                return;
            }
             if (!gotIAPService)
            {
                Debug.LogWarning("BattlePassPanelUI: IIAPService non registrato. Il pulsante Acquista Premium sarà disabilitato.");
                if (purchasePremiumButton != null) purchasePremiumButton.gameObject.SetActive(false);
            }

            if (passTierItemPrefab == null || tiersContentParent == null)
            {
                Debug.LogError("BattlePassPanelUI: Prefab del tier o parent del contenuto non assegnati!");
                gameObject.SetActive(false);
                return;
            }

            if (_passService.CurrentSeason == null) {
                Debug.LogError("BattlePassPanelUI: Nessuna stagione Pass caricata in PassService!");
                if (seasonNameText != null) seasonNameText.text = "Nessuna Stagione Attiva";
                if (passXPText != null) passXPText.text = "";
                if (purchasePremiumButton != null) purchasePremiumButton.gameObject.SetActive(false);
                return;
            }

            if (purchasePremiumButton != null)
            {
                purchasePremiumButton.onClick.RemoveAllListeners();
                purchasePremiumButton.onClick.AddListener(OnPurchasePremiumClicked);
            }

            PopulateTiers();
            RefreshPassData();
            _isInitialized = true;
        }

        private bool TryGetService<T>(out T service) where T : class, IService
        {
            if (ServiceLocator.IsRegistered<T>())
            {
                service = ServiceLocator.Get<T>();
                if (service is IPassService pass && pass.CurrentSeason == null) Debug.LogWarning($"BattlePassPanelUI: {typeof(T).Name} recuperato ma CurrentSeason è null.");
                return service != null;
            }
            service = null;
            Debug.LogWarning($"BattlePassPanelUI: Servizio {typeof(T).Name} non registrato in ServiceLocator.");
            return false;
        }

        void OnEnable()
        {
            if (_isInitialized)
            {
                RefreshAllUI();
            }
        }

        public void RefreshAllUI()
        {
            if (!_isInitialized || _passService == null || _passService.CurrentSeason == null)
            {
                return;
            }

            RefreshPassData();
            foreach (var item in _uiTierItems)
            {
                if (item != null) item.PopulateUI();
            }
        }

        private void RefreshPassData()
        {
            if (_passService == null || _passService.CurrentSeason == null) return;

            if (seasonNameText != null)
            {
                seasonNameText.text = _passService.CurrentSeason.seasonName;
            }

            int currentTierLevel = _passService.CurrentPassTier;
            long currentPassXP = _passService.CurrentPassXP;
            var orderedTiers = _passService.CurrentSeason.tiers.OrderBy(t => t.tierLevel).ToList();

            if (passXPText != null)
            {
                if (!orderedTiers.Any())
                {
                    passXPText.text = $"XP: {currentPassXP}";
                }
                else if (currentTierLevel == 0)
                {
                    passXPText.text = $"Tier {currentTierLevel}: {currentPassXP} / {orderedTiers.First().xpToUnlockThisTier} XP";
                }
                else
                {
                    PassTierData currentTierSO = orderedTiers.FirstOrDefault(t => t.tierLevel == currentTierLevel);
                    PassTierData nextTierSO = orderedTiers.FirstOrDefault(t => t.tierLevel == currentTierLevel + 1);
                    long xpForCurrentTierLevelStart = GetCumulativeXpForTier(currentTierLevel - 1); // XP per sbloccare il tier *precedente*

                    if (currentTierSO != null)
                    {
                        long progressInCurrentTier = currentPassXP - xpForCurrentTierLevelStart;
                        long currentTierXpNeeded = currentTierSO.xpToUnlockThisTier;
                         if (nextTierSO != null) // C'è un prossimo tier
                        {
                            passXPText.text = $"Tier {currentTierLevel}: {progressInCurrentTier} / {currentTierXpNeeded} XP";
                        }
                        else // Siamo all'ultimo tier definito
                        {
                             passXPText.text = $"Tier {currentTierLevel}: {progressInCurrentTier} / {currentTierXpNeeded} XP (Max)";
                        }
                    } else { // currentTierLevel > 0 ma non trovato (dati corrotti?)
                         passXPText.text = $"XP: {currentPassXP}";
                    }
                }
            }

            if (purchasePremiumButton != null)
            {
                if (purchasePremiumButtonText != null)
                {
                    purchasePremiumButtonText.text = _passService.HasPremiumPass ? "Premium Attivo" : "Acquista Premium";
                }
                purchasePremiumButton.interactable = !_passService.HasPremiumPass && (_iapService != null && _iapService.IsInitialized);
            }
        }

        private long GetCumulativeXpForTier(int targetTierLevel)
        {
            if (_passService == null || _passService.CurrentSeason == null || _passService.CurrentSeason.tiers == null) return 0;

            long cumulative = 0;
            foreach(var tier in _passService.CurrentSeason.tiers.OrderBy(t => t.tierLevel))
            {
                if (tier.tierLevel <= targetTierLevel)
                {
                    cumulative += tier.xpToUnlockThisTier;
                }
                else
                {
                    break;
                }
            }
            return cumulative;
        }

        private void PopulateTiers()
        {
            foreach (Transform child in tiersContentParent)
            {
                Destroy(child.gameObject);
            }
            _uiTierItems.Clear();

            if (_passService.CurrentSeason?.tiers == null)
            {
                Debug.LogWarning("BattlePassPanelUI: Nessun tier definito nella stagione corrente.");
                return;
            }

            foreach (PassTierData tier in _passService.CurrentSeason.tiers.OrderBy(t => t.tierLevel))
            {
                if (passTierItemPrefab == null)
                {
                    Debug.LogError("BattlePassPanelUI: passTierItemPrefab non assegnato!");
                    continue;
                }
                GameObject itemGO = Instantiate(passTierItemPrefab, tiersContentParent);
                PassTierUIItem uiItem = itemGO.GetComponent<PassTierUIItem>();
                if (uiItem != null)
                {
                    uiItem.Setup(tier, _passService, OnClaimRewardClicked);
                    _uiTierItems.Add(uiItem);
                } else { Debug.LogError("Prefab PassTierItem non ha lo script PassTierUIItem!"); Destroy(itemGO); }
            }
        }

        private void OnClaimRewardClicked(int tierLevel, bool isPremiumReward, RewardData rewardData)
        {
            // TODO_SFX: UI_Button_Click_Standard (sul pulsante di claim dentro PassRewardUIItem)
            Debug.Log($"BattlePassPanelUI: Tentativo claim: Tier {tierLevel}, Premium: {isPremiumReward}, Reward: {rewardData.displayName}");
            bool success = _passService.ClaimReward(tierLevel, isPremiumReward, rewardData);
            if (success)
            {
                // TODO_SFX: UI_Reward_Claimed_Success
                Debug.Log("Claim riuscito, aggiorno UI.");
                RefreshAllUI();
            }
            else
            {
                // TODO_SFX: UI_Error_Sound
                Debug.LogWarning("Claim fallito. Vedi log di PassManager.");
            }
        }

        private void OnPurchasePremiumClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            if (_iapService != null && _iapService.IsInitialized)
            {
                Debug.Log($"BattlePassPanelUI: Tentativo acquisto IAP per Pass Premium: {premiumPassIAPProductID}");
                _iapService.PurchaseProduct(premiumPassIAPProductID, (success, reason, transactionID) => {
                    if (success)
                    {
                        // TODO_SFX: UI_PremiumPass_Purchased
                        Debug.Log("BattlePassPanelUI: Acquisto IAP Pass Premium RIUSCITO (simulato). Attivo Pass in PassService.");
                        if (_passService != null) _passService.PurchasePremiumPass();
                        RefreshAllUI();
                    }
                    else
                    {
                        // TODO_SFX: UI_Error_Sound
                        Debug.LogError($"BattlePassPanelUI: Acquisto IAP Pass Premium FALLITO (simulato). Ragione: {reason}, Msg: {transactionID}");
                    }
                });
            }
            else
            {
                Debug.LogError("BattlePassPanelUI: IAPService non disponibile/inizializzato per acquistare Pass Premium.");
            }
        }
    }
}
