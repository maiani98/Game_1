using UnityEngine;
using System.Collections.Generic;
using ChaosCosmos.Gameplay.Progression;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.RemoteConfig;
using ChaosCosmos.Services.Configuration;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Services.Analytics;
using System;

namespace ChaosCosmos.Services.Progression
{
    public class ProgressManager : IProgressService
    {
        public int CurrentXP { get; private set; }
        public int CurrentPlayerLevel { get; private set; }

        private Dictionary<string, int> _purchasedUpgradeLevels = new Dictionary<string, int>(); // Prefixed

        private readonly IConfigDataService _configDataService;
        private readonly IRemoteConfigService _remoteConfigService;

        public ProgressManager(IConfigDataService configDataService, IRemoteConfigService remoteConfigService)
        {
            _configDataService = configDataService ?? throw new ArgumentNullException(nameof(configDataService));
            _remoteConfigService = remoteConfigService ?? throw new ArgumentNullException(nameof(remoteConfigService));

            if (!_configDataService.IsInitialized)
            {
                Debug.LogWarning("ProgressManager: IConfigDataService non è inizializzato al momento della costruzione di ProgressManager.");
            }
            if (!_remoteConfigService.IsReady)
            {
                Debug.LogWarning("ProgressManager: IRemoteConfigService non è pronto al momento della costruzione di ProgressManager.");
            }

            LoadProgress();
            if (CurrentPlayerLevel == 0)
            {
               CurrentPlayerLevel = 1;
            }
            Debug.Log($"ProgressManager: Costruito. XP: {CurrentXP}, Livello: {CurrentPlayerLevel}. Purchased Upgrades: {_purchasedUpgradeLevels.Count}");
        }

        public void InitializeNewPlayerIfApplicable()
        {
            if (CurrentXP == 0 && _purchasedUpgradeLevels.Count == 0 && CurrentPlayerLevel == 1)
            {
                if (_remoteConfigService != null && _remoteConfigService.IsReady)
                {
                    int initialXpBonus = _remoteConfigService.GetInt(RemoteConfigKeyPatterns.GetInitialXpBonusKey(), 0);
                    if (initialXpBonus > 0)
                    {
                        AddXP(initialXpBonus);
                        Debug.Log($"ProgressManager: Concesso bonus XP iniziale A/B Test: {initialXpBonus}");

                        if (ServiceLocator.IsRegistered<IAnalyticsService>())
                        {
                            var analytics = ServiceLocator.Get<IAnalyticsService>(); // var è ok qui
                            analytics.TrackEvent("ABTest_UserSegmentAssigned", new Dictionary<string, object> {
                                {"experiment_name", "InitialXpBonus"},
                                {"variant_name", $"Bonus_{initialXpBonus}"}
                            });
                        }
                    }
                } else {
                     Debug.LogWarning("ProgressManager: RemoteConfigService non pronto per bonus XP iniziale al momento di InitializeNewPlayerIfApplicable.");
                }
            }
        }

        private int GetXpCostForUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null || string.IsNullOrEmpty(upgrade.upgradeID))
            { // Added braces
                return int.MaxValue;
            }
            if (_remoteConfigService != null && _remoteConfigService.IsReady)
            {
                return _remoteConfigService.GetInt(RemoteConfigKeyPatterns.GetUpgradeXpCostKey(upgrade.upgradeID), upgrade.xpCost);
            }
            return upgrade.xpCost;
        }

        public void AddXP(int amount)
        {
            if (amount <= 0)
            { // Added braces
                return;
            }
            CurrentXP += amount;
            int newLevel = 1 + (CurrentXP / 1000);
            if (newLevel > CurrentPlayerLevel)
            {
                CurrentPlayerLevel = newLevel;
                Debug.Log($"ProgressManager: Level Up! Nuovo Livello: {CurrentPlayerLevel}");
            }
            SaveProgress();
        }

        public bool CanAffordUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
            { // Added braces
                return false;
            }
            int actualCost = GetXpCostForUpgrade(upgrade);
            if (CurrentPlayerLevel < upgrade.requiredPlayerLevel)
            { // Added braces
                return false;
            }
            if (upgrade.prerequisites != null)
            {
                foreach (var prereq in upgrade.prerequisites) // var è ok qui
                {
                    if (prereq == null)
                    { // Added braces
                        continue;
                    }
                    if (GetUpgradeLevel(prereq.upgradeID) < 1)
                    { // Added braces
                        return false;
                    }
                }
            }
            return CurrentXP >= actualCost && GetUpgradeLevel(upgrade.upgradeID) < upgrade.maxLevel;
        }

        public bool PurchaseUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null || string.IsNullOrEmpty(upgrade.upgradeID))
            {
                Debug.LogWarning("ProgressManager: Tentativo di acquisto di un upgrade nullo o con ID non valido.");
                return false;
            }
            int actualCost = GetXpCostForUpgrade(upgrade);

            if (CurrentXP < actualCost) { Debug.LogWarning($"ProgressManager: XP insufficienti per '{upgrade.upgradeName}'. Richiesti: {actualCost}, Posseduti: {CurrentXP}"); return false; }
            if (CurrentPlayerLevel < upgrade.requiredPlayerLevel) { Debug.LogWarning($"ProgressManager: Livello giocatore insuff. per '{upgrade.upgradeName}'. Richiesto: {upgrade.requiredPlayerLevel}, Attuale: {CurrentPlayerLevel}"); return false; }
            if (GetUpgradeLevel(upgrade.upgradeID) >= upgrade.maxLevel) { Debug.LogWarning($"ProgressManager: Upgrade '{upgrade.upgradeName}' già al livello massimo."); return false; }
            if (upgrade.prerequisites != null)
            {
                foreach (var prereq in upgrade.prerequisites) // var è ok qui
                {
                    if (prereq == null)
                    { // Added braces
                        continue;
                    }
                    if (GetUpgradeLevel(prereq.upgradeID) < 1)
                    {
                         Debug.LogWarning($"ProgressManager: Prerequisito '{prereq.upgradeName}' per '{upgrade.upgradeName}' non soddisfatto.");
                        return false;
                    }
                }
            }

            CurrentXP -= actualCost;
            int currentLevel = GetUpgradeLevel(upgrade.upgradeID);
            int newLevel = currentLevel + 1;
            _purchasedUpgradeLevels[upgrade.upgradeID] = newLevel; // Used prefixed
            Debug.Log($"ProgressManager: Upgrade '{upgrade.upgradeName}' acquistato (Nuovo Livello {newLevel}). Costo: {actualCost}. XP rimanenti: {CurrentXP}");
            SaveProgress();
            return true;
        }

        public int GetUpgradeLevel(string upgradeID)
        {
            if (string.IsNullOrEmpty(upgradeID))
            { // Added braces
                return 0;
            }
            _purchasedUpgradeLevels.TryGetValue(upgradeID, out int level); // Used prefixed
            return level;
        }

        public float GetStatValue(StatType stat, float baseValue)
        {
            float modifiedValue = baseValue;
            if (_configDataService == null || !_configDataService.IsInitialized)
            {
                Debug.LogWarning($"ProgressManager.GetStatValue: IConfigDataService non disponibile o non inizializzato per {stat}. Restituito baseValue.");
                return baseValue;
            }

            foreach (var purchasedEntry in _purchasedUpgradeLevels) // var è ok qui, Used prefixed
            {
                UpgradeData ud = _configDataService.GetUpgradeData(purchasedEntry.Key);
                if (ud != null && ud.statToUpgrade == stat)
                {
                    for (int i = 0; i < purchasedEntry.Value; i++)
                    {
                        if (ud.isPercentageBased) { modifiedValue *= (1f + ud.upgradeValue); }
                        else { modifiedValue += ud.upgradeValue; }
                    }
                }
            }
            return modifiedValue;
        }

        public bool GrantFreeUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null || string.IsNullOrEmpty(upgrade.upgradeID))
            {
                Debug.LogWarning("ProgressManager: Tentativo di concedere un UpgradeData nullo o con ID non valido.");
                return false;
            }
            int currentLevel = GetUpgradeLevel(upgrade.upgradeID);
            if (currentLevel >= upgrade.maxLevel)
            {
                Debug.LogWarning($"ProgressManager: Impossibile concedere l'upgrade gratuito '{upgrade.upgradeName}'. Già al livello massimo {currentLevel}/{upgrade.maxLevel}.");
                return false;
            }
            _purchasedUpgradeLevels[upgrade.upgradeID] = currentLevel + 1; // Used prefixed
            Debug.Log($"ProgressManager: Upgrade '{upgrade.upgradeName}' concesso gratuitamente (Nuovo Livello {currentLevel + 1}).");
            SaveProgress();
            return true;
        }

        public void SaveProgress()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.XP_SAVE_KEY, CurrentXP);
            PlayerPrefs.SetInt(PlayerPrefsKeys.PLAYER_LEVEL_SAVE_KEY, CurrentPlayerLevel);
            foreach (var entry in _purchasedUpgradeLevels) // var è ok qui, Used prefixed
            {
                PlayerPrefs.SetInt(PlayerPrefsKeys.UPGRADE_LEVEL_PREFIX + entry.Key, entry.Value);
            }
            PlayerPrefs.Save();
        }

        public void LoadProgress()
        {
            CurrentXP = PlayerPrefs.GetInt(PlayerPrefsKeys.XP_SAVE_KEY, 0);
            CurrentPlayerLevel = PlayerPrefs.GetInt(PlayerPrefsKeys.PLAYER_LEVEL_SAVE_KEY, 1);
            _purchasedUpgradeLevels.Clear(); // Used prefixed

            if (_configDataService != null && _configDataService.IsInitialized)
            {
                foreach (UpgradeData upgrade in _configDataService.GetAllUpgradeData())
                {
                    if (upgrade == null || string.IsNullOrEmpty(upgrade.upgradeID))
                    { // Added braces
                        continue;
                    }
                    int level = PlayerPrefs.GetInt(PlayerPrefsKeys.UPGRADE_LEVEL_PREFIX + upgrade.upgradeID, 0);
                    if (level > 0)
                    {
                        _purchasedUpgradeLevels[upgrade.upgradeID] = level; // Used prefixed
                    }
                }
                Debug.Log($"ProgressManager.LoadProgress: Caricati {_purchasedUpgradeLevels.Count} livelli di upgrade da PlayerPrefs."); // Used prefixed
            }
            else
            {
                Debug.LogWarning("ProgressManager.LoadProgress: IConfigDataService non disponibile/inizializzato. I livelli degli upgrade non saranno caricati.");
            }
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.XP_SAVE_KEY);
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.PLAYER_LEVEL_SAVE_KEY);

            if (_configDataService != null && _configDataService.IsInitialized)
            {
                foreach (UpgradeData upgrade in _configDataService.GetAllUpgradeData())
                {
                    if (upgrade == null || string.IsNullOrEmpty(upgrade.upgradeID))
                    { // Added braces
                        continue;
                    }
                    PlayerPrefs.DeleteKey(PlayerPrefsKeys.UPGRADE_LEVEL_PREFIX + upgrade.upgradeID);
                }
                 Debug.Log("ProgressManager.ResetProgress: Cancellate chiavi PlayerPrefs per upgrade conosciuti.");
            }
            else
            {
                Debug.LogWarning("ProgressManager.ResetProgress: IConfigDataService non disponibile/inizializzato. Chiavi upgrade potrebbero non essere state cancellate.");
            }

            _purchasedUpgradeLevels.Clear(); // Used prefixed
            CurrentXP = 0;
            CurrentPlayerLevel = 1;
            PlayerPrefs.Save();
            Debug.Log("ProgressManager: Progresso resettato.");
        }
    }
}
