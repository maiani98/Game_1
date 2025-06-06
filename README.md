# Chaos Cosmos.io

Questo progetto è un gioco sviluppato con **Unity 2022.3 LTS**. Per aprirlo è consigliato utilizzare l'ultima versione disponibile della linea 2022.3 (es. 2022.3.20f1 o successiva) tramite Unity Hub.

## Avvio rapido
1. Clona il repository.
2. Aggiungi la cartella del progetto in Unity Hub e seleziona la versione 2022.3 LTS installata.
3. Carica la scena iniziale da `Assets/Scenes` (ad esempio `BootScene` o `MainMenuScene`).
4. Premi **Play** per avviare il gioco nell'Editor.

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
