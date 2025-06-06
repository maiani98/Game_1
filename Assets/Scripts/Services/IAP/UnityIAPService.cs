using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ChaosCosmos.Services.IAP
{
    /// <summary>
    /// Real implementation of <see cref="IIAPService"/> using Unity IAP.
    /// The Unity IAP package must be installed via the Package Manager.
    /// </summary>
    public class UnityIAPService : IStoreListener, IIAPService
    {
        public bool IsInitialized { get; private set; }
        private IStoreController _controller;
        private IExtensionProvider _extensions;

        private readonly Dictionary<string, ProductType> _products = new Dictionary<string, ProductType>
        {
            { IAPFacade.ProductID_RemoveAds, ProductType.NonConsumable },
            { IAPFacade.ProductID_TestConsumable, ProductType.Consumable }
        };

        public void Initialize(Action<bool, string> onInitialized)
        {
            if (IsInitialized)
            {
                onInitialized?.Invoke(true, "already_initialized");
                return;
            }
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var kvp in _products)
            {
                builder.AddProduct(kvp.Key, kvp.Value);
            }
            UnityPurchasing.Initialize(this, builder);
            _initCallback = onInitialized;
        }

        public ProductDetails GetProductDetails(string productID)
        {
            var product = _controller?.products.WithID(productID);
            if (product == null) return null;
            return new ProductDetails
            {
                id = product.definition.id,
                title = product.metadata.localizedTitle,
                description = product.metadata.localizedDescription,
                priceString = product.metadata.localizedPriceString
            };
        }

        public void PurchaseProduct(string productID, Action<bool, PurchaseFailureReason, string> onPurchaseCompleted)
        {
            if (!IsInitialized)
            {
                onPurchaseCompleted?.Invoke(false, PurchaseFailureReason.InitializationFailed, "iap_not_initialized");
                return;
            }
            _purchaseCallback = onPurchaseCompleted;
            _controller.InitiatePurchase(productID);
        }

        public void RestorePurchases(Action<bool, string> onRestoreCompleted)
        {
            if (!IsInitialized)
            {
                onRestoreCompleted?.Invoke(false, "iap_not_initialized");
                return;
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                _extensions.GetExtension<IAppleExtensions>().RestoreTransactions(result =>
                {
                    onRestoreCompleted?.Invoke(result, result ? "restore_ok" : "restore_failed");
                });
            }
            else
            {
                onRestoreCompleted?.Invoke(false, "not_supported");
            }
        }

        public bool HasUserPurchased(string productID)
        {
            var product = _controller?.products.WithID(productID);
            return product?.hasReceipt ?? false;
        }

        private Action<bool, string> _initCallback;
        private Action<bool, PurchaseFailureReason, string> _purchaseCallback;

        // IStoreListener callbacks
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            IsInitialized = true;
            _initCallback?.Invoke(true, "ok");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"UnityIAP initialization failed: {error}");
            IsInitialized = false;
            _initCallback?.Invoke(false, error.ToString());
        }

#if UNITY_2022_1_OR_NEWER
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"UnityIAP initialization failed: {error} - {message}");
            IsInitialized = false;
            _initCallback?.Invoke(false, message);
        }
#endif

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            _purchaseCallback?.Invoke(true, PurchaseFailureReason.Unknown, e.purchasedProduct.transactionID);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            _purchaseCallback?.Invoke(false, failureReason, product.transactionID);
        }
    }
}
