using ChaosCosmos.Core.Services;
using ChaosCosmos.Gameplay.Progression;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Gameplay.LiveOps;    // Aggiunto using per EventData
using System.Collections.Generic;

namespace ChaosCosmos.Services.Configuration
{
    public interface IConfigDataService : IService
    {
        void Initialize();
        bool IsInitialized { get; }

        IEnumerable<UpgradeData> GetAllUpgradeData();
        UpgradeData GetUpgradeData(string upgradeID);

        IEnumerable<PassSeasonData> GetAllPassSeasonData();
        PassSeasonData GetPassSeasonData(string seasonID);

        IEnumerable<EventData> GetAllEventData();
        EventData GetEventData(string eventID);
    }
}
