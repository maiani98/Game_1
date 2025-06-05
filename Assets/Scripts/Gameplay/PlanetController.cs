using UnityEngine;
using ChaosCosmos.Core;
using ChaosCosmos.Core.Constants;

namespace ChaosCosmos.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlanetController : MonoBehaviour
    {
        [Header("Settings")]
        public float baseSpeed = 2f;
        public PlanetGrowthData growthData;
        public ParticleSystem absorbVfx;

        private Rigidbody2D _rb; // Prefixed
        private float _currentMass = 1f; // Prefixed
        public float CurrentMass => _currentMass; // Uses prefixed field
        private float _currentSpeedMultiplier = 1f; // Prefixed

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>(); // Used prefixed
            if (growthData == null)
            { // Added braces
                Debug.LogError("PlanetGrowthData non assegnato a PlanetController!");
            }
            UpdateScale();
        }

        void Update()
        {
            HandleInput();
        }

        void HandleInput()
        {
            if (Input.GetMouseButton(0))
            {
                if (Camera.main == null)
                { // Added braces
                    Debug.LogError("Main Camera non trovata nella scena. Assicurati che ci sia una Main Camera taggata correttamente.");
                    SetMovementInput(Vector2.zero);
                    return;
                }
                Vector3 mouseScreenPosition = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                worldPosition.z = transform.position.z;

                Vector2 direction = (worldPosition - transform.position);
                SetMovementInput(direction);
            }
            else
            {
                SetMovementInput(Vector2.zero);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameTags.COLLECTIBLE_TAG))
            {
                MassSource massSource = other.GetComponent<MassSource>();
                if (massSource != null)
                {
                    AddMass(massSource.massValue);

                    if (absorbVfx != null)
                    { // Added braces
                        absorbVfx.Play();
                    }

                    Destroy(other.gameObject);
                    // Debug.Log($"Ingerito: {other.name}, Massa Ottenuta: {massSource.massValue}. Nuova Massa Totale: {_currentMass}"); // Log verboso
                }
                else
                {
                    Debug.LogWarning($"Oggetto {other.name} con tag '{GameTags.COLLECTIBLE_TAG}' non ha il componente MassSource.");
                }
            }
        }

        void UpdateScale()
        {
            if (growthData != null && growthData.massToRadiusCurve != null)
            {
                float radius = growthData.massToRadiusCurve.Evaluate(_currentMass); // Used prefixed
                transform.localScale = Vector3.one * radius;
            }
            else
            {
                Debug.LogWarning("PlanetGrowthData o la sua curva non sono configurati. La scala non verrà aggiornata.");
            }
        }

        private float SpeedModifier()
        {
            return Mathf.Lerp(1.4f, 0.6f, Mathf.InverseLerp(1f, 40f, _currentMass)); // Used prefixed
        }

        public void AddMass(float massAmount)
        {
            if (massAmount <= 0)
            { // Added braces
                return;
            }
            _currentMass += massAmount; // Used prefixed
            UpdateScale();
            // Debug.Log($"Massa aggiunta: {massAmount}. Nuova massa: {_currentMass}"); // Log verboso
        }

        public void ApplySpeedMultiplier(float multiplier, bool apply)
        {
            if (apply)
            {
                _currentSpeedMultiplier *= multiplier; // Used prefixed
                // Debug.Log($"Speed multiplier applicato: {multiplier}. Totale ora: {_currentSpeedMultiplier}");
            }
            else
            {
                if (multiplier != 0)
                { // Added braces
                    _currentSpeedMultiplier /= multiplier; // Used prefixed
                    // Debug.Log($"Speed multiplier rimosso: {multiplier}. Totale ora: {_currentSpeedMultiplier}");
                }
            }
            _currentSpeedMultiplier = Mathf.Max(0.1f, _currentSpeedMultiplier); // Used prefixed
        }

        public float GetCurrentMass()
        {
            return _currentMass; // Used prefixed
        }

        public float GetCurrentSpeedModifierValue()
        {
            return SpeedModifier();
        }

        public void SetMovementInput(Vector2 inputDirection)
        {
            if (_rb == null) // Used prefixed
            {
                _rb = GetComponent<Rigidbody2D>(); // Used prefixed
                if (_rb == null) // Used prefixed
                { // Added braces
                    Debug.LogError("PlanetController: Rigidbody2D non trovato su " + gameObject.name);
                    return;
                }
            }

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                Vector2 effectiveDirection = inputDirection.normalized;
                _rb.velocity = effectiveDirection * baseSpeed * GetCurrentSpeedModifierValue() * _currentSpeedMultiplier; // Used prefixed _rb and _currentSpeedMultiplier
            }
            else
            {
                _rb.velocity = Vector2.zero; // Used prefixed
            }
        }
    }
}
