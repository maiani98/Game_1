using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Per OrderBy
using ChaosCosmos.Gameplay.Progression;
using ChaosCosmos.Services.Progression;
using ChaosCosmos.Services.Configuration;
using ChaosCosmos.Services.RemoteConfig;
using ChaosCosmos.Core.Services;

namespace ChaosCosmos.UI.Meta
{
    public class UpgradesPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI playerXPText;
        public TextMeshProUGUI playerLevelText;
        public RectTransform upgradesContentParent;
        public GameObject upgradeItemPrefab;

        private IProgressService _progressService;
        private IConfigDataService _configDataService;
        private IRemoteConfigService _remoteConfigService;

        private List<UpgradeUIItem> _uiItems = new List<UpgradeUIItem>();
        private bool _isInitialized = false;

        void Start()
        {
            InitializePanel();
        }

        void OnEnable()
        {
            if (_isInitialized)
            {
                RefreshAllUI();
            }
        }

        private void InitializePanel()
        {
            if (_isInitialized) return;

            bool gotProgress = TryGetService(out _progressService);
            bool gotConfig = TryGetService(out _configDataService);
            bool gotRemoteConfig = TryGetService(out _remoteConfigService);

            if (!gotProgress || !gotConfig)
            {
                Debug.LogError("UpgradesPanelUI: Impossibile ottenere tutti i servizi necessari (Progress o Config). Il pannello potrebbe non funzionare.");
                if (playerXPText) playerXPText.text = "Errore Servizi";
                if (playerLevelText) playerLevelText.text = "";
                // gameObject.SetActive(false); // Evita di disattivare, potrebbe nascondere errori importanti
                return;
            }

            if (upgradeItemPrefab == null || upgradesContentParent == null)
            {
                Debug.LogError("UpgradesPanelUI: Prefab dell'item upgrade o parent del contenuto non assegnati!");
                gameObject.SetActive(false);
                return;
            }

            PopulateUpgrades();
            RefreshPlayerData();
            _isInitialized = true;
        }

        private bool TryGetService<T>(out T service) where T : class, IService
        {
            if (ServiceLocator.IsRegistered<T>())
            {
                service = ServiceLocator.Get<T>();
                if (service != null)
                {
                    if (service is IConfigDataService config && !config.IsInitialized)
                    {
                        Debug.LogWarning($"Servizio {typeof(T).Name} recuperato ma non ancora inizializzato.");
                    }
                    if (service is IRemoteConfigService remote && !remote.IsReady)
                    {
                         Debug.LogWarning($"Servizio {typeof(T).Name} recuperato ma non ancora pronto.");
                    }
                    return true;
                }
            }
            service = null;
            Debug.LogError($"Servizio {typeof(T).Name} non registrato in ServiceLocator."); // Cambiato a Error per più visibilità
            return false;
        }

        public void RefreshAllUI()
        {
            if (!_isInitialized)
            {
                if (_progressService == null || _configDataService == null)
                {
                     Debug.LogWarning("UpgradesPanelUI.RefreshAllUI: Servizi non ancora pronti. Tentativo di inizializzazione.");
                     InitializePanel();
                     if (!_isInitialized) return;
                }
            }

            RefreshPlayerData();
            foreach (var item in _uiItems)
            {
                if (item != null) item.UpdateUI();
            }
        }

        private void RefreshPlayerData()
        {
            if (_progressService == null) return;

            if (playerXPText != null)
            {
                playerXPText.text = $"XP: {_progressService.CurrentXP}";
            }
            if (playerLevelText != null)
            {
                playerLevelText.text = $"Livello: {_progressService.CurrentPlayerLevel}";
            }
        }

        private void PopulateUpgrades()
        {
            foreach (Transform child in upgradesContentParent)
            {
                Destroy(child.gameObject);
            }
            _uiItems.Clear();

            if (_configDataService == null || !_configDataService.IsInitialized)
            {
                Debug.LogError("UpgradesPanelUI: ConfigDataService non pronto. Impossibile popolare gli upgrade.");
                return;
            }

            IEnumerable<UpgradeData> allUpgrades = _configDataService.GetAllUpgradeData();
            if (allUpgrades == null || !allUpgrades.Any())
            {
                Debug.LogWarning("UpgradesPanelUI: Nessun UpgradeData trovato in ConfigDataService.");
                return;
            }

            foreach (UpgradeData upgrade in allUpgrades.OrderBy(u => u.upgradeName))
            {
                GameObject itemGO = Instantiate(upgradeItemPrefab, upgradesContentParent);
                UpgradeUIItem uiItem = itemGO.GetComponent<UpgradeUIItem>();
                if (uiItem != null)
                {
                    uiItem.Setup(upgrade, _progressService, _remoteConfigService, OnAnUpgradeWasPurchased);
                    _uiItems.Add(uiItem);
                }
                else
                {
                    Debug.LogError($"UpgradesPanelUI: Prefab '{upgradeItemPrefab.name}' non ha il componente UpgradeUIItem!");
                    Destroy(itemGO);
                }
            }
        }

        private void OnAnUpgradeWasPurchased()
        {
            // TODO_SFX: UI_Upgrade_Purchased_Success
            Debug.Log("UpgradesPanelUI: Rilevato acquisto upgrade, aggiorno UI.");
            RefreshAllUI();
        }
    }
}
