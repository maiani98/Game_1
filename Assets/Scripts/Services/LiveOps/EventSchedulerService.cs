using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ChaosCosmos.Gameplay.LiveOps;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.RemoteConfig;

namespace ChaosCosmos.Services.LiveOps
{
    public class EventSchedulerService : IEventSchedulerService
    {
        public bool IsInitialized { get; private set; }
        private Dictionary<string, EventData> _eventDefinitions = new Dictionary<string, EventData>();
        private IRemoteConfigService _remoteConfigService;

        public EventSchedulerService(IRemoteConfigService remoteConfigService)
        {
            _remoteConfigService = remoteConfigService;
            if (_remoteConfigService == null)
            {
                 Debug.LogWarning("EventSchedulerService: IRemoteConfigService non fornito durante la costruzione. L'attivazione degli eventi dipendente da Remote Config non funzionerà.");
            }
        }

        public void Initialize(IEnumerable<EventData> allEventDefinitions)
        {
            if (allEventDefinitions == null)
            {
                Debug.LogError("EventSchedulerService: Tentativo di inizializzare con una lista nulla di definizioni di eventi.");
                _eventDefinitions = new Dictionary<string, EventData>();
                IsInitialized = true; // Inizializzato, ma vuoto
                return;
            }

            _eventDefinitions = allEventDefinitions
                .Where(e => e != null && !string.IsNullOrEmpty(e.eventID))
                .GroupBy(e => e.eventID)
                .ToDictionary(g => g.Key, g => {
                    if (g.Count() > 1)
                    { // Added braces
                        Debug.LogWarning($"EventSchedulerService: Definizione evento duplicata per ID '{g.Key}'. Verrà usata la prima trovata.");
                    }
                    return g.First();
                });

            IsInitialized = true;
            Debug.Log($"EventSchedulerService: Inizializzato con {_eventDefinitions.Count} definizioni di eventi.");
        }

        public bool IsEventActive(string eventID)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("EventSchedulerService: IsEventActive chiamato prima dell'inizializzazione.");
                return false;
            }
            if (_remoteConfigService == null || !_remoteConfigService.IsReady)
            {
                Debug.LogWarning($"EventSchedulerService: IRemoteConfigService non disponibile o non pronto per controllare l'evento '{eventID}'. L'evento sarà considerato non attivo.");
                return false;
            }
            if (!_eventDefinitions.TryGetValue(eventID, out EventData eventData) || eventData == null || string.IsNullOrEmpty(eventData.remoteConfigActivationKey))
            {
                return false;
            }
            return _remoteConfigService.GetBool(eventData.remoteConfigActivationKey, false);
        }

        public T GetEventParameter<T>(string eventID, System.Func<EventData, T> valueSelector, T defaultValue)
        {
            if (IsEventActive(eventID) && _eventDefinitions.TryGetValue(eventID, out EventData eventData) && eventData != null)
            {
                try
                {
                    return valueSelector(eventData);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"EventSchedulerService: Eccezione durante la selezione del parametro per l'evento '{eventID}': {ex.Message}");
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }
}
