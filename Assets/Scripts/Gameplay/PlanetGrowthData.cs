using UnityEngine;

namespace ChaosCosmos.Gameplay
{
    [CreateAssetMenu(fileName = "NewPlanetGrowthData", menuName = "ChaosCosmos/Planet Growth Data")]
    public class PlanetGrowthData : ScriptableObject
    {
        public AnimationCurve massToRadiusCurve = new AnimationCurve(
            new Keyframe(0, 0.5f), // Massa 0, raggio 0.5 (valore iniziale)
            new Keyframe(1, 1f),   // Massa 1, raggio 1
            new Keyframe(10, 3f),  // Massa 10, raggio 3
            new Keyframe(40, 5f)   // Massa 40, raggio 5 (come da SpeedModifier)
        );
    }
}
