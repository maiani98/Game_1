using ChaosCosmos.Core.Services;
using System;

namespace ChaosCosmos.Services.Tutorial
{
    public interface ITutorialService : IService
    {
        bool IsInitialized { get; }
        void Initialize(Action onInitializedCallback);

        bool IsTutorialCompleted(); // Verifica se il tutorial è stato completato in passato
        void StartTutorialFlowIfNotCompleted(); // Avvia il flusso del tutorial se non già completato

        // Metodi che potrebbero essere chiamati da UI di Debug o comandi cheat
        void MarkTutorialAsFullyCompleted();
        void ResetTutorialStatus();
    }
}
