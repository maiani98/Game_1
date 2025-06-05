using UnityEngine;
using System.Collections;
using ChaosCosmos.Services.RemoteConfig; // Aggiunto using
using ChaosCosmos.Core.Services; // Per ServiceLocator
using ChaosCosmos.Services.Analytics; // Per IAnalyticsService
using System.Collections.Generic; // Per Dictionary
using ChaosCosmos.Core.Constants; // Per RemoteConfigKeyPatterns

namespace ChaosCosmos.Gameplay.PowerUps
{
    [CreateAssetMenu(fileName = "NewSpeedBoostEffect", menuName = "ChaosCosmos/PowerUp Effects/Speed Boost")]
    public class SpeedBoostEffect : PowerUpEffect
    {
        public float speedMultiplier = 1.5f;

        // Nota: la firma di Apply è cambiata per includere IRemoteConfigService
        public override void Apply(PlanetController targetPlanet, IRemoteConfigService rcService)
        {
            float actualDuration = this.duration; // Valore di default dallo ScriptableObject

            // Usiamo il nome dell'asset ScriptableObject (che è univoco per istanza SO) come parte della chiave.
            // O, meglio, usare 'effectName' se è pensato per essere un ID univoco e stabile per questo *tipo* di effetto.
            // Se 'effectName' può cambiare o non è univoco, usare 'name' (nome dell'asset SO) è più sicuro per distinguere
            // diverse configurazioni SO dello stesso tipo di effetto.
            // Per questo esempio, assumiamo che 'name' (nome dell'asset SO) sia un identificatore decente per l'A/B test.
            // Es. se l'asset si chiama "SpeedBoost_FastVariant.asset", la chiave sarà "ab_test_SpeedBoost_FastVariant_duration_seconds"
            // Se si chiama "SpeedBoostEffect.asset", la chiave sarà "ab_test_SpeedBoostEffect_duration_seconds"
            // Se si vuole una chiave più generica per "tutti gli speed boost", usare this.effectName (se standardizzato)

            string keySuffix = name; // Usa il nome dell'asset SO per la chiave.
                                     // Potrebbe essere meglio avere un campo `effectKey` in PowerUpEffect.cs

            string remoteConfigKey = RemoteConfigKeyPatterns.GetPowerUpDurationKey(keySuffix);
            // GetPowerUpDurationKey potrebbe essere: $"powerup_{powerUpName}_duration"
            // Per A/B test, la chiave in RemoteConfigService è: "ab_test_SpeedBoostEffect_duration_seconds"
            // Quindi dobbiamo adattare la chiave o il pattern.
            // Per ora, costruiamo la chiave come specificato nel task:
            remoteConfigKey = $"ab_test_{this.GetType().Name}_duration_seconds"; // Es: "ab_test_SpeedBoostEffect_duration_seconds"


            if (rcService != null && rcService.IsReady)
            {
                actualDuration = rcService.GetFloat(remoteConfigKey, this.duration);
                Debug.Log($"[SpeedBoostEffect] '{this.name}': Durata SO: {this.duration}, Chiave RC: '{remoteConfigKey}', Valore da RC: {rcService.GetString(remoteConfigKey, "NON TROVATO")}, Durata Effettiva: {actualDuration}");
            }
            else
            {
                Debug.LogWarning($"[SpeedBoostEffect] '{this.name}': IRemoteConfigService non disponibile/pronto. Uso durata da SO: {this.duration}");
            }

            Debug.Log($"[SpeedBoostEffect] Applicato '{effectName}' a {targetPlanet.gameObject.name} per {actualDuration}s. Moltiplicatore velocità: {speedMultiplier}");
            targetPlanet.StartCoroutine(SpeedBoostCoroutine(targetPlanet, actualDuration));

            // Traccia l'assegnazione al segmento A/B se la durata è diversa dal default del SO
            // E solo se Remote Config è stato effettivamente consultato e ha fornito un valore (anche se uguale al default, ma la chiave esiste)
            if (rcService != null && rcService.IsReady && rcService.GetString(remoteConfigKey, null) != null) // Controlla se la chiave esiste in RC
            {
                 if (ServiceLocator.IsRegistered<IAnalyticsService>())
                {
                    var analytics = ServiceLocator.Get<IAnalyticsService>();
                    string experimentName = $"{keySuffix}DurationABTest";
                    analytics.TrackEvent("ABTest_UserSegmentAssigned", new Dictionary<string, object> {
                        {"experiment_name", experimentName},
                        {"variant_name", $"Duration_{actualDuration}s"} // Potrebbe essere Variant_A, Variant_B etc.
                    });
                }
            }
        }

        private IEnumerator SpeedBoostCoroutine(PlanetController targetPlanet, float actualDuration)
        {
            targetPlanet.ApplySpeedMultiplier(speedMultiplier, true);
            yield return new WaitForSeconds(actualDuration);
            targetPlanet.ApplySpeedMultiplier(speedMultiplier, false);
            Debug.Log($"[SpeedBoostEffect] '{effectName}' terminato per {targetPlanet.gameObject.name}");
        }
    }
}
