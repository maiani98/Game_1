namespace ChaosCosmos.Services.Consent
{
    // Per GDPR (semplificato)
    public enum GDPRConsentStatus { Unknown, Given, Denied }

    // Per ATT (iOS) - rispecchia ATTrackingManager.AuthorizationStatus
    public enum TrackingAuthorizationStatus { NotDetermined, Restricted, Denied, Authorized }
}
