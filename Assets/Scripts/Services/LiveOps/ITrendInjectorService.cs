using ChaosCosmos.Core.Services;

namespace ChaosCosmos.Services.LiveOps
{
    public interface ITrendInjectorService : IService
    {
        void Initialize(); // Per applicare i trend all'avvio
        bool IsInitialized { get; }
        // In futuro: metodi per ottenere valori specifici di trend
        // string GetCurrentArenaTheme();
        // float GetCollectibleSpawnMultiplier(string collectibleType);
    }
}
