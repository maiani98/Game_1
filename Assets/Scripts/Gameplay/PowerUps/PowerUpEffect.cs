using UnityEngine;

namespace ChaosCosmos.Gameplay.PowerUps
{
    public abstract class PowerUpEffect : ScriptableObject
    {
        public float duration = 5f; // Durata standard, può essere overridata o non usata
        public string effectName = "Base PowerUp";
        [TextArea]
        public string description = "Descrizione base dell'effetto.";
        public Sprite icon; // Icona per UI, opzionale

        // Metodo astratto che le sottoclassi implementeranno per applicare l'effetto
        public abstract void Apply(PlanetController targetPlanet);

        // Opzionale: metodo per rimuovere l'effetto (se temporaneo e necessita pulizia)
        public virtual void Remove(PlanetController targetPlanet) { }
    }
}
