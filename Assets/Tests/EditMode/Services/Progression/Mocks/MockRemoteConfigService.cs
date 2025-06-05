using ChaosCosmos.Services.RemoteConfig;
using System;
using System.Collections.Generic;

namespace ChaosCosmos.Tests.EditMode.Services.Progression.Mocks
{
    public class MockRemoteConfigService : IRemoteConfigService
    {
        public bool IsReady { get; set; } = true; // Default a true per i test
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        public void Initialize(Action<bool> onInitializedCallback)
        {
            IsReady = true;
            onInitializedCallback?.Invoke(true);
        }

        public void SetValue(string key, string value) => _values[key] = value;
        public void SetValue(string key, int value) => _values[key] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public void SetValue(string key, float value) => _values[key] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public void SetValue(string key, bool value) => _values[key] = value.ToString().ToLowerInvariant();


        public string GetString(string key, string defaultValue) => _values.TryGetValue(key, out var val) ? val : defaultValue;

        public int GetInt(string key, int defaultValue)
        {
            if (_values.TryGetValue(key, out var valStr) && int.TryParse(valStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue)
        {
             if (_values.TryGetValue(key, out var valStr) && float.TryParse(valStr, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            if (_values.TryGetValue(key, out var valStr) && bool.TryParse(valStr, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        public void ClearValues() => _values.Clear();
    }
}
