using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SceneLoadingTests
{
    [UnityTest]
    public IEnumerator SceneLoading_PlanetTestScene_LoadsAndHasPlanet()
    {
        // Carica la scena
        SceneManager.LoadScene("PlanetTestScene"); // Assicurati che sia nelle Build Settings

        // Aspetta un frame per permettere alla scena di caricarsi e Start/Awake di essere chiamati
        yield return null;

        // Verifica che la scena caricata sia quella giusta
        Assert.AreEqual("PlanetTestScene", SceneManager.GetActiveScene().name);

        // Verifica che esista un GameObject con il PlanetController (assumendo che sia taggato "Player" o abbia un nome specifico)
        // Per semplicità, cerchiamo un GameObject che abbia il PlanetController.
        // In un test reale, si potrebbe cercare un tag o un nome specifico.
        var planet = GameObject.FindObjectOfType<ChaosCosmos.Gameplay.PlanetController>();
        Assert.IsNotNull(planet, "PlanetController non trovato nella scena PlanetTestScene.");

        // Ulteriori asserzioni potrebbero verificare lo stato iniziale del pianeta, se necessario.
        yield return null;
    }
}
