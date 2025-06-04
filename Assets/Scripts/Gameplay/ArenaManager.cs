using UnityEngine;
using System.Collections; // Per le Coroutine
using System.Collections.Generic; // Per le liste (opzionale per tracciare i collectible)

namespace ChaosCosmos.Gameplay
{
    public class ArenaManager : MonoBehaviour
    {
        [Header("Arena Settings")]
        public Vector2 arenaCenter = Vector2.zero;
        public Vector2 arenaSize = new Vector2(50f, 30f); // Larghezza, Altezza

        [Header("Collectible Spawn Settings")]
        public GameObject collectiblePrefab; // Da assegnare nell'Inspector
        public int maxCollectibles = 20;
        public float spawnInterval = 2f; // Intervallo tra uno spawn e l'altro
        public float initialSpawnDelay = 1f;
        public LayerMask spawnOverlapCheckLayerMask; // Layer per controllare che non si spawni sopra ostacoli/giocatore

        // Opzionale: per tenere traccia dei collectible spawnati
                private List<GameObject> spawnedCollectibles = new List<GameObject>();

        private Coroutine _spawnCoroutine;

        void Start()
        {
            if (collectiblePrefab == null)
            {
                Debug.LogError("ArenaManager: Collectible Prefab non assegnato!");
                enabled = false;
                return;
            }
            _spawnCoroutine = StartCoroutine(SpawnCollectiblesRoutine());
        }

        private IEnumerator SpawnCollectiblesRoutine()
        {
            yield return new WaitForSeconds(initialSpawnDelay);

            while (true)
            {
                // Opzionale: controlla il numero di collectible attivi prima di spawnare
                // int activeCollectibles = 0;
                // foreach(var item in spawnedCollectibles) { if(item != null) activeCollectibles++; }
                // spawnedCollectibles.RemoveAll(item => item == null); // Pulisce la lista
                // if (activeCollectibles < maxCollectibles)

                // Semplificato: per ora spawna senza contare quelli attivi (verranno distrutti dal player)
                // In una versione più avanzata, si terrebbe conto del numero corrente.
                // Per ora, limitiamo solo il numero di tentativi di spawn per ciclo per non bloccare tutto.
                if (CountActiveCollectibles() < maxCollectibles)
                {
                    SpawnCollectible();
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        void SpawnCollectible()
        {
            if (collectiblePrefab == null) return;

            float spawnX = Random.Range(arenaCenter.x - arenaSize.x / 2, arenaCenter.x + arenaSize.x / 2);
            float spawnY = Random.Range(arenaCenter.y - arenaSize.y / 2, arenaCenter.y + arenaSize.y / 2);
            Vector2 spawnPosition = new Vector2(spawnX, spawnY);

            // Semplice controllo di overlap (opzionale, ma buona pratica)
            // Richiede che i collectible e gli ostacoli siano su layer specifici.
            // Se non si usa spawnOverlapCheckLayerMask, questo controllo può essere omesso.
            if (spawnOverlapCheckLayerMask.value != 0) // Se la layermask è impostata
            {
                Collider2D overlap = Physics2D.OverlapCircle(spawnPosition, 1f, spawnOverlapCheckLayerMask); // Raggio di check
                if (overlap != null)
                {
                    Debug.LogWarning($"ArenaManager: Tentativo di spawn in posizione occupata a {spawnPosition}. Riprovo al prossimo ciclo.");
                    return; // Non spawna se c'è qualcosa
                }
            }

            GameObject newCollectible = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity, transform); // Figlio dell'ArenaManager
            spawnedCollectibles.Add(newCollectible); // Se si vuole tracciare
            Debug.Log($"ArenaManager: Collezionabile spawnato a {spawnPosition}");
        }

        private int CountActiveCollectibles()
        {
            // Rimuove i collectible che sono stati distrutti (ingeriti)
            spawnedCollectibles.RemoveAll(item => item == null);
            return spawnedCollectibles.Count;
        }


        // Metodi per la gestione dei confini
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

        // Disegna i gizmos nell'editor per visualizzare i confini
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(arenaCenter, arenaSize);
        }
    }
}
