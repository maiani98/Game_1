using ChaosCosmos.Services.Progression;
using ChaosCosmos.Gameplay.Progression; // Per UpgradeData, StatType
using System.Collections.Generic; // Per List in GetStatValue (anche se non usata attivamente)

namespace ChaosCosmos.Tests.EditMode.Services.PassSystem.Mocks
{
    public class MockProgressService : IProgressService
    {
        public int CurrentXP { get; set; }
        public int CurrentPlayerLevel { get; set; }

        public int XPGained { get; private set; }
        public UpgradeData LastGrantedUpgrade { get; private set; }
        public bool GrantUpgradeSuccess = true; // Per simulare fallimenti

        public MockProgressService()
        {
            // Inizializza con valori di default se necessario per i test
            CurrentPlayerLevel = 1;
        }

        public void AddXP(int amount) { XPGained += amount; CurrentXP += amount; } // Aggiorna anche CurrentXP per coerenza
        public bool CanAffordUpgrade(UpgradeData upgrade) => true; // Non rilevante per i test di PassManager
        public bool PurchaseUpgrade(UpgradeData upgrade) => true; // Non rilevante

        public bool GrantFreeUpgrade(UpgradeData upgrade)
        {
            if (GrantUpgradeSuccess) LastGrantedUpgrade = upgrade;
            return GrantUpgradeSuccess;
        }

        public int GetUpgradeLevel(string upgradeID) => 0; // Non rilevante
        public float GetStatValue(StatType stat, float baseValue) => baseValue; // Non rilevante

        public void SaveProgress() { /* No op */ }
        public void LoadProgress() { /* No op */ }
        public void ResetProgress() { XPGained = 0; LastGrantedUpgrade = null; CurrentXP = 0; CurrentPlayerLevel = 1; }

        // Metodi helper per i test
        public void ResetTestState() { XPGained = 0; LastGrantedUpgrade = null; GrantUpgradeSuccess = true; }
    }
}
