using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using ChaosCosmos.Services.Configuration;
using ChaosCosmos.Gameplay.Progression;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Gameplay.LiveOps;
using System.IO;
using System.Linq;
using UnityEngine.TestTools; // Per LogAssert

namespace ChaosCosmos.Tests.EditMode.Services.Configuration
{
    public class ConfigDataServiceTests
    {
        private ConfigDataService _configDataService;

        // Usiamo path relativi a "Assets/Resources/" per i test hook
        private const string TestUpgradesSubPath = "TestConfig/Upgrades";
        private const string TestPassSeasonsSubPath = "TestConfig/PassSeasons";
        private const string TestEventsSubPath = "TestConfig/Events";

        private string GetFullResourcesTestPath(string subPath) => Path.Combine(Application.dataPath, "Resources", subPath);

        [OneTimeSetUp]
        public void OneTimeSetUpParentDirs()
        {
            // Crea le cartelle base in Assets/Resources se non esistono, una sola volta per tutti i test
            Directory.CreateDirectory(GetFullResourcesTestPath("TestConfig"));
        }

        [SetUp]
        public void SetUp()
        {
            // Imposta i path di override per ConfigDataService PRIMA di creare l'istanza
            ConfigDataService.UpgradesPathOverride = TestUpgradesSubPath;
            ConfigDataService.PassSeasonsPathOverride = TestPassSeasonsSubPath;
            ConfigDataService.EventsPathOverride = TestEventsSubPath;

            _configDataService = new ConfigDataService();

            // Crea le cartelle specifiche per ogni test (o assicurati che siano pulite)
            CleanUpTestAssetFolders(); // Pulisce prima di creare nuove cartelle di test
            Directory.CreateDirectory(GetFullResourcesTestPath(TestUpgradesSubPath));
            Directory.CreateDirectory(GetFullResourcesTestPath(TestPassSeasonsSubPath));
            Directory.CreateDirectory(GetFullResourcesTestPath(TestEventsSubPath));
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            CleanUpTestAssetFolders();
            // Ripristina i path di override ai default (o null)
            ConfigDataService.UpgradesPathOverride = null;
            ConfigDataService.PassSeasonsPathOverride = null;
            ConfigDataService.EventsPathOverride = null;
        }

        [OneTimeTearDown]
        public void OneTimeTearDownParentDirs()
        {
             // Cancella la cartella radice "TestConfig" dentro Assets/Resources
            string testConfigRootResources = GetFullResourcesTestPath("TestConfig");
            if (Directory.Exists(testConfigRootResources))
            {
                Directory.Delete(testConfigRootResources, true);
                // Rimuovi il file .meta della cartella TestConfig se esiste
                if(File.Exists(testConfigRootResources + ".meta")) File.Delete(testConfigRootResources + ".meta");
                AssetDatabase.Refresh();
            }
        }


        private void CleanUpTestAssetFolders()
        {
            string[] pathsToClean = {
                GetFullResourcesTestPath(TestUpgradesSubPath),
                GetFullResourcesTestPath(TestPassSeasonsSubPath),
                GetFullResourcesTestPath(TestEventsSubPath)
            };

            foreach (var path in pathsToClean)
            {
                if (Directory.Exists(path))
                {
                    // Cancella i file .meta prima delle cartelle
                    string[] metaFiles = Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories);
                    foreach(string metaFile in metaFiles) File.Delete(metaFile);
                    Directory.Delete(path, true);
                }
            }
            AssetDatabase.Refresh();
        }

        private T CreateScriptableObjectAsset<T>(string resourcesSubPath, string assetName) where T : ScriptableObject
        {
            string fullDir = GetFullResourcesTestPath(resourcesSubPath);
            Directory.CreateDirectory(fullDir); // Assicura che la dir esista

            T instance = ScriptableObject.CreateInstance<T>();
            // Path relativo ad Assets/ per CreateAsset
            string assetPath = Path.Combine("Assets/Resources", resourcesSubPath, assetName + ".asset").Replace("\\", "/");

            AssetDatabase.CreateAsset(instance, assetPath);
            EditorUtility.SetDirty(instance); // Marca come modificato per il salvataggio
            return instance;
        }


        [Test]
        public void Initialize_LoadsUpgradeDataFromResources()
        {
            var ug1 = CreateScriptableObjectAsset<UpgradeData>(TestUpgradesSubPath, "UG01_Test");
            ug1.upgradeID = "ug01"; ug1.upgradeName = "Test Upgrade 1";
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            _configDataService.Initialize();

            Assert.IsTrue(_configDataService.IsInitialized);
            var loadedUpgrade = _configDataService.GetUpgradeData("ug01");
            Assert.IsNotNull(loadedUpgrade);
            Assert.AreEqual("Test Upgrade 1", loadedUpgrade.upgradeName);
            Assert.AreEqual(1, _configDataService.GetAllUpgradeData().Count());
        }

        [Test]
        public void Initialize_HandlesDuplicateUpgradeIDsGracefully_KeepsFirstLoaded()
        {
            var ug1 = CreateScriptableObjectAsset<UpgradeData>(TestUpgradesSubPath, "UG_Dup_1");
            ug1.upgradeID = "dup01"; ug1.upgradeName = "First Dup";
            EditorUtility.SetDirty(ug1);

            // Per controllare quale viene caricato per primo, dobbiamo forzare l'ordine di caricamento di Resources.LoadAll
            // Questo non è direttamente controllabile in modo semplice.
            // Il test verificherà che uno sia caricato e che un warning sia loggato.
            var ug2 = CreateScriptableObjectAsset<UpgradeData>(TestUpgradesSubPath, "UG_Dup_2");
            ug2.upgradeID = "dup01"; ug2.upgradeName = "Second Dup"; // Stesso ID
            EditorUtility.SetDirty(ug2);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Trovato UpgradeID duplicato 'dup01'.*"));
            _configDataService.Initialize();

            var loaded = _configDataService.GetUpgradeData("dup01");
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.upgradeName == "First Dup" || loaded.upgradeName == "Second Dup");
            Assert.AreEqual(1, _configDataService.GetAllUpgradeData().Count());
        }

        [Test]
        public void GetMethods_ReturnNullOrEmpty_IfNotInitializedOrKeyNotFound()
        {
            Assert.IsFalse(_configDataService.IsInitialized); // Non ancora inizializzato
            Assert.IsNull(_configDataService.GetUpgradeData("anyID"));
            Assert.IsEmpty(_configDataService.GetAllUpgradeData());
            Assert.IsNull(_configDataService.GetPassSeasonData("anySeason"));
            Assert.IsEmpty(_configDataService.GetAllPassSeasonData());
            Assert.IsNull(_configDataService.GetEventData("anyEvent"));
            Assert.IsEmpty(_configDataService.GetAllEventData());

            _configDataService.Initialize(); // Inizializza con cache vuote (nessun asset creato)
            Assert.IsTrue(_configDataService.IsInitialized);
            Assert.IsNull(_configDataService.GetUpgradeData("anyID"));
            Assert.IsEmpty(_configDataService.GetAllUpgradeData());
        }

        [Test]
        public void Initialize_LoadsPassSeasonDataFromResources()
        {
            var season1 = CreateScriptableObjectAsset<PassSeasonData>(TestPassSeasonsSubPath, "S01_Test");
            season1.seasonID = "s01"; season1.seasonName = "Test Season 1";
            EditorUtility.SetDirty(season1);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            _configDataService.Initialize();

            var loadedSeason = _configDataService.GetPassSeasonData("s01");
            Assert.IsNotNull(loadedSeason);
            Assert.AreEqual("Test Season 1", loadedSeason.seasonName);
            Assert.AreEqual(1, _configDataService.GetAllPassSeasonData().Count());
        }

        [Test]
        public void Initialize_LoadsEventDataFromResources()
        {
            var event1 = CreateScriptableObjectAsset<EventData>(TestEventsSubPath, "EV01_Test");
            event1.eventID = "ev01"; event1.eventName = "Test Event 1";
            EditorUtility.SetDirty(event1);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            _configDataService.Initialize();

            var loadedEvent = _configDataService.GetEventData("ev01");
            Assert.IsNotNull(loadedEvent);
            Assert.AreEqual("Test Event 1", loadedEvent.eventName);
            Assert.AreEqual(1, _configDataService.GetAllEventData().Count());
        }
    }
}
