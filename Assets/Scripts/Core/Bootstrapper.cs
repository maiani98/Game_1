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
using ChaosCosmos.Core.Constants;
using System;

namespace ChaosCosmos.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        public string nextSceneName = SceneNames.LOBBY_SCENE;

        void Start()
        {
            InitializeServices();
        }

        void InitializeServices()
        {
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
                // LoadNextScene() sarà chiamato alla fine di InitializeGameplayServicesPostRemoteConfig
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
                    InitializeGameplayServicesPostRemoteConfig();
                });
            }
            else
            {
                IRemoteConfigService rc = ServiceLocator.Get<IRemoteConfigService>();
                if (rc != null && rc.IsReady) { InitializeGameplayServicesPostRemoteConfig(); }
                else if (rc != null) { Debug.LogWarning("Bootstrapper: RemoteConfigService registrato ma non ancora pronto."); }
                else { Debug.LogError("Bootstrapper: RemoteConfigService non registrato."); }
            }
        }

        private void InitializeGameplayServicesPostRemoteConfig()
        {
            IConfigDataService configDataService = ServiceLocator.Get<IConfigDataService>();
            IRemoteConfigService rcService = ServiceLocator.Get<IRemoteConfigService>();

            if (configDataService == null || !configDataService.IsInitialized || rcService == null || !rcService.IsReady)
            {
                Debug.LogError("Bootstrapper: ConfigDataService o RemoteConfigService non pronti. Impossibile inizializzare i servizi di gameplay.");
                LoadNextScene(); // Carica comunque la scena per non bloccare il gioco, ma con errori.
                return;
            }

            if (!ServiceLocator.IsRegistered<IProgressService>())
            {
                ProgressManager progressInstance = new ProgressManager(configDataService, rcService);
                ServiceLocator.Register<IProgressService>(progressInstance);
                Debug.Log("Bootstrapper: ProgressManager registrato con dipendenze.");
                progressInstance.InitializeNewPlayerIfApplicable(); // Chiama il check per il bonus nuovo utente
            }

            if (!ServiceLocator.IsRegistered<IPassService>())
            {
                IProgressService progressService = ServiceLocator.Get<IProgressService>();
                if (progressService == null) { Debug.LogError("Bootstrapper: IProgressService non registrato prima di IPassService!"); }
                else
                {
                    PassManager passManagerInstance = new PassManager(progressService);
                    ServiceLocator.Register<IPassService>(passManagerInstance);
                    Debug.Log("Bootstrapper: PassManager registrato.");
                    PassSeasonData currentSeasonData = Resources.Load<PassSeasonData>("GameData/PassSeasons/Season1_Default");
                    if (currentSeasonData == null) currentSeasonData = configDataService.GetPassSeasonData("Season1_Default");
                    if (currentSeasonData != null) { passManagerInstance.Initialize(currentSeasonData); }
                    else { Debug.LogError("Bootstrapper: Impossibile caricare PassSeasonData 'Season1_Default'."); }
                }
            }

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

            if (!ServiceLocator.IsRegistered<IIAPService>())
            {
                IAPFacade iapInstance = new IAPFacade();
                ServiceLocator.Register<IIAPService>(iapInstance);
                Debug.Log("Bootstrapper: IAPFacade registrato.");
                iapInstance.Initialize((iapSuccess, message) => {
                    if(iapSuccess) Debug.Log($"Bootstrapper: IAPFacade inizializzato: {message}");
                    else Debug.LogError($"Bootstrapper: Fallimento IAPFacade init: {message}");
                    InitializeAdsServiceOnly();
                });
            }
            else { InitializeAdsServiceOnly(); }

            EnsureAnalyticsServiceRegistered(); // Assicura che Analytics sia pronto
            LoadNextScene();
        }

        private void InitializeAdsServiceOnly()
        {
            if (!ServiceLocator.IsRegistered<IAdsService>())
            {
                IIAPService iapService = ServiceLocator.Get<IIAPService>();
                if (iapService == null) { Debug.LogError("Bootstrapper: IIAPService non registrato prima di IAdsService!"); return; }
                if (!iapService.IsInitialized) { Debug.LogWarning("Bootstrapper: IIAPService non ancora inizializzato prima di IAdsService.");}

                AdsFacade adsInstance = new AdsFacade(iapService);
                ServiceLocator.Register<IAdsService>(adsInstance);
                Debug.Log("Bootstrapper: AdsFacade registrato.");
                adsInstance.Initialize();
            }
        }

        private void EnsureAnalyticsServiceRegistered()
        {
            if (!ServiceLocator.IsRegistered<IAnalyticsService>())
            {
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
