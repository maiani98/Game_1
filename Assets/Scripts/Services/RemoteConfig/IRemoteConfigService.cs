using System;
using ChaosCosmos.Core.Services;

namespace ChaosCosmos.Services.RemoteConfig
{
    public interface IRemoteConfigService : IService
    {
        bool IsReady { get; }
        void Initialize(Action<bool> onInitializedCallback); // true se inizializzato con successo

        string GetString(string key, string defaultValue);
        int GetInt(string key, int defaultValue);
        float GetFloat(string key, float defaultValue);
        bool GetBool(string key, bool defaultValue);

        // Opzionale: Forzare un fetch dei valori (non implementato nella simulazione)
        // void FetchValues(Action<bool> onFetchCompletedCallback);
    }
}
