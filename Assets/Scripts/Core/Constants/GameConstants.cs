namespace ChaosCosmos.Core.Constants
{
    public static class PlayerPrefsKeys
    {
        // ProgressManager Keys
        public const string XP_SAVE_KEY = "ChaosCosmos_PlayerXP";
        public const string PLAYER_LEVEL_SAVE_KEY = "ChaosCosmos_PlayerLevel"; // Rinominato per chiarezza
        public const string UPGRADE_LEVEL_PREFIX = "ChaosCosmos_UpgradeLevel_"; // Es: ChaosCosmos_UpgradeLevel_upgradeID

        // PassManager Keys
        public const string PASS_SYSTEM_PREFIX = "ChaosCosmos_PassSystem_";
        // Dinamiche: PASS_SYSTEM_PREFIX + seasonID + "_XP"
        //            PASS_SYSTEM_PREFIX + seasonID + "_HasPremium"
        //            PASS_SYSTEM_PREFIX + seasonID + "_Tier" + tierLevel + (isPremium ? "_P" : "_F") + "_" + rewardID + "_Claimed"
        // Metodi helper per queste chiavi dinamiche verranno messi in PassManager o qui se preferito.
        // Per ora, lasciamo che PassManager costruisca le sue chiavi dinamiche usando questo prefisso.

        // ConsentManager Keys
        public const string GDPR_CONSENT_KEY = "ChaosCosmos_GDPRConsent";
        public const string ATT_STATUS_KEY = "ChaosCosmos_ATTStatus";

        // IAPFacade Keys (ProductID è già una const in IAPFacade, ma la usiamo come chiave PlayerPrefs)
        // Non serve una nuova const se IAPFacade.ProductID_RemoveAds è già public const string.
        // Altrimenti, definirla qui: public const string IAP_REMOVE_ADS_KEY = "com.chaoscosmos.removeads";
        // Decisione presa: non duplicare IAPFacade.ProductID_RemoveAds qui.
    }

    public static class GameTags
    {
        public const string PLAYER_TAG = "Player";
        public const string COLLECTIBLE_TAG = "Collectible";
        // Aggiungere altri tag usati nel gioco
    }

    public static class SceneNames
    {
        public const string BOOT_SCENE = "BootScene";
        public const string LOBBY_SCENE = "LobbyScene";
        public const string GAMEPLAY_SCENE_PLACEHOLDER = "PlanetTestScene"; // O il nome della scena di gioco principale
        // Aggiungere altri nomi di scene se necessario
    }

    public static class RemoteConfigKeyPatterns
    {
        // Esempio di come si potrebbero definire pattern o metodi per chiavi Remote Config
        public static string GetUpgradeXpCostKey(string upgradeID) => $"upgrade_{upgradeID}_xpCost";
        public static string GetPowerUpDurationKey(string powerUpName) => $"powerup_{powerUpName}_duration";
        public static string GetArenaMaxCollectiblesKey() => "arena_maxCollectibles";
        public static string GetWelcomeMessageKey() => "welcomeMessage";
        public static string GetExperimentalFeatureKey(string featureName) => $"enableExperimentalFeature{featureName}";
        public static string GetInitialXpBonusKey() => "ab_test_initialXpBonus_amount"; // Aggiunto
        // Altri pattern...
    }
}
