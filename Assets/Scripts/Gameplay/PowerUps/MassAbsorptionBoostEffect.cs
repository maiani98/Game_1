using UnityEngine;
using System.Collections;
using ChaosCosmos.Services.RemoteConfig;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.Analytics;
using System.Collections.Generic;
using ChaosCosmos.Core.Constants;

namespace ChaosCosmos.Gameplay.PowerUps
{
    [CreateAssetMenu(fileName = "NewMassAbsorptionBoostEffect", menuName = "ChaosCosmos/PowerUp Effects/Mass Absorption Boost")]
    public class MassAbsorptionBoostEffect : PowerUpEffect
    {
        [Tooltip("Moltiplicatore aggiuntivo. Es: 0.5 per +50% massa assorbita (massa * (1 + 0.5)).")]
        public float massMultiplierBonus = 0.5f;

        public override void Apply(PlanetController targetPlanet, IRemoteConfigService rcService)
        {
            string durationKey = $"ab_test_{this.name.Replace(" ", "")}_duration_seconds";
            string bonusKey = $"ab_test_{this.name.Replace(" ", "")}_bonus_value"; // Non _multiplier, ma il valore del bonus stesso

            float actualDuration = GetConfiguredFloat(rcService, durationKey, this.duration);
            float actualBonus = GetConfiguredFloat(rcService, bonusKey, this.massMultiplierBonus);

            Debug.Log($"[MassAbsorptionBoost] Applicato '{effectName}' a {targetPlanet.gameObject.name} per {actualDuration}s. Bonus assorbimento: +{actualBonus*100}%.");
            targetPlanet.StartCoroutine(AbsorptionBoostCoroutine(targetPlanet, actualBonus, actualDuration));

            // Analytics per A/B test
            if (rcService != null && rcService.IsReady)
            {
                bool durationRemotelySet = rcService.GetString(durationKey, null) != null;
                bool bonusRemotelySet = rcService.GetString(bonusKey, null) != null;

                if (durationRemotelySet || bonusRemotelySet)
                {
                    if (ServiceLocator.IsRegistered<IAnalyticsService>())
                    {
                        var analytics = ServiceLocator.Get<IAnalyticsService>();
                        var parameters = new Dictionary<string, object> {
                            {"power_up_name", effectName},
                            {"source", "remote_config"}
                        };
                        if (durationRemotelySet) parameters.Add("duration_value", actualDuration);
                        if (bonusRemotelySet) parameters.Add("bonus_value", actualBonus);

                        analytics.TrackEvent("ABTest_PowerUpParameter", parameters);
                    }
                }
            }
        }

        private IEnumerator AbsorptionBoostCoroutine(PlanetController targetPlanet, float bonus, float effectDuration)
        {
            // TODO_SFX: MassBoost_Activated_Loop (start)
            // TODO_VFX: Player_MassBoost_Aura_On (attivare aura/effetto visivo sul player)
            targetPlanet.SetMassAbsorptionMultiplier(1.0f + bonus);
            yield return new WaitForSeconds(effectDuration);
            // TODO_SFX: MassBoost_Deactivated_Loop (stop)
            // TODO_VFX: Player_MassBoost_Aura_Off (disattivare aura)
            targetPlanet.SetMassAbsorptionMultiplier(1.0f);
            Debug.Log($"[MassAbsorptionBoost] Effetto '{effectName}' terminato per {targetPlanet.gameObject.name}");
        }

        // Metodo helper generico per float
        private float GetConfiguredFloat(IRemoteConfigService rcService, string key, float defaultValue)
        {
            if (rcService != null && rcService.IsReady)
            {
                return rcService.GetFloat(key, defaultValue);
            }
            return defaultValue;
        }
    }
}
