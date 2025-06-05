using UnityEngine;
// using System.Collections.Generic; // Non più necessario dopo aver rimosso List<EventParameter>

namespace ChaosCosmos.Gameplay.LiveOps
{
    public enum EventEffectType { XPMultiplier, SpawnRateBonus, SpecialCollectible, Theming }

    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "ChaosCosmos/LiveOps/Game Event Data")]
    public class EventData : ScriptableObject
    {
        public string eventID;
        public string eventName = "New Game Event";
        [TextArea] public string description = "Description of the event.";
        public Sprite eventIcon;

        public string remoteConfigActivationKey;

        public EventEffectType effectType = EventEffectType.XPMultiplier;
        public float effectValue = 2f;
        public string stringValue = "";
        // public List<EventParameter> parameters; // Mantenuto commentato

        void OnValidate()
        {
            if (string.IsNullOrEmpty(eventID))
            {
                 eventID = System.Guid.NewGuid().ToString();
            }
        }
    }
}
