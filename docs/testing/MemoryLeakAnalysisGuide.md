# Guida all'Analisi di Memory Leak - Chaos Cosmos.io

Questa guida descrive come usare l'Unity Profiler e lo scenario "Golden Run" per identificare potenziali memory leak.

## Prerequisiti
1.  Unity Profiler (Window > Analysis > Profiler).
2.  Una build di sviluppo del gioco o esecuzione in Editor.
3.  Lo script `MemoryMonitorDebug.cs` (opzionale) aggiunto a un GameObject persistente nella scena (es. `_Bootstrapper` o un `_DebugManager`) per log/GC manuali.
4.  Familiarità con lo [Scenario Golden Run](./GoldenRunScenario.md).

## Procedura

1.  **Preparazione:**
    *   Aprire la scena di gioco principale (es. `PlanetTestScene` o la scena di gameplay designata).
    *   Se si testa su un device, connetterlo e selezionarlo come target nel Profiler (menu a tendina "Editor" nel Profiler > selezionare il device).
    *   Nel Profiler, assicurarsi che il modulo "Memory" sia attivo (selezionato nella barra in alto del Profiler). Potrebbe essere necessario cliccare "Record" per iniziare a catturare dati.

2.  **Snapshot Iniziale (Baseline):**
    *   Avviare la scena di gioco. Lasciare che si stabilizzi per qualche secondo (es. fine del caricamento, nessun input utente).
    *   (Opzionale) Se si usa `MemoryMonitorDebug.cs`, premere il pulsante "Force GC.Collect()" sulla UI di debug per cercare di ottenere una baseline di memoria più pulita. Attendere qualche istante.
    *   Nel Profiler (modulo Memory), cliccare su "Take Sample: Editor" (o il nome del device se connesso). Rinominare questo snapshot (es. cliccandoci sopra nella lista degli snapshot) in **Snapshot_Baseline**.
    *   (Opzionale) Usare il pulsante "Log Current Memory Stats" da `MemoryMonitorDebug` per avere un log testuale dello stato attuale della memoria nella console di Unity.

3.  **Esecuzione della Golden Run:**
    *   Giocare seguendo lo [Scenario Golden Run](./GoldenRunScenario.md) per circa 120 secondi.
    *   Cercare di essere consistenti nell'esecuzione se si devono ripetere i test per confronti.

4.  **Snapshot Dopo la Golden Run:**
    *   Al termine esatto dei 120 secondi (o il più vicino possibile), se possibile mettere in pausa il gioco in modo non invasivo (es. NON tornando al menu principale, ma pausando l'azione di gioco se esiste una meccanica di pausa). Altrimenti, semplicemente prendere lo snapshot mentre il gioco è ancora nella scena di gameplay.
    *   Nel Profiler, cliccare di nuovo su "Take Sample". Rinominare questo snapshot in **Snapshot_EndOfRun**.
    *   (Opzionale) Loggare le statistiche con `MemoryMonitorDebug`.

5.  **Snapshot Dopo Ritorno al Menu (Opzionale ma Cruciale per Leak tra Scene):**
    *   Se il gioco ha un flusso per tornare alla scena Lobby/Menu principale:
        *   Eseguire l'azione per tornare al menu (es. tramite un pulsante "Quit to Menu" o simile).
        *   Attendere qualche secondo che la scena Lobby/Menu si carichi e si stabilizzi.
        *   (Opzionale) Usare "Force GC.Collect()" dalla UI di debug. Attendere.
        *   Nel Profiler, cliccare su "Take Sample". Rinominare questo snapshot in **Snapshot_AfterMenuReturn**.
        *   (Opzionale) Loggare le statistiche.

6.  **Analisi degli Snapshot (Diff):**
    *   Nel Profiler, nella vista "Memory", si vedranno gli snapshot presi (es. Snapshot_Baseline, Snapshot_EndOfRun, Snapshot_AfterMenuReturn).
    *   Per confrontare, selezionare lo snapshot più recente (es. `Snapshot_EndOfRun`) e poi, nel pannello dei dettagli dello snapshot (o tramite un menu contestuale/pulsante "Diff"), scegliere lo snapshot precedente (es. `Snapshot_Baseline`) da confrontare.
    *   L'Unity Profiler mostrerà le differenze nel numero di oggetti e nella memoria usata per tipo.
    *   **Cosa Cercare nel Diff (Snapshot_Baseline vs Snapshot_EndOfRun):**
        *   Un aumento generale di "Total Reserved Memory" e "Mono Used Size" è normale durante il gioco.
        *   Nella vista dettagliata (es. "Simple" o "Detailed", selezionare "Diff A to B"), cercare oggetti il cui conteggio (`Count`) o dimensione (`Size`) è aumentato significativamente e che non dovrebbero accumularsi indefinitamente. Esempi:
            *   GameObjects specifici del gameplay (collezionabili, proiettili, effetti particellari) che non vengono distrutti.
            *   ScriptableObjects clonati (se si usa `Instantiate()` su SO invece di referenziarli).
            *   Materiali (spesso clonati implicitamente se si modifica `Renderer.material` invece di `Renderer.sharedMaterial`).
            *   Texture.
            *   Componenti specifici del gioco (`PlanetController`, `MassSource`, ecc.).
    *   **Cosa Cercare nel Diff (Snapshot_Baseline vs Snapshot_AfterMenuReturn):**
        *   Questo confronto è **fondamentale** per identificare leak che persistono dopo aver lasciato la scena di gameplay.
        *   Idealmente, dopo essere tornati al menu, la memoria (specialmente per oggetti specifici del gameplay) dovrebbe tornare a livelli molto simili a `Snapshot_Baseline`.
        *   Cercare oggetti relativi alla scena di gameplay (controllare nomi, tipi) che hanno ancora un `Count` > 0 o una `Size` significativa. Questi sono forti candidati per essere dei leak. Cause comuni:
            *   Riferimenti statici (`static event`, `static Dictionary`, Singleton che mantengono riferimenti a oggetti della scena).
            *   Eventi a cui i componenti della scena si sono registrati ma da cui non hanno fatto `Unsubscribe` in `OnDestroy()`.
            *   Coroutine non interrotte (`StopCoroutine()`) quando il GameObject che le ha avviate viene distrutto o disattivato.
            *   Riferimenti incrociati tra oggetti che impediscono al Garbage Collector (GC) di liberare memoria.

7.  **Iterazione e Correzione:**
    *   Una volta identificato un tipo di oggetto sospetto, usare la vista dettagliata del Profiler per cercare di capire da dove provengono i riferimenti (se il Profiler lo mostra) o analizzare il codice relativo alla creazione/distruzione di quegli oggetti.
    *   Apportare le correzioni necessarie (es. `Unsubscribe` da eventi, `StopCoroutine`, nullificare riferimenti statici, assicurarsi che `Destroy()` sia chiamato).
    *   Ripetere il test (dallo step 1) per verificare se il leak è stato risolto. Confrontare i nuovi snapshot con i precedenti.

## Suggerimenti Aggiuntivi
*   **Build di Sviluppo:** Eseguire i test di memoria preferibilmente su build di sviluppo sul dispositivo target, poiché il comportamento della memoria in Editor può differire significativamente.
*   **Consistenza:** Essere il più consistenti possibile nell'esecuzione della Golden Run per rendere i confronti tra snapshot più affidabili.
*   **Piccoli Leak:** Anche piccoli leak possono accumularsi nel tempo e causare crash o rallentamenti in sessioni di gioco prolungate.
*   **`MemoryMonitorDebug.cs`:** Usare lo script `MemoryMonitorDebug` per avere un'idea rapida dei conteggi di oggetti chiave e per forzare il GC, ma affidarsi al Profiler per l'analisi dettagliata.
*   **Profiler "Memory" Tab Views:** Esplorare le diverse viste nel tab "Memory" del Profiler ("Simple", "Detailed", "Hierarchy") per ottenere diverse prospettive sui dati di memoria.
