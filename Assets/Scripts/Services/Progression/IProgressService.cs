using ChaosCosmos.Core.Services;
using ChaosCosmos.Gameplay.Progression; // Per UpgradeData e StatType
using System.Collections.Generic;

namespace ChaosCosmos.Services.Progression
{
    public interface IProgressService : IService
    {
        int CurrentXP { get; }
        int CurrentPlayerLevel { get; } // Implementazione base, potrebbe essere solo basato su XP

        void AddXP(int amount);
        bool CanAffordUpgrade(UpgradeData upgrade);
        bool PurchaseUpgrade(UpgradeData upgrade);
        int GetUpgradeLevel(string upgradeID); // Livello attuale di un upgrade specifico
        float GetStatValue(StatType stat, float baseValue); // Ottiene il valore di una stat considerando gli upgrade

        void SaveProgress();
        void LoadProgress();
        void ResetProgress(); // Utile per testing
    }
}
