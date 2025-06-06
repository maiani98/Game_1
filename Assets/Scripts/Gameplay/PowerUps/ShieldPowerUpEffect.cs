using UnityEngine;
using System.Collections;
using ChaosCosmos.Services.RemoteConfig; // Necessario per IRemoteConfigService
using ChaosCosmos.Core.Services;      // Per ServiceLocator (se usato per Analytics)
using ChaosCosmos.Services.Analytics;  // Per IAnalyticsService (se usato per Analytics)
using System.Collections.Generic;    // Per Dictionary (se usato per Analytics)
using ChaosCosmos.Core.Constants;    // Per RemoteConfigKeyPatterns (se si standardizzano le chiavi)

namespace ChaosCosmos.Gameplay.PowerUps
{
    [CreateAssetMenu(fileName = "NewShieldEffect", menuName = "ChaosCosmos/PowerUp Effects/Shield")]
    public class ShieldPowerUpEffect : PowerUpEffect
    {
        // duration è ereditato da PowerUpEffect

        public override void Apply(PlanetController targetPlanet, IRemoteConfigService rcService)
        {
            string remoteConfigKey = $"ab_test_{this.name.Replace(" ", "")}_duration_seconds";
            // Oppure, se si vuole una chiave più stabile, usare effectName o un ID specifico
            // string remoteConfigKey = RemoteConfigKeyPatterns.GetPowerUpDurationKey(this.effectName); // Se effectName è un ID univoco

            float actualDuration = GetConfiguredDuration(rcService, remoteConfigKey, this.duration);

            Debug.Log($"[ShieldEffect] Applicato '{effectName}' a {targetPlanet.gameObject.name} per {actualDuration}s.");
            targetPlanet.StartCoroutine(ShieldCoroutine(targetPlanet, actualDuration));

            // Analytics per A/B test (opzionale, se la durata è stata effettivamente remotizzata)
            if (rcService != null && rcService.IsReady && rcService.GetString(remoteConfigKey, null) != null)
            {
                if (ServiceLocator.IsRegistered<IAnalyticsService>())
                {
                    var analytics = ServiceLocator.Get<IAnalyticsService>();
                    analytics.TrackEvent("ABTest_PowerUpParameter", new Dictionary<string, object> {
                        {"power_up_name", effectName},
                        {"parameter_name", "duration"},
                        {"parameter_value", actualDuration},
                        {"source", "remote_config"}
                    });
                }
            }
        }

        private IEnumerator ShieldCoroutine(PlanetController targetPlanet, float effectDuration)
        {
            // SFX/VFX di attivazione sono già in PlanetController.SetShieldActive(true)
            targetPlanet.SetShieldActive(true);
            yield return new WaitForSeconds(effectDuration);
            // SFX/VFX di disattivazione sono già in PlanetController.SetShieldActive(false)
            targetPlanet.SetShieldActive(false);
            Debug.Log($"[ShieldEffect] Effetto '{effectName}' (durata coroutine) terminato per {targetPlanet.gameObject.name}");
        }

        // Metodo helper per ottenere la durata, potrebbe essere spostato in una classe base se comune a molti effetti
        private float GetConfiguredDuration(IRemoteConfigService rcService, string key, float defaultDuration)
        {
            if (rcService != null && rcService.IsReady)
            {
                return rcService.GetFloat(key, defaultDuration);
            }
            return defaultDuration;
        }
    }
}
