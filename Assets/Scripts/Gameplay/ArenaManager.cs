using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ChaosCosmos.Gameplay
{
    public class ArenaManager : MonoBehaviour
    {
        [Header("Arena Settings")]
        public Vector2 arenaCenter = Vector2.zero;
        public Vector2 arenaSize = new Vector2(50f, 30f);

        [Header("Collectible Spawn Settings")]
        public GameObject collectiblePrefab;
        public int maxCollectibles = 20;
        public float spawnInterval = 2f;
        public float initialSpawnDelay = 1f;
        public LayerMask spawnOverlapCheckLayerMask;

        private List<GameObject> _spawnedCollectibles = new List<GameObject>(); // Prefixed
        private Coroutine _spawnCoroutine; // Prefixed

        void Start()
        {
            if (collectiblePrefab == null)
            {
                Debug.LogError("ArenaManager: Collectible Prefab non assegnato!");
                enabled = false;
                return;
            }
            // StartCoroutine è su MonoBehaviour, quindi this.StartCoroutine è implicito.
            _spawnCoroutine = StartCoroutine(SpawnCollectiblesRoutine());
        }

        private IEnumerator SpawnCollectiblesRoutine()
        {
            yield return new WaitForSeconds(initialSpawnDelay);

            while (true) // Parentesi graffa implicita per il while, ma meglio esplicita se ci fossero più statement, qui è ok.
            {
                if (CountActiveCollectibles() < maxCollectibles)
                {
                    SpawnCollectible();
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // Reso public per testabilità, altrimenti sarebbe private
        public void SpawnCollectible() // Modificato per test, potrebbe tornare private
        {
            if (collectiblePrefab == null)
            { // Aggiunte graffe
                return;
            }

            float spawnX = Random.Range(arenaCenter.x - arenaSize.x / 2, arenaCenter.x + arenaSize.x / 2);
            float spawnY = Random.Range(arenaCenter.y - arenaSize.y / 2, arenaCenter.y + arenaSize.y / 2);
            Vector2 spawnPosition = new Vector2(spawnX, spawnY);

            if (spawnOverlapCheckLayerMask.value != 0)
            {
                // Usare il raggio del collider del prefab se disponibile, altrimenti un valore fisso.
                float checkRadius = 0.5f; // Default
                CircleCollider2D prefabCollider = collectiblePrefab.GetComponent<CircleCollider2D>();
                if (prefabCollider != null)
                {
                    checkRadius = prefabCollider.radius;
                }

                Collider2D overlap = Physics2D.OverlapCircle(spawnPosition, checkRadius, spawnOverlapCheckLayerMask);
                if (overlap != null)
                {
                    Debug.LogWarning($"ArenaManager: Tentativo di spawn in posizione occupata a {spawnPosition} da {overlap.name}. Riprovo al prossimo ciclo.");
                    return;
                }
            }

            GameObject newCollectible = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity, transform);
            _spawnedCollectibles.Add(newCollectible);
            // Debug.Log($"ArenaManager: Collezionabile spawnato a {spawnPosition}"); // Log un po' verboso per ogni spawn
        }

        private int CountActiveCollectibles()
        {
            _spawnedCollectibles.RemoveAll(item => item == null);
            return _spawnedCollectibles.Count;
        }

        public bool IsPositionInBounds(Vector2 position)
        {
            Rect bounds = new Rect(
                arenaCenter.x - arenaSize.x / 2,
                arenaCenter.y - arenaSize.y / 2,
                arenaSize.x,
                arenaSize.y
            );
            return bounds.Contains(position);
        }

        public Vector2 ClampPositionToBounds(Vector2 position)
        {
            Rect bounds = new Rect(
                arenaCenter.x - arenaSize.x / 2,
                arenaCenter.y - arenaSize.y / 2,
                arenaSize.x,
                arenaSize.y
            );
            float clampedX = Mathf.Clamp(position.x, bounds.xMin, bounds.xMax);
            float clampedY = Mathf.Clamp(position.y, bounds.yMin, bounds.yMax);
            return new Vector2(clampedX, clampedY);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(arenaCenter, arenaSize);
        }
    }
}
