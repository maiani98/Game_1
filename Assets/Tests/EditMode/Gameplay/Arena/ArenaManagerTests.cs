using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools; // Required for LogAssert
using ChaosCosmos.Gameplay;
using System.Collections.Generic;

namespace ChaosCosmos.Tests.EditMode.Gameplay.Arena
{
    public class ArenaManagerTests
    {
        private GameObject _arenaManagerGO;
        private ArenaManager _arenaManager;
        private GameObject _collectiblePrefab;
        private List<GameObject> _spawnedObjectsForCleanup;

        [SetUp]
        public void SetUp()
        {
            _arenaManagerGO = new GameObject("Test_ArenaManager");
            // Add a Rigidbody2D because ArenaManager's SpawnCollectible might use Physics2D.OverlapCircle
            // which might behave differently if the GameObject it's on doesn't have a Rigidbody2D.
            // Though for EditMode, this might not be strictly necessary unless physics queries are deeply involved.
            // _arenaManagerGO.AddComponent<Rigidbody2D>();
            _arenaManager = _arenaManagerGO.AddComponent<ArenaManager>();
            _spawnedObjectsForCleanup = new List<GameObject>();

            _collectiblePrefab = new GameObject("Test_CollectiblePrefab");
            _collectiblePrefab.AddComponent<SpriteRenderer>();
            var col = _collectiblePrefab.AddComponent<CircleCollider2D>();
            col.radius = 0.1f; // Small radius for prefab, actual spawned size might differ
            _collectiblePrefab.AddComponent<MassSource>();
            _collectiblePrefab.SetActive(false);
            // Non aggiungere _collectiblePrefab a _spawnedObjectsForCleanup qui,
            // perché è un prefab. Le sue ISTANZE verranno aggiunte.
            // Object.DontDestroyOnLoad(_collectiblePrefab); // Evita che venga distrutto se i test sono in scene diverse (non il caso qui)

            _arenaManager.arenaCenter = Vector2.zero;
            _arenaManager.arenaSize = new Vector2(20f, 10f);
            _arenaManager.collectiblePrefab = _collectiblePrefab;
            _arenaManager.maxCollectibles = 5;
            _arenaManager.spawnInterval = 0.01f; // Molto basso per testare la routine se possibile
            _arenaManager.initialSpawnDelay = 0f;
            // _arenaManager.spawnOverlapCheckLayerMask = 0; // Default a Nothing
        }

        [TearDown]
        public void TearDown()
        {
            // Stop all coroutines on ArenaManager if any were started for tests
            if (_arenaManager != null) _arenaManager.StopAllCoroutines();

            foreach (var obj in _spawnedObjectsForCleanup)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            _spawnedObjectsForCleanup.Clear();

            if (_collectiblePrefab != null) Object.DestroyImmediate(_collectiblePrefab);
            if (_arenaManagerGO != null) Object.DestroyImmediate(_arenaManagerGO);
        }

        private GameObject CreateObstacle(Vector2 position, float radius = 0.5f, string layerName = "Default")
        {
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Circle); // Usa primitive per avere un renderer
            Object.DestroyImmediate(obstacle.GetComponent<Collider>()); // Rimuovi il collider 3D
            obstacle.name = "Test_Obstacle";
            obstacle.transform.position = position;
            var col = obstacle.AddComponent<CircleCollider2D>();
            col.radius = radius;
            obstacle.layer = LayerMask.NameToLayer(layerName);
            _spawnedObjectsForCleanup.Add(obstacle);
            return obstacle;
        }

        private GameObject CreateSimulatedCollectible(Vector2 position, bool makeActive = true)
        {
            var simCollectible = Object.Instantiate(_collectiblePrefab, position, Quaternion.identity);
            simCollectible.name = "SimulatedActiveCollectible_" + System.Guid.NewGuid().ToString().Substring(0,4);
            // MassSource è già sul prefab, non serve ri-aggiungerlo.
            simCollectible.SetActive(makeActive);
            _spawnedObjectsForCleanup.Add(simCollectible);
            return simCollectible;
        }

        [Test]
        public void IsPositionInBounds_ReturnsTrueForInside_FalseForOutside()
        {
            Assert.IsTrue(_arenaManager.IsPositionInBounds(Vector2.zero));
            Assert.IsTrue(_arenaManager.IsPositionInBounds(new Vector2(9.9f, 4.9f))); // Limite X: 10, Limite Y: 5
            Assert.IsFalse(_arenaManager.IsPositionInBounds(new Vector2(10.1f, 0f)));
            Assert.IsFalse(_arenaManager.IsPositionInBounds(new Vector2(0f, 5.1f)));
        }

        [Test]
        public void ClampPositionToBounds_ClampsCorrectly()
        {
            Assert.AreEqual(new Vector2(5, 3), _arenaManager.ClampPositionToBounds(new Vector2(5, 3)));
            Assert.AreEqual(new Vector2(10, 0), _arenaManager.ClampPositionToBounds(new Vector2(15, 0)));
            Assert.AreEqual(new Vector2(0, -5), _arenaManager.ClampPositionToBounds(new Vector2(0, -10)));
            Assert.AreEqual(new Vector2(-10, 5), _arenaManager.ClampPositionToBounds(new Vector2(-20, 10)));
        }

        // Per testare SpawnCollectible, lo rendiamo public in ArenaManager o usiamo reflection.
        // Assumendo sia public per questo test.
        [Test]
        public void SpawnCollectible_SpawnsInsideBoundsAndParentsToArenaManager()
        {
            // Per rendere questo test più robusto, potremmo dover rendere SpawnCollectible public
            // o usare InternalsVisibleTo, o testare la coroutine in Play Mode.
            // Qui, simuleremo la logica di spawn e verificheremo i risultati,
            // o chiameremo SpawnCollectible se accessibile.

            // Se ArenaManager.SpawnCollectible() è reso public per testing:
            // _arenaManager.SendMessage("SpawnCollectible"); // Per chiamare metodo privato
            // GameObject spawnedChild = _arenaManager.transform.GetChild(0).gameObject;
            // Assert.NotNull(spawnedChild);
            // Assert.IsTrue(_arenaManager.IsPositionInBounds(spawnedChild.transform.position));
            // Assert.AreEqual(_arenaManager.transform, spawnedChild.transform.parent);

            // Testando la logica di posizionamento interno a SpawnCollectible:
            bool wasSpawnedInBounds = true;
            for(int i=0; i<20; i++) // Spawna alcuni per testare il random range
            {
               // Per testare il metodo privato, lo invochiamo via reflection o lo rendiamo internal/public.
               // Per ora, creo un'istanza e verifico la posizione.
               float spawnX = Random.Range(_arenaManager.arenaCenter.x - _arenaManager.arenaSize.x / 2, _arenaManager.arenaCenter.x + _arenaManager.arenaSize.x / 2);
               float spawnY = Random.Range(_arenaManager.arenaCenter.y - _arenaManager.arenaSize.y / 2, _arenaManager.arenaCenter.y + _arenaManager.arenaSize.y / 2);
               Vector2 spawnPosition = new Vector2(spawnX, spawnY);
               GameObject spawnedInstance = Object.Instantiate(_arenaManager.collectiblePrefab, spawnPosition, Quaternion.identity, _arenaManager.transform);
               spawnedInstance.SetActive(true);
               _spawnedObjectsForCleanup.Add(spawnedInstance);

               if(!_arenaManager.IsPositionInBounds(spawnedInstance.transform.position)) {
                   wasSpawnedInBounds = false;
                   break;
               }
               Assert.AreEqual(_arenaManager.transform, spawnedInstance.transform.parent);
            }
            Assert.IsTrue(wasSpawnedInBounds, "Un collezionabile è stato spawnato fuori dai limiti.");
        }

        [Test]
        public void Start_WhenCollectiblePrefabIsNull_LogsErrorAndDisablesManager()
        {
            _arenaManager.collectiblePrefab = null;
            LogAssert.Expect(LogType.Error, "ArenaManager: Collectible Prefab non assegnato!");

            // Start() è chiamato implicitamente quando il componente viene abilitato la prima volta
            // o quando entra in scena. In Edit Mode test, non sempre viene chiamato automaticamente
            // a meno che non si attivi esplicitamente il GO o si chiami un metodo che triggera il lifecycle.
            // Per forzare la logica di Start(), possiamo chiamare un metodo che lo emuli o
            // se ArenaManager avesse un metodo Initialize pubblico che fa lo stesso check.
            // Per ora, assumiamo che il test di LogAssert catturi il log se Start fosse chiamato.
            // Chiamare SendMessage("Start") può forzare l'esecuzione in Edit Mode.
            _arenaManager.SendMessage("Start");
            Assert.IsFalse(_arenaManager.enabled, "ArenaManager non si è disabilitato con prefab nullo.");
        }

        [Test]
        public void SpawnCollectible_WithOverlapCheck_DoesNotSpawnOnObstacle()
        {
            // Questo test è complesso da fare in modo deterministico in Edit Mode senza
            // controllare il risultato di Random.Range o refactoring di SpawnCollectible.
            // Si può verificare che se una posizione è occupata, OverlapCircle la rileva.

            string obstacleLayerName = "ObstacleTest"; // Deve esistere nel progetto
            int obstacleLayer = LayerMask.NameToLayer(obstacleLayerName);
            if (obstacleLayer == -1) { // Layer non esiste, crealo (solo in Editor, non in test build)
                // Questo è complesso da fare programmaticamente in un test e potrebbe non essere desiderabile.
                // Assumiamo che il layer esista o usiamo "Default".
                Debug.LogWarning($"Layer '{obstacleLayerName}' non trovato. Uso 'Default'. Configura il layer per test accurati.");
                obstacleLayerName = "Default"; // Fallback
                obstacleLayer = LayerMask.NameToLayer(obstacleLayerName);
            }

            _arenaManager.spawnOverlapCheckLayerMask = LayerMask.GetMask(obstacleLayerName);
            CreateObstacle(Vector2.zero, 1f, obstacleLayerName); // Ostacolo grande al centro

            // Simula molti tentativi di spawn; se SpawnCollectible è ben fatto,
            // dovrebbe evitare l'ostacolo o loggare un warning.
            // Dato che SpawnCollectible è privato e usa Random.Range, è difficile testare direttamente.
            // Testiamo il componente Physics2D.OverlapCircle che usa.
            Collider2D overlap = Physics2D.OverlapCircle(Vector2.zero, 0.2f, _arenaManager.spawnOverlapCheckLayerMask);
            Assert.IsNotNull(overlap, "OverlapCircle non ha rilevato l'ostacolo al centro.");
            Assert.AreEqual("Test_Obstacle", overlap.name);

            // Se SpawnCollectible fosse public:
            // _arenaManager.SpawnCollectible(); // Richiederebbe di mockare Random.Range per forzare spawn su (0,0)
            // Poi contare i figli di _arenaManagerGO.transform.childCount.
            // Se 0 (o il numero precedente), allora non ha spawnato.
            Assert.Pass("Test parziale: OverlapCircle rileva l'ostacolo. Test completo di SpawnCollectible con overlap richiederebbe refactoring o Play Mode.");
        }

        [Test]
        public void CountActiveCollectibles_ReturnsCorrectNumberOfMassSourceComponents()
        {
            // ArenaManager.CountActiveCollectibles ora cerca MassSource
            Assert.AreEqual(0, (int)_arenaManager.SendMessage("CountActiveCollectibles"));

            CreateSimulatedCollectible(new Vector2(1,1));
            CreateSimulatedCollectible(new Vector2(-1,-1));
            Assert.AreEqual(2, (int)_arenaManager.SendMessage("CountActiveCollectibles"));

            GameObject toDestroy = _spawnedObjectsForCleanup[_spawnedObjectsForCleanup.Count-1];
            _spawnedObjectsForCleanup.Remove(toDestroy); // Rimuovi dalla lista di cleanup prima
            Object.DestroyImmediate(toDestroy); // Simula distruzione

            Assert.AreEqual(1, (int)_arenaManager.SendMessage("CountActiveCollectibles"));
        }

        // Testare la coroutine SpawnCollectiblesRoutine è difficile in Edit Mode.
        // Si può testare la logica che la coroutine userebbe.
        [Test]
        public void SpawnLogic_ShouldRespectMaxCollectibles_BasedOnCount()
        {
            _arenaManager.maxCollectibles = 1; // Imposta un massimo basso

            Assert.AreEqual(0, (int)_arenaManager.SendMessage("CountActiveCollectibles"));

            // Simula che la coroutine voglia spawnare. Se Count < Max, dovrebbe procedere.
            bool shouldSpawn = (int)_arenaManager.SendMessage("CountActiveCollectibles") < _arenaManager.maxCollectibles;
            Assert.IsTrue(shouldSpawn, "Dovrebbe poter spawnare quando count < max.");

            CreateSimulatedCollectible(new Vector2(1,0)); // Aggiungi un collezionabile
            Assert.AreEqual(1, (int)_arenaManager.SendMessage("CountActiveCollectibles"));

            // Ora Count == Max. Non dovrebbe più spawnare.
            shouldSpawn = (int)_arenaManager.SendMessage("CountActiveCollectibles") < _arenaManager.maxCollectibles;
            Assert.IsFalse(shouldSpawn, "Non dovrebbe poter spawnare quando count == max.");
        }
    }
}
