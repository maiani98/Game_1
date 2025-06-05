using UnityEngine;
// using ChaosCosmos.Gameplay.Progression; // Se una ricompensa può essere un UpgradeData

namespace ChaosCosmos.Gameplay.PassSystem
{
    public enum RewardType { XPCurrency, SoftCurrency, HardCurrency, SpecificUpgrade, Skin }

    [CreateAssetMenu(fileName = "NewReward", menuName = "ChaosCosmos/Pass System/Reward Data")]
    public class RewardData : ScriptableObject
    {
        public string rewardID; // Es. "xp_100", "skin_planet_lava"
        public RewardType type = RewardType.XPCurrency;
        public int amount = 0; // Per valute o XP
        public Sprite icon;
        public string displayName = "New Reward";
        [TextArea] public string description = "Reward description.";

        [Header("Specific Reward Types")]
        public ChaosCosmos.Gameplay.Progression.UpgradeData upgradeToGrant; // Se type è SpecificUpgrade
        // public GameObject skinPrefabToUnlock; // Se type è Skin (mantenere commentato per ora)

        void OnValidate()
        {
            if (string.IsNullOrEmpty(rewardID))
            {
                rewardID = System.Guid.NewGuid().ToString();
            }
        }
    }
}
