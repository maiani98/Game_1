using UnityEngine;
using ChaosCosmos.Core; // Per eventuali futuri riferimenti a servizi Core

namespace ChaosCosmos.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlanetController : MonoBehaviour
    {
        [Header("Settings")]
        public float baseSpeed = 2f;
        public PlanetGrowthData growthData; // Riferimento allo ScriptableObject
        public ParticleSystem absorbVfx; // Sarà assegnato nell'editor

        private Rigidbody2D rb;
        private float currentMass = 1f; // Massa iniziale
        private float currentSpeedMultiplier = 1f; // Moltiplicatore di velocità per i power-up
        // private Coroutine activeSpeedBoostCoroutine; // Commentato: la logica di gestione è cambiata


        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (growthData == null)
            {
                Debug.LogError("PlanetGrowthData non assegnato a PlanetController!");
                // Potrebbe essere utile caricare un default o disabilitare il componente
            }
            // Imposta la scala iniziale basata sulla massa iniziale
            UpdateScale();
        }

        void Update()
        {
            HandleInput();
        }

        void HandleInput()
        {
            // Simula one-thumb drag con il mouse per test in editor
            if (Input.GetMouseButton(0)) // Tasto sinistro del mouse premuto
            {
                if (Camera.main == null)
                {
                    Debug.LogError("Main Camera non trovata nella scena. Assicurati che ci sia una Main Camera taggata correttamente.");
                    return;
                }
                Vector3 mouseScreenPosition = Input.mousePosition;
                // mouseScreenPosition.z = Camera.main.nearClipPlane + 10; // Distanza arbitraria dalla camera
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                worldPosition.z = transform.position.z; // Mantiene la stessa Z del pianeta

                Vector2 direction = (worldPosition - transform.position);

                // Normalizza solo se la magnitudine è significativa per evitare movimenti a zero
                if (direction.sqrMagnitude > 0.01f)
                {
                   // MODIFICA QUI: Aggiunto * currentSpeedMultiplier
                   rb.velocity = direction.normalized * baseSpeed * SpeedModifier() * currentSpeedMultiplier;
                }
                else
                {
                   rb.velocity = Vector2.zero;
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }

        // OnTriggerEnter2D sarà espanso nel prossimo step per l'ingestione
        void OnTriggerEnter2D(Collider2D other)
        {
                    if (other.CompareTag("Collectible")) // Assicurati che il tag esista nel progetto Unity
            {
                        MassSource massSource = other.GetComponent<MassSource>();
                        if (massSource != null)
                        {
                            AddMass(massSource.massValue); // Usa il metodo AddMass già implementato

                            if (absorbVfx != null)
                            {
                                // Opzionale: configurare la posizione dell'effetto o genitore
                                // absorbVfx.transform.position = other.transform.position; // Esempio
                                absorbVfx.Play();
                            }

                            Destroy(other.gameObject);
                            Debug.Log($"Ingerito: {other.name}, Massa Ottenuta: {massSource.massValue}. Nuova Massa Totale: {currentMass}");
                        }
                        else
                        {
                            Debug.LogWarning($"Oggetto {other.name} con tag 'Collectible' non ha il componente MassSource.");
                        }
            }
        }

        void UpdateScale()
        {
            if (growthData != null && growthData.massToRadiusCurve != null)
            {
                float radius = growthData.massToRadiusCurve.Evaluate(currentMass);
                transform.localScale = Vector3.one * radius;
            }
            else
            {
                // Fallback o errore se growthData non è configurato
                // Per ora, usa una scala di default o logga un errore più specifico.
                // Se massToRadiusCurve è l'AnimationCurve diretta (come nell'esempio originale):
                // float radius = massToRadius.Evaluate(currentMass); // Dove massToRadius è public AnimationCurve
                // transform.localScale = Vector3.one * radius;
                // Ma usando growthData è più pulito.
                Debug.LogWarning("PlanetGrowthData o la sua curva non sono configurati. La scala non verrà aggiornata.");
            }
        }

        float SpeedModifier()
        {
            // Formula come da specifiche: più grosso = più lento
            // Lerp(maxSpeedFactor, minSpeedFactor, InverseLerp(minMass, maxMass, currentMass))
            // Esempio: maxSpeedFactor = 1.4f, minSpeedFactor = 0.6f, minMass = 1, maxMass = 40
            return Mathf.Lerp(1.4f, 0.6f, Mathf.InverseLerp(1f, 40f, currentMass));
        }

        // Metodo pubblico per aumentare la massa (sarà usato dal sistema di ingestione)
        public void AddMass(float massAmount)
        {
            if (massAmount <= 0) return;
            currentMass += massAmount;
            UpdateScale(); // Aggiorna la scala dopo aver cambiato la massa
            Debug.Log($"Massa aggiunta: {massAmount}. Nuova massa: {currentMass}");
        }

        // Nuovo metodo pubblico per applicare/rimuovere un moltiplicatore di velocità
        public void ApplySpeedMultiplier(float multiplier, bool apply)
        {
            if (apply)
            {
                currentSpeedMultiplier *= multiplier;
                Debug.Log($"Speed multiplier applicato: {multiplier}. Totale ora: {currentSpeedMultiplier}");
            }
            else // Rimuove il moltiplicatore (dividendo per esso)
            {
                if (multiplier != 0) // Evita divisione per zero
                {
                    currentSpeedMultiplier /= multiplier;
                    Debug.Log($"Speed multiplier rimosso: {multiplier}. Totale ora: {currentSpeedMultiplier}");
                }
            }
            // Assicurati che il moltiplicatore non scenda sotto un valore minimo sensato (es. 0.1f)
            currentSpeedMultiplier = Mathf.Max(0.1f, currentSpeedMultiplier);
        }

        // Metodi Getter per BotBrain (aggiunti nel task precedente o da istruzioni)
        public float GetCurrentMass()
        {
            return currentMass;
        }

        public float GetCurrentSpeedModifier()
        {
            // Questo metodo ora dovrebbe includere anche currentSpeedMultiplier se i bot devono beneficiarne/esserne affetti.
            // Oppure BotBrain deve essere consapevole di currentSpeedMultiplier separatamente.
            // Per ora, BotBrain usa SpeedModifier() che è basato sulla massa, e poi moltiplica per il suo currentSpeedMultiplier.
            // Se SpeedModifier() è privato, BotBrain usa la sua logica.
            // Questa implementazione di GetCurrentSpeedModifier restituisce solo il modificatore basato sulla massa.
            return SpeedModifier();
        }
    }
}
