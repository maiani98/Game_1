using UnityEngine;
using System.Collections.Generic;
// using System.Linq; // Rimosso perché non usato
using ChaosCosmos.Gameplay.Progression;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Gameplay.LiveOps;

namespace ChaosCosmos.Services.Configuration
{
    public class ConfigDataService : IConfigDataService
    {
        public bool IsInitialized { get; private set; }

        private Dictionary<string, UpgradeData> _upgradeDataCache;
        private Dictionary<string, PassSeasonData> _passSeasonDataCache;
        private Dictionary<string, EventData> _eventDataCache;

        private const string DefaultUpgradesPath = "GameData/Upgrades";
        private const string DefaultPassSeasonsPath = "GameData/PassSeasons";
        private const string DefaultEventsPath = "GameData/Events";

        public static string UpgradesPathOverride = null;
        public static string PassSeasonsPathOverride = null;
        public static string EventsPathOverride = null;

        private string CurrentUpgradesPath => UpgradesPathOverride ?? DefaultUpgradesPath;
        private string CurrentPassSeasonsPath => PassSeasonsPathOverride ?? DefaultPassSeasonsPath;
        private string CurrentEventsPath => EventsPathOverride ?? DefaultEventsPath;

        public ConfigDataService()
        {
            _upgradeDataCache = new Dictionary<string, UpgradeData>();
            _passSeasonDataCache = new Dictionary<string, PassSeasonData>();
            _eventDataCache = new Dictionary<string, EventData>();
        }

        public void Initialize()
        {
            if (IsInitialized)
            { // Aggiunte graffe
                return;
            }

            LoadAllUpgradeDataFromResources();
            LoadAllPassSeasonDataFromResources();
            LoadAllEventDataFromResources();

            IsInitialized = true;
            Debug.Log($"ConfigDataService: Inizializzato. Caricati {_upgradeDataCache.Count} upgrades, {_passSeasonDataCache.Count} stagioni, e {_eventDataCache.Count} eventi da Resources.");
        }

        private void LoadAllUpgradeDataFromResources()
        {
            UpgradeData[] loadedUpgrades = Resources.LoadAll<UpgradeData>(CurrentUpgradesPath);
            foreach (var upgrade in loadedUpgrades) // var appropriato
            {
                if (upgrade == null || string.IsNullOrEmpty(upgrade.upgradeID))
                {
                    Debug.LogWarning($"ConfigDataService: Trovato un UpgradeData nullo o con ID vuoto in '{CurrentUpgradesPath}'. Sarà ignorato.");
                    continue;
                }
                if (!_upgradeDataCache.ContainsKey(upgrade.upgradeID))
                {
                    _upgradeDataCache.Add(upgrade.upgradeID, upgrade);
                }
                else
                {
                    Debug.LogWarning($"ConfigDataService: Trovato UpgradeID duplicato '{upgrade.upgradeID}' in '{CurrentUpgradesPath}'. Il primo caricato ('{_upgradeDataCache[upgrade.upgradeID].name}') verrà mantenuto, l'istanza '{upgrade.name}' sarà ignorata.");
                }
            }
        }

        private void LoadAllPassSeasonDataFromResources()
        {
            PassSeasonData[] loadedSeasons = Resources.LoadAll<PassSeasonData>(CurrentPassSeasonsPath);
            foreach (var season in loadedSeasons) // var appropriato
            {
                 if (season == null || string.IsNullOrEmpty(season.seasonID))
                {
                    Debug.LogWarning($"ConfigDataService: Trovato un PassSeasonData nullo o con ID vuoto in '{CurrentPassSeasonsPath}'. Sarà ignorato.");
                    continue;
                }
                if (!_passSeasonDataCache.ContainsKey(season.seasonID))
                {
                    _passSeasonDataCache.Add(season.seasonID, season);
                }
                else
                {
                    Debug.LogWarning($"ConfigDataService: Trovato SeasonID duplicato '{season.seasonID}' in '{CurrentPassSeasonsPath}'. Il primo caricato ('{_passSeasonDataCache[season.seasonID].name}') verrà mantenuto, l'istanza '{season.name}' sarà ignorata.");
                }
            }
        }

        private void LoadAllEventDataFromResources()
        {
            EventData[] loadedEvents = Resources.LoadAll<EventData>(CurrentEventsPath);
            foreach (var gameEvent in loadedEvents) // var appropriato
            {
                if (gameEvent == null || string.IsNullOrEmpty(gameEvent.eventID))
                {
                    Debug.LogWarning($"ConfigDataService: Trovato un EventData nullo o con ID vuoto in '{CurrentEventsPath}'. Sarà ignorato.");
                    continue;
                }
                if (!_eventDataCache.ContainsKey(gameEvent.eventID))
                {
                    _eventDataCache.Add(gameEvent.eventID, gameEvent);
                }
                else
                {
                    Debug.LogWarning($"ConfigDataService: Trovato EventID duplicato '{gameEvent.eventID}' in '{CurrentEventsPath}'. Il primo caricato ('{_eventDataCache[gameEvent.eventID].name}') verrà mantenuto, l'istanza '{gameEvent.name}' sarà ignorata.");
                }
            }
        }

        public IEnumerable<UpgradeData> GetAllUpgradeData() => _upgradeDataCache.Values;
        public UpgradeData GetUpgradeData(string upgradeID)
        {
            if (string.IsNullOrEmpty(upgradeID))
            { // Aggiunte graffe
                return null;
            }
            _upgradeDataCache.TryGetValue(upgradeID, out UpgradeData data);
            return data;
        }

        public IEnumerable<PassSeasonData> GetAllPassSeasonData() => _passSeasonDataCache.Values;
        public PassSeasonData GetPassSeasonData(string seasonID)
        {
            if (string.IsNullOrEmpty(seasonID))
            { // Aggiunte graffe
                return null;
            }
            _passSeasonDataCache.TryGetValue(seasonID, out PassSeasonData data);
            return data;
        }

        public IEnumerable<EventData> GetAllEventData() => _eventDataCache.Values;
        public EventData GetEventData(string eventID)
        {
            if (string.IsNullOrEmpty(eventID))
            { // Aggiunte graffe
                return null;
            }
            _eventDataCache.TryGetValue(eventID, out EventData data);
            return data;
        }
    }
}
