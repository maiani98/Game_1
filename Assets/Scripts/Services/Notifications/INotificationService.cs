using System;
using ChaosCosmos.Core.Services;

namespace ChaosCosmos.Services.Notifications
{
    public interface INotificationService : IService
    {
        bool IsInitialized { get; }
        void Initialize(Action<bool> onInitCompleteCallback); // true se i permessi sono stati gestiti/richiesti

        // Restituisce l'ID della notifica schedulata, o -1/null se fallisce
        int ScheduleLocalNotification(string title, string body, string subText, DateTime fireTime, string channelID = "default_channel", string smallIcon = null, string largeIcon = null);
        void CancelLocalNotification(int notificationID); // Principalmente per Android o se si mappa ID per iOS
        void CancelAllLocalNotifications();

        // Per future notifiche remote
        // void RegisterForPushNotifications();
        // string GetDevicePushToken();
    }
}
