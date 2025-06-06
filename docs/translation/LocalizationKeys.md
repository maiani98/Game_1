# Chiavi di Localizzazione Aggiuntive - Chaos Cosmos.io

Questo documento elenca le nuove chiavi di localizzazione e le relative traduzioni placeholder (EN, IT, DE) per i contenuti aggiunti di recente (Power-Up, concetti di Biomi e Bot).

**Istruzioni per l'Utente:**
1.  Aprire la String Table principale del progetto (es. `GameUIText` situata in `Assets/Localization/Tables/StringTables/` o percorso simile).
2.  Aggiungere le seguenti nuove chiavi e le rispettive traduzioni nelle colonne per English (en), Italiano (it), e Tedesco (de).

---

## Nuovi Power-Up

**Nota:** I campi `effectName` e `description` negli ScriptableObject `PowerUpEffect` (es. `ShieldPowerUpEffect.cs`) contengono attualmente stringhe dirette (es. "Shield", "Grants temporary invulnerability."). Per localizzare questi testi, si dovrebbe:
1.  Modificare gli ScriptableObject `PowerUpEffect` per contenere chiavi di localizzazione (es. `nameLocalizationKey = "#powerup_shield_name"`) invece di testo diretto.
2.  La UI che mostra i dettagli di un power-up (se esistente) dovrebbe usare `LocalizationSettings.StringDatabase.GetLocalizedStringAsync(powerUpData.nameLocalizationKey)` per caricare il testo localizzato.
Le chiavi sottostanti sono fornite come se questo refactoring fosse stato fatto, o per una UI che descrive i power-up usando queste chiavi.

### ShieldPowerUpEffect
*   **Chiave:** `powerup_shield_name`
    *   EN: "Shield"
    *   IT: "Scudo"
    *   DE: "Schild"
*   **Chiave:** `powerup_shield_desc`
    *   EN: "Grants temporary protection from harm."
    *   IT: "Conferisce protezione temporanea dai danni."
    *   DE: "Gewährt vorübergehenden Schutz vor Schaden."

### MassAbsorptionBoostEffect
*   **Chiave:** `powerup_massboost_name`
    *   EN: "Mass Boost"
    *   IT: "Boost Massa"
    *   DE: "Massenboost"
*   **Chiave:** `powerup_massboost_desc`
    *   EN: "Temporarily increases mass gained from collectibles."
    *   IT: "Aumenta temporaneamente la massa ottenuta dai collezionabili."
    *   DE: "Erhöht vorübergehend die von Sammlerstücken gewonnene Masse."

### RepelBotsPowerUpEffect
*   **Chiave:** `powerup_repelbots_name`
    *   EN: "Bot Repel"
    *   IT: "Respingi Bot"
    *   DE: "Bot-Abwehr"
*   **Chiave:** `powerup_repelbots_desc`
    *   EN: "Periodically pushes nearby bots away."
    *   IT: "Respinge periodicamente i bot vicini."
    *   DE: "Stößt regelmäßig nahegelegene Bots weg."

---

## Biomi Concettuali (per futura UI)

Queste chiavi sarebbero usate se ci fosse una UI che nomina o descrive i biomi attivi.

### Bioma: Campo di Asteroidi Denso
*   **Chiave:** `biome_asteroid_field_name`
    *   EN: "Dense Asteroid Field"
    *   IT: "Campo di Asteroidi Denso"
    *   DE: "Dichtes Asteroidenfeld"
*   **Chiave:** `biome_asteroid_field_desc` (Opzionale)
    *   EN: "Navigate carefully through the dense asteroids. Increased small collectibles."
    *   IT: "Naviga con cautela nel denso campo di asteroidi. Aumento dei collezionabili piccoli."
    *   DE: "Navigiere vorsichtig durch das dichte Asteroidenfeld. Erhöhte Anzahl kleiner Sammlerstücke."

### Bioma: Nebulosa Elettrica
*   **Chiave:** `biome_electric_nebula_name`
    *   EN: "Electric Nebula"
    *   IT: "Nebulosa Elettrica"
    *   DE: "Elektrischer Nebel"
*   **Chiave:** `biome_electric_nebula_desc` (Opzionale)
    *   EN: "Hazardous charged zones and empowered shield power-ups."
    *   IT: "Zone cariche pericolose e power-up Scudo potenziati."
    *   DE: "Gefährliche geladene Zonen und verstärkte Schild-Power-Ups."

### Bioma: Cimitero Spaziale Antico
*   **Chiave:** `biome_ship_graveyard_name`
    *   EN: "Ancient Ship Graveyard"
    *   IT: "Cimitero Astronavi Antico"
    *   DE: "Antiker Schiffsfriedhof"
*   **Chiave:** `biome_ship_graveyard_desc` (Opzionale)
    *   EN: "Salvage valuable debris but beware of scavenger bots."
    *   IT: "Recupera detriti di valore ma fai attenzione ai bot spazzini."
    *   DE: "Berge wertvolle Trümmer, aber hüte dich vor Plünderer-Bots."

---

## Tipi di Bot Concettuali (per futura UI)

Queste chiavi sarebbero usate se ci fosse una UI che nomina i tipi di bot (es. kill feed, informazioni).

### Bot: TankBot
*   **Chiave:** `bot_type_tank_name`
    *   EN: "Tank Bot"
    *   IT: "Bot Tank"
    *   DE: "Panzer Bot"

### Bot: ScoutBot
*   **Chiave:** `bot_type_scout_name`
    *   EN: "Scout Bot"
    *   IT: "Bot Esploratore"
    *   DE: "Späher Bot"

---

**Nota:** La gestione effettiva della visualizzazione di questi testi localizzati dipenderà da come e dove queste informazioni verranno presentate all'utente nell'interfaccia di gioco. Per i `PowerUpEffect`, se i loro campi `effectName` e `description` devono essere localizzati, tali campi dovrebbero contenere queste chiavi (es., `#powerup_shield_name`) e la UI dovrebbe utilizzare un componente `LocalizeStringEvent` o caricare la stringa tramite script. Per questo task, la modifica degli ScriptableObject `PowerUpEffect` per usare chiavi invece di testo diretto non è stata effettuata, ma le chiavi sono definite qui per riferimento futuro.
