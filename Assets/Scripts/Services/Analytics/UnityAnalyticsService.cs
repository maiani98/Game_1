using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace ChaosCosmos.Services.Analytics
{
    /// <summary>
    /// Analytics implementation using Unity Gaming Services Analytics.
    /// Requires the "Unity Services" packages and project configuration.
    /// </summary>
    public class UnityAnalyticsService : IAnalyticsService
    {
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
}
