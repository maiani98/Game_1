using UnityEngine;
using System.Collections.Generic;

namespace ChaosCosmos.Gameplay.PassSystem
{
    [CreateAssetMenu(fileName = "NewPassTier", menuName = "ChaosCosmos/Pass System/Pass Tier Data")]
    public class PassTierData : ScriptableObject
    {
        public int tierLevel = 1;
        public int xpToUnlockThisTier = 100; // XP necessari *per questo specifico tier*, non totali

        public List<RewardData> freeRewards;
        public List<RewardData> premiumRewards; // Richiede acquisto del pass premium
    }
}
