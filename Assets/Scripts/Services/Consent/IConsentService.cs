using System;
using ChaosCosmos.Core.Services;

namespace ChaosCosmos.Services.Consent
{
    public interface IConsentService : IService
    {
        bool IsInitialized { get; }
        // Metodo per avviare il flusso di richiesta consensi all'avvio
        void RequestConsentFlow(Action onFlowCompleted);

        GDPRConsentStatus UserGDPRConsent { get; }
        TrackingAuthorizationStatus UserATTStatus { get; } // Solo rilevante per iOS

        // Metodi helper per un facile accesso
        bool HasGivenGDPRConsentForAnalytics(); // Esempio: se GDPR è Given
        bool HasGivenGDPRConsentForPersonalizedAds(); // Esempio: se GDPR è Given
        bool CanTrackDeviceForAds(); // Se ATT è Authorized (e GDPR lo permette)

        // Metodi per simulare la risposta dell'utente ai popup (usati internamente o per debug)
        // void SimulateGDPRChoice(GDPRConsentStatus status); // Reso privato
        // void SimulateATTChoice(TrackingAuthorizationStatus status); // Reso privato
        void ResetConsentStatus(); // Per testing
    }
}
