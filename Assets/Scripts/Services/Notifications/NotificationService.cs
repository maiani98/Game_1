#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
#define MOBILE_NOTIFICATIONS_SUPPORTED
#endif

#if MOBILE_NOTIFICATIONS_SUPPORTED
    #if UNITY_ANDROID
    using Unity.Notifications.Android;
    #elif UNITY_IOS
    using Unity.Notifications.iOS;
    #endif
#endif

using UnityEngine;
using System;

namespace ChaosCosmos.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        public bool IsInitialized { get; private set; }
        private const string DefaultAndroidChannelID = "chaos_cosmos_default_channel";
        private const string DefaultAndroidChannelName = "Notifiche Generali";
        private const string DefaultAndroidChannelDesc = "Notifiche generali dal gioco Chaos Cosmos.";

        public NotificationService() { }

        public void Initialize(Action<bool> onInitCompleteCallback) // Firma corretta
        {
            if (IsInitialized) {
                onInitCompleteCallback?.Invoke(true);
                return;
            }
            Debug.Log("NotificationService: Inizializzazione...");

            #if MOBILE_NOTIFICATIONS_SUPPORTED
                #if UNITY_ANDROID
                var channel = new AndroidNotificationChannel()
                {
                    Id = DefaultAndroidChannelID,
                    Name = DefaultAndroidChannelName,
                    Importance = Importance.Default, // O Importance.High per suoni/vibrazioni più evidenti
                    Description = DefaultAndroidChannelDesc,
                    EnableLights = true,
                    EnableVibration = true
                };
                AndroidNotificationCenter.RegisterNotificationChannel(channel);
                Debug.Log("NotificationService: Canale Android registrato.");
                IsInitialized = true;
                onInitCompleteCallback?.Invoke(true);

                #elif UNITY_IOS
                // Su iOS, la richiesta di autorizzazione è gestita diversamente.
                // Spesso, si chiede il permesso quando si tenta di schedulare la prima notifica,
                // o si usa iOSNotificationCenter.RequestAuthorization per un controllo più esplicito.
                // Per questa simulazione, assumiamo che il permesso sia implicitamente OK per le locali.
                Debug.Log("NotificationService: Inizializzazione per iOS. Permessi per notifiche locali di base sono generalmente concessi o chiesti dal sistema alla prima notifica.");
                IsInitialized = true;
                onInitCompleteCallback?.Invoke(true);

                #else // Editor o altra piattaforma con MOBILE_NOTIFICATIONS_SUPPORTED definito
                Debug.Log("NotificationService: Mobile Notifications parzialmente supportate in Editor. Inizializzazione simulata completata.");
                IsInitialized = true;
                onInitCompleteCallback?.Invoke(true);
                #endif
            #else
            Debug.LogWarning("NotificationService: Mobile Notifications NON supportate su questa piattaforma. Funzionalità limitata a log.");
            IsInitialized = true; // Marcato come inizializzato per non bloccare, ma le funzionalità non ci saranno
            onInitCompleteCallback?.Invoke(false); // Indica che il supporto reale manca
            #endif
        }

        public int ScheduleLocalNotification(string title, string body, string subText, DateTime fireTime,
                                              string channelID = DefaultAndroidChannelID, string smallIcon = null, string largeIcon = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("NotificationService: Servizio non inizializzato. Impossibile schedulare notifica.");
                return -1;
            }
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(body))
            {
                Debug.LogError("NotificationService: Titolo o corpo della notifica non possono essere nulli o vuoti.");
                return -1;
            }
             if (fireTime <= DateTime.Now.AddSeconds(1)) // Aggiunto un piccolo buffer per evitare schedulazioni nel passato immediato
            {
                Debug.LogWarning($"NotificationService: Tentativo di schedulare notifica locale nel passato o troppo vicina al presente ({fireTime}). Notifica non schedulata.");
                return -1;
            }


            #if MOBILE_NOTIFICATIONS_SUPPORTED
                int notificationId = -1;
                #if UNITY_ANDROID
                var androidNotification = new AndroidNotification
                {
                    Title = title,
                    Text = body,
                    SmallIcon = smallIcon,
                    LargeIcon = largeIcon,
                    FireTime = fireTime,
                    ShouldAutoCancel = true, // Rimuovi la notifica quando cliccata
                };
                if (!string.IsNullOrEmpty(subText)) androidNotification.Group = subText; // Usa Group per SubText o info aggiuntive

                notificationId = AndroidNotificationCenter.SendNotification(androidNotification, channelID);
                Debug.Log($"NotificationService [Android]: Notifica schedulata (ID: {notificationId}) per {fireTime} con titolo '{title}'");
                return notificationId;

                #elif UNITY_IOS
                var timeInterval = fireTime.ToUniversalTime() - DateTime.UtcNow;
                // Il check fireTime <= DateTime.Now è già stato fatto sopra.
                // timeInterval dovrebbe essere sempre > 0 qui.

                var iosNotification = new iOSNotification()
                {
                    Identifier = System.Guid.NewGuid().ToString(),
                    Title = title,
                    Body = body,
                    Subtitle = subText,
                    ShowInForeground = true,
                    ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound | PresentationOption.Badge,
                    CategoryIdentifier = "default_category",
                    ThreadIdentifier = "default_thread", // Per raggruppare notifiche
                    Trigger = new iOSNotificationTimeIntervalTrigger() { TimeInterval = timeInterval }
                };

                iOSNotificationCenter.ScheduleNotification(iosNotification);
                Debug.Log($"NotificationService [iOS]: Notifica schedulata per {fireTime} con titolo '{title}' (ID stringa: {iosNotification.Identifier})");
                return 0;

                #else
                Debug.Log($"[SIMULAZIONE NOTIFICA EDITOR] Schedulata per {fireTime}: Titolo='{title}', Corpo='{body}', Sottotesto='{subText}', Canale='{channelID}'");
                return System.Guid.NewGuid().GetHashCode();
                #endif
            #else
            Debug.Log($"[SIMULAZIONE NOTIFICA (NO SUPPORT)] Schedulata per {fireTime}: Titolo='{title}', Corpo='{body}'");
            return System.Guid.NewGuid().GetHashCode();
            #endif
        }

        public void CancelLocalNotification(int notificationID)
        {
            if (!IsInitialized) { Debug.LogWarning("NotificationService non inizializzato."); return; }
            #if MOBILE_NOTIFICATIONS_SUPPORTED && UNITY_ANDROID
            var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(notificationID);
            if (notificationStatus == NotificationStatus.Scheduled)
            {
                AndroidNotificationCenter.CancelNotification(notificationID);
                Debug.Log($"NotificationService [Android]: Cancellata notifica locale schedulata con ID: {notificationID}");
            } else if (notificationStatus == NotificationStatus.Delivered) {
                AndroidNotificationCenter.CancelDisplayedNotification(notificationID); // Per notifiche già mostrate ma non cancellate
                 Debug.Log($"NotificationService [Android]: Cancellata notifica locale già mostrata con ID: {notificationID}");
            } else {
                 Debug.Log($"NotificationService [Android]: Notifica con ID: {notificationID} non trovata o già scaduta/cancellata. Status: {notificationStatus}");
            }
            #else
            Debug.LogWarning($"NotificationService: CancelLocalNotification(int id) è principalmente per Android. ID Richiesto: {notificationID}");
            #endif
        }

        public void CancelAllLocalNotifications()
        {
            if (!IsInitialized) { Debug.LogWarning("NotificationService non inizializzato."); return; }
            #if MOBILE_NOTIFICATIONS_SUPPORTED
                #if UNITY_ANDROID
                AndroidNotificationCenter.CancelAllScheduledNotifications();
                AndroidNotificationCenter.CancelAllDisplayedNotifications();
                Debug.Log("NotificationService [Android]: Tutte le notifiche locali (schedulate e mostrate) cancellate.");
                #elif UNITY_IOS
                iOSNotificationCenter.RemoveAllScheduledNotifications();
                iOSNotificationCenter.RemoveAllDeliveredNotifications();
                Debug.Log("NotificationService [iOS]: Tutte le notifiche locali (schedulate e mostrate) cancellate.");
                #else
                Debug.Log("[SIMULAZIONE NOTIFICA EDITOR] Tutte le notifiche cancellate.");
                #endif
            #else
            Debug.Log("[SIMULAZIONE NOTIFICA (NO SUPPORT)] Tutte le notifiche cancellate.");
            #endif
        }
    }
}
