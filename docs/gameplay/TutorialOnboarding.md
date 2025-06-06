# Flusso Tutorial (Onboarding) - Chaos Cosmos.io

Questo documento descrive il flusso di onboarding per i nuovi giocatori, gestito (in modo simulato) da `TutorialManagerService`.

## Obiettivo del Tutorial
Introdurre le meccaniche di base del gioco in modo graduale e contestuale durante la prima partita del giocatore.

## Passaggi del Tutorial

**Nota:** L'implementazione attuale di `TutorialManagerService` logga principalmente questi passaggi. La UI e gli highlight specifici non sono implementati.

1.  **Passo 1: Benvenuto e Movimento**
    *   **ID Univoco:** `TUT_STEP_WELCOME_MOVE`
    *   **Trigger:** Inizio della prima partita per un nuovo giocatore (tutorial non ancora completato).
    *   **Azione UI (Simulata):**
        *   Mostra un messaggio di benvenuto.
        *   Spiega come muovere il pianeta (es. "Trascina sullo schermo per muoverti").
        *   Potrebbe evidenziare l'area di gioco o il pianeta del giocatore.
    *   **Condizione di Completamento (Simulata):** Il giocatore muove il proprio pianeta per una certa distanza o per un breve periodo.
        *   *Sistema Esterno che chiama:* `TutorialManagerService.CompleteStep("TUT_STEP_WELCOME_MOVE")` dopo aver rilevato il movimento.

2.  **Passo 2: Ingestione e Crescita**
    *   **ID Univoco:** `TUT_STEP_INGEST`
    *   **Trigger:** Completamento del Passo 1. Idealmente, un `Collectible` è già visibile o spawna vicino al giocatore.
    *   **Azione UI (Simulata):**
        *   Messaggio: "Ingerisci i globi più piccoli per crescere!"
        *   Potrebbe evidenziare un `Collectible` vicino.
        *   Potrebbe evidenziare l'indicatore di Massa nell'HUD.
    *   **Condizione di Completamento (Simulata):** Il giocatore ingerisce 2-3 `Collectible`.
        *   *Sistema Esterno che chiama:* `TutorialManagerService.CompleteStep("TUT_STEP_INGEST")` dopo N ingestioni.

3.  **Passo 3: Obiettivo del Gioco (Opzionale, se non già chiaro)**
    *   **ID Univoco:** `TUT_STEP_OBJECTIVE`
    *   **Trigger:** Completamento del Passo 2.
    *   **Azione UI (Simulata):**
        *   Messaggio: "L'obiettivo è diventare il pianeta più grande! Ingerisci per aumentare la tua massa."
        *   Potrebbe mostrare brevemente il timer di partita (se non sempre visibile).
    *   **Condizione di Completamento (Simulata):** Timeout breve (es. 5 secondi) o un click/tap dell'utente per confermare.
        *   *Sistema Esterno che chiama:* `TutorialManagerService.CompleteStep("TUT_STEP_OBJECTIVE")`.

4.  **Passo 4: Introduzione Power-Up (Opzionale, se i Power-Up appaiono presto)**
    *   **ID Univoco:** `TUT_STEP_POWERUP_INTRO`
    *   **Trigger:** Il primo `PowerUp` spawna ed è vicino/visibile al giocatore (e il Passo 3 è completo).
    *   **Azione UI (Simulata):**
        *   Messaggio: "Questi sono Power-Up! Raccoglili per ottenere bonus temporanei." (Mostra nome/icona del power-up specifico se possibile).
        *   Potrebbe evidenziare il `PowerUp` spawnato.
    *   **Condizione di Completamento (Simulata):** Il giocatore raccoglie il `PowerUp`.
        *   *Sistema Esterno che chiama:* `TutorialManagerService.CompleteStep("TUT_STEP_POWERUP_INTRO")` dopo la raccolta.

5.  **Passo 5: Introduzione Bot (Opzionale, se i Bot appaiono presto)**
    *   **ID Univoco:** `TUT_STEP_BOT_INTRO`
    *   **Trigger:** Il primo `BotPlanet` entra nel campo visivo del giocatore o interagisce con esso (e il Passo X precedente è completo).
    *   **Azione UI (Simulata):**
        *   Messaggio: "Attenzione ai pianeti controllati da altri! Ingerisci quelli più piccoli di te, evita quelli più grandi."
        *   Potrebbe evidenziare il Bot.
    *   **Condizione di Completamento (Simulata):** Timeout breve, o il giocatore si allontana/avvicina al bot.
        *   *Sistema Esterno che chiama:* `TutorialManagerService.CompleteStep("TUT_STEP_BOT_INTRO")`.

6.  **Passo N: Fine Tutorial**
    *   **ID Univoco:** `TUT_STEP_FINAL` (o l'ultimo step significativo)
    *   **Trigger:** Completamento dell'ultimo passo del tutorial (es. Passo 4 o 5).
    *   **Azione UI (Simulata):**
        *   Messaggio: "Ottimo! Ora sei pronto. Sopravvivi e cresci il più possibile!"
    *   **Azione di Sistema:**
        *   `TutorialManagerService.MarkTutorialAsFullyCompleted()` viene chiamato.
        *   Lo stato "tutorial completato" viene salvato in `PlayerPrefs`.

## Flusso Logico (Semplificato in `TutorialManagerService`)

L'attuale `TutorialManagerService` ha una logica di progressione molto semplice in `CompleteStep()`:
*   `TUT_STEP_WELCOME_MOVE` -> triggera `TUT_STEP_INGEST`
*   `TUT_STEP_INGEST` -> triggera `TUT_STEP_OBJECTIVE`
*   `TUT_STEP_OBJECTIVE` -> triggera `TUT_STEP_POWERUP_INTRO`
*   `TUT_STEP_POWERUP_INTRO` -> chiama `MarkTutorialAsFullyCompleted()`

Questo è un placeholder e un sistema reale avrebbe una macchina a stati più robusta o scriptable objects per definire ogni step, le sue condizioni di trigger/completamento, e le azioni UI.

## Integrazione
*   `GameManagerService.StartNewMatch()` dovrebbe chiamare `ITutorialService.StartTutorialFlowIfNotCompleted()`.
*   Vari sistemi di gioco (movimento del giocatore, ingestione, raccolta power-up) dovrebbero notificare al `ITutorialService` il completamento degli step rilevanti chiamando `CompleteStep("STEP_ID")`.
*   La UI del Tutorial (non implementata in questo task) si registrerebbe a eventi del `ITutorialService` o verrebbe controllata da esso per mostrare/nascondere messaggi e highlights.
