using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization; // Per Locale
using System.Collections.Generic;

namespace ChaosCosmos.Core.DebugUtils
{
    public class LocalizationDebug : MonoBehaviour
    {
        public bool showOnGUIDebug = true;
        private Vector2 _scrollPosition = Vector2.zero; // Per scroll view se ci sono molte lingue

        void OnGUI()
        {
            if (!showOnGUIDebug)
            {
                return;
            }

            // Area per i controlli di localizzazione
            // Aumenta la larghezza e l'altezza dell'area per contenere più pulsanti e testo
            Rect areaRect = new Rect(10, Screen.height - 150, 350, 140); // Spostato più in alto e allargato
            GUILayout.BeginArea(areaRect, GUI.skin.box);


            if (LocalizationSettings.InitializationOperation.IsDone == false || LocalizationSettings.AvailableLocales == null)
            {
                GUILayout.Label("Localization System not ready or no locales available.");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label($"Current Locale: {LocalizationSettings.SelectedLocale.LocaleName}");
            GUILayout.Space(5);

            GUILayout.Label("Change Locale (Debug):");

            // ScrollView per la lista delle lingue se sono troppe
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(80)); // Altezza fissa per lo scroll

            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (locales != null)
            {
                foreach (var locale in locales) // var è ok qui
                {
                    if (locale == null) continue;

                    if (GUILayout.Button(locale.LocaleName))
                    {
                        // Controlla se la localizzazione è pronta prima di cambiare
                        if (LocalizationSettings.InitializationOperation.IsDone)
                        {
                            LocalizationSettings.SelectedLocale = locale;
                            Debug.Log($"[LocalizationDebug] Lingua cambiata a: {locale.LocaleName} (ID: {locale.Identifier.Code})");
                        }
                        else
                        {
                            Debug.LogWarning("[LocalizationDebug] Tentativo di cambiare lingua ma Localization System non è ancora pronto.");
                        }
                    }
                }
            }
            else
            {
                GUILayout.Label("No locales found in LocalizationSettings.AvailableLocales.Locales");
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
