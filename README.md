# Chaos Cosmos.io

Questo progetto è un gioco sviluppato con **Unity 6.1**. Per aprirlo è consigliato utilizzare la versione `6000.1.6f1` tramite Unity Hub.

## Avvio rapido
1. Clona il repository.
2. Aggiungi la cartella del progetto in Unity Hub e seleziona la versione 6.1 installata (es. `6000.1.6f1`).
3. Carica la scena iniziale da `Assets/Scenes` (ad esempio `BootScene` o `MainMenuScene`).
4. Premi **Play** per avviare il gioco nell'Editor.

## Integrazione API
Il `Bootstrapper` registra automaticamente `ApiService`, un wrapper semplice per
effettuare richieste HTTP al backend. Per impostazione predefinita utilizza
`https://api.example.com` come base URL. Puoi recuperare l'istanza tramite
`ServiceLocator.Get<IApiService>()` e chiamare i metodi `GetAsync` o `PostAsync`
per comunicare con il tuo server.

## Integrazione SDK
Il progetto include implementazioni pronte per **Unity Ads**, **Unity IAP** e **Unity Analytics**.
Assicurati di installare i relativi pacchetti tramite il *Package Manager* prima di compilare:

- `Advertisement` (Unity Ads)
- `Unity Purchasing` (Unity IAP)
- `Unity Services Core` e `Unity Analytics`

Le classi dei servizi sono racchiuse da direttive di compilazione; se i pacchetti non sono installati verranno usate implementazioni di fallback che registrano avvisi nei log.

Nel file `Bootstrapper` i servizi vengono registrati automaticamente tramite le classi
`UnityAdsService`, `UnityIAPService` e `UnityAnalyticsService`. Aggiorna gli ID di gioco per
Unity Ads direttamente nel `Bootstrapper` o tramite *Remote Config* prima di rilasciare.

## Documentazione e Testing
Nella cartella `docs/testing` sono presenti risorse utili:
- `GoldenRunScenario.md` illustra uno scenario di gameplay di 120 secondi per verifiche di performance e QA.
- `CrossDeviceTestMatrix.md` elenca dispositivi di riferimento e aree chiave da testare.
- `MemoryLeakAnalysisGuide.md` spiega come utilizzare l'Unity Profiler per individuare possibili memory leak.

Per generare la documentazione API è possibile usare **DocFX** eseguendo `docfx docfx.json` (richiede DocFX installato).

### Esecuzione automatica dei test
Per i test Unity il comando `scripts/run_unity_tests.sh` cerca automaticamente l'eseguibile `unity` nel PATH se la variabile `UNITY_PATH` non è definita. In alternativa imposta `UNITY_PATH` con il percorso completo e lancia:

```bash
scripts/run_unity_tests.sh
```

Il comando avvierà i test Edit Mode e Play Mode in modalità batch e salverà i risultati in `TestResults.xml`.

Sono inoltre presenti test di esempio per altre piattaforme:

- **Node.js**: esegui `npm test` (mostrerà un messaggio perché non sono previsti test JavaScript).
- **Python**: esegui `pytest` nella cartella principale; è incluso un test segnaposto.
- **.NET**: nella cartella `DotNetTests` è presente un progetto con test xUnit. Per eseguirli usa `dotnet test DotNetTests/DotNetTests.csproj`.
