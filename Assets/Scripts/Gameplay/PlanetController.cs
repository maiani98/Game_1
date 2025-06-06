using UnityEngine;
using ChaosCosmos.Core.Services;         // Per ServiceLocator
using ChaosCosmos.Services.GameManagement; // Per IGameManagerService
using ChaosCosmos.Services.Progression;    // Per IProgressService
using ChaosCosmos.Gameplay.GameLogic;    // Per GameState
using ChaosCosmos.Core.Constants;        // Per GameplayBalance e GameTags

namespace ChaosCosmos.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlanetController : MonoBehaviour
    {
        [Header("Settings")]
        public float baseSpeed = 2f;
        public PlanetGrowthData growthData;
        public ParticleSystem absorbVfx;

        private Rigidbody2D _rb;
        private float _currentMass = 1f;
        public float CurrentMass => _currentMass;
        private float _currentSpeedMultiplier = 1f;

        private bool _isShieldActive = false;
        private float _currentMassAbsorptionMultiplier = 1.0f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (growthData == null)
            {
                Debug.LogError($"[{gameObject.name}] PlanetGrowthData non assegnato!");
            }
            UpdateScale();
        }

        void Update()
        {
            if (CurrentGameStateIsNotPlayable()) // Non processare input se il gioco non è attivo
            {
                if (_rb != null && _rb.velocity != Vector2.zero) SetMovementInput(Vector2.zero); // Ferma il pianeta
                return;
            }
            HandleInput();
            // TODO_SFX: Player_Thruster_Loop (start/stop basato su _rb.velocity.magnitude > ~0, pitch by speed)
            // TODO_VFX: Gestire attivazione/disattivazione e intensità scia motore in base a _rb.velocity
        }

        void HandleInput()
        {
            // Questo HandleInput è per il giocatore. I bot usano SetMovementInput direttamente.
            if (gameObject.CompareTag(GameTags.PLAYER_TAG)) // Solo il giocatore usa Input.GetMouseButton
            {
                if (Input.GetMouseButton(0))
                {
                    if (Camera.main == null)
                    {
                        Debug.LogError($"[{gameObject.name}] Main Camera non trovata.");
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
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (CurrentGameStateIsNotPlayable()) return;

            if (_isShieldActive && !other.CompareTag(GameTags.COLLECTIBLE_TAG))
            {
                Debug.Log($"[{gameObject.name}] Impatto con {other.name} bloccato dallo scudo!");
                // TODO_SFX: Shield_Impact_Generic
                // TODO_VFX: Shield_Impact_Effect_at_collision_point
                SetShieldActive(false); // Scudo si rompe al primo colpo (o N colpi)
                if (other.CompareTag(GameTags.BOT_TAG) || other.CompareTag(GameTags.PLAYER_TAG)) // Esempio: distruggi proiettili o piccoli oggetti
                {
                    // Potrebbe distruggere "proiettili" o oggetti deboli, non altri pianeti
                }
                return;
            }

            if (other.CompareTag(GameTags.COLLECTIBLE_TAG))
            {
                MassSource massSource = other.GetComponent<MassSource>();
                if (massSource != null)
                {
                    float massCollected = massSource.massValue;
                    AddMass(massCollected);

                    if (absorbVfx != null)
                    {
                        absorbVfx.Play();
                    }
                    // TODO_SFX: Collectible_Ingested_Small/Medium/Large
                    Destroy(other.gameObject);
                }
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (CurrentGameStateIsNotPlayable()) return;

            PlanetController otherPlanet = collision.gameObject.GetComponent<PlanetController>();
            if (otherPlanet == null || !otherPlanet.enabled || otherPlanet.CurrentGameStateIsNotPlayable())
            {
                return;
            }
            if (otherPlanet == this) return; // Auto-collisione

            // TODO_SFX: Planet_Collision_Soft/Hard (basato su differenziale di massa o velocità impatto)

            float myMass = this._currentMass; // Accesso diretto al campo per evitare chiamate ricorsive a CurrentMass se avesse logica
            float opponentMass = otherPlanet._currentMass; // Accesso diretto (se permesso, o via getter se no)

            if (myMass > opponentMass * GameplayBalance.COLLISION_DAMAGE_THRESHOLD_FACTOR)
            {
                float massStolen = opponentMass * GameplayBalance.COLLISION_MASS_LOSS_PERCENTAGE;
                Debug.Log($"[{gameObject.name}] danneggia [{otherPlanet.name}]. Massa rubata: {massStolen}");
                otherPlanet.TakeDamage(massStolen);
                // this.AddMass(massStolen * 0.25f); // Opzionale: Guadagna una frazione della massa rubata/persa dall'altro
            }
            else if (opponentMass > myMass * GameplayBalance.COLLISION_DAMAGE_THRESHOLD_FACTOR)
            {
                float massLost = myMass * GameplayBalance.COLLISION_MASS_LOSS_PERCENTAGE;
                Debug.Log($"[{gameObject.name}] subisce danno da [{otherPlanet.name}]. Massa persa: {massLost}");
                this.TakeDamage(massLost);
            }
        }

        public void TakeDamage(float massLossAmount)
        {
            if (_isShieldActive)
            {
                Debug.Log($"[{gameObject.name}] Danno di {massLossAmount} bloccato dallo scudo!");
                // TODO_SFX: Shield_Blocked_Damage
                // TODO_VFX: Shield_Impact_Visual_Effect
                SetShieldActive(false); // Scudo si rompe
                return;
            }
            if (CurrentGameStateIsNotPlayable()) return;

            _currentMass -= massLossAmount;
            // TODO_SFX: Player_Took_Damage or Bot_Took_Damage
            // TODO_VFX: Damage_Impact_Effect

            if (_currentMass <= GameplayBalance.MINIMUM_MASS_TO_SURVIVE)
            {
                _currentMass = GameplayBalance.MINIMUM_MASS_TO_SURVIVE; // Non andare sotto
                UpdateScale(); // Aggiorna la scala all'ultima massa valida prima della sconfitta
                HandleDefeat();
            }
            else
            {
                UpdateScale();
                Debug.Log($"[{gameObject.name}] Ha subito danno! Massa persa: {massLossAmount}. Massa attuale: {_currentMass}.");
            }
        }

        private void HandleDefeat()
        {
            // CurrentGameStateIsNotPlayable() qui potrebbe essere ridondante se TakeDamage lo chiama già,
            // ma è una sicurezza se HandleDefeat fosse chiamato da altrove.
            // Lo stato del gioco verrà cambiato da GameManagerService per il giocatore.
            Debug.Log($"[{gameObject.name}] È stato sconfitto (massa <= {GameplayBalance.MINIMUM_MASS_TO_SURVIVE})!");
            // TODO_SFX: Player_Defeated or Bot_Defeated
            // TODO_VFX: Planet_Explosion_Effect

            if (gameObject.CompareTag(GameTags.PLAYER_TAG))
            {
                if (ServiceLocator.IsRegistered<IGameManagerService>())
                {
                    ServiceLocator.Get<IGameManagerService>().EndMatchPrematurely();
                }
                this.enabled = false;
                if(_rb != null) _rb.simulated = false;
            }
            else if (gameObject.CompareTag(GameTags.BOT_TAG))
            {
                float massToDrop = _currentMass * GameplayBalance.BOT_DEATH_MASS_DROP_PERCENTAGE;
                Debug.Log($"[{gameObject.name}] Bot sconfitto, rilascia {massToDrop} massa come collectible (simulato).");
                // TODO: Istanziare collectible(s) con ArenaManager.SpawnCollectibleAt(transform.position, massToDrop);

                if (ServiceLocator.IsRegistered<IProgressService>())
                {
                    ServiceLocator.Get<IProgressService>().AddXP(GameplayBalance.BOT_DEATH_XP_REWARD);
                     Debug.Log($"Concessi {GameplayBalance.BOT_DEATH_XP_REWARD} XP per aver sconfitto {gameObject.name}.");
                }
                Destroy(gameObject, 0.1f);
            }
        }


        void UpdateScale() { /* ... come prima ... */
            if (growthData != null && growthData.massToRadiusCurve != null)
            {
                float oldRadius = transform.localScale.x;
                float newRadius = growthData.massToRadiusCurve.Evaluate(_currentMass);
                transform.localScale = Vector3.one * newRadius;
                if (newRadius > oldRadius && oldRadius > 0.51f)
                {
                    // TODO_SFX: Player_Grow_Pulse
                    // TODO_VFX: Player_Grow_Pulse_Effect
                }
            } else { Debug.LogWarning($"[{gameObject.name}] PlanetGrowthData o curva non configurati.");}
        }
        private float SpeedModifier() { return Mathf.Lerp(1.4f, 0.6f, Mathf.InverseLerp(1f, 40f, _currentMass)); }
        public void AddMass(float massAmount) { /* ... modificato sopra ... */
            if (massAmount <= 0) return;
            float actualMassToAdd = massAmount * _currentMassAbsorptionMultiplier;
            _currentMass += actualMassToAdd;
            UpdateScale();
        }
        public void ApplySpeedMultiplier(float multiplier, bool apply) { /* ... come prima ... */
            if (apply) _currentSpeedMultiplier *= multiplier;
            else if (multiplier != 0) _currentSpeedMultiplier /= multiplier;
            _currentSpeedMultiplier = Mathf.Max(0.1f, _currentSpeedMultiplier);
        }
        public void SetShieldActive(bool isActive) { /* ... modificato sopra ... */
            _isShieldActive = isActive;
            // TODO commenti per SFX/VFX sono già lì
            Debug.Log($"[{gameObject.name}] Scudo {(isActive ? "ATTIVATO" : "DISATTIVATO")}");
        }
        public bool IsShieldActive() => _isShieldActive;
        public void SetMassAbsorptionMultiplier(float multiplier) { /* ... modificato sopra ... */
             _currentMassAbsorptionMultiplier = Mathf.Max(0.1f, multiplier);
            Debug.Log($"[{gameObject.name}] Moltiplicatore Assorbimento Massa: {_currentMassAbsorptionMultiplier}");
        }
        public float GetCurrentMass() { return _currentMass; } // GetCurrentMass non più necessaria, c'è CurrentMass public property
        public float GetCurrentSpeedModifierValue() { return SpeedModifier(); }
        public void SetMovementInput(Vector2 inputDirection) { /* ... come prima ... */
            if (_rb == null) { _rb = GetComponent<Rigidbody2D>(); if (_rb == null) { Debug.LogError($"[{gameObject.name}] Rigidbody2D non trovato."); return; }}
            if (inputDirection.sqrMagnitude > 0.01f) { _rb.velocity = inputDirection.normalized * baseSpeed * GetCurrentSpeedModifierValue() * _currentSpeedMultiplier; }
            else { _rb.velocity = Vector2.zero; }
        }

        private bool CurrentGameStateIsNotPlayable()
        {
            if (ServiceLocator.IsRegistered<IGameManagerService>())
            {
                var gm = ServiceLocator.Get<IGameManagerService>();
                // Aggiungi gm != null check per robustezza, sebbene ServiceLocator dovrebbe garantirlo se IsRegistered è true
                return gm != null && (gm.CurrentGameState == GameState.GameOver || gm.CurrentGameState == GameState.Pregame);
            }
            // Se non c'è GameManager (es. test di scena isolato), considera giocabile per non bloccare i test.
            // O lancia un errore se si assume che GameManagerService debba sempre esistere.
            // Debug.LogWarning($"[{gameObject.name}] IGameManagerService non trovato, stato di gioco considerato giocabile per fallback.");
            return false;
        }
    }
}
