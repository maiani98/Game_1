using System.Collections.Generic;
using ChaosCosmos.Core.Services; // Per IService

namespace ChaosCosmos.Services.Analytics
{
    public interface IAnalyticsService : IService
    {
        void TrackEvent(string eventName);
        void TrackEvent(string eventName, Dictionary<string, object> parameters);
    }
}
