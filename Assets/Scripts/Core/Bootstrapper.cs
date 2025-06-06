using UnityEngine;
using UnityEngine.SceneManagement;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.Analytics;
using ChaosCosmos.Services.Progression;
using ChaosCosmos.Services.PassSystem;
using ChaosCosmos.Gameplay.PassSystem;
using ChaosCosmos.Services.IAP;
using ChaosCosmos.Services.Ads;
using ChaosCosmos.Services.RemoteConfig;
using ChaosCosmos.Services.Consent;
using ChaosCosmos.Services.Configuration;
using ChaosCosmos.Services.LiveOps;
using ChaosCosmos.Gameplay.LiveOps;
using ChaosCosmos.Services.Notifications;
using ChaosCosmos.Services.GameManagement;
using ChaosCosmos.Services.Tutorial; // Aggiunto using per Tutorial
using ChaosCosmos.Core.Constants;
using System;

namespace ChaosCosmos.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        public string nextSceneName = SceneNames.LOBBY_SCENE; // Ora MAIN_MENU_SCENE

        void Start()
        {
            // Aggiorna nextSceneName se LOBBY_SCENE è stato rinominato in MAIN_MENU_SCENE
            if (nextSceneName == "LobbyScene" && SceneNames.LOBBY_SCENE != SceneNames.MAIN_MENU_SCENE)
            {
                nextSceneName = SceneNames.MAIN_MENU_SCENE;
            }
            InitializeServices();
        }

        void InitializeServices()
        {
            // TODO_MUSIC: Play_Menu_Music_Loop (se questa è la primissima scena e si vuole musica subito.
            //             Altrimenti, MainMenuManager.Start() è un posto migliore se BootScene è silenziosa).
            //             Decisione: MainMenuManager è più appropriato per la musica del menu.
            Debug.Log("Bootstrapper: Inizio inizializzazione servizi...");
            Application.targetFrameRate = 60;

            IConsentService consentService = null;
            if (!ServiceLocator.IsRegistered<IConsentService>())
            {
                consentService = new ConsentManager();
                ServiceLocator.Register<IConsentService>(consentService);
                Debug.Log("Bootstrapper: ConsentManager registrato.");
            }
            else { consentService = ServiceLocator.Get<IConsentService>(); }

            consentService.RequestConsentFlow(() => {
                Debug.Log("Bootstrapper: Flusso di consenso completato. Procedo con servizi dipendenti.");
                InitializeCoreServicesPostConsent();
            });
        }

        private void InitializeCoreServicesPostConsent()
        {
            if (!ServiceLocator.IsRegistered<IConfigDataService>())
            {
                ConfigDataService configInstance = new ConfigDataService();
                configInstance.Initialize();
                ServiceLocator.Register<IConfigDataService>(configInstance);
                Debug.Log("Bootstrapper: ConfigDataService registrato e inizializzato.");
            }

            if (!ServiceLocator.IsRegistered<IRemoteConfigService>())
            {
                RemoteConfigService rcInstance = new RemoteConfigService();
                ServiceLocator.Register<IRemoteConfigService>(rcInstance);
                Debug.Log("Bootstrapper: RemoteConfigService registrato.");
                rcInstance.Initialize(rcSuccess => {
                    if(rcSuccess) Debug.Log("Bootstrapper: RemoteConfigService inizializzato.");
                    else Debug.LogError("Bootstrapper: Fallimento inizializzazione RemoteConfigService.");
                    InitializeGameplayServicesPostRemoteConfig(); // Spostato qui per assicurare che RC sia pronto
                });
            }
            else
            {
                IRemoteConfigService rc = ServiceLocator.Get<IRemoteConfigService>();
                if (rc != null && rc.IsReady) { InitializeGameplayServicesPostRemoteConfig(); }
                else if (rc != null) { Debug.LogWarning("Bootstrapper: RemoteConfigService registrato ma non ancora pronto, attendo il suo callback."); }
                else { Debug.LogError("Bootstrapper: RemoteConfigService non registrato, impossibile procedere."); }
            }
        }

        private void InitializeGameplayServicesPostRemoteConfig()
        {
            IConfigDataService configDataService = ServiceLocator.Get<IConfigDataService>();
            IRemoteConfigService rcService = ServiceLocator.Get<IRemoteConfigService>();

            if (configDataService == null || !configDataService.IsInitialized || rcService == null || !rcService.IsReady)
            {
                Debug.LogError("Bootstrapper: ConfigDataService o RemoteConfigService non pronti. Impossibile inizializzare alcuni servizi di gameplay.");
                // Non chiamare LoadNextScene qui, aspetta che tutti i rami di init finiscano.
                // LoadNextScene verrà chiamato alla fine di questa catena di inizializzazione.
            }

            if (!ServiceLocator.IsRegistered<IProgressService>())
            {
                ProgressManager progressInstance = new ProgressManager(configDataService, rcService);
                ServiceLocator.Register<IProgressService>(progressInstance);
                Debug.Log("Bootstrapper: ProgressManager registrato con dipendenze.");
                progressInstance.InitializeNewPlayerIfApplicable();
            }

            // Tutorial Service (dipende da PlayerPrefs, ma non strettamente da altri servizi per il suo init base)
            if (!ServiceLocator.IsRegistered<ITutorialService>())
            {
                TutorialManagerService tutorialInstance = new TutorialManagerService();
                ServiceLocator.Register<ITutorialService>(tutorialInstance);
                tutorialInstance.Initialize(() => { // Aggiunto callback vuoto per coerenza
                    Debug.Log("Bootstrapper: TutorialManagerService inizializzato.");
                });
            }


            // GameManagerService (dipende da IProgressService)
            IProgressService progressServ = ServiceLocator.Get<IProgressService>();
            if (progressServ != null && !ServiceLocator.IsRegistered<IGameManagerService>())
            {
                GameManagerService gmInstance = new GameManagerService(progressServ);
                ServiceLocator.Register<IGameManagerService>(gmInstance);
                Debug.Log("Bootstrapper: GameManagerService registrato.");
            }
            else if (progressServ == null) { Debug.LogError("Bootstrapper: IProgressService non disponibile per GameManagerService."); }


            if (!ServiceLocator.IsRegistered<IPassService>())
            {
                IProgressService tempProgressService = ServiceLocator.Get<IProgressService>();
                if (tempProgressService == null) { Debug.LogError("Bootstrapper: IProgressService non registrato prima di IPassService!"); }
                else
                {
                    PassManager passManagerInstance = new PassManager(tempProgressService);
                    ServiceLocator.Register<IPassService>(passManagerInstance);
                    Debug.Log("Bootstrapper: PassManager registrato.");
                    PassSeasonData currentSeasonData = Resources.Load<PassSeasonData>("GameData/PassSeasons/Season1_Default");
                    if (currentSeasonData == null && configDataService != null && configDataService.IsInitialized) currentSeasonData = configDataService.GetPassSeasonData("Season1_Default");
                    if (currentSeasonData != null) { passManagerInstance.Initialize(currentSeasonData); }
                    else { Debug.LogError("Bootstrapper: Impossibile caricare PassSeasonData 'Season1_Default'."); }
                }
            }

            if (rcService != null && rcService.IsReady && configDataService != null && configDataService.IsInitialized)
            {
                if (!ServiceLocator.IsRegistered<IEventSchedulerService>())
                {
                    EventSchedulerService eventInstance = new EventSchedulerService(rcService);
                    eventInstance.Initialize(configDataService.GetAllEventData());
                    ServiceLocator.Register<IEventSchedulerService>(eventInstance);
                    Debug.Log("Bootstrapper: EventSchedulerService registrato e inizializzato.");
                }
                if (!ServiceLocator.IsRegistered<ITrendInjectorService>())
                {
                    TrendInjectorService trendInstance = new TrendInjectorService(rcService);
                    trendInstance.Initialize();
                    ServiceLocator.Register<ITrendInjectorService>(trendInstance);
                    Debug.Log("Bootstrapper: TrendInjectorService registrato e inizializzato.");
                }
            } else { Debug.LogError("Bootstrapper: RemoteConfig o ConfigData non pronti per LiveOps Services."); }

            if (!ServiceLocator.IsRegistered<INotificationService>())
            {
                NotificationService notificationInstance = new NotificationService();
                ServiceLocator.Register<INotificationService>(notificationInstance);
                Debug.Log("Bootstrapper: NotificationService registrato.");
                notificationInstance.Initialize(initSuccess => {
                    if(initSuccess) Debug.Log("Bootstrapper: NotificationService inizializzato.");
                    else Debug.LogWarning("Bootstrapper: NotificationService init, ma supporto reale potrebbe mancare.");
                    #if DEBUG && (UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR)
                    if (initSuccess && notificationInstance.IsInitialized) {
                        Debug.Log("Bootstrapper: Schedulazione notifica di test DEBUG...");
                        DateTime fireTime = DateTime.Now.AddSeconds(15);
                        notificationInstance.ScheduleLocalNotification("Chaos Cosmos Test", "Questa è una notifica di prova!", "Test App", fireTime, smallIcon: "ic_stat_notify", largeIcon: "ic_launcher_foreground");
                    }
                    #endif
                    InitializeIAPAndAdsServicesAndLoadScene(); // Spostato qui per essere l'ultimo step prima di caricare scena
                });
            } else {
                 InitializeIAPAndAdsServicesAndLoadScene();
            }
        }

        private void InitializeIAPAndAdsServicesAndLoadScene()
        {
            if (!ServiceLocator.IsRegistered<IIAPService>())
            {
                IAPFacade iapInstance = new IAPFacade();
                ServiceLocator.Register<IIAPService>(iapInstance);
                Debug.Log("Bootstrapper: IAPFacade registrato.");
                iapInstance.Initialize((iapSuccess, message) => {
                    if(iapSuccess) Debug.Log($"Bootstrapper: IAPFacade inizializzato: {message}");
                    else Debug.LogError($"Bootstrapper: Fallimento IAPFacade init: {message}");
                    InitializeAdsServiceOnlyAndLoadScene();
                });
            }
            else { InitializeAdsServiceOnlyAndLoadScene(); }
        }

        private void InitializeAdsServiceOnlyAndLoadScene()
        {
            if (!ServiceLocator.IsRegistered<IAdsService>())
            {
                IIAPService iapService = ServiceLocator.Get<IIAPService>();
                if (iapService == null) { Debug.LogError("Bootstrapper: IIAPService non registrato prima di IAdsService!");}

                AdsFacade adsInstance = new AdsFacade(iapService);
                ServiceLocator.Register<IAdsService>(adsInstance);
                Debug.Log("Bootstrapper: AdsFacade registrato.");
                adsInstance.Initialize();
            }

            EnsureAnalyticsServiceRegistered();
            LoadNextScene();
        }

        private void EnsureAnalyticsServiceRegistered()
        {
            if (!ServiceLocator.IsRegistered<IAnalyticsService>())
            {
                // AnalyticsService potrebbe dipendere da IConsentService, che è già registrato
                // IConsentService consentServ = ServiceLocator.Get<IConsentService>();
                // AnalyticsService analyticsInstance = new AnalyticsService(consentServ); // Se il costruttore lo prendesse
                AnalyticsService analyticsInstance = new AnalyticsService();
                ServiceLocator.Register<IAnalyticsService>(analyticsInstance);
                Debug.Log("Bootstrapper: AnalyticsService registrato.");
            }
        }

        void LoadNextScene()
        {
            Debug.Log("Bootstrapper: Tutti i servizi dipendenti inizializzati o inizializzazione avviata. Caricamento prossima scena...");
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError("Bootstrapper: nextSceneName non è impostato!");
                return;
            }
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
