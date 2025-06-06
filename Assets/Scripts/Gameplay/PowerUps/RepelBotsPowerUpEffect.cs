using UnityEngine;
using System.Collections;
using ChaosCosmos.Gameplay.AI;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Services.RemoteConfig;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.Analytics;
using System.Collections.Generic;


namespace ChaosCosmos.Gameplay.PowerUps
{
    [CreateAssetMenu(fileName = "NewRepelBotsEffect", menuName = "ChaosCosmos/PowerUp Effects/Repel Bots")]
    public class RepelBotsPowerUpEffect : PowerUpEffect
    {
        [Tooltip("Raggio dell'impulso di repulsione.")]
        public float repelRadius = 15f;
        [Tooltip("Forza dell'impulso di repulsione.")]
        public float repelForce = 1000f;
        [Tooltip("Ogni quanto tempo applicare l'impulso in secondi.")]
        public float pulseInterval = 0.5f;

        public override void Apply(PlanetController targetPlanet, IRemoteConfigService rcService)
        {
            string durationKey = $"ab_test_{this.name.Replace(" ", "")}_duration_seconds";
            string radiusKey = $"ab_test_{this.name.Replace(" ", "")}_radius_value";
            string forceKey = $"ab_test_{this.name.Replace(" ", "")}_force_value";
            // pulseInterval potrebbe anche essere remotizzato

            float actualDuration = GetConfiguredFloat(rcService, durationKey, this.duration);
            float actualRadius = GetConfiguredFloat(rcService, radiusKey, this.repelRadius);
            float actualForce = GetConfiguredFloat(rcService, forceKey, this.repelForce);

            Debug.Log($"[RepelBotsEffect] Applicato '{effectName}' a {targetPlanet.gameObject.name} per {actualDuration}s. Raggio: {actualRadius}, Forza: {actualForce}");
            targetPlanet.StartCoroutine(RepelBotsCoroutine(targetPlanet, actualDuration, actualRadius, actualForce));

            // Analytics per A/B test
             if (rcService != null && rcService.IsReady)
            {
                bool durationSet = rcService.GetString(durationKey, null) != null;
                bool radiusSet = rcService.GetString(radiusKey, null) != null;
                bool forceSet = rcService.GetString(forceKey, null) != null;

                if (durationSet || radiusSet || forceSet)
                {
                    if (ServiceLocator.IsRegistered<IAnalyticsService>())
                    {
                        var analytics = ServiceLocator.Get<IAnalyticsService>();
                        var parameters = new Dictionary<string, object> {
                            {"power_up_name", effectName},
                            {"source", "remote_config"}
                        };
                        if (durationSet) parameters.Add("duration_value", actualDuration);
                        if (radiusSet) parameters.Add("radius_value", actualRadius);
                        if (forceSet) parameters.Add("force_value", actualForce);

                        analytics.TrackEvent("ABTest_PowerUpParameter", parameters);
                    }
                }
            }
        }

        private IEnumerator RepelBotsCoroutine(PlanetController sourcePlanet, float effectDuration, float currentRepelRadius, float currentRepelForce)
        {
            float timer = 0;
            float lastPulseTime = -pulseInterval;

            while (timer < effectDuration)
            {
                if (Time.time - lastPulseTime >= pulseInterval)
                {
                    PerformRepelPulse(sourcePlanet, currentRepelRadius, currentRepelForce);
                    lastPulseTime = Time.time;
                }
                timer += Time.deltaTime;
                yield return null;
            }
            Debug.Log($"[RepelBotsEffect] Effetto '{effectName}' terminato per {sourcePlanet.gameObject.name}");
        }

        private void PerformRepelPulse(PlanetController sourcePlanet, float radius, float force)
        {
            if (sourcePlanet == null) return;

            // TODO_SFX: RepelBots_Pulse_Wave (suono dell'onda che parte da sourcePlanet)
            // TODO_VFX: Play_Repel_Pulse_Visual_at_sourcePlanet_position (effetto onda/particellare che si espande)
            Debug.Log($"[RepelBotsEffect] Eseguendo impulso di repulsione da {sourcePlanet.name}");


            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(sourcePlanet.transform.position, radius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == sourcePlanet.gameObject) continue; // Non respingere se stesso

                // Controlla se è un bot usando il tag da GameConstants
                if (hitCollider.CompareTag(GameTags.BOT_TAG))
                {
                    // Potrebbe avere BotBrain o solo PlanetController se i bot sono anche PlanetController
                    Rigidbody2D botRb = hitCollider.GetComponent<Rigidbody2D>();
                    if (botRb != null)
                    {
                        Vector2 direction = (hitCollider.transform.position - sourcePlanet.transform.position).normalized;
                        if (direction == Vector2.zero)
                        {
                            direction = Random.insideUnitCircle.normalized;
                            if (direction == Vector2.zero) direction = Vector2.right;
                        }
                        botRb.AddForce(direction * force, ForceMode2D.Impulse);
                        // TODO_SFX: Bot_Repelled_Impact (suono sul bot che viene spinto, opzionale)
                        // TODO_VFX: Bot_Repelled_Effect (effetto sul bot che viene spinto, opzionale)
                        // Debug.Log($"[RepelBotsEffect] Respingendo {hitCollider.name} con forza {force}");
                    }
                }
            }
        }

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
