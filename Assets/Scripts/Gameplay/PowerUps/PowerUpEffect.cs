using UnityEngine;
using ChaosCosmos.Services.RemoteConfig; // Aggiunto using

namespace ChaosCosmos.Gameplay.PowerUps
{
    public abstract class PowerUpEffect : ScriptableObject
    {
        public float duration = 5f;
        public string effectName = "Base PowerUp";
        [TextArea]
        public string description = "Descrizione base dell'effetto.";
        public Sprite icon;

        // Metodo astratto che le sottoclassi implementeranno per applicare l'effetto
        // public abstract void Apply(PlanetController targetPlanet); // Vecchia firma
        public abstract void Apply(PlanetController targetPlanet, IRemoteConfigService rcService); // Nuova firma con using

        // Opzionale: metodo per rimuovere l'effetto (se temporaneo e necessita pulizia)
        public virtual void Remove(PlanetController targetPlanet)
        {
            // Default no-op
        }
    }
}
