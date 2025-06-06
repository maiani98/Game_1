# Note sul Bilanciamento - Chaos Cosmos.io (Revisione Iniziale)

Questo documento contiene una revisione concettuale e suggerimenti iniziali per il bilanciamento dei parametri di gioco. Un bilanciamento effettivo richiederà playtesting approfondito e iterazioni.

## Power-Ups

Le durate e i valori dei Power-Up dovrebbero essere testati per il loro impatto sul loop di gameplay e sulla percezione di valore da parte del giocatore. Considerare anche la frequenza di spawn di ciascun Power-Up.

*   **`SpeedBoostEffect`**
    *   **Parametri Attuali (SO Default):** `duration = 5s`, `speedMultiplier = 1.5x`
    *   **Remote Config (A/B Test):** `ab_test_SpeedBoostEffect_duration_seconds` (es. `7.0s`)
    *   *Considerazioni:* Un moltiplicatore 1.5x è significativo. La durata (5s o 7s) deve essere sufficiente per permettere al giocatore di sfruttare il bonus ma non così lunga da rendere banale la raccolta o la fuga per troppo tempo.
    *   *Testare:*
        *   Il giocatore riesce a compiere azioni significative (es. raggiungere collezionabili distanti, sfuggire a un bot) durante l'effetto?
        *   L'effetto termina troppo presto o dura troppo a lungo, sbilanciando la sfida?
        *   L'A/B test sulla durata fornisce insight utili?

*   **`ShieldPowerUpEffect`**
    *   **Parametri Attuali (SO Default):** `duration = 5s`
    *   **Remote Config (A/B Test):** Es. `ab_test_ShieldEffect_duration_seconds` (se si vuole testare)
    *   *Considerazioni:* Attualmente, lo scudo non ha una meccanica di "danno" da bloccare, quindi la sua utilità è limitata (es. potrebbe ignorare il respingimento da parte di altri bot se implementato, o effetti negativi di biomi). Se il danno venisse implementato, 5s di invulnerabilità sono molti.
    *   *Suggerimenti (con meccanica di danno):*
        *   Considerare una durata più breve (es. 3s) o un numero limitato di "colpi assorbiti".
        *   Se lo scudo ha un effetto secondario (es. leggero respingimento al contatto), la durata attuale potrebbe essere interessante.
    *   *Testare:* Percezione di utilità attuale. Se il danno viene aggiunto, testare l'impatto sulla sopravvivenza.

*   **`MassAbsorptionBoostEffect`**
    *   **Parametri Attuali (SO Default):** `duration = 8s` (ipotetico, non specificato prima, la base è 5s), `massMultiplierBonus = 0.5f` (+50%)
    *   **Remote Config (A/B Test):** Es. `ab_test_MassAbsorptionBoostEffect_duration_seconds`, `ab_test_MassAbsorptionBoostEffect_bonus_value`
    *   *Considerazioni:* Un bonus del +50% sulla massa è un forte incentivo. Una durata di 8s permette diverse ingestioni.
    *   *Testare:*
        *   Quanto accelera la crescita rispetto al normale? È troppo? È soddisfacente?
        *   Impatto sull'economia XP (dato che XP è calcolato anche sulla massa finale). Se questo power-up è comune, potrebbe inflazionare l'XP.

*   **`RepelBotsPowerUpEffect`**
    *   **Parametri Attuali (SO Default):** `duration = 5s`, `repelRadius = 15f`, `repelForce = 1000f`, `pulseInterval = 0.5s`
    *   **Remote Config (A/B Test):** Es. per durata, raggio, forza.
    *   *Considerazioni:*
        *   `repelRadius`: 15 unità è un raggio ampio, potrebbe colpire molti bot.
        *   `repelForce`: 1000f con `ForceMode2D.Impulse` è una forza considerevole. Deve essere bilanciata rispetto alla massa dei bot (un TankBot dovrebbe essere meno affetto di uno ScoutBot).
        *   `pulseInterval`: 0.5s crea un effetto "pulsante" frequente.
    *   *Testare:*
        *   Efficacia nel creare spazio sicuro per il giocatore.
        *   I bot vengono spinti troppo lontano (frustrante per il giocatore se voleva interagire) o troppo poco?
        *   I bot più pesanti sono affetti in modo appropriato?
        *   L'effetto "stunlock" sui bot è eccessivo o desiderato?

## Progressione XP e Costi Upgrade (`ProgressManager`)

*   **Curva XP Livello Giocatore:** Attualmente 1000 XP per livello (lineare dopo il livello 1).
    *   *Considerazioni:* Adeguata per l'inizio. Diventa troppo veloce o troppo lenta ai livelli alti?
    *   *Suggerimento:* Implementare una formula configurabile o un `AnimationCurve` per `xpNeededForLevel[currentLevel]` per un controllo più fine. Questo potrebbe essere gestito da `ConfigDataService` o `RemoteConfigService`.
    *   *Esempio Formula:* `XP_per_livello = BaseXP * Math.Pow(FattoreCrescita, LivelloAttuale - 1)`

*   **Costi Upgrade (`UpgradeData`):**
    *   Esempio `SpeedUpgradeLvl1` (ID: `basespeed_lvl_1`): Costo SO 100 XP (potrebbe essere 120 da RC) per +10% velocità, max 3 livelli.
    *   Esempio `InitialMassUpgrade` (ID: `initialmass_lvl_1`): Costo SO 50 XP (potrebbe essere 65 da RC) per +5 massa.
    *   *Considerazioni:*
        *   I costi iniziali devono essere accessibili.
        *   **Scaling dei Costi per Livelli Multipli:** Se un upgrade ha `maxLevel > 1`, come scala il costo? Se `basespeed_lvl_1` costa 100/120, quanto costano `basespeed_lvl_2` e `lvl_3`? Non è definito attualmente.
        *   **Impatto dell'Upgrade:** Un +10% alla velocità è significativo? Un +5 alla massa iniziale è utile?
    *   *Suggerimenti:*
        *   Per upgrade multi-livello, definire una formula di aumento del costo per livello (es. moltiplicativo o additivo). Questo potrebbe essere parte di `UpgradeData` o calcolato da `ProgressManager`.
        *   Bilanciare il costo dell'upgrade con il suo beneficio percepito e la velocità con cui si guadagna XP.
        *   Assicurarsi che gli `upgradeID` usati per Remote Config (`upgrade_{upgradeID}_xpCost`) siano stabili e corretti.

## Battle Pass (`PassManager`)

*   **XP per Tier (`PassTierData`):** Esempio: Tier 1 = 100 XP pass, Tier 2 = 150 XP pass (per un totale di 250 XP pass per sbloccare il tier 2).
    *   *Considerazioni:* La progressione XP per il pass è separata dall'XP del giocatore. Questo è comune. La quantità di XP per tier dovrebbe aumentare gradualmente.
    *   *Suggerimento:* Come per l'XP giocatore, considerare una curva di XP per i tier del pass, specialmente se il pass ha molti tier (es. 50-100).
    *   Le ricompense per tier devono essere bilanciate rispetto allo sforzo per raggiungerle.

## Economia Generale (Valute - Considerazioni Future)

*   *Fonti di Valuta:* Come si ottengono Soft Currency (SC) e Hard Currency (HC)? Missioni, ricompense tier pass, drop rari, acquisti IAP (per HC)?
*   *Utilizzi (Sinks) di Valuta:* Cosa si può comprare? Upgrade (alternativa a XP?), cosmetici, skip tier pass, offerte speciali?
*   *Bilanciamento:* I tassi di guadagno di SC devono essere bilanciati con i costi degli oggetti/servizi acquistabili con SC. HC dovrebbe essere una risorsa premium, ottenibile in piccole quantità gratuitamente o tramite IAP.
*   Evitare inflazione (troppa valuta, nulla da spendere) o deflazione/grind eccessivo (valuta troppo scarsa, tutto troppo costoso).

## Parametri da Remote Config / A/B Test

*   **Bonus XP Iniziale (`ab_test_initialXpBonus_amount`):** Attualmente 50 XP.
    *   *Testare:* Questo bonus accelera significativamente l'inizio del gioco? Rende i primi upgrade troppo facili/veloci?
*   **Durata Power-Up (es. `ab_test_SpeedBoostEffect_duration_seconds`):** Attualmente 7s vs 5s (SO).
    *   *Testare:* La differenza di 2 secondi è percepibile? Cambia significativamente l'utilità del power-up?
*   **Costi Upgrade (es. `upgrade_basespeed_lvl_1_xpCost`):** Attualmente 120 (RC) vs 100 (SO).
    *   *Testare:* Come un aumento del 20% del costo impatta la velocità di progressione del giocatore?

## Considerazioni Generali sul Bilanciamento

*   **Playtesting Iterativo:** Il bilanciamento è un processo che richiede molto playtesting da parte di diversi tipi di giocatori.
*   **Raccolta Dati (Analytics):** Una volta che il gioco è live (o in beta estesa), usare analytics per monitorare:
    *   Tassi di completamento del tutorial.
    *   Velocità di progressione XP giocatore e Battle Pass.
    *   Acquisto e utilizzo degli upgrade.
    *   Utilizzo dei Power-Up.
    *   Punti di difficoltà o frustrazione.
*   **Segmentazione Utenti:** L'analisi dei dati per diversi segmenti di giocatori (nuovi, intermedi, avanzati) può rivelare problemi di bilanciamento specifici.
*   **Flessibilità:** Progettare i sistemi (specialmente tramite `ConfigDataService` e `RemoteConfigService`) per permettere facili aggiustamenti dei parametri di bilanciamento senza dover ricompilare il gioco.

Questo documento serve come punto di partenza. Il bilanciamento dovrà essere rivisto e affinato continuamente durante lo sviluppo e dopo il lancio.
