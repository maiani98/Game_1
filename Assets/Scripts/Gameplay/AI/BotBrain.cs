using UnityEngine;
using ChaosCosmos.Core.Constants;

namespace ChaosCosmos.Gameplay.AI
{
    [RequireComponent(typeof(PlanetController))]
    public class BotBrain : MonoBehaviour
    {
        public enum BotState { Roaming, ChasingPlayer }

        [Header("AI Settings")]
        public BotState currentState = BotState.Roaming;
        public float roamChangeTargetDistance = 1f;
        public float chasePlayerDetectionRadius = 10f;
        public float losePlayerChaseRadius = 15f;
        public float stateChangeIntervalMin = 3f;
        public float stateChangeIntervalMax = 7f;

        [Header("References")]
        public ArenaManager arenaManager;

        private PlanetController _planetController; // Prefixed
        private Transform _playerTransform; // Prefixed
        private Vector2 _roamTargetPosition; // Prefixed
        private float _timeForNextStateChange; // Prefixed

        void Awake()
        {
            _planetController = GetComponent<PlanetController>();
        }

        void Start()
        {
            if (arenaManager == null)
            {
                arenaManager = FindObjectOfType<ArenaManager>();
                if (arenaManager == null)
                {
                    Debug.LogError("BotBrain: ArenaManager non trovato nella scena!");
                    enabled = false;
                    return;
                }
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag(GameTags.PLAYER_TAG);
            if (playerObject != null)
            {
                _playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("BotBrain: Giocatore (tag '" + GameTags.PLAYER_TAG + "') non trovato. Il bot non potrà inseguire.");
            }

            SetNewRandomRoamTarget();
            ScheduleNextStateChange();
        }

        void Update()
        {
            if (Time.time >= _timeForNextStateChange) // Used prefixed
            {
                DecideNextState();
                ScheduleNextStateChange();
            }

            switch (currentState)
            {
                case BotState.Roaming:
                    ExecuteRoaming();
                    break;
                case BotState.ChasingPlayer:
                    ExecuteChasingPlayer();
                    break;
            }
        }

        void ScheduleNextStateChange()
        {
            _timeForNextStateChange = Time.time + Random.Range(stateChangeIntervalMin, stateChangeIntervalMax);
        }

        void DecideNextState()
        {
            if (_playerTransform != null) // Used prefixed
            {
                float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position); // Used prefixed
                if (currentState == BotState.Roaming && distanceToPlayer < chasePlayerDetectionRadius)
                {
                    currentState = BotState.ChasingPlayer;
                    Debug.Log($"{gameObject.name} sta inseguendo il giocatore!");
                    return;
                }
                if (currentState == BotState.ChasingPlayer && distanceToPlayer > losePlayerChaseRadius)
                {
                    currentState = BotState.Roaming;
                    SetNewRandomRoamTarget();
                    Debug.Log($"{gameObject.name} ha perso il giocatore, torna a vagare.");
                    return;
                }
            }
            else // Se _playerTransform è diventato null (es. giocatore distrutto)
            {
                if (currentState == BotState.ChasingPlayer)
                {
                    currentState = BotState.Roaming;
                    SetNewRandomRoamTarget();
                    Debug.Log($"{gameObject.name} ha perso il target del giocatore (distrutto?), torna a vagare.");
                }
            }
        }

        void ExecuteRoaming()
        {
            if (Vector2.Distance(transform.position, _roamTargetPosition) < roamChangeTargetDistance) // Used prefixed
            {
                SetNewRandomRoamTarget();
            }
            MoveTowards(_roamTargetPosition); // Used prefixed
        }

        void SetNewRandomRoamTarget()
        {
            if (arenaManager != null)
            {
                float randomX = Random.Range(arenaManager.arenaCenter.x - arenaManager.arenaSize.x / 2, arenaManager.arenaCenter.x + arenaManager.arenaSize.x / 2);
                float randomY = Random.Range(arenaManager.arenaCenter.y - arenaManager.arenaSize.y / 2, arenaManager.arenaCenter.x + arenaManager.arenaSize.y / 2); // Typo: arenaCenter.x -> arenaManager.arenaCenter.y
                _roamTargetPosition = new Vector2(randomX, randomY); // Used prefixed
            }
            else
            {
                _roamTargetPosition = (Vector2)transform.position + Random.insideUnitCircle * 5f; // Used prefixed
            }
             Debug.Log($"{gameObject.name} nuovo target di roam: {_roamTargetPosition}"); // Used prefixed
        }

        void ExecuteChasingPlayer()
        {
            if (_playerTransform != null) // Used prefixed
            {
                MoveTowards(_playerTransform.position); // Used prefixed
            }
            else
            {
                currentState = BotState.Roaming;
                SetNewRandomRoamTarget();
            }
        }

        void MoveTowards(Vector2 targetPosition)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position);
            if (_planetController != null) // Check if _planetController is set
            {
                _planetController.SetMovementInput(direction); // Used prefixed
            }
        }

        void OnDrawGizmosSelected()
        {
            if (currentState == BotState.Roaming)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _roamTargetPosition); // Used prefixed
                Gizmos.DrawWireSphere(_roamTargetPosition, 0.5f); // Used prefixed
            }
            else if (currentState == BotState.ChasingPlayer && _playerTransform != null) // Used prefixed
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _playerTransform.position); // Used prefixed
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chasePlayerDetectionRadius);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, losePlayerChaseRadius);
        }
    }
}
