using UnityEngine;
using System.Collections;

namespace ChaosCosmos.Gameplay.AI
{
    [RequireComponent(typeof(PlanetController))]
    public class BotBrain : MonoBehaviour
    {
        public enum BotState { Roaming, ChasingPlayer }

        [Header("AI Settings")]
        public BotState currentState = BotState.Roaming;
        public float roamChangeTargetDistance = 1f; // Quanto vicino deve arrivare al target di roam prima di cambiarlo
        public float chasePlayerDetectionRadius = 10f;
        public float losePlayerChaseRadius = 15f; // Se il giocatore esce da questo raggio, smette di inseguire
        public float stateChangeIntervalMin = 3f; // Minimo tempo in uno stato prima di riconsiderare
        public float stateChangeIntervalMax = 7f;

        [Header("References")]
        public ArenaManager arenaManager; // Assegnare per i confini del roam

        private PlanetController planetController;
        private Transform playerTransform;
        private Vector2 roamTargetPosition;
        private float timeForNextStateChange;

        void Awake()
        {
            planetController = GetComponent<PlanetController>();
        }

        void Start()
        {
            // Trova l'ArenaManager se non assegnato (potrebbe essere rischioso se ce ne sono multipli)
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

            // Trova il giocatore (assumendo che sia taggato "Player")
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("BotBrain: Giocatore (tag 'Player') non trovato. Il bot non potrà inseguire.");
            }

            SetNewRandomRoamTarget();
            ScheduleNextStateChange();
        }

        void Update()
        {
            if (Time.time >= timeForNextStateChange)
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
            timeForNextStateChange = Time.time + Random.Range(stateChangeIntervalMin, stateChangeIntervalMax);
        }

        void DecideNextState()
        {
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                if (currentState == BotState.Roaming && distanceToPlayer < chasePlayerDetectionRadius)
                {
                    currentState = BotState.ChasingPlayer;
                    Debug.Log($"{gameObject.name} sta inseguendo il giocatore!");
                    return;
                }
                if (currentState == BotState.ChasingPlayer && distanceToPlayer > losePlayerChaseRadius)
                {
                    currentState = BotState.Roaming;
                    SetNewRandomRoamTarget(); // Imposta un nuovo target di roam quando smette di inseguire
                    Debug.Log($"{gameObject.name} ha perso il giocatore, torna a vagare.");
                    return;
                }
            }
            // Se nessuna condizione di cambio stato è soddisfatta, potrebbe rimanere nello stato attuale o alternare.
            // Per semplicità, se è in Chasing e il giocatore è ancora nel raggio, continua.
            // Se è in Roaming e il giocatore non è rilevato, continua.
        }

        void ExecuteRoaming()
        {
            if (Vector2.Distance(transform.position, roamTargetPosition) < roamChangeTargetDistance)
            {
                SetNewRandomRoamTarget();
            }
            MoveTowards(roamTargetPosition);
        }

        void SetNewRandomRoamTarget()
        {
            if (arenaManager != null)
            {
                float randomX = Random.Range(arenaManager.arenaCenter.x - arenaManager.arenaSize.x / 2, arenaManager.arenaCenter.x + arenaManager.arenaSize.x / 2);
                float randomY = Random.Range(arenaManager.arenaCenter.y - arenaManager.arenaSize.y / 2, arenaManager.arenaCenter.y + arenaManager.arenaSize.y / 2);
                roamTargetPosition = new Vector2(randomX, randomY);
            }
            else // Fallback se arenaManager non è disponibile
            {
                roamTargetPosition = (Vector2)transform.position + Random.insideUnitCircle * 5f;
            }
             Debug.Log($"{gameObject.name} nuovo target di roam: {roamTargetPosition}");
        }

        void ExecuteChasingPlayer()
        {
            if (playerTransform != null)
            {
                MoveTowards(playerTransform.position);
            }
            else
            {
                // Se il giocatore non c'è (es. è stato distrutto), torna a vagare
                currentState = BotState.Roaming;
                SetNewRandomRoamTarget();
            }
        }

        void MoveTowards(Vector2 targetPosition)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position);
            if (direction.sqrMagnitude > 0.01f)
            {
                Rigidbody2D botRb = GetComponent<Rigidbody2D>();
                if (botRb != null)
                {
                    botRb.velocity = direction.normalized * planetController.baseSpeed * GetSpeedModifierFromPlanetController();
                }
            }
            else
            {
                Rigidbody2D botRb = GetComponent<Rigidbody2D>();
                if (botRb != null) botRb.velocity = Vector2.zero;
            }
        }

        float GetSpeedModifierFromPlanetController()
        {
            // Questo è un placeholder. L'utente deve modificare PlanetController.cs per esporre
            // currentMass o SpeedModifier() per una corretta integrazione.
            // Vedi istruzioni nel report del task.
            // Per ora, restituisce 1.0f per permettere la compilazione.
            // Se PlanetController.cs fosse aggiornato con:
            // public float GetCurrentSpeedModifier() { return SpeedModifier(); }
            // Allora qui si potrebbe usare:
            // return planetController.GetCurrentSpeedModifier();
            return 1.0f;
        }

        void OnDrawGizmosSelected()
        {
            if (currentState == BotState.Roaming)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, roamTargetPosition);
                Gizmos.DrawWireSphere(roamTargetPosition, 0.5f);
            }
            else if (currentState == BotState.ChasingPlayer && playerTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chasePlayerDetectionRadius);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, losePlayerChaseRadius);
        }
    }
}
