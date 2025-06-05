using UnityEngine;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.RemoteConfig;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Core.AssetManagement;
// using UnityEngine.ResourceManagement.AsyncOperations; // Rimosso

namespace ChaosCosmos.Services.LiveOps
{
    public class TrendInjectorService : ITrendInjectorService
    {
        public bool IsInitialized { get; private set; }
        private IRemoteConfigService _remoteConfigService;

        // Chiavi esempio per Remote Config (meglio se definite in GameConstants.RemoteConfigKeyPatterns)
        private const string RC_KEY_TREND_ARENA_THEME = "trend_arenaTheme";
        private const string RC_KEY_TREND_COLLECTIBLE_X_SPAWN_MULT = "trend_collectibleX_spawnMultiplier";


        public TrendInjectorService(IRemoteConfigService remoteConfigService)
        {
            _remoteConfigService = remoteConfigService;
             if (_remoteConfigService == null)
            {
                 Debug.LogWarning("TrendInjectorService: IRemoteConfigService non fornito durante la costruzione.");
            }
        }

        public void Initialize()
        {
            if (_remoteConfigService == null || !_remoteConfigService.IsReady)
            { // Aggiunte graffe
                Debug.LogError("TrendInjectorService: Impossibile inizializzare, IRemoteConfigService non pronto.");
                return;
            }

            ApplyCurrentTrends();
            IsInitialized = true;
            Debug.Log("TrendInjectorService: Inizializzato e trend applicati (simulato).");
        }

        private void ApplyCurrentTrends()
        {
            // Esempio: Leggi un tema per l'arena
            // La prima riga con RemoteConfigKeyPatterns.GetArenaMaxCollectiblesKey() è stata rimossa perché errata per il tema.
            string currentTheme = _remoteConfigService.GetString(RC_KEY_TREND_ARENA_THEME, "default_theme");
            Debug.Log($"[TrendInjector] Tema Arena Corrente (da RC '{RC_KEY_TREND_ARENA_THEME}'): {currentTheme}. (TODO: Applicare al sistema di theming visivo)");

            // Esempio: Leggi un moltiplicatore di spawn per un tipo di collezionabile "TypeX"
            float collectibleXMultiplier = _remoteConfigService.GetFloat(RC_KEY_TREND_COLLECTIBLE_X_SPAWN_MULT, 1.0f);
            Debug.Log($"[TrendInjector] Moltiplicatore Spawn Collectible 'X' (da RC '{RC_KEY_TREND_COLLECTIBLE_X_SPAWN_MULT}'): {collectibleXMultiplier}. (TODO: Passare ad ArenaManager)");

            // Esempio con una chiave da GameConstants
            int maxCollectibles = _remoteConfigService.GetInt(RemoteConfigKeyPatterns.GetArenaMaxCollectiblesKey(), 30);
            Debug.Log($"[TrendInjector] Max Collectibles (da RC '{RemoteConfigKeyPatterns.GetArenaMaxCollectiblesKey()}'): {maxCollectibles}. (TODO: Applicare ad ArenaManager)");

            // Esempio di caricamento asset tramite Addressables se il tema è "halloween_theme"
            if (currentTheme == "halloween_theme" && AddressableAssetLoader.Instance != null)
            {
                string spriteAddress = "halloween_collectible_sprite";
                AddressableAssetLoader.Instance.LoadAssetAsync<Sprite>(spriteAddress,
                (loadedSprite) => {
                    if (loadedSprite != null) // Aggiunte graffe
                    {
                        Debug.Log($"[TrendInjector] Sprite per Halloween caricato con successo: {loadedSprite.name} (da {spriteAddress})");
                    }
                },
                (errorMsg) => {
                     Debug.LogError($"[TrendInjector] Fallito caricamento sprite Halloween '{spriteAddress}': {errorMsg}");
                });

                string prefabAddress = "halloween_decoration_prefab";
                AddressableAssetLoader.Instance.InstantiateGameObjectAsync(prefabAddress,
                (loadedGO, handle) => {
                    if (loadedGO != null) // Aggiunte graffe
                    {
                        Debug.Log($"[TrendInjector] Prefab decorazione Halloween istanziato: {loadedGO.name} (da {prefabAddress})");
                    }
                },
                (errorMsg) => {
                    Debug.LogError($"[TrendInjector] Fallita istanziazione prefab decorazione Halloween '{prefabAddress}': {errorMsg}");
                }
                );
            }
        }
    }
}
