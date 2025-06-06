using UnityEngine;
using System.Collections;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Gameplay; // Per PlanetController

namespace ChaosCosmos.Gameplay.AI
{
    public enum BotPersonality { Standard, Tank, Scout }

    [RequireComponent(typeof(PlanetController))]
    public class BotBrain : MonoBehaviour
    {
        public enum BotState { Roaming, ChasingPlayer, Fleeing } // Aggiunto Fleeing

        [Header("AI Settings")]
        public BotState currentState = BotState.Roaming;
        public float roamChangeTargetDistance = 1f;
        public float chasePlayerDetectionRadius = 10f;
        public float losePlayerChaseRadius = 15f;
        public float stateChangeIntervalMin = 3f;
        public float stateChangeIntervalMax = 7f;

        [Header("AI Personality")]
        public BotPersonality personality = BotPersonality.Standard;

        [Tooltip("Scout: Flee if player mass is this many times greater OR if player has shield.")]
        public float scoutFleeMassRatioThreshold = 2.5f;
        [Tooltip("Scout: How far to set the flee target when fleeing.")]
        public float scoutFleeDistanceFactor = 0.75f; // Moltiplicatore per la dimensione dell'arena

        [Tooltip("Tank: Multiplies losePlayerChaseRadius to determine actual persistence in chasing.")]
        public float tankChasePersistenceFactor = 1.5f;

        [Header("References")]
        public ArenaManager arenaManager;

        private PlanetController _planetController;
        private Transform _playerTransform;
        private PlanetController _playerPlanetControllerCache; // Cache per il PlanetController del giocatore
        private Vector2 _targetPosition; // Unificato per roam, chase, flee
        private float _timeForNextStateChange;

        void Awake()
        {
            _planetController = GetComponent<PlanetController>();
            if (_planetController == null)
            {
                Debug.LogError($"[{gameObject.name}] BotBrain necessita di un PlanetController sullo stesso GameObject! Disabilito BotBrain.");
                enabled = false;
            }
        }

        void Start()
        {
            if (arenaManager == null)
            {
                arenaManager = FindObjectOfType<ArenaManager>();
                if (arenaManager == null)
                {
                    Debug.LogError($"[{gameObject.name}] BotBrain: ArenaManager non trovato nella scena! Disabilito BotBrain.");
                    enabled = false;
                    return;
                }
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag(GameTags.PLAYER_TAG);
            if (playerObject != null)
            {
                _playerTransform = playerObject.transform;
                _playerPlanetControllerCache = playerObject.GetComponent<PlanetController>();
                if (_playerPlanetControllerCache == null)
                {
                     Debug.LogError($"[{gameObject.name}] BotBrain: Giocatore con tag '{GameTags.PLAYER_TAG}' non ha PlanetController!");
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] BotBrain: Giocatore (tag '{GameTags.PLAYER_TAG}') non trovato. Il bot non potrà inseguire o fuggire attivamente.");
            }

            SetNewRandomRoamTarget(); // Imposta il target iniziale
            ScheduleNextStateChange();
            // Lo stato iniziale è Roaming, non serve cambiarlo qui a meno di logiche specifiche
        }

        void Update()
        {
            // Aggiorna il riferimento al PlanetController del giocatore se diventa nullo (es. giocatore distrutto e respawnato)
            // Questo è un semplice re-fetch, un sistema a eventi sarebbe più robusto.
            if (_playerTransform != null && (_playerPlanetControllerCache == null || !_playerPlanetControllerCache.enabled || _playerPlanetControllerCache.CurrentGameStateIsNotPlayable()))
            {
                _playerPlanetControllerCache = _playerTransform.GetComponent<PlanetController>();
            }


            if (Time.time >= _timeForNextStateChange)
            {
                ScheduleNextStateChange(); // Riprogramma subito per evitare chiamate multiple se DecideNextState non cambia stato
                DecideNextState();
            }

            switch (currentState)
            {
                case BotState.Roaming:
                    ExecuteRoaming();
                    break;
                case BotState.ChasingPlayer:
                    ExecuteChasingPlayer();
                    break;
                case BotState.Fleeing:
                    ExecuteFleeing();
                    break;
            }
        }

        void ScheduleNextStateChange()
        {
            _timeForNextStateChange = Time.time + Random.Range(stateChangeIntervalMin, stateChangeIntervalMax);
        }

        void DecideNextState()
        {
            if (_playerTransform == null || _playerPlanetControllerCache == null || !_playerPlanetControllerCache.isActiveAndEnabled || _playerPlanetControllerCache.CurrentGameStateIsNotPlayable())
            {
                if (currentState != BotState.Roaming)
                {
                    Debug.Log($"[{gameObject.name}, {personality}] Giocatore non valido/disponibile. Passo a Roaming.");
                    currentState = BotState.Roaming;
                    SetNewRandomRoamTarget();
                }
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            bool playerHasShield = _playerPlanetControllerCache.IsShieldActive();
            float playerMass = _playerPlanetControllerCache.CurrentMass;
            float myMass = _planetController.CurrentMass;

            // 1. Logica di Fuga (prioritaria se applicabile)
            bool shouldFlee = false;
            if (playerHasShield)
            {
                 // Tutti i bot (eccetto forse Tank se si vuole una logica iper-aggressiva) potrebbero voler evitare un giocatore con scudo se abbastanza vicini
                if (distanceToPlayer < chasePlayerDetectionRadius * 0.75f) // Se il giocatore con scudo è abbastanza vicino
                {
                    Debug.Log($"[{gameObject.name}, {personality}] Giocatore ({_playerTransform.name}) ha scudo attivo ed è vicino! Tento la fuga.");
                    shouldFlee = true;
                }
            }
            else if (personality == BotPersonality.Scout && playerMass > myMass * scoutFleeMassRatioThreshold)
            {
                if (distanceToPlayer < chasePlayerDetectionRadius) // Fugge solo se il player grande è nel raggio di "consapevolezza"
                {
                    Debug.Log($"[{gameObject.name} - Scout] Giocatore ({_playerTransform.name}) troppo grande ({playerMass} vs {myMass}), fuggo!");
                    shouldFlee = true;
                }
            }

            if (shouldFlee)
            {
                if (currentState != BotState.Fleeing) // Evita di reimpostare il target di fuga ogni frame se già in fuga
                {
                    currentState = BotState.Fleeing;
                    SetFleeTarget(_playerTransform.position);
                }
                return; // Decisione presa: fuggire
            }

            // 2. Logica di Inseguimento
            if (distanceToPlayer < chasePlayerDetectionRadius && !playerHasShield) // Non inseguire se il giocatore ha scudo
            {
                // Il Tank potrebbe avere una logica diversa per iniziare l'inseguimento (es. solo se più grande o se il giocatore è molto vicino)
                // Lo Standard e lo Scout iniziano ad inseguire se non stanno fuggendo
                if (currentState != BotState.ChasingPlayer)
                {
                    Debug.Log($"[{gameObject.name}, {personality}] Inizio inseguimento giocatore ({_playerTransform.name}).");
                    currentState = BotState.ChasingPlayer;
                    // Il target per ChasingPlayer è direttamente _playerTransform.position, aggiornato in ExecuteChasingPlayer
                }
                return; // Decisione presa: inseguire (o continuare a inseguire)
            }

            // 3. Logica per Perdere l'Inseguimento o Smettere di Fuggire
            if (currentState == BotState.ChasingPlayer)
            {
                float actualLoseRadius = losePlayerChaseRadius;
                if (personality == BotPersonality.Tank)
                {
                    actualLoseRadius *= tankChasePersistenceFactor;
                }
                if (distanceToPlayer > actualLoseRadius || playerHasShield) // Smetti di inseguire se troppo lontano O se il giocatore attiva lo scudo
                {
                    Debug.Log($"[{gameObject.name}, {personality}] Giocatore ({_playerTransform.name}) perso o scudo attivo. Torno a vagare. Dist: {distanceToPlayer}, LoseRadius: {actualLoseRadius}, Shield: {playerHasShield}");
                    currentState = BotState.Roaming;
                    SetNewRandomRoamTarget();
                }
            }
            else if (currentState == BotState.Fleeing)
            {
                // Smetti di fuggire se il giocatore è lontano o non è più una minaccia
                // O se il target di fuga è stato raggiunto (gestito in ExecuteFleeing)
                bool threatPersists = (playerHasShield && distanceToPlayer < losePlayerChaseRadius) ||
                                      (personality == BotPersonality.Scout && playerMass > myMass * scoutFleeMassRatioThreshold && distanceToPlayer < losePlayerChaseRadius);
                if (!threatPersists)
                {
                    Debug.Log($"[{gameObject.name}, {personality}] Minaccia giocatore cessata. Torno a vagare.");
                    currentState = BotState.Roaming;
                    SetNewRandomRoamTarget();
                }
            }
            // Se nessuna condizione sopra, e lo stato è Roaming, rimane in Roaming.
            // Se è Chasing e le condizioni per smettere non sono soddisfatte, continua Chasing.
            // Se è Fleeing e le condizioni per smettere non sono soddisfatte, continua Fleeing.
        }

        void ExecuteRoaming()
        {
            if (Vector2.Distance(transform.position, _targetPosition) < roamChangeTargetDistance)
            {
                SetNewRandomRoamTarget();
            }
            MoveTowards(_targetPosition);
        }

        void ExecuteFleeing()
        {
            // Se ha raggiunto il target di fuga, o se la minaccia non c'è più (già gestito in DecideNextState),
            // potrebbe ricalcolare un nuovo target di fuga o passare a Roaming.
            // Per ora, continua verso il target di fuga finché DecideNextState non cambia lo stato.
            if (Vector2.Distance(transform.position, _targetPosition) < roamChangeTargetDistance * 2f) // Raggio più grande per "raggiunto" in fuga
            {
                // Raggiunto il punto di fuga, ricalcola o passa a roaming se la minaccia è passata
                // DecideNextState dovrebbe gestire il passaggio a Roaming se la minaccia cessa.
                // Se la minaccia persiste, imposta un nuovo target di fuga.
                if (_playerTransform != null) SetFleeTarget(_playerTransform.position);
                else SetNewRandomRoamTarget(); // Giocatore sparito
            }
            MoveTowards(_targetPosition);
        }


        void SetNewRandomRoamTarget() // Sovraccarico per compatibilità, chiama quello principale
        {
             SetNewRandomRoamTargetInternal(false, null);
        }

        private void SetFleeTarget(Vector2 fleeFromPosition)
        {
            SetNewRandomRoamTargetInternal(true, fleeFromPosition);
        }

        // Rinominato il metodo originale per chiarezza e reso privato
        private void SetNewRandomRoamTargetInternal(bool flee = false, Vector2? fleeFromPosition = null)
        {
            if (arenaManager == null)
            {
                _targetPosition = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * 5f;
                Debug.LogError($"[{gameObject.name}] ArenaManager nullo in SetNewRandomRoamTargetInternal, uso fallback target.");
                return;
            }

            if (flee && fleeFromPosition.HasValue)
            {
                Vector2 directionAway = ((Vector2)transform.position - fleeFromPosition.Value).normalized;
                if (directionAway == Vector2.zero)
                {
                    directionAway = UnityEngine.Random.insideUnitCircle.normalized;
                    if(directionAway == Vector2.zero) directionAway = Vector2.right; // Fallback estremo
                }

                float fleeDist = arenaManager.arenaSize.magnitude * scoutFleeDistanceFactor * Random.Range(0.7f, 1.3f) ; // Distanza di fuga basata sulla diagonale dell'arena
                _targetPosition = (Vector2)transform.position + directionAway * fleeDist;
                _targetPosition = arenaManager.ClampPositionToBounds(_targetPosition);
                // Debug.Log($"[{gameObject.name}, {personality}] Sta fuggendo da {fleeFromPosition.Value}! Nuovo target di fuga: {_targetPosition}");
            }
            else
            {
                float randomX = UnityEngine.Random.Range(arenaManager.arenaCenter.x - arenaManager.arenaSize.x / 2, arenaManager.arenaCenter.x + arenaManager.arenaSize.x / 2);
                float randomY = UnityEngine.Random.Range(arenaManager.arenaCenter.y - arenaManager.arenaSize.y / 2, arenaManager.arenaCenter.y + arenaManager.arenaSize.y / 2);
                _targetPosition = new Vector2(randomX, randomY);
            }
            // Debug.Log($"[{gameObject.name}, {personality}] Nuovo target: {_targetPosition} (Flee: {flee})");
        }

        void ExecuteChasingPlayer()
        {
            if (_playerTransform != null)
            {
                _targetPosition = _playerTransform.position; // Aggiorna il target all'ultima posizione nota del giocatore
                MoveTowards(_targetPosition);
            }
            else // Giocatore perso/distrutto
            {
                if (currentState != BotState.Roaming) // Evita log e cambio di target se già in roaming
                {
                    Debug.Log($"[{gameObject.name}, {personality}] Giocatore perso durante Chasing. Passo a Roaming.");
                    currentState = BotState.Roaming;
                    SetNewRandomRoamTarget();
                }
            }
        }

        void MoveTowards(Vector2 targetPosition)
        {
            if (_planetController == null) return;
            Vector2 direction = (targetPosition - (Vector2)transform.position);
            _planetController.SetMovementInput(direction);
        }

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return; // Non disegnare se non in play mode o se _targetPosition non è valido

            switch(currentState)
            {
                case BotState.Roaming:
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, _targetPosition);
                    Gizmos.DrawWireSphere(_targetPosition, roamChangeTargetDistance);
                    break;
                case BotState.ChasingPlayer:
                     if (_playerTransform != null)
                     {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(transform.position, _playerTransform.position);
                     }
                    break;
                case BotState.Fleeing:
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, _targetPosition);
                    Gizmos.DrawWireSphere(_targetPosition, roamChangeTargetDistance * 1.5f);
                    if (_playerTransform != null) Gizmos.DrawLine(transform.position, _playerTransform.position); // Linea al player da cui fugge
                    break;
            }

            // Raggi di percezione
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chasePlayerDetectionRadius);

            float actualLoseRadius = losePlayerChaseRadius;
            if (personality == BotPersonality.Tank && currentState == BotState.ChasingPlayer) // Mostra raggio di persistenza tank
            {
                actualLoseRadius *= tankChasePersistenceFactor;
                Gizmos.color = new Color(0.5f, 0.5f, 0.1f); // Giallo scuro per persistenza tank
            } else {
                 Gizmos.color = Color.gray;
            }
            Gizmos.DrawWireSphere(transform.position, actualLoseRadius);


            // Raggio di fuga per Scout (se personalità Scout e player rilevato)
            if (personality == BotPersonality.Scout && _playerTransform != null && _playerPlanetControllerCache != null)
            {
                if (_playerPlanetControllerCache.CurrentMass > _planetController.CurrentMass * scoutFleeMassRatioThreshold || _playerPlanetControllerCache.IsShieldActive())
                {
                     Gizmos.color = new Color(0f, 0.8f, 1f); // Ciano per indicare potenziale fuga
                     Gizmos.DrawWireSphere(transform.position, chasePlayerDetectionRadius); // Il raggio di detection è quello che triggera il check di fuga
                }
            }
        }
    }
}
