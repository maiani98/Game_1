# Note sulla Compliance Privacy - Chaos Cosmos.io

Questo documento riassume le considerazioni sulla privacy per il gioco "Chaos Cosmos.io" basate sullo stato di sviluppo attuale (con molti servizi chiave simulati) e fornisce raccomandazioni per la preparazione a un rilascio pubblico.
**Nota Importante:** Queste sono note tecniche e non costituiscono consulenza legale. È sempre raccomandata una revisione legale formale prima del rilascio.

## 1. Gestione del Consenso Utente (`ConsentManager`)

*   **Implementazione Attuale:**
    *   `ConsentManager.cs` simula la richiesta di consenso GDPR (generico) e ATT (per iOS).
    *   Le scelte sono salvate in `PlayerPrefs` (`PlayerPrefsKeys.GDPR_CONSENT_KEY`, `PlayerPrefsKeys.ATT_STATUS_KEY`).
    *   I servizi `AnalyticsService` e `AdsFacade` (per ads personalizzate) sono progettati per interrogare `IConsentService` e rispettare le scelte.
*   **Punti di Attenzione e Raccomandazioni per Produzione:**
    *   **Privacy Policy Dettagliata:** È obbligatorio fornire una Privacy Policy chiara, completa e facilmente accessibile (link nel popup GDPR, in una sezione "Impostazioni/Info" del gioco). Deve spiegare quali dati vengono raccolti, perché, come vengono usati, conservati, e i diritti dell'utente (accesso, rettifica, cancellazione).
    *   **Granularità del Consenso GDPR:** Il consenso GDPR attuale è binario (Dato/Negato). Valutare se offrire scelte più granulari (es. consenso per analytics essenziali vs. ads personalizzate vs. altre forme di tracciamento). Le SDK dei CMP (Consent Management Platform) spesso gestiscono questo.
    *   **Revoca del Consenso:** Implementare una sezione nelle impostazioni del gioco dove l'utente possa facilmente visualizzare e modificare le proprie preferenze di consenso in qualsiasi momento. La modifica deve essere altrettanto facile quanto dare il consenso.
    *   **Specificità Regionale GDPR:** La necessità e la forma del popup GDPR possono dipendere dalla regione dell'utente. SDK reali dei CMP aiutano a gestire questa logica.
    *   **Test Flussi Reali:** Testare approfonditamente i flussi di consenso con le SDK reali dei CMP o con le implementazioni manuali su dispositivi reali e diverse versioni OS.
    *   **Logica ATT (iOS):** Assicurarsi che la stringa `NSUserTrackingUsageDescription` sia correttamente configurata nel file `Info.plist` della build iOS, spiegando chiaramente perché l'app richiede il tracciamento. La chiamata effettiva a `ATTrackingManager.RequestTrackingAuthorization` va implementata e gestita correttamente (può essere richiesta solo una volta).

## 2. Analytics (`AnalyticsService`)

*   **Implementazione Attuale:**
    *   `AnalyticsService.cs` è un placeholder che logga eventi in console.
    *   Rispetta il flag `HasGivenGDPRConsentForAnalytics()` da `IConsentService` prima di inviare (loggare) eventi.
*   **Punti di Attenzione e Raccomandazioni per Produzione:**
    *   **Scelta e Configurazione SDK:** Selezionare un provider di Analytics (es. Unity Analytics, Firebase Analytics, GameAnalytics, Amplitude, Mixpanel). Configurarlo attentamente per:
        *   Rispettare le scelte di consenso (es. inizializzare l'SDK solo dopo consenso, passare flag per limitare la raccolta dati o disabilitare identificativi utente/dispositivo se il consenso è negato o parziale).
        *   Definire chiaramente quali eventi e parametri vengono tracciati. Evitare la raccolta eccessiva.
    *   **Anonimizzazione/Pseudonimizzazione:** Dare priorità alla raccolta di dati aggregati e anonimizzati/pseudonimizzati. Evitare di tracciare PII (Personally Identifiable Information) non necessarie per le analisi.
    *   **Data Retention:** Comprendere le policy di conservazione dei dati del provider scelto e informare gli utenti nella Privacy Policy.
    *   **Opt-out da Analytics:** Oltre al consenso iniziale, considerare se offrire un'opzione di opt-out completo da analytics nelle impostazioni, se richiesto dalle normative o per trasparenza.

## 3. Pubblicità (`AdsFacade`)

*   **Implementazione Attuale:**
    *   `AdsFacade.cs` simula la visualizzazione di annunci e il caricamento.
    *   Rispetta il flag "Remove Ads" (da `IAPFacade` e `PlayerPrefs`).
    *   Verifica `HasGivenGDPRConsentForPersonalizedAds()` e `CanTrackDeviceForAds()` (che considera ATT) prima di (simulare) la richiesta di annunci personalizzati.
*   **Punti di Attenzione e Raccomandazioni per Produzione:**
    *   **Scelta Network Pubblicitari e SDK:** Scegliere network (es. AdMob, Unity Ads, AppLovin, IronSource) e integrare le loro SDK.
    *   **Configurazione Consenso SDK Ads:** Tutte le principali SDK pubblicitarie richiedono di passare lo stato del consenso GDPR e ATT (e altri eventuali framework come TCF) per determinare se mostrare annunci personalizzati o non personalizzati (NPA), o se limitare il tracciamento. Questo è cruciale e va configurato con attenzione per ogni SDK.
    *   **IDFA (iOS) e GAID (Android):** L'IDFA (iOS) può essere usato per ads personalizzate solo se `TrackingAuthorizationStatus` è `Authorized`. Per Android, il GAID è generalmente accessibile ma l'utente può resettarlo o limitarne l'uso per la personalizzazione.
    *   **Child-Directed Apps (COPPA, KSA, etc.):** Se l'app è diretta a bambini (o potrebbe esserlo), ci sono regole molto più stringenti sulla pubblicità (tipi di annunci, tracciamento) e la raccolta dati. (Non sembra il caso qui, ma da tenere a mente per progetti futuri).

## 4. Acquisti In-App (`IAPFacade`)

*   **Implementazione Attuale:**
    *   `IAPFacade.cs` simula acquisti. L'acquisto "Remove Ads" è salvato in `PlayerPrefs`.
*   **Punti di Attenzione e Raccomandazioni per Produzione:**
    *   **Integrazione SDK IAP Reale:** Usare Unity IAP (raccomandato per cross-platform) o SDK specifiche della piattaforma.
    *   **Validazione Ricevute:** Per prodotti non consumabili importanti (come "Remove Ads") o per valute, implementare la validazione delle ricevute lato server per prevenire frodi. Unity IAP fornisce strumenti e servizi per questo, o si possono usare soluzioni custom.
    *   **Sicurezza:** Non fare affidamento solo su `PlayerPrefs` per lo stato di acquisti permanenti se la sicurezza è una preoccupazione elevata; la validazione server-side e/o l'offuscamento dei dati locali sono più sicuri.
    *   **Interfaccia Utente Chiara:** Prezzi, descrizioni, e processo di acquisto devono essere chiari e trasparenti. Gestire errori di acquisto e fornire feedback appropriato all'utente.
    *   **Ripristino Acquisti:** Implementare correttamente la funzionalità "Restore Purchases" (obbligatoria su iOS per prodotti non-consumabili e sottoscrizioni auto-rinnovabili).

## 5. Salvataggio Dati Locali (`PlayerPrefs`)

*   **Implementazione Attuale:**
    *   `PlayerPrefs` è usato per progressi del giocatore (XP, livelli upgrade), stato del pass battaglia, scelte di consenso, e acquisto "Remove Ads".
*   **Punti di Attenzione e Raccomandazioni per Produzione:**
    *   **Sicurezza/Modificabilità:** `PlayerPrefs` non sono sicuri e possono essere facilmente modificati su dispositivi rooted/jailbroken o con strumenti di editing della memoria. Se la prevenzione del cheating sulla progressione è importante (es. per future classifiche o multiplayer), considerare soluzioni di salvataggio più sicure (es. file criptati, offuscamento dei dati, o idealmente stato autorevole server-side per dati critici).
    *   **Backup (Cloud Save):** Per migliorare l'esperienza utente, considerare l'integrazione con servizi di cloud save (es. Google Play Games Saved Games, iCloud Key-Value Storage/CloudKit) per permettere il trasferimento dei progressi tra dispositivi o il recupero dopo disinstallazione.
    *   **Informare l'Utente:** Se i dati sono solo locali, l'utente dovrebbe essere informato (nella UI o Privacy Policy) del rischio di perdita dati se l'app viene disinstallata o i dati dell'app vengono cancellati.

## 6. Notifiche (`NotificationService`)

*   **Implementazione Attuale:**
    *   `NotificationService.cs` gestisce la schedulazione di notifiche locali.
    *   L'inizializzazione gestisce la creazione di canali (Android) e logga per i permessi.
*   **Punti di Attenzione e Raccomandazioni per Produzione:**
    *   **Permessi Espliciti (iOS):** Per iOS 10+, anche per le notifiche locali che includono suoni, badge o alert, è best practice (e spesso necessario per certe funzionalità) richiedere esplicitamente l'autorizzazione all'utente usando `iOSNotificationCenter.RequestAuthorization()` (o l'equivalente API del pacchetto Mobile Notifications).
    *   **Contenuto e Frequenza:** Assicurarsi che le notifiche siano utili, pertinenti, e non eccessive. L'utente dovrebbe poterle gestire (se possibile, tramite impostazioni in-app o di sistema).
    *   **Test su Dispositivi Reali:** Le notifiche sono molto dipendenti dalla piattaforma e dalla versione OS; testare approfonditamente su una varietà di dispositivi.
    *   **Notifiche Push Remote:** Se si implementeranno in futuro, queste richiedono un backend, un provider di servizi push (es. Firebase Cloud Messaging, OneSignal), e una gestione attenta del consenso e dei token dispositivo, oltre a considerazioni sulla sicurezza del server.

## 7. Dati Utente Generali e Minimizzazione

*   **Principio di Minimizzazione:** Raccogliere e conservare solo i dati strettamente necessari per il funzionamento del gioco e dei servizi scelti.
*   **Accesso ai Dati:** Implementare meccanismi (se richiesto dalle normative) per permettere agli utenti di richiedere l'accesso ai propri dati o la loro cancellazione. Questo è spesso gestito dal provider di backend o analytics, ma l'app deve facilitare la richiesta.

## Conclusioni e Prossimi Passi Raccomandati

*   **Privacy Policy:** Redigere e rendere accessibile una Privacy Policy completa e trasparente.
*   **Flussi di Consenso Reali:** Sostituire `ConsentManager` simulato con un'implementazione che usi un CMP validato o le API native per GDPR e ATT. Testare attentamente.
*   **SDK Terze Parti:** Quando si integrano SDK reali per Analytics, Ads, IAP, ecc., rivedere attentamente la loro documentazione sulla privacy, le opzioni di configurazione del consenso, e i dati che raccolgono. Configurarli per rispettare le scelte di consenso dell'utente.
*   **Revisione Legale:** Consultare un esperto legale specializzato in privacy e normative dei videogiochi per assicurare la piena conformità con tutte le normative applicabili (GDPR, CCPA, COPPA, normative specifiche delle piattaforme Apple/Google, ecc.) prima del lancio. Questo è particolarmente vero se si prevede di monetizzare o avere una significativa base di utenti internazionali.
*   **Aggiornamenti Continui:** Le normative sulla privacy evolvono. Mantenersi aggiornati e rivedere periodicamente le pratiche di gestione dei dati.
```
