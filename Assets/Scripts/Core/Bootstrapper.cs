using UnityEngine;
using UnityEngine.SceneManagement;
using ChaosCosmos.Core.Services; // For ServiceLocator
using ChaosCosmos.Services.Analytics; // For IAnalyticsService and AnalyticsService
using ChaosCosmos.Services.Progression; // For IProgressService and ProgressManager

namespace ChaosCosmos.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        // Nome della scena da caricare dopo il boot
        public string nextSceneName = "LobbyScene"; // Default, può essere cambiato nell'Inspector

        void Start()
        {
            InitializeServices();
            LoadNextScene();
        }

        void InitializeServices()
        {
            // Registra AnalyticsService
            if (!ServiceLocator.IsRegistered<IAnalyticsService>())
            {
                ServiceLocator.Register<IAnalyticsService>(new AnalyticsService());
                Debug.Log("Bootstrapper: AnalyticsService registrato.");
            }

            // Registra ProgressManager
            if (!ServiceLocator.IsRegistered<IProgressService>())
            {
                ServiceLocator.Register<IProgressService>(new ProgressManager());
                Debug.Log("Bootstrapper: ProgressManager (IProgressService) registrato.");
            }
            // Altri servizi possono essere registrati qui...

            Debug.Log("Bootstrapper: Tutti i servizi principali sono stati inizializzati (o verificati).");

            // Impostazioni globali di base
            Application.targetFrameRate = 60;
            // Screen.sleepTimeout = SleepTimeout.NeverSleep; // Esempio
        }

        void LoadNextScene()
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError("Bootstrapper: nextSceneName non è impostato!");
                return;
            }
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
