# Guida agli Effetti Audio e Visivi - Chaos Cosmos.io

Questo documento elenca i punti chiave dove effetti sonori (SFX), effetti visivi (VFX), e musica dovrebbero essere integrati per migliorare l'esperienza di gioco ("juice").

## Effetti Globali / Giocatore (`PlanetController`)
*   **Movimento:**
    *   SFX: Loop di propulsione (dinamico con velocità, es. pitch più alto per velocità maggiore). Suono di avvio e arresto del movimento.
    *   VFX: Scia particellare dietro il pianeta, che potrebbe cambiare intensità/lunghezza con la velocità. Effetti di spinta visibili.
*   **Ingestione Collezionabile:**
    *   SFX: Suono di raccolta soddisfacente e chiaro. Potrebbe variare leggermente per dimensione/tipo di collezionabile (es. tono più basso per massa maggiore).
    *   VFX: Particelle che dal collezionabile vanno verso il pianeta. Breve flash o "implosione" del collezionabile. Feedback visivo sull'HUD della massa (es. numero che aumenta con animazione). (`absorbVfx` in `PlanetController` è un placeholder per questo).
*   **Crescita (Superamento Soglie Massa o ogni N ingestioni):**
    *   SFX: Suono distintivo di "level-up" della massa, che dia un senso di progressione e potenza.
    *   VFX: Breve flash, onda d'urto, o espansione visiva temporanea del pianeta.
*   **Scudo (PowerUp):**
    *   SFX: Suono di attivazione chiaro e distintivo. Suono di disattivazione. (Futuro) Suono di impatto quando lo scudo assorbe un colpo.
    *   VFX: Shader/effetto particellare per uno scudo visibile attorno al pianeta (es. una sfera energetica trasparente o un pattern esagonale). Effetto di impatto localizzato se colpito.
*   **Speed Boost (PowerUp):**
    *   SFX: Suono di attivazione energico. Suono di disattivazione. Eventuale loop leggero durante l'effetto.
    *   VFX: Scia di movimento più intensa/lunga, aura colorata attorno al pianeta, o distorsione leggera del background.
*   **Mass Absorption Boost (PowerUp):**
    *   SFX: Suono di attivazione "magnetico" o "potenziante". Suono di disattivazione.
    *   VFX: Aura o particelle speciali che appaiono attorno al pianeta o vengono emesse durante l'ingestione mentre l'effetto è attivo.
*   **Repel Bots (PowerUp):**
    *   SFX: Suono per ogni impulso di repulsione (es. un "whoosh" energetico o un suono "thump").
    *   VFX: Onda d'urto visiva (es. un anello che si espande) emanata dal pianeta ad ogni impulso.

## Oggetti di Gioco
*   **Collezionabili:**
    *   VFX: Leggero pulsing, brillio, o alone colorato per renderli più visibili e attrattivi. Animazione di spawn (es. fade-in o un piccolo effetto particellare).
*   **PowerUp Pickups:**
    *   VFX: Effetto visivo distintivo e più evidente per ogni tipo di power-up. Brillio intenso, forse un'icona che ruota o pulsa sopra di esso. Animazione di spawn e un effetto più marcato alla raccolta.
*   **Bot Planets:**
    *   SFX: Suoni di movimento specifici per tipo di bot, se si vuole distinguerli dal giocatore (es. TankBot più profondo, ScoutBot più acuto). Suono di spawn.
    *   VFX: Scie di movimento (magari di colore diverso dal giocatore). Effetti di spawn (es. un "warp-in" o materializzazione).

## Interfaccia Utente (UI)
*   **Bottoni (Generali):** SFX standard ma piacevole per click/pressione. Feedback visivo (cambio colore, scala).
*   **Pannelli (Menu):** SFX per transizioni di apertura/chiusura dei pannelli principali (es. un "whoosh" leggero o un suono di interfaccia).
*   **Azioni Significative UI:**
    *   Acquisto Upgrade: SFX di successo distintivo. SFX di fallimento (es. XP insufficienti).
    *   Claim Ricompensa Pass: SFX di ricompensa ottenuta, celebrativo.
    *   Acquisto Premium Pass: SFX particolarmente positivo e di valore.
    *   Selezione Lingua: Click leggero.
    *   Slider Volume: Feedback sonoro mentre si trascina (opzionale) o un suono di conferma.
*   **Notifiche UI:** SFX per messaggi importanti, avvisi, o errori mostrati all'utente.

## Musica
*   **Menu Principale (`MainMenuScene`):** Traccia musicale d'atmosfera, non troppo invasiva, che inviti all'esplorazione delle opzioni.
*   **Gameplay (`PlanetTestScene` / Scena di Gioco):** Traccia/e musicali più energiche e dinamiche. Potrebbe cambiare intensità o tema in base allo stato del gioco (es. pochi secondi rimasti, molti nemici, bioma attivo). Deve essere loopabile in modo fluido.
*   **Fine Partita (`ResultsPanelUI`):** Breve stinger musicale che rifletta il risultato (es. più trionfale per alta massa, più neutro o sommesso altrimenti). Questo potrebbe sovrapporsi o sostituire la musica di gameplay che fa fade out.

## Eventi di Gioco / Stinger
*   **Inizio Partita (`GameManagerService.StartNewMatch`):** Stinger musicale/SFX breve e incisivo per segnalare l'inizio dell'azione.
*   **Fine Partita (Timer Scaduto o Fine Prematura):** Stinger/SFX che segnali la fine della partita, prima di mostrare il pannello risultati.
*   **Level Up Giocatore (Progressione):** SFX celebrativo e forse un breve VFX sull'HUD dell'XP/livello.
*   **Sblocco Tier Battle Pass:** SFX e forse VFX sull'UI del Battle Pass quando un nuovo tier viene sbloccato.
*   **Attivazione Evento LiveOps/Bioma:** (Futuro) Un suono o un cambio musicale sottile potrebbe indicare l'inizio o la fine di un evento di gioco o l'entrata in un bioma con regole speciali.

## Note Generali sull'Implementazione
*   **`AudioManagerService`:** È fortemente raccomandata la creazione di un servizio dedicato per gestire la riproduzione centralizzata di SFX (con pooling di AudioSource) e Musica (con transizioni), e per applicare i controlli di volume globali definiti nelle impostazioni.
*   **VFX:** Possono essere realizzati con Unity Particle Systems, Shader Graph per effetti di superficie/aura, o animazioni Sprite/UI.
*   **Addressables per Effetti:** Considerare di rendere Addressable gli asset VFX e SFX più pesanti o tematici (es. effetti specifici di un bioma) per caricarli dinamicamente.
*   **Consistenza:** Mantenere uno stile audio-visivo coerente in tutto il gioco.
*   **Feedback Utente:** Ogni interazione significativa dell'utente dovrebbe avere un feedback audio e/o visivo.
*   **Performance:** Ottimizzare VFX (specialmente particellari) per non impattare negativamente le performance, soprattutto su dispositivi mobile. Usare texture atlas, limitare overdraw, etc.

Questo documento è una guida iniziale e andrà espanso e raffinato con l'evoluzione del design e dell'implementazione del gioco.
