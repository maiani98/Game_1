using UnityEngine;
using System;
using System.Collections.Generic;
// using ChaosCosmos.Core.Constants; // Rimosso perché ProductID_RemoveAds è definito localmente

namespace ChaosCosmos.Services.IAP
{
    public class IAPFacade : IIAPService
    {
        public bool IsInitialized { get; private set; }
        public const string ProductID_RemoveAds = "com.chaoscosmos.removeads";
        public const string ProductID_TestConsumable = "com.chaoscosmos.testconsumable";

        private HashSet<string> _definedProductIDs = new HashSet<string>();
        private Dictionary<string, ProductDetails> _productDetailsCache = new Dictionary<string, ProductDetails>();
        private Dictionary<string, bool> _simulatedPurchases = new Dictionary<string, bool>();

        public IAPFacade()
        {
            _definedProductIDs.Add(ProductID_RemoveAds);
            _definedProductIDs.Add(ProductID_TestConsumable);
        }

        public void Initialize(Action<bool, string> onInitialized)
        {
            Debug.Log("IAPFacade: Inizializzazione (simulata)...");
            _productDetailsCache.Clear();
            foreach(string id in _definedProductIDs)
            {
                _productDetailsCache[id] = new ProductDetails {
                    id = id,
                    title = $"Prodotto {id}",
                    description = $"Descrizione per {id}",
                    priceString = (id == ProductID_RemoveAds ? "€4.99" : (id == ProductID_TestConsumable ? "€0.99" : "€1.99"))
                };
            }
            _simulatedPurchases[ProductID_RemoveAds] = PlayerPrefs.GetInt(ProductID_RemoveAds, 0) == 1;

            IsInitialized = true;
            Debug.Log("IAPFacade: Inizializzato con successo (simulato).");
            onInitialized?.Invoke(true, "IAPFacade inizializzazione simulata completata.");
        }

        public ProductDetails GetProductDetails(string productID)
        {
            if (string.IsNullOrEmpty(productID))
            {
                Debug.LogWarning("IAPFacade.GetProductDetails: productID nullo o vuoto.");
                return null;
            }
            if (!IsInitialized)
            {
                Debug.LogError("IAPFacade.GetProductDetails: Servizio non inizializzato.");
                return null;
            }
            _productDetailsCache.TryGetValue(productID, out ProductDetails details);
            if(details == null)
            {
                Debug.LogWarning($"IAPFacade: Dettagli prodotto non trovati per ID: {productID}");
            }
            return details;
        }

        public void PurchaseProduct(string productID, Action<bool, PurchaseFailureReason, string> onPurchaseCompleted)
        {
            if (string.IsNullOrEmpty(productID))
            {
                Debug.LogError("IAPFacade.PurchaseProduct: productID nullo o vuoto.");
                onPurchaseCompleted?.Invoke(false, PurchaseFailureReason.ProductUnavailable, "ID Prodotto invalido");
                return;
            }
            if (onPurchaseCompleted == null)
            {
                Debug.LogWarning($"IAPFacade.PurchaseProduct: onPurchaseCompleted callback è nullo per productID '{productID}'.");
            }

            if (!IsInitialized)
            {
                Debug.LogError("IAPFacade.PurchaseProduct: Servizio non inizializzato.");
                onPurchaseCompleted?.Invoke(false, PurchaseFailureReason.InitializationFailed, "IAP non inizializzato");
                return;
            }
            if (!_definedProductIDs.Contains(productID))
            {
                 Debug.LogError($"IAPFacade.PurchaseProduct: Prodotto non definito: {productID}");
                onPurchaseCompleted?.Invoke(false, PurchaseFailureReason.ProductUnavailable, "Prodotto non definito");
                return;
            }

            Debug.Log($"IAPFacade: Tentativo di acquisto per '{productID}' (simulato)...");
            if (productID == ProductID_RemoveAds)
            {
                PlayerPrefs.SetInt(ProductID_RemoveAds, 1);
                PlayerPrefs.Save();
                _simulatedPurchases[ProductID_RemoveAds] = true;
                Debug.Log($"IAPFacade: Prodotto '{productID}' acquistato e salvato in PlayerPrefs (simulato).");
                onPurchaseCompleted?.Invoke(true, PurchaseFailureReason.Unknown, "acquisto_simulato_successo_" + productID);
            }
            else if (productID == ProductID_TestConsumable)
            {
                Debug.Log($"IAPFacade: Prodotto consumabile '{productID}' acquistato (simulato).");
                onPurchaseCompleted?.Invoke(true, PurchaseFailureReason.Unknown, "acquisto_consumabile_simulato_successo_" + productID);
            }
            else
            {
                Debug.LogWarning($"IAPFacade: Acquisto per '{productID}' non ha una logica di simulazione specifica oltre al successo.");
                onPurchaseCompleted?.Invoke(true, PurchaseFailureReason.Unknown, "acquisto_generico_simulato_successo_" + productID);
            }
        }

        public void RestorePurchases(Action<bool, string> onRestoreCompleted)
        {
            if (!IsInitialized)
            {
                Debug.LogError("IAPFacade.RestorePurchases: Servizio non inizializzato.");
                onRestoreCompleted?.Invoke(false, "IAP non inizializzato");
                return;
            }
            if (onRestoreCompleted == null)
            {
                 Debug.LogWarning("IAPFacade.RestorePurchases: onRestoreCompleted callback è nullo.");
            }

            Debug.Log("IAPFacade: Ripristino acquisti (simulato)...");
            bool restoredAny = HasUserPurchased(ProductID_RemoveAds);

            Debug.Log($"IAPFacade: Ripristino acquisti completato (simulato). Stato 'RemoveAds': {HasUserPurchased(ProductID_RemoveAds)}");
            onRestoreCompleted?.Invoke(true, restoredAny ? "Ripristino completato, acquisti (simulati) trovati." : "Ripristino completato, nessun acquisto precedente (simulato) trovato.");
        }

        public bool HasUserPurchased(string productID)
        {
            if (string.IsNullOrEmpty(productID))
            {
                Debug.LogWarning("IAPFacade.HasUserPurchased: productID nullo o vuoto.");
                return false;
            }
            if (!IsInitialized)
            {
                Debug.LogWarning("IAPFacade.HasUserPurchased: Servizio non inizializzato. Restituisco false.");
                return false;
            }
            if (_simulatedPurchases.TryGetValue(productID, out bool purchased))
            {
                return purchased;
            }
            return false;
        }
    }
}
