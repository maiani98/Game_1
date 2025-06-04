using UnityEngine;
using ChaosCosmos.Core.Services; // Per ServiceLocator
using ChaosCosmos.Services.Analytics; // Per IAnalyticsService
using System.Collections.Generic; // Per Dictionary
using ChaosCosmos.Gameplay; // Per PlanetController (se non già visibile per namespace)

namespace ChaosCosmos.Gameplay.PowerUps
{
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpPickup : MonoBehaviour
    {
        public PowerUpEffect powerUpEffect; // Assegnare l'asset ScriptableObject nell'Inspector

        void Awake()
        {
            // Assicurati che il collider sia un trigger
            GetComponent<Collider2D>().isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (powerUpEffect == null)
            {
                Debug.LogError("PowerUpPickup: PowerUpEffect non assegnato!");
                return;
            }

            PlanetController planet = other.GetComponent<PlanetController>();
            if (planet != null)
            {
                // Applica l'effetto al pianeta
                powerUpEffect.Apply(planet);

                IAnalyticsService analytics = ServiceLocator.Get<IAnalyticsService>();
                if (analytics != null)
                {
                    analytics.TrackEvent("PowerUpCollected", new Dictionary<string, object>
                    {
                        { "powerup_type", powerUpEffect.effectName },
                        // Assicurati che PlanetController abbia GetCurrentMass() o un modo per accedere alla massa
                        { "planet_mass_at_collection", planet.GetCurrentMass() }
                    });
                }

                // Disattiva o distruggi il power-up dopo la raccolta
                // Per ora lo distruggiamo. In futuro potrebbe avere un respawn.
                Destroy(gameObject);
                Debug.Log($"PowerUp '{powerUpEffect.effectName}' raccolto da {other.name}");
            }
        }
    }
}
