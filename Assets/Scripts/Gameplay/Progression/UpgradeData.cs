using UnityEngine;
using System.Collections.Generic; // Per prerequisites

namespace ChaosCosmos.Gameplay.Progression
{
    // Enum per identificare le statistiche modificabili
    public enum StatType
    {
        BaseSpeed,
        InitialMass,
        MassToRadiusMultiplier, // Esempio di un moltiplicatore sulla curva di crescita
        SpeedBoostDuration,
        PowerUpSpawnRate // Esempio di statistica globale
    }

    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "ChaosCosmos/Progression/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Info")]
        public string upgradeID; // ID univoco, es. "base_speed_lvl_1"
        public string upgradeName = "New Upgrade";
        [TextArea] public string description = "Description of the upgrade.";
        public Sprite icon;

        [Header("Progression")]
        public int requiredPlayerLevel = 0; // Livello giocatore richiesto per sbloccare questo upgrade nell'ipotetico shop
        public List<UpgradeData> prerequisites; // Altri upgrade che devono essere acquistati prima

        [Header("Cost")]
        public int xpCost = 100;
        // public int premiumCurrencyCost = 0; // Per futura monetizzazione

        [Header("Effect")]
        public StatType statToUpgrade;
        public float upgradeValue; // Valore assoluto o moltiplicatore a seconda della stat
        public bool isPercentageBased = false; // Se true, upgradeValue è 0.1 per +10%
        public int maxLevel = 1; // Se l'upgrade può essere acquistato più volte

        // Helper per generare un ID se non impostato
        void OnValidate()
        {
            if (string.IsNullOrEmpty(upgradeID))
            {
                // Genera un ID basato sul nome dell'asset file per renderlo più leggibile, se possibile
                // Ma per robustezza, un GUID è meglio se il nome non è garantito unico o stabile.
                // Per semplicità qui usiamo il nome dell'asset + un GUID per evitare conflitti se si rinomina.
                // upgradeID = name + "_" + System.Guid.NewGuid().ToString();
                // Oppure solo GUID per totale unicità
                upgradeID = System.Guid.NewGuid().ToString();
            }
        }
    }
}
