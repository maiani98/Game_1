# Scenario Golden Run (120 Secondi) - Chaos Cosmos.io

Questo documento descrive uno scenario di gameplay tipico di 120 secondi ("Golden Run") da utilizzare per test di performance, QA manuale, e analisi di memoria.

**Obiettivo Primario del Giocatore:** Crescere il più possibile ingerendo collezionabili ed evitando/sconfiggendo bot (se la sconfitta è implementata).

**Setup Iniziale:**
*   Avviare il gioco dalla scena Lobby.
*   Iniziare una nuova partita (es. sulla scena `PlanetTestScene` o la scena di gameplay principale).
*   Il giocatore parte con la massa e le statistiche di base.

**Sequenza Azioni (approssimativa):**

*   **0s - 15s: Crescita Iniziale**
    *   Il giocatore si muove attivamente nell'arena.
    *   Obiettivo: Ingerire almeno 5-8 `Collectible` base.
    *   Evitare i muri dell'arena se ci sono penalità.
*   **15s - 30s: Primo Incontro con Bot**
    *   Un `BotPlanet` (o più) è presente o spawna.
    *   Obiettivo: Continuare a ingerire `Collectible` evitando il contatto con bot più grandi, o tentando di ingerire bot più piccoli (se la meccanica esiste).
    *   Mantenere la massa in crescita.
*   **30s - 45s: Raccolta Power-Up**
    *   Un `PowerUp` (es. `SpeedBoostPowerUp`) spawna o è già presente.
    *   Obiettivo: Raccogliere il Power-Up.
*   **45s - 75s: Utilizzo Power-Up e Crescita Accelerata**
    *   Il giocatore sfrutta l'effetto del Power-Up (es. maggiore velocità per raccogliere più velocemente).
    *   Obiettivo: Ingerire rapidamente altri 10-15 `Collectible`. Aumentare significativamente la massa.
*   **75s - 100s: Interazione Avanzata con Bot/Arena**
    *   Più bot potrebbero essere presenti. L'arena potrebbe avere più collezionabili.
    *   Obiettivo: Navigare strategicamente, continuare a crescere, e (se implementato) tentare di usare la propria massa o abilità per avere la meglio sui bot o per raggiungere aree ricche di risorse.
*   **100s - 120s: Sprint Finale / Sopravvivenza**
    *   Il giocatore tenta di massimizzare la propria massa negli ultimi secondi.
    *   Evitare pericoli che potrebbero ridurre la massa o terminare la partita (se tali meccaniche esistono).

**Metriche da Osservare (Manualmente o tramite Log):**
*   Massa finale raggiunta.
*   Numero totale di `Collectible` ingeriti.
*   Numero di `PowerUp` raccolti e usati.
*   Numero di interazioni con i `BotPlanet` (evitati, ingeriti, o che hanno ingerito il giocatore).
*   FPS medi (se si monitorano le performance).
*   Eventuali glitch, bug visivi, o comportamenti inattesi.

**Uso per Analisi Memoria:**
*   Prendere uno snapshot della memoria PRIMA di iniziare la Golden Run (dopo il caricamento della scena).
*   Eseguire la Golden Run.
*   Prendere uno snapshot della memoria ALLA FINE dei 120 secondi.
*   (Opzionale) Tornare alla scena Lobby/Menu e prendere un terzo snapshot.
*   Confrontare gli snapshot usando l'Unity Profiler per identificare leak.
