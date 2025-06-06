# Bot e Intelligenza Artificiale (AI) - Chaos Cosmos.io

Questo documento descrive i tipi di Bot presenti nel gioco e le logiche AI che li governano.

## Tipi di Bot Esistenti

### 1. Bot Standard (`BotPlanet.prefab`)
*   **Descrizione:** Un bot bilanciato che utilizza `PlanetController` per il movimento e l'ingestione, e `BotBrain` per l'IA.
*   **Logica AI (`BotBrain.cs`):**
    *   Alterna tra stati principali: `Roaming`, `ChasingPlayer`, `Fleeing`.
    *   **Roaming:** Si muove verso target casuali all'interno dei confini dell'arena.
    *   **ChasingPlayer:** Insegue il giocatore (taggato `GameTags.PLAYER_TAG`) se entra nel `chasePlayerDetectionRadius` (e non ci sono condizioni di fuga).
    *   **Fleeing:** Si allontana dal giocatore se certe condizioni sono vere (es. giocatore con scudo, o per personalità `Scout` se il giocatore è troppo grande).
    *   La velocità è influenzata da `PlanetController.baseSpeed` e dal `SpeedModifier()` (basato sulla massa), e da eventuali `currentSpeedMultiplier` (PowerUp).
*   **Parametri Configurabili sul Prefab:**
    *   **`PlanetController`:** `baseSpeed`, `growthData`.
    *   **`BotBrain`:**
        *   `Personality`: (Enum: `Standard`, `Tank`, `Scout`) - Determina il comportamento generale.
        *   `Scout Flee Mass Ratio Threshold`: (Usato se `Personality` è `Scout`) Moltiplicatore di massa del giocatore rispetto al bot che triggera la fuga.
        *   `Scout Flee Distance Factor`: (Usato se `Personality` è `Scout`) Fattore per calcolare la distanza di fuga.
        *   `Tank Chase Persistence Factor`: (Usato se `Personality` è `Tank`) Moltiplicatore per `losePlayerChaseRadius` per renderlo più persistente.
        *   `chasePlayerDetectionRadius`, `losePlayerChaseRadius`, `stateChangeIntervalMin/Max`, `roamChangeTargetDistance`, riferimento ad `ArenaManager`. (Parametri esistenti).

## Nuovi Tipi di Bot (Concettuali, basati sulla configurazione del Prefab)

Questi nuovi tipi di bot possono essere creati duplicando `BotPlanet.prefab` e modificando i parametri dei componenti `PlanetController` e `BotBrain` come descritto di seguito.

### 1. Bot Tipo: "TankBot"

*   **Concetto:** Un bot lento ma massiccio e resistente. Difficile da spostare con il `RepelBotsPowerUpEffect` a causa della sua massa elevata.
*   **Comportamento AI Desiderato:** Meno propenso a iniziare l'inseguimento da lontano, ma molto persistente una volta che ha agganciato il target. Non fugge facilmente.
*   **Configurazione Prefab (`TankBot.prefab`):**
    *   **`PlanetController`:**
        *   `baseSpeed`: Basso (es. `2.0f` - `3.0f`).
        *   `growthData`: Potrebbe usare una curva che parte da una massa/raggio iniziale leggermente più alta.
        *   `Rigidbody2D.mass` (sul componente Rigidbody2D): Valore più alto (es. `2.0f` - `5.0f`).
    *   **`BotBrain`:**
        *   **`Personality`**: `Tank`
        *   `chasePlayerDetectionRadius`: Medio-Basso (es. `8.0f`).
        *   `losePlayerChaseRadius`: Valore base (es. `15.0f`), che sarà moltiplicato dal `Tank Chase Persistence Factor`.
        *   **`Tank Chase Persistence Factor`**: Alto (es. `1.5f` - `2.0f`), rendendo il raggio di perdita effettivo, ad esempio, `15.0f * 1.5f = 22.5f`.
        *   `stateChangeIntervalMin/Max`: Intervalli più lunghi (es. `5.0f` - `10.0f`), per renderlo meno erratico.
    *   **Visivo:** Sprite più grande, scuro, o con texture "pesante".

### 2. Bot Tipo: "ScoutBot"

*   **Concetto:** Un bot veloce e agile, ma probabilmente più fragile (massa iniziale più bassa). Bravo a esplorare e incline alla fuga se in svantaggio o se il giocatore ha uno scudo.
*   **Comportamento AI Desiderato:** Molto mobile, cambia spesso target di roaming. Reagisce rapidamente alla presenza del giocatore, fuggendo se il giocatore è significativamente più grande o ha uno scudo attivo.
*   **Configurazione Prefab (`ScoutBot.prefab`):**
    *   **`PlanetController`:**
        *   `baseSpeed`: Alto (es. `6.0f` - `8.0f`).
        *   `growthData`: Potrebbe iniziare con una massa/raggio più piccoli.
        *   `Rigidbody2D.mass` (sul componente Rigidbody2D): Valore più basso (es. `0.5f` - `0.8f`).
    *   **`BotBrain`:**
        *   **`Personality`**: `Scout`
        *   `chasePlayerDetectionRadius`: Alto (es. `14.0f`).
        *   `losePlayerChaseRadius`: Medio (es. `16.0f`).
        *   **`Scout Flee Mass Ratio Threshold`**: Medio (es. `2.0f` - `3.0f`). Se il giocatore è X volte più massiccio, lo Scout fugge.
        *   **`Scout Flee Distance Factor`**: Medio (es. `0.5f` - `0.75f`), usato per calcolare quanto lontano fuggire.
        *   `stateChangeIntervalMin/Max`: Intervalli più brevi (es. `2.0f` - `4.0f`).
        *   `roamChangeTargetDistance`: Valore più piccolo (es. `0.5f`).
    *   **Visivo:** Sprite più piccolo, agile, magari con colori brillanti.
    *   **Nota Comportamentale:** Lo Scout ora fuggirà anche da un giocatore con scudo attivo se entra nel suo raggio di rilevamento (`chasePlayerDetectionRadius`).

---
## Istruzioni per la Creazione dei Prefab di Bot

Per creare nuovi tipi di Bot come "TankBot" o "ScoutBot":

1.  **Duplica Prefab Esistente:**
    *   Nel Project panel, naviga in `Assets/Prefabs/` (o dove si trova `BotPlanet.prefab`).
    *   Duplica `BotPlanet.prefab` (Ctrl+D o Cmd+D).
    *   Rinomina il duplicato (es. `TankBot.prefab`, `ScoutBot.prefab`).

2.  **Seleziona il Nuovo Prefab:**
    *   Apri il prefab per la modifica (doppio click o selezionandolo e modificando nell'Inspector in modalità Prefab).

3.  **Modifica Componente `PlanetController`:**
    *   `Base Speed`.
    *   `Growth Data` (opzionale, per diverse curve di crescita/dimensione).
    *   (Componente `Rigidbody2D`) `Mass` (per la fisica).

4.  **Modifica Componente `BotBrain`:**
    *   **`Personality`**: Selezionare `Tank` o `Scout` (o `Standard`).
    *   **`Scout Flee Mass Ratio Threshold`**: Impostare se `Personality` è `Scout`.
    *   **`Scout Flee Distance Factor`**: Impostare se `Personality` è `Scout`.
    *   **`Tank Chase Persistence Factor`**: Impostare se `Personality` è `Tank`.
    *   `Chase Player Detection Radius`.
    *   `Lose Player Chase Radius`.
    *   `State Change Interval Min/Max`.
    *   `Roam Change Target Distance`.

5.  **Modifica Aspetto Visivo (Opzionale):**
    *   Nel `SpriteRenderer` del prefab, cambia lo `Sprite` o il `Color` per distinguere visivamente questo tipo di bot.

6.  **Assegna il Tag "Bot":**
    *   Assicurati che il GameObject radice del prefab del bot abbia il Tag "Bot" (`GameTags.BOT_TAG`) assegnato. Questo è usato, ad esempio, da `RepelBotsPowerUpEffect`. Se il tag "Bot" non esiste, crealo tramite "Add Tag..." nell'Inspector.

7.  **Salva il Prefab:**
    *   Assicurati di salvare le modifiche al prefab.

8.  **Test:**
    *   Trascina istanze dei tuoi nuovi prefab di bot nella scena `PlanetTestScene` per testare il loro comportamento.
    *   Osserva come interagiscono con il giocatore, specialmente in relazione alla massa e allo scudo del giocatore.

9.  **Considerazioni Future per lo Spawn:**
    *   Attualmente, `ArenaManager` spawna solo un tipo di `collectiblePrefab`. Per spawnare tipi diversi di bot, `ArenaManager` (o un nuovo `BotSpawnManager`) dovrebbe essere esteso.
    *   Potrebbe avere una lista di prefab di bot da cui scegliere, magari con probabilità diverse o basate su condizioni di gioco (es. tempo trascorso, livello del giocatore, bioma attivo via `TrendInjectorService`).
