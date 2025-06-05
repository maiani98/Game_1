using ChaosCosmos.Services.Configuration;
using ChaosCosmos.Gameplay.Progression;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Gameplay.LiveOps; // Per EventData
using System.Collections.Generic;
using System.Linq;

namespace ChaosCosmos.Tests.EditMode.Services.Progression.Mocks
{
    public class MockConfigDataService : IConfigDataService
    {
        public bool IsInitialized { get; set; } = true; // Default a true per i test

        private Dictionary<string, UpgradeData> _upgrades = new Dictionary<string, UpgradeData>();
        private Dictionary<string, PassSeasonData> _seasons = new Dictionary<string, PassSeasonData>();
        private Dictionary<string, EventData> _events = new Dictionary<string, EventData>();


        public void Initialize() { IsInitialized = true; }

        public void AddUpgrade(UpgradeData upgrade)
        {
            if (upgrade != null && !string.IsNullOrEmpty(upgrade.upgradeID))
            {
                _upgrades[upgrade.upgradeID] = upgrade;
            }
        }

        public IEnumerable<UpgradeData> GetAllUpgradeData() => _upgrades.Values;
        public UpgradeData GetUpgradeData(string upgradeID) => _upgrades.TryGetValue(upgradeID, out var data) ? data : null;

        // Metodi per PassSeasonData e EventData (non usati da ProgressManagerTests, ma per completezza dell'interfaccia)
        public void AddSeason(PassSeasonData season)
        {
            if (season != null && !string.IsNullOrEmpty(season.seasonID))
            {
                _seasons[season.seasonID] = season;
            }
        }
        public IEnumerable<PassSeasonData> GetAllPassSeasonData() => _seasons.Values;
        public PassSeasonData GetPassSeasonData(string seasonID) => _seasons.TryGetValue(seasonID, out var data) ? data : null;

        public void AddEvent(EventData gameEvent)
        {
            if (gameEvent != null && !string.IsNullOrEmpty(gameEvent.eventID))
            {
                _events[gameEvent.eventID] = gameEvent;
            }
        }
        public IEnumerable<EventData> GetAllEventData() => _events.Values;
        public EventData GetEventData(string eventID) => _events.TryGetValue(eventID, out var data) ? data : null;

        public void ClearData()
        {
            _upgrades.Clear();
            _seasons.Clear();
            _events.Clear();
        }
    }
}
