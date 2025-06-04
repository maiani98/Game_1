using UnityEngine;
using System.Collections;

namespace ChaosCosmos.Gameplay.PowerUps
{
    [CreateAssetMenu(fileName = "NewSpeedBoostEffect", menuName = "ChaosCosmos/PowerUp Effects/Speed Boost")]
    public class SpeedBoostEffect : PowerUpEffect
    {
        public float speedMultiplier = 1.5f;
        // private float originalSpeedModifierValue; // Per ripristinare // Commentato perché l'approccio è cambiato

        public override void Apply(PlanetController targetPlanet)
        {
            Debug.Log($"Applicato {effectName} a {targetPlanet.gameObject.name} per {duration}s. Moltiplicatore velocità: {speedMultiplier}");
            // Avvia la coroutine su targetPlanet, che è un MonoBehaviour
            targetPlanet.StartCoroutine(SpeedBoostCoroutine(targetPlanet));
        }

        private IEnumerator SpeedBoostCoroutine(PlanetController targetPlanet)
        {
            // Applica il moltiplicatore di velocità tramite il metodo in PlanetController
            targetPlanet.ApplySpeedMultiplier(speedMultiplier, true);

            yield return new WaitForSeconds(duration);

            // Rimuove il moltiplicatore di velocità tramite il metodo in PlanetController
            targetPlanet.ApplySpeedMultiplier(speedMultiplier, false);
            Debug.Log($"{effectName} terminato per {targetPlanet.gameObject.name}");
        }

        // Non è necessario un Remove() esplicito se la coroutine gestisce il ripristino,
        // e PlanetController ora gestisce l'accumulo/rimozione dei moltiplicatori.
    }
}
