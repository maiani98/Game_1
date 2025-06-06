using UnityEngine;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.RemoteConfig;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Core.AssetManagement;
// using UnityEngine.ResourceManagement.AsyncOperations; // Rimosso perché non usato direttamente qui

namespace ChaosCosmos.Services.LiveOps
{
    public class TrendInjectorService : ITrendInjectorService
    {
        public bool IsInitialized { get; private set; }
        private IRemoteConfigService _remoteConfigService;

        // Chiavi per Remote Config
        private const string RC_ARENA_THEME_KEY = "trend_generic_arenaTheme"; // Rinominato per chiarezza vs bioma
        private const string RC_ACTIVE_BIOME_ID_KEY = "trend_active_biome_id";
        // private const string RC_COLLECTIBLE_X_SPAWN_MULT_KEY = "trend_collectibleX_spawnMultiplier"; // Esempio, potrebbe essere gestito per bioma

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
            {
                Debug.LogError("TrendInjectorService: Impossibile inizializzare, IRemoteConfigService non pronto.");
                return;
            }

            ApplyCurrentTrends();
            IsInitialized = true;
            Debug.Log("TrendInjectorService: Inizializzato e trend/biomi (simulati) applicati.");
        }

        private void ApplyCurrentTrends()
        {
            // Leggi il bioma attivo
            string activeBiomeID = _remoteConfigService.GetString(RC_ACTIVE_BIOME_ID_KEY, "default_biome");
            Debug.Log($"[TrendInjector] Bioma Attivo ID (da RC '{RC_ACTIVE_BIOME_ID_KEY}'): {activeBiomeID}");

            // Applica logica specifica del bioma o default
            switch (activeBiomeID)
            {
                case "asteroid_field_dense":
                    Debug.Log("[TrendInjector] SIMULAZIONE: Attivato Bioma 'Campo di Asteroidi Denso'.");
                    Debug.Log("[TrendInjector] -> Modificherebbe: CollectibleSpawnRateMultiplier_Small = 1.5 (es. via RemoteConfig: " + _remoteConfigService.GetFloat("trend_asteroid_collectible_mult", 1.0f) + ")");
                    Debug.Log("[TrendInjector] -> Modificherebbe: PowerUpSpawnRateMultiplier = 0.5 (es. via RemoteConfig: " + _remoteConfigService.GetFloat("trend_asteroid_powerup_mult", 1.0f) + ")");
                    Debug.Log("[TrendInjector] -> Modificherebbe: BotBaseSpeedMultiplier = 0.8 (es. via RemoteConfig: " + _remoteConfigService.GetFloat("trend_asteroid_bot_speed_mult", 1.0f) + ")");
                    LoadBiomeAssets("theme_asteroid_field_bg", "obstacle_asteroid_small");
                    break;
                case "electric_nebula":
                    Debug.Log("[TrendInjector] SIMULAZIONE: Attivato Bioma 'Nebulosa Elettrica'.");
                    Debug.Log("[TrendInjector] -> Modificherebbe: Hazard_ElectricZone_Active = true (es. via RemoteConfig: " + _remoteConfigService.GetBool("trend_nebula_hazard_active", false) + ")");
                    Debug.Log("[TrendInjector] -> Modificherebbe: PowerUp_Shield_DurationMultiplier = 1.5 (es. via RemoteConfig: " + _remoteConfigService.GetFloat("trend_nebula_shield_mult", 1.0f) + ")");
                    LoadBiomeAssets("theme_electric_nebula_vfx", "collectible_energy_orb");
                    break;
                case "ship_graveyard_ancient":
                    Debug.Log("[TrendInjector] SIMULAZIONE: Attivato Bioma 'Cimitero Spaziale Antico'.");
                    Debug.Log("[TrendInjector] -> Modificherebbe: SpecialCollectible_ShipDebris_Active = true (es. via RemoteConfig: " + _remoteConfigService.GetBool("trend_graveyard_debris_active", false) + ")");
                    Debug.Log("[TrendInjector] -> Modificherebbe: BotTypeSpawnFocus = \"ScavengerBot\" (es. via RemoteConfig: " + _remoteConfigService.GetString("trend_graveyard_bot_focus", "default") + ")");
                    LoadBiomeAssets("theme_ship_graveyard_debris", "collectible_scrap_metal");
                    break;
                case "default_biome":
                default:
                    Debug.Log($"[TrendInjector] Bioma di default o ID '{activeBiomeID}' non specificamente gestito. Applico tema generico se presente.");
                    string genericTheme = _remoteConfigService.GetString(RC_ARENA_THEME_KEY, "default_theme_assets");
                    Debug.Log($"[TrendInjector] Tema Arena Generico (da RC '{RC_ARENA_THEME_KEY}'): {genericTheme}. (TODO: Applicare al sistema di theming visivo)");
                    if (genericTheme != "default_theme_assets") LoadBiomeAssets(genericTheme, null); // Carica solo il tema se definito
                    break;
            }

            // Esempio di lettura di un parametro di trend generico (già presente)
            int maxCollectibles = _remoteConfigService.GetInt(RemoteConfigKeyPatterns.GetArenaMaxCollectiblesKey(), 30);
            Debug.Log($"[TrendInjector] Max Collectibles (da RC '{RemoteConfigKeyPatterns.GetArenaMaxCollectiblesKey()}'): {maxCollectibles}. (TODO: Applicare ad ArenaManager)");
        }

        private void LoadBiomeAssets(string themeAssetKey, string specialCollectibleKey)
        {
            if (AddressableAssetLoader.Instance == null)
            {
                Debug.LogWarning("[TrendInjector] AddressableAssetLoader.Instance è nullo. Impossibile caricare asset per il bioma.");
                return;
            }

            if (!string.IsNullOrEmpty(themeAssetKey))
            {
                AddressableAssetLoader.Instance.LoadAssetAsync<GameObject>(themeAssetKey, // Assumendo che il tema sia un prefab o simile
                (loadedAsset) => {
                    if (loadedAsset != null) Debug.Log($"[TrendInjector] Asset tema '{themeAssetKey}' caricato: {loadedAsset.name}. (TODO: Istanziare/Applicare)");
                },
                (errorMsg) => Debug.LogError($"[TrendInjector] Fallito caricamento asset tema '{themeAssetKey}': {errorMsg}"));
            }

            if (!string.IsNullOrEmpty(specialCollectibleKey))
            {
                 AddressableAssetLoader.Instance.LoadAssetAsync<GameObject>(specialCollectibleKey, // Assumendo prefab per collezionabile
                (loadedAsset) => {
                    if (loadedAsset != null) Debug.Log($"[TrendInjector] Asset collezionabile speciale '{specialCollectibleKey}' caricato: {loadedAsset.name}. (TODO: Usare per spawn)");
                },
                (errorMsg) => Debug.LogError($"[TrendInjector] Fallito caricamento asset collezionabile speciale '{specialCollectibleKey}': {errorMsg}"));
            }
        }
    }
}
