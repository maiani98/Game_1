using UnityEngine;
using System.Collections.Generic;

namespace ChaosCosmos.Gameplay.PassSystem
{
    [CreateAssetMenu(fileName = "NewPassSeason", menuName = "ChaosCosmos/Pass System/Pass Season Data")]
    public class PassSeasonData : ScriptableObject
    {
        public string seasonID = "Season1";
        public string seasonName = "Stagione 1";
        public List<PassTierData> tiers; // Lista ordinata dei tier
        // public System.DateTime startDate; // Per future implementazioni
        // public System.DateTime endDate;   // Per future implementazioni
    }
}
