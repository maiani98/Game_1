# Matrice di Test Cross-Device e Strategie - Chaos Cosmos.io

Questo documento definisce una matrice di dispositivi target ipotetica, le aree chiave da testare su tali dispositivi, e suggerisce strategie e strumenti per il testing cross-device di Chaos Cosmos.io.

## 1. Matrice di Dispositivi Target Ipotetica

La seguente lista rappresenta una selezione bilanciata di dispositivi per coprire diverse piattaforme, produttori, fasce di prestazioni, e fattori di forma. In un progetto reale, questa lista andrebbe aggiornata in base ai dati di analytics sui dispositivi più usati dal proprio target di utenti e alla disponibilità dei device per il testing.

**Legenda:**
*   **OS Ver:** Versione minima o tipica del Sistema Operativo da testare su quel dispositivo.
*   **Focus:** Aree di test particolarmente importanti per quel dispositivo/categoria.

| Piattaforma | Categoria   | Produttore | Modello Esempio        | OS Ver.      | Focus                                                                 | Note                                         |
| :---------- | :---------- | :--------- | :--------------------- | :----------- | :-------------------------------------------------------------------- | :------------------------------------------- |
| **iOS**     | High-End    | Apple      | iPhone 14/15 Pro       | iOS 16+      | Performance Max, UI (Notch/Dynamic Island), Nuove API OS              | Benchmark per performance ottimali           |
| iOS         | Mid-Range   | Apple      | iPhone 12/13           | iOS 16+      | Performance Buona, UI (Notch)                                         | Rappresenta una larga base di utenti         |
| iOS         | Low-End     | Apple      | iPhone XR / SE (Gen 2/3) | iOS 15/16+   | Performance Minima, UI (Notch/Tradizionale), Stabilità               | Importante per accessibilità e base utenti ampia |
| iOS         | Tablet      | Apple      | iPad Air (Gen 4/5)     | iPadOS 16+   | UI/Layout Tablet, Performance                                         | Tipico tablet di fascia media/alta           |
| iOS         | Tablet Mini | Apple      | iPad mini (Gen 5/6)    | iPadOS 16+   | UI/Layout Tablet Piccolo, Input Touch su schermo più piccolo        |                                              |
| **Android** | High-End    | Samsung    | Galaxy S22/S23/S24     | Android 12/13+ | Performance Max, UI (Samsung OneUI), Features specifiche Samsung        | Dispositivo Android di riferimento high-end  |
| Android     | High-End    | Google     | Pixel 7/8 Pro          | Android 13/14+ | Performance Max, Android "Puro", Nuove API OS                         | Riferimento per Android stock                |
| Android     | Mid-Range   | Samsung    | Galaxy A53/A54         | Android 12/13+ | **Performance KPI (come da specifiche)**, UI (OneUI), Stabilità     | Target FPS specifico, molto diffuso        |
| Android     | Mid-Range   | Xiaomi     | Redmi Note 11/12/13    | Android 11/12+ | Performance, UI (MIUI), Diffusione Mercati Emergenti                | Ampia base utenti, possibile diversità HW    |
| Android     | Mid-Range   | Google     | Pixel 6a/7a            | Android 13/14+ | Performance Buona, Android "Puro"                                     | Buon rapporto qualità/prezzo                 |
| Android     | Low-End     | Samsung    | Galaxy A13/A14         | Android 11/12+ | Performance Minima, UI, Stabilità, Utilizzo Memoria                 | Rappresenta dispositivi con 3-4GB RAM        |
| Android     | Low-End     | Nokia/Motorola | Modello Entry-Level  | Android 11+ (Go Ed.?) | Performance Estrema (Minima), UI, Utilizzo Memoria                  | Testare limiti inferiori (es. 2GB RAM)       |
| Android     | Tablet      | Samsung    | Galaxy Tab S8/S9       | Android 12/13+ | UI/Layout Tablet, Performance                                         | Tablet Android di riferimento high-end       |
| Android     | Tablet      | Lenovo/Amazon | Tab M10 / Fire HD 10  | Android 11+    | UI/Layout Tablet (Low/Mid), Performance, Adattabilità UI a schermi grandi | Rappresenta tablet Android più economici     |

**Considerazioni Aggiuntive:**
*   **Chipset:** Variare tra Snapdragon, Exynos (Samsung), MediaTek, Google Tensor.
*   **GPU:** Adreno, Mali, PowerVR, Apple GPU.
*   **RAM:** Includere dispositivi con quantità di RAM diverse (es. 2GB, 3GB, 4GB, 6GB, 8GB+).

## 2. Aree Chiave da Testare per Categoria di Dispositivo

Per ogni dispositivo (o almeno per ogni categoria rappresentativa), le seguenti aree dovrebbero essere testate:

1.  **Installazione e Avvio Iniziale:**
    *   L'applicazione si installa correttamente dallo store o da build di sviluppo.
    *   Il primo avvio è senza crash.
    *   Tempi di caricamento iniziali accettabili.
    *   Flusso di consenso (GDPR/ATT) presentato e funzionante correttamente (simulato).

2.  **Performance (basato sulla "Golden Run"):**
    *   **High-End:** FPS costantemente alti (es. 60 FPS target), assenza di lag. Tempi di caricamento rapidi.
    *   **Mid-Range:** FPS stabili intorno al target definito (es. 30-60 FPS per Galaxy A52/A53), lag minimo o assente. Tempi di caricamento ragionevoli.
    *   **Low-End:** FPS giocabili (es. >25-30 FPS minimi), lag accettabile che non comprometta il gameplay. Tempi di caricamento più lunghi ma non eccessivi.
    *   Verifica di assenza di surriscaldamento eccessivo del dispositivo dopo la Golden Run o sessioni più lunghe.

3.  **UI e Layout:**
    *   **Tutti i Dispositivi:**
        *   Corretta visualizzazione dell'HUD (Massa, Timer, etc.).
        *   Menu e schermate UI (Lobby, Settings se presenti) leggibili e interagibili.
        *   Nessuna sovrapposizione di elementi UI.
        *   Nessun troncamento di testo o immagini.
        *   Elementi UI non posizionati fuori schermo.
    *   **Specifico per Fattore di Forma/Risoluzione:**
        *   Adattamento corretto a diverse aspect ratio (es. smartphone molto lunghi vs. più squadrati).
        *   Leggibilità e layout appropriati su tablet (evitare UI "stretchata" da smartphone).
        *   Gestione corretta di aree sicure, notch, dynamic island, barre di sistema.

4.  **Input Touch:**
    *   **Tutti i Dispositivi:**
        *   Controllo del pianeta reattivo e preciso al tocco/trascinamento.
        *   Pulsanti UI e altri elementi interattivi rispondono correttamente al tocco.
        *   Nessun input lag evidente.

5.  **Funzionalità Core del Gameplay (Golden Run):**
    *   **Tutti i Dispositivi:**
        *   Ingestione di collezionabili e crescita del pianeta funzionanti come atteso.
        *   Effetti dei PowerUp (es. SpeedBoost) si attivano e terminano correttamente.
        *   Comportamento dei Bot (roaming, chasing) come da design.
        *   Funzionamento corretto dell'ArenaManager (spawn collezionabili, confini arena se implementata la collisione).
        *   Sistemi di progressione (XP, Leveling, Acquisto Upgrade - simulato) funzionano.
        *   Sistema Battle Pass (XP Pass, Tier Unlock, Claim Ricompense - simulato) funziona.

6.  **Memoria (principalmente su Low/Mid-Range e dispositivi con problemi noti):**
    *   Eseguire lo scenario di analisi memoria definito in `MemoryLeakAnalysisGuide.md`.
    *   Monitorare picchi di utilizzo e verificare che la memoria venga rilasciata correttamente dopo aver lasciato scene di gioco pesanti.

7.  **Notifiche Locali (se applicabile e testabile):**
    *   Verificare che le notifiche schedulate (es. per test o da eventi di gioco) appaiano correttamente, con titolo, testo e icone (se configurate) corretti.

8.  **Ads e IAP (Simulati):**
    *   Verificare il flusso di acquisto "Remove Ads" (salvataggio PlayerPrefs e non visualizzazione di interstitial).
    *   Verificare la visualizzazione (simulata) di rewarded e interstitial ads se "Remove Ads" non è attivo e il consenso è dato.

9.  **Connessione di Rete (se Remote Config/Analytics fossero reali):**
    *   (Futuro) Comportamento del gioco con connessioni lente, intermittenti, o assenti. Gestione di timeout e fallback a valori di default.

10. **Stabilità:**
    *   **Tutti i Dispositivi:** Assenza di crash durante la Golden Run.
    *   **Dispositivi Critici (Low/Mid):** Testare sessioni di gioco più lunghe (es. 15-30 minuti) per verificare stabilità a lungo termine e potenziale degradamento delle performance o memory leak non evidenti in sessioni brevi.

## 3. Strategie e Strumenti di Test Cross-Device

1.  **Testing Manuale Strutturato:**
    *   Utilizzare una **checklist di test** derivata dalle "Aree Chiave da Testare" e dallo scenario "Golden Run".
    *   Assegnare specifici dispositivi o categorie di dispositivi a diversi tester per garantire copertura.
    *   Effettuare sessioni di test esplorativo oltre alle checklist per scoprire bug imprevisti.
    *   Documentare bug in modo chiaro con informazioni sul dispositivo, versione OS, passaggi per riprodurre, screenshot/video.

2.  **Unity Device Simulator:**
    *   **Utilizzo:** Pacchetto Unity (installabile da Package Manager) che permette di simulare diverse risoluzioni schermo, aspect ratio, aree sicure (notch, cutout) e alcune caratteristiche del dispositivo direttamente nell'Editor di Unity.
    *   **Vantaggi:** Ottimo per test UI/layout rapidi senza dover buildare su un dispositivo fisico. Utile per identificare problemi di responsività dell'interfaccia.
    *   **Limiti:** Non simula le reali performance hardware, né le peculiarità specifiche dell'OS o del produttore del dispositivo. Non sostituisce il test su device reali.

3.  **Servizi Cloud di Testing su Device Reali:**
    *   **Esempi:** Firebase Test Lab (Android), AWS Device Farm, BrowserStack (App Live / App Automate), Sauce Labs, LambdaTest.
    *   **Vantaggi:**
        *   Accesso a una vasta gamma di dispositivi fisici reali senza la necessità di acquistarli e mantenerli.
        *   Possibilità di eseguire test automatizzati (script UI) e test manuali remoti.
        *   Spesso forniscono log, video della sessione, e dati di performance.
    *   **Svantaggi:**
        *   Costi (solitamente basati su tempo di utilizzo del dispositivo).
        *   Possibile latenza nell'interazione per test manuali remoti.
        *   Setup iniziale della pipeline di test e caricamento delle build può richiedere tempo.
        *   Non tutti i servizi supportano Unity in modo nativo o semplice per test UI complessi.

4.  **Piattaforme di Distribuzione Build per Test:**
    *   **Esempi:** Firebase App Distribution, TestFlight (per iOS, richiede account Apple Developer), Visual Studio App Center, deployGate.
    *   **Vantaggi:**
        *   Facilitano la distribuzione di build di sviluppo (.apk, .aab, .ipa) a un gruppo di tester interni o esterni.
        *   Spesso offrono gestione dei tester, versioning delle build, e raccolta di feedback base o crash report.
    *   **Utilizzo:** Essenziale per ottenere feedback da test su una varietà di dispositivi personali dei tester.

5.  **Logging e Crash Reporting Avanzato:**
    *   **Servizi Dedicati:** Firebase Crashlytics, Sentry, Bugsnag, Unity Crash and Exception Reporting (servizio Unity).
    *   **Vantaggi:**
        *   Raccolta automatica e aggregata di crash report da build di sviluppo e produzione.
        *   Stack trace dettagliate, informazioni sul dispositivo, versione OS, e talvolta log personalizzati.
        *   Aiutano a identificare e prioritizzare la correzione di crash che avvengono su specifici dispositivi o in condizioni particolari.
    *   **Logging In-Game:** Implementare un sistema di logging custom (come visto per i servizi) che possa essere abilitato/disabilitato in build di debug e che possa scrivere su file o inviare log a un server per analisi remote, specialmente per problemi difficili da riprodurre.

6.  **Profilazione su Dispositivo Reale:**
    *   **Unity Profiler:** Connettere l'Unity Editor a una build di sviluppo in esecuzione su un dispositivo fisico (via USB per Android, via rete per iOS/Android).
    *   **Xcode Instruments (iOS) / Android Studio Profiler (Android):** Per analisi di performance e memoria a livello nativo, più approfondite di quelle offerte dall'Unity Profiler per certi aspetti (es. utilizzo GPU, impatto processi di sistema).
    *   **Obiettivo:** Identificare colli di bottiglia specifici del dispositivo non evidenti in Editor o su altre piattaforme.

7.  **Automazione dei Test (Considerazione Futura):**
    *   Per test di regressione su larga scala, considerare framework di UI automation come Appium, Unity Test Framework (per test Play Mode eseguibili su device), o specifici strumenti offerti dai servizi cloud di testing.
    *   Richiede un investimento significativo in scrittura e manutenzione degli script di test.

Adottando una combinazione di queste strategie e strumenti, si può ottenere una buona copertura di test cross-device, migliorando la qualità e la stabilità di Chaos Cosmos.io su un'ampia gamma di dispositivi.
