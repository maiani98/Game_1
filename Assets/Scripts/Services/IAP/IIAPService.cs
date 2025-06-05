using System;
using System.Collections.Generic;
using ChaosCosmos.Core.Services;

namespace ChaosCosmos.Services.IAP
{
    // Semplice classe per dettagli prodotto, espandere se necessario
    public class ProductDetails
    {
        public string id;
        public string priceString;
        public string title;
        public string description;
    }

    public enum PurchaseFailureReason
    {
        Unknown,
        UserCancelled,
        ProductUnavailable,
        PurchasePending,
        InitializationFailed, // Aggiunto per coprire fallimenti di init IAP
        ServiceUnavailable    // Aggiunto per coprire servizio IAP non disponibile
    }

    public interface IIAPService : IService
    {
        bool IsInitialized { get; }
        void Initialize(Action<bool, string> onInitialized); // Callback con successo/fallimento e messaggio

        ProductDetails GetProductDetails(string productID);
        void PurchaseProduct(string productID, Action<bool, PurchaseFailureReason, string> onPurchaseCompleted); // bool successo, ragione fallimento, ID transazione/messaggio
        void RestorePurchases(Action<bool, string> onRestoreCompleted); // bool successo, messaggio
        bool HasUserPurchased(string productID); // Per non-consumabili
    }
}
