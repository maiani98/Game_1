using UnityEngine;
using System.Collections.Generic;
using System.Text;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.Consent;

namespace ChaosCosmos.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        private IConsentService _consentService;
        private bool _consentChecked = false;

        public AnalyticsService()
        {
            Debug.Log("AnalyticsService: Inizializzato (Placeholder). Consenso sarà verificato al primo evento.");
        }

        private bool CanTrack()
        {
            if (!_consentChecked)
            {
                if (ServiceLocator.IsRegistered<IConsentService>())
                {
                    _consentService = ServiceLocator.Get<IConsentService>();
                }
                _consentChecked = true;
                if (_consentService == null)
                {
                     Debug.LogWarning("[Analytics] IConsentService non trovato. Eventi inviati senza verifica consenso (comportamento permissivo).");
                }
            }

            if (_consentService != null)
            {
                if (!_consentService.IsInitialized)
                {
                    Debug.LogWarning("[Analytics] IConsentService non ancora finalizzato (flusso in corso). Evento non inviato.");
                    return false;
                }
                if (!_consentService.HasGivenGDPRConsentForAnalytics())
                {
                    Debug.Log("[Analytics] Consenso GDPR per Analytics non dato. Evento non inviato."); // Cambiato a Log da LogWarning per meno rumore se è un caso atteso
                    return false;
                }
            }
            // Se _consentService è null (e _consentChecked è true), si arriva qui, permettendo il tracciamento.
            // Questo è un comportamento permissivo se il ConsentService non viene trovato.
            return true;
        }

        public void TrackEvent(string eventName)
        {
            if (!CanTrack())
            { // Aggiunte graffe
                return;
            }
            Debug.Log($"[Analytics] Evento Tracciato: {eventName}");
        }

        public void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!CanTrack())
            { // Aggiunte graffe
                return;
            }

            StringBuilder paramString = new StringBuilder();
            if (parameters != null)
            {
                foreach (var param in parameters) // var è appropriato qui
                {
                    paramString.Append($"\n  {param.Key}: {param.Value}");
                }
            }
            Debug.Log($"[Analytics] Evento Tracciato: {eventName}{(parameters != null && parameters.Count > 0 ? " con parametri:" + paramString.ToString() : "")}");
        }
    }
}
