using UnityEngine;
using System.Collections.Generic;
using ChaosCosmos.Gameplay.Progression; // Per UpgradeData e StatType

namespace ChaosCosmos.Services.Progression
{
    public class ProgressManager : IProgressService
    {
        public int CurrentXP { get; private set; }
        public int CurrentPlayerLevel { get; private set; } // Semplice: 1 livello ogni X XP

        private const string XP_SAVE_KEY = "ChaosCosmos_PlayerXP";
        private const string LEVEL_SAVE_KEY = "ChaosCosmos_PlayerLevel";
        private const string UPGRADES_SAVE_KEY_PREFIX = "ChaosCosmos_UpgradeLevel_";

        // Dizionario per tracciare i livelli degli upgrade acquistati (ID -> livello)
        private Dictionary<string, int> purchasedUpgradeLevels = new Dictionary<string, int>();

        // Lista di tutti gli UpgradeData disponibili nel gioco (da popolare, es. da una risorsa)
        // Per ora, questo manager non sa quali sono tutti gli upgrade possibili,
        // ma solo quelli che gli vengono passati per l'acquisto.
        // In un sistema più avanzato, caricherebbe tutti gli UpgradeData da una cartella Resources.
        // private List<UpgradeData> allGameUpgrades; // Esempio

        public ProgressManager()
        {
            LoadProgress();
            // Se CurrentPlayerLevel è 0 dopo il Load (es. prima esecuzione o reset senza livello salvato a 1), impostalo a 1.
            if (CurrentPlayerLevel == 0)
            {
               CurrentPlayerLevel = 1;
            }
            Debug.Log($"ProgressManager: Inizializzato. XP: {CurrentXP}, Livello: {CurrentPlayerLevel}");
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            CurrentXP += amount;

            int newLevel = 1 + (CurrentXP / 1000); // Esempio: 1000 XP per livello dopo il primo
            if (newLevel > CurrentPlayerLevel)
            {
                CurrentPlayerLevel = newLevel;
                Debug.Log($"ProgressManager: Level Up! Nuovo Livello: {CurrentPlayerLevel}");
                // Qui si potrebbero triggerare eventi di level up
            }
            Debug.Log($"ProgressManager: XP Aggiunti: {amount}. XP Totali: {CurrentXP}");
            SaveProgress();
        }

        public bool CanAffordUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null) return false;
            // Aggiungi controllo sul livello giocatore richiesto
            if (CurrentPlayerLevel < upgrade.requiredPlayerLevel) return false;

            // Aggiungi controllo sui prerequisiti
            if (upgrade.prerequisites != null)
            {
                foreach (var prereq in upgrade.prerequisites)
                {
                    if (prereq == null) continue;
                    // Assumiamo che un prerequisito debba essere almeno a livello 1 (cioè acquistato una volta)
                    if (GetUpgradeLevel(prereq.upgradeID) < 1) return false;
                }
            }

            return CurrentXP >= upgrade.xpCost && GetUpgradeLevel(upgrade.upgradeID) < upgrade.maxLevel;
        }

        public bool PurchaseUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogWarning("ProgressManager: Tentativo di acquisto di un upgrade nullo.");
                return false;
            }
            if (!CanAffordUpgrade(upgrade))
            {
                Debug.LogWarning($"ProgressManager: Impossibile acquistare l'upgrade '{upgrade.upgradeName}'. Requisiti non soddisfatti o fondi/livello insufficienti. Costo: {upgrade.xpCost} XP, Posseduti: {CurrentXP} XP, Livello Giocatore Richiesto: {upgrade.requiredPlayerLevel}, Livello Giocatore Attuale: {CurrentPlayerLevel}, Livello Upgrade Acquistato: {GetUpgradeLevel(upgrade.upgradeID)}/{upgrade.maxLevel}");
                return false;
            }

            CurrentXP -= upgrade.xpCost;
            int currentLevel = GetUpgradeLevel(upgrade.upgradeID);
            purchasedUpgradeLevels[upgrade.upgradeID] = currentLevel + 1;

            Debug.Log($"ProgressManager: Upgrade '{upgrade.upgradeName}' acquistato (Nuovo Livello {currentLevel + 1}). XP rimanenti: {CurrentXP}");
            SaveProgress();
            return true;
        }

        public int GetUpgradeLevel(string upgradeID)
        {
            if (string.IsNullOrEmpty(upgradeID)) return 0;
            purchasedUpgradeLevels.TryGetValue(upgradeID, out int level);
            return level;
        }

        public float GetStatValue(StatType stat, float baseValue)
        {
            float modifiedValue = baseValue;
            // NOTA: Questa implementazione è un placeholder. Per funzionare correttamente,
            // ProgressManager dovrebbe avere accesso a una lista di tutti gli UpgradeData disponibili
            // (es. caricati da Resources/ScriptableObjects) per poterli ciclare e applicare.
            // Il codice commentato sotto è un esempio concettuale di come potrebbe funzionare.

            /*
            if (allGameUpgrades == null) {
                // Carica tutti gli UpgradeData da Resources/Settings/UpgradeData (esempio)
                // allGameUpgrades = new List<UpgradeData>(Resources.LoadAll<UpgradeData>("Settings/UpgradeData"));
                // Debug.Log($"Caricati {allGameUpgrades.Count} upgrade totali dal sistema.");
            }

            foreach (var purchasedEntry in purchasedUpgradeLevels)
            {
                string upgradeID = purchasedEntry.Key;
                int purchasedLevel = purchasedEntry.Value;

                // Trova l'UpgradeData corrispondente all'ID
                UpgradeData ud = allGameUpgrades?.Find(u => u.upgradeID == upgradeID);

                if (ud != null && ud.statToUpgrade == stat)
                {
                    for (int i = 0; i < purchasedLevel; i++) // Applica l'effetto per ogni livello acquistato
                    {
                        if (ud.isPercentageBased)
                        {
                            modifiedValue *= (1f + ud.upgradeValue);
                        }
                        else
                        {
                            modifiedValue += ud.upgradeValue;
                        }
                    }
                }
            }
            */
            // Se allGameUpgrades non è popolato o la logica sopra non è attiva:
            if (purchasedUpgradeLevels.Count > 0) // Solo per indicare che la logica non è completa
            {
                 Debug.LogWarning($"ProgressManager.GetStatValue per {stat} è un placeholder e restituisce baseValue. L'applicazione degli upgrade acquistati ({purchasedUpgradeLevels.Count} tipi) va implementata qui, iterando sugli UpgradeData e applicando i loro effetti.");
            }

            return modifiedValue;
        }

        public void SaveProgress()
        {
            PlayerPrefs.SetInt(XP_SAVE_KEY, CurrentXP);
            PlayerPrefs.SetInt(LEVEL_SAVE_KEY, CurrentPlayerLevel);
            foreach (var entry in purchasedUpgradeLevels)
            {
                PlayerPrefs.SetInt(UPGRADES_SAVE_KEY_PREFIX + entry.Key, entry.Value);
            }
            PlayerPrefs.Save();
            Debug.Log("ProgressManager: Progresso salvato in PlayerPrefs.");
        }

        public void LoadProgress()
        {
            CurrentXP = PlayerPrefs.GetInt(XP_SAVE_KEY, 0);
            CurrentPlayerLevel = PlayerPrefs.GetInt(LEVEL_SAVE_KEY, 1);

            purchasedUpgradeLevels.Clear();
            // Per caricare correttamente i livelli degli upgrade, è necessario conoscere tutti gli ID possibili.
            // Senza una lista di tutti gli UpgradeData (allGameUpgrades), non possiamo sapere quali chiavi cercare.
            // Se `allGameUpgrades` fosse popolato (es. da Resources), si potrebbe fare:
            /*
            if (allGameUpgrades != null)
            {
                foreach (UpgradeData ud in allGameUpgrades)
                {
                    if (!string.IsNullOrEmpty(ud.upgradeID))
                    {
                        int level = PlayerPrefs.GetInt(UPGRADES_SAVE_KEY_PREFIX + ud.upgradeID, 0);
                        if (level > 0)
                        {
                            purchasedUpgradeLevels[ud.upgradeID] = level;
                        }
                    }
                }
                Debug.Log($"ProgressManager.LoadProgress: Caricati {purchasedUpgradeLevels.Count} livelli di upgrade da PlayerPrefs.");
            }
            else
            {
                Debug.LogWarning("ProgressManager.LoadProgress: `allGameUpgrades` non è popolato. Impossibile caricare i livelli degli upgrade.");
            }
            */
             Debug.LogWarning("ProgressManager.LoadProgress: Caricamento livelli upgrade da PlayerPrefs non implementato completamente (richiede lista di tutti gli ID upgrade per iterare e caricare). I livelli degli upgrade verranno resettati a 0 finché non si popola `allGameUpgrades` e si completa la logica di caricamento.");
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(XP_SAVE_KEY);
            PlayerPrefs.DeleteKey(LEVEL_SAVE_KEY);

            // Anche qui, per cancellare tutti gli upgrade, servirebbe una lista di tutti gli ID
            // o un meccanismo per trovare tutte le chiavi con UPGRADES_SAVE_KEY_PREFIX.
            // PlayerPrefs non ha un "DeleteKeysWithPrefix", quindi è manuale.
            /*
            if (allGameUpgrades != null)
            {
                foreach (UpgradeData ud in allGameUpgrades)
                {
                    if (!string.IsNullOrEmpty(ud.upgradeID))
                    {
                        PlayerPrefs.DeleteKey(UPGRADES_SAVE_KEY_PREFIX + ud.upgradeID);
                    }
                }
            }
            */
            Debug.LogWarning("ProgressManager.ResetProgress: Reset dei livelli di upgrade da PlayerPrefs non implementato completamente (richiede lista di tutti gli ID upgrade per iterare e cancellare).");

            purchasedUpgradeLevels.Clear();
            CurrentXP = 0;
            CurrentPlayerLevel = 1;
            PlayerPrefs.Save();
            Debug.Log("ProgressManager: Progresso resettato. XP e Livello cancellati. Livelli upgrade in PlayerPrefs potrebbero persistere se non si itera su tutti gli ID per cancellarli.");
        }
    }
}
