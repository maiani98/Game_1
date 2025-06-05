using ChaosCosmos.Core.Services;
using ChaosCosmos.Gameplay.LiveOps; // Per EventData
using System.Collections.Generic;

namespace ChaosCosmos.Services.LiveOps
{
    public interface IEventSchedulerService : IService
    {
        void Initialize(IEnumerable<EventData> allEventDefinitions);
        bool IsInitialized { get; }
        bool IsEventActive(string eventID);
        T GetEventParameter<T>(string eventID, System.Func<EventData, T> valueSelector, T defaultValue);
        // Esempio: GetEventParameter("event_id", ev => ev.effectValue, 1f);
    }
}
