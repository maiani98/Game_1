using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ChaosCosmos.Core.Constants; // Aggiunto using per SceneNames

public class SceneLoadingTests
{
    [UnityTest]
    public IEnumerator SceneLoading_PlanetTestScene_LoadsAndHasPlanet()
    {
        // Carica la scena
        SceneManager.LoadScene(SceneNames.GAMEPLAY_SCENE_PLACEHOLDER); // Usa SceneNames

        // Aspetta un frame per permettere alla scena di caricarsi e Start/Awake di essere chiamati
        yield return null;

        // Verifica che la scena caricata sia quella giusta
        Assert.AreEqual(SceneNames.GAMEPLAY_SCENE_PLACEHOLDER, SceneManager.GetActiveScene().name); // Usa SceneNames

        // Verifica che esista un GameObject con il PlanetController
        var planet = GameObject.FindObjectOfType<ChaosCosmos.Gameplay.PlanetController>();
        Assert.IsNotNull(planet, "PlanetController non trovato nella scena " + SceneNames.GAMEPLAY_SCENE_PLACEHOLDER);

        yield return null;
    }
}
