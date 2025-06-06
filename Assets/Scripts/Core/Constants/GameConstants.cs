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

        // SettingsPanelUI Keys
        public const string SETTINGS_MUSIC_VOLUME = "ChaosCosmos_Settings_MusicVolume";
        public const string SETTINGS_SFX_VOLUME = "ChaosCosmos_Settings_SfxVolume";
        // public const string SETTINGS_SELECTED_LANGUAGE = "ChaosCosmos_Settings_SelectedLanguage"; // Non usato se ci affidiamo a LocalizationSettings

        // TutorialManagerService Keys
        public const string TUTORIAL_COMPLETED_KEY = "ChaosCosmos_TutorialCompleted";
        // public const string TUTORIAL_STEP_COMPLETED_PREFIX = "ChaosCosmos_TutorialStep_"; // Per granularità futura
    }

    public static class GameTags
    {
        public const string PLAYER_TAG = "Player";
        public const string COLLECTIBLE_TAG = "Collectible";
        public const string BOT_TAG = "Bot"; // Aggiunto
        // Aggiungere altri tag usati nel gioco
    }

    public static class SceneNames
    {
        public const string BOOT_SCENE = "BootScene";
        public const string MAIN_MENU_SCENE = "MainMenuScene"; // Rinominato da LOBBY_SCENE
        public const string GAMEPLAY_SCENE_PLACEHOLDER = "PlanetTestScene";
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
        public static string GetInitialXpBonusKey() => "ab_test_initialXpBonus_amount";
        // Altri pattern...
    }

    public static class GameplayBalance
    {
        public const float MINIMUM_MASS_TO_SURVIVE = 0.2f;
        public const float COLLISION_DAMAGE_THRESHOLD_FACTOR = 1.3f;
        public const float COLLISION_MASS_LOSS_PERCENTAGE = 0.15f;
        public const float BOT_DEATH_MASS_DROP_PERCENTAGE = 0.25f;
        public const int BOT_DEATH_XP_REWARD = 50;
    }
}
