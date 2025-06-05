using UnityEngine;
using System;
using System.Collections.Generic;

namespace ChaosCosmos.Services.RemoteConfig
{
    public class RemoteConfigService : IRemoteConfigService
    {
        public bool IsReady { get; private set; }

        private Dictionary<string, string> _simulatedValues = new Dictionary<string, string>();

        public RemoteConfigService()
        {
            _simulatedValues["upgrade_basespeed_lvl_1_xpCost"] = "120";
            _simulatedValues["upgrade_initialmass_lvl_1_xpCost"] = "65";
            _simulatedValues["arena_maxCollectibles"] = "25";
            _simulatedValues["welcomeMessage"] = "Benvenuto in Chaos Cosmos! (da Remote Config Simulato)";
            _simulatedValues["enableExperimentalFeatureX"] = "true";
            _simulatedValues["ab_test_initialXpBonus_amount"] = "50";
            _simulatedValues["ab_test_SpeedBoostEffect_duration_seconds"] = "7.0";
        }

        public void Initialize(Action<bool> onInitializedCallback)
        {
            Debug.Log("RemoteConfigService: Inizializzazione (simulata)...");
            IsReady = true;
            Debug.Log("RemoteConfigService: Valori recuperati e pronti (simulato).");
            onInitializedCallback?.Invoke(true);
        }

        public string GetString(string key, string defaultValue)
        {
            if (IsReady && _simulatedValues.TryGetValue(key, out string value))
            {
                // Debug.Log($"[RemoteConfig] GetString: Chiave '{key}', Valore restituito: '{value}' (da cache simulata)"); // Può essere verboso
                return value;
            }
            Debug.Log($"[RemoteConfig] GetString: Chiave '{key}' non trovata o servizio non pronto. Restituito default: '{defaultValue}'");
            return defaultValue;
        }

        public int GetInt(string key, int defaultValue)
        {
            if (IsReady && _simulatedValues.TryGetValue(key, out string valueStr))
            {
                if (int.TryParse(valueStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int result))
                {
                    // Debug.Log($"[RemoteConfig] GetInt: Chiave '{key}', Valore restituito: {result} (da cache simulata)"); // Può essere verboso
                    return result;
                }
                Debug.LogWarning($"[RemoteConfig] GetInt: Impossibile fare il parsing di '{valueStr}' per la chiave '{key}'. Restituito default: {defaultValue}");
            }
            else // Chiave non trovata o servizio non pronto
            {
                 Debug.Log($"[RemoteConfig] GetInt: Chiave '{key}' non trovata o servizio non pronto. Restituito default: {defaultValue}");
            }
            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue)
        {
            if (IsReady && _simulatedValues.TryGetValue(key, out string valueStr))
            {
                if (float.TryParse(valueStr, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out float result))
                {
                    // Debug.Log($"[RemoteConfig] GetFloat: Chiave '{key}', Valore restituito: {result} (da cache simulata)"); // Può essere verboso
                    return result;
                }
                Debug.LogWarning($"[RemoteConfig] GetFloat: Impossibile fare il parsing di '{valueStr}' per la chiave '{key}'. Restituito default: {defaultValue}");
            }
            else
            {
                Debug.Log($"[RemoteConfig] GetFloat: Chiave '{key}' non trovata o servizio non pronto. Restituito default: {defaultValue}");
            }
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            if (IsReady && _simulatedValues.TryGetValue(key, out string valueStr))
            {
                if (bool.TryParse(valueStr, out bool result))
                {
                    // Debug.Log($"[RemoteConfig] GetBool: Chiave '{key}', Valore restituito: {result} (da cache simulata)"); // Può essere verboso
                    return result;
                }
                Debug.LogWarning($"[RemoteConfig] GetBool: Impossibile fare il parsing di '{valueStr}' per la chiave '{key}'. Restituito default: {defaultValue}");
            }
            else
            {
                Debug.Log($"[RemoteConfig] GetBool: Chiave '{key}' non trovata o servizio non pronto. Restituito default: {defaultValue}");
            }
            return defaultValue;
        }
    }
}
