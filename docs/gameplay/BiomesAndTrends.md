# Biomi e Trend Dinamici - Chaos Cosmos.io

Questo documento descrive i concetti per biomi di gioco e trend dinamici che possono essere attivati tramite `TrendInjectorService` e `RemoteConfigService` per variare l'esperienza di gioco.

## Concetti di Bioma

I biomi sono variazioni tematiche e meccaniche dell'arena di gioco. L'attivazione di un bioma può modificare tassi di spawn, tipi di collezionabili, comportamenti dei bot, e aspetti visivi.

### 1. Bioma: Campo di Asteroidi Denso

*   **Descrizione:** Un'area dello spazio piena di asteroidi di varie dimensioni. La navigazione è più difficile e la visibilità potrebbe essere ridotta.
*   **Visivo Placeholder (Asset Addressable):**
    *   Background: `theme_asteroid_field_bg` (es. un tilemap o texture di sfondo con molti asteroidi distanti)
    *   Ostacoli Dinamici/Statici: `obstacle_asteroid_small`, `obstacle_asteroid_medium` (prefab di asteroidi con cui i pianeti potrebbero collidere o che semplicemente decorano)
*   **Meccaniche Chiave (Simulate/Loggate da `TrendInjectorService`):**
    *   **Visibilità Ridotta:** (Concettuale) Potrebbe essere un effetto post-processing o una nebbia di guerra più densa.
    *   **Aumento Spawn Collectible Piccoli:** Tasso di spawn per i `Collectible` base (piccola massa) aumentato.
        *   *Parametro TrendInjector (simulato):* `CollectibleSpawnRateMultiplier_Small = 1.5` (es. +50%)
    *   **Riduzione Spawn PowerUp:** I PowerUp appaiono meno frequentemente.
        *   *Parametro TrendInjector (simulato):* `PowerUpSpawnRateMultiplier = 0.5` (es. -50%)
    *   **Rallentamento Bot:** I Bot potrebbero avere una velocità base leggermente ridotta a causa della difficoltà di navigazione.
        *   *Parametro TrendInjector (simulato):* `BotBaseSpeedMultiplier = 0.8` (es. -20% velocità)
*   **ID Bioma (per Remote Config):** `asteroid_field_dense`

### 2. Bioma: Nebulosa Elettrica

*   **Descrizione:** Una regione gassosa e instabile, carica di energia elettrica. Alcune zone della nebulosa potrebbero danneggiare o respingere i pianeti periodicamente. L'energia ambientale potenzia certi tipi di PowerUp.
*   **Visivo Placeholder (Asset Addressable):**
    *   Background/Effetti VFX: `theme_electric_nebula_vfx` (es. particellari, fulmini distanti, shader per distorsione)
    *   Collectible Speciali: `collectible_energy_orb` (un tipo di collezionabile che potrebbe dare più XP o un piccolo scudo temporaneo)
*   **Meccaniche Chiave (Simulate/Loggate da `TrendInjectorService`):**
    *   **Zone Pericolose (Elettriche):** Aree specifiche dell'arena che applicano un piccolo impulso o "danno" (se il danno fosse implementato) a intervalli.
        *   *Parametro TrendInjector (simulato):* `Hazard_ElectricZone_Active = true`
    *   **Potenziamento PowerUp Scudo:** La durata dei PowerUp di tipo Scudo è aumentata.
        *   *Parametro TrendInjector (simulato):* `PowerUp_Shield_DurationMultiplier = 1.5` (es. +50% durata)
*   **ID Bioma (per Remote Config):** `electric_nebula`

### 3. Bioma: Cimitero Spaziale Antico

*   **Descrizione:** Un'area disseminata di relitti di grandi navi spaziali e detriti tecnologici. Alcuni detriti più grandi possono essere ingeriti per un grande bonus di massa. Frequentato da bot "spazzini".
*   **Visivo Placeholder (Asset Addressable):**
    *   Background/Oggetti Scenario: `theme_ship_graveyard_debris` (grandi sprite/modelli 3D di relitti sullo sfondo o come ostacoli passabili)
    *   Collectible Speciali: `collectible_scrap_metal` (collezionabile standard), `collectible_ship_core_fragment` (collezionabile raro di grande valore/massa)
*   **Meccaniche Chiave (Simulate/Loggate da `TrendInjectorService`):**
    *   **Spawn Collezionabili Speciali ("Relitti"):** Attiva lo spawn di `collectible_ship_core_fragment`.
        *   *Parametro TrendInjector (simulato):* `SpecialCollectible_ShipDebris_Active = true`
    *   **Focus Spawn Bot "Spazzini":** Aumenta la probabilità di spawn per i bot di tipo "ScavengerBot" (se definiti).
        *   *Parametro TrendInjector (simulato):* `BotTypeSpawnFocus = "ScavengerBot"`
*   **ID Bioma (per Remote Config):** `ship_graveyard_ancient`

---

**Nota sull'Implementazione:**
L'attivazione di questi biomi e la modifica dei parametri di gioco sarebbero gestite da `TrendInjectorService` leggendo la configurazione da `RemoteConfigService`. L'effettiva applicazione delle meccaniche richiederebbe modifiche ai sistemi corrispondenti (es. `ArenaManager` per tassi di spawn, `PlanetController` per interazioni con hazard, `PowerUpEffect` per modificatori di durata/effetto, `BotManager` per tipi di spawn). Gli asset visivi verrebbero caricati dinamicamente tramite `AddressableAssetLoader`.
