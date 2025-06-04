using UnityEngine;
using System.Collections.Generic;
using System.Text; // Per StringBuilder

namespace ChaosCosmos.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        public AnalyticsService()
        {
            // In un'implementazione reale, qui si inizializzerebbe l'SDK di analytics (es. Unity Analytics, Firebase)
            Debug.Log("AnalyticsService: Inizializzato (Placeholder - nessun dato inviato).");
        }

        public void TrackEvent(string eventName)
        {
            // Implementazione reale: Unity.Analytics.Analytics.CustomEvent(eventName);
            // o chiamata all'SDK di terze parti.
            Debug.Log($"[Analytics] Evento Tracciato: {eventName}");
        }

        public void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            // Implementazione reale: Unity.Analytics.Analytics.CustomEvent(eventName, parameters);
            // o chiamata all'SDK di terze parti.

            StringBuilder paramString = new StringBuilder();
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    paramString.Append($"\n  {param.Key}: {param.Value}"); // Aggiunto \n per leggibilità
                }
            }
            Debug.Log($"[Analytics] Evento Tracciato: {eventName}{ (parameters != null && parameters.Count > 0 ? " con parametri:" + paramString.ToString() : "") }");
        }
    }
}
