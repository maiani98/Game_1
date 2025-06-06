using System.Collections.Generic;
#if UNITY_SERVICES
using Unity.Services.Analytics;
using Unity.Services.Core;
#endif
using UnityEngine;

namespace ChaosCosmos.Services.Analytics
{
    /// <summary>
    /// Analytics implementation using Unity Gaming Services Analytics.
    /// Requires the "Unity Services" packages and project configuration.
    /// </summary>
    public class UnityAnalyticsService : IAnalyticsService
    {
#if UNITY_SERVICES
        private bool _initialized;

        private async void EnsureInitialized()
        {
            if (_initialized)
                return;
            try
            {
                await UnityServices.InitializeAsync();
                AnalyticsService.Instance.StartDataCollection();
                _initialized = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"UnityAnalyticsService initialization failed: {ex.Message}");
            }
        }

        public void TrackEvent(string eventName)
        {
            EnsureInitialized();
            if (!_initialized) return;
            AnalyticsService.Instance.CustomData(eventName);
        }

        public void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            EnsureInitialized();
            if (!_initialized) return;
            AnalyticsService.Instance.CustomData(eventName, parameters);
        }
    }
#else
    /// <summary>
    /// Fallback implementation used when Unity Services Analytics is missing.
    /// </summary>
    public class UnityAnalyticsService : IAnalyticsService
    {
        public void TrackEvent(string eventName)
        {
            Debug.LogWarning("UnityAnalyticsService: Unity Services package not installed.");
        }

        public void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            Debug.LogWarning("UnityAnalyticsService: Unity Services package not installed.");
        }
    }
#endif
}
