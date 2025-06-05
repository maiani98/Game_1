using UnityEngine;
using UnityEngine.AddressableAssets; // Richiede il pacchetto Addressables
using UnityEngine.ResourceManagement.AsyncOperations; // Per AsyncOperationHandle
using System;
using System.Collections.Generic; // Per tracciare le operazioni

namespace ChaosCosmos.Core.AssetManagement
{
    public class AddressableAssetLoader : MonoBehaviour
    {
        public static AddressableAssetLoader Instance { get; private set; }

        private Dictionary<string, AsyncOperationHandle> _loadedAssetHandles = new Dictionary<string, AsyncOperationHandle>();
        // Per tracciare istanze multiple per chiave e poterle rilasciare individualmente o tutte per quella chiave
        private Dictionary<string, List<AsyncOperationHandle<GameObject>>> _instantiatedGameObjectHandles = new Dictionary<string, List<AsyncOperationHandle<GameObject>>>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // Addressables.InitializeAsync(); // Inizializza Addressables se non già fatto
                // L'inizializzazione di Addressables è gestita automaticamente al primo accesso se non si usa un hosting custom complesso.
                // Se si usa hosting custom, si fa InitializeAsync con un path al catalogo custom.
                // Per ora, lasciamo che Unity lo gestisca.
                Debug.Log("AddressableAssetLoader: Istanza creata e impostata come persistente.");
            }
            else
            {
                Debug.LogWarning("AddressableAssetLoader: Istanza duplicata distrutta.");
                Destroy(gameObject);
            }
        }

        public void LoadAssetAsync<T>(string assetAddress, Action<T> onLoaded, Action<string> onError = null) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetAddress))
            {
                Debug.LogError("AddressableAssetLoader.LoadAssetAsync: assetAddress è nullo o vuoto.");
                onError?.Invoke("Indirizzo asset nullo o vuoto.");
                return;
            }

            // Gestione del caso in cui l'asset sia già caricato e valido
            if (_loadedAssetHandles.TryGetValue(assetAddress, out AsyncOperationHandle existingHandle) && existingHandle.IsValid() && existingHandle.IsDone && existingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"AddressableAssetLoader.LoadAssetAsync: Asset '{assetAddress}' già caricato e valido. Uso istanza in cache.");
                onLoaded?.Invoke(existingHandle.ResultAs<T>());
                return;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetAddress);
            // Sovrascrive l'handle precedente se esiste e non era valido, o aggiunge il nuovo.
            _loadedAssetHandles[assetAddress] = handle;

            handle.Completed += (operation) =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    onLoaded?.Invoke(operation.Result);
                }
                else
                {
                    Debug.LogError($"AddressableAssetLoader.LoadAssetAsync: Fallito caricamento asset da '{assetAddress}'. Errore: {operation.OperationException}");
                    onError?.Invoke(operation.OperationException?.Message ?? "Errore sconosciuto");
                    if (_loadedAssetHandles.ContainsKey(assetAddress) && _loadedAssetHandles[assetAddress].Equals(handle))
                    {
                        _loadedAssetHandles.Remove(assetAddress);
                    }
                }
            };
        }

        public void InstantiateGameObjectAsync(string assetAddress, Action<GameObject, AsyncOperationHandle<GameObject>> onInstantiated, Action<string> onError = null, Transform parent = null, bool trackInstance = true)
        {
            if (string.IsNullOrEmpty(assetAddress))
            {
                Debug.LogError("AddressableAssetLoader.InstantiateGameObjectAsync: assetAddress è nullo o vuoto.");
                onError?.Invoke("Indirizzo asset nullo o vuoto per Instantiate.");
                return;
            }

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(assetAddress, parent);

            if(trackInstance)
            {
                if (!_instantiatedGameObjectHandles.ContainsKey(assetAddress))
                {
                    _instantiatedGameObjectHandles[assetAddress] = new List<AsyncOperationHandle<GameObject>>();
                }
                _instantiatedGameObjectHandles[assetAddress].Add(handle);
            }

            handle.Completed += (operation) =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    onInstantiated?.Invoke(operation.Result, operation); // Passa anche l'handle per un rilascio specifico
                }
                else
                {
                    Debug.LogError($"AddressableAssetLoader.InstantiateGameObjectAsync: Fallita istanziazione da '{assetAddress}'. Errore: {operation.OperationException}");
                    onError?.Invoke(operation.OperationException?.Message ?? "Errore sconosciuto istanziazione");
                    if(trackInstance && _instantiatedGameObjectHandles.ContainsKey(assetAddress))
                    {
                        _instantiatedGameObjectHandles[assetAddress]?.Remove(handle);
                    }
                }
            };
        }

        public void ReleaseAsset(string assetAddress)
        {
            if (_loadedAssetHandles.TryGetValue(assetAddress, out AsyncOperationHandle handle))
            {
                if (handle.IsValid())
                {
                   Addressables.Release(handle);
                }
                _loadedAssetHandles.Remove(assetAddress);
                Debug.Log($"AddressableAssetLoader.ReleaseAsset: Asset '{assetAddress}' rilasciato.");
            }
        }

        // Rilascia una specifica istanza tracciata tramite il suo handle
        public void ReleaseSingleTrackedInstance(string assetAddress, AsyncOperationHandle<GameObject> handleToRelease)
        {
            if (string.IsNullOrEmpty(assetAddress) || !handleToRelease.IsValid()) return;

            if (_instantiatedGameObjectHandles.TryGetValue(assetAddress, out List<AsyncOperationHandle<GameObject>> handles))
            {
                if (handles.Remove(handleToRelease))
                {
                    if (handleToRelease.Result != null) // Assicurati che l'oggetto esista ancora
                    {
                        Addressables.ReleaseInstance(handleToRelease);
                        Debug.Log($"AddressableAssetLoader.ReleaseSingleTrackedInstance: Istanza per '{assetAddress}' (handle specifico) rilasciata.");
                    }
                }
            }
        }

        // Rilascia TUTTE le istanze per un dato indirizzo
        public void ReleaseAllInstancesOf(string assetAddress)
        {
            if (_instantiatedGameObjectHandles.TryGetValue(assetAddress, out List<AsyncOperationHandle<GameObject>> handles))
            {
                foreach (var handle in handles)
                {
                    if (handle.IsValid() && handle.Result != null)
                    {
                        Addressables.ReleaseInstance(handle);
                    }
                }
                handles.Clear();
                // Considera se rimuovere la chiave da _instantiatedGameObjectHandles o solo pulire la lista
                // Se si rimuove la chiave, la prossima istanza ricreerà la lista. Se si pulisce, la lista rimane ma vuota.
                // Per coerenza con il comportamento di tracciamento, pulire la lista è sufficiente.
                Debug.Log($"AddressableAssetLoader.ReleaseAllInstancesOf: Tutte le istanze per '{assetAddress}' rilasciate.");
            }
        }


        void OnDestroy()
        {
            Debug.Log("AddressableAssetLoader: OnDestroy chiamato. Rilascio di tutti gli asset e le istanze tracciate...");
            foreach (var entry in _loadedAssetHandles)
            {
                if(entry.Value.IsValid()) Addressables.Release(entry.Value);
            }
            _loadedAssetHandles.Clear();

            foreach (var entry in _instantiatedGameObjectHandles)
            {
                foreach (var handle in entry.Value)
                {
                   if(handle.IsValid() && handle.Result != null) Addressables.ReleaseInstance(handle);
                }
            }
            _instantiatedGameObjectHandles.Clear();

            if (Instance == this)
            {
                Instance = null;
            }
            Debug.Log("AddressableAssetLoader: Tutti gli asset tracciati sono stati rilasciati.");
        }
    }
}
