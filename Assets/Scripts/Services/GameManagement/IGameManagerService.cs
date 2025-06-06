using System;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Gameplay.GameLogic; // Per GameState, GameResultData

namespace ChaosCosmos.Services.GameManagement
{
    public interface IGameManagerService : IService
    {
        GameState CurrentGameState { get; }
        float CurrentTimeElapsed { get; } // Tempo trascorso dall'inizio della partita
        float TimeRemaining { get; }      // Tempo rimanente nella partita
        float MatchDurationSeconds { get; set; }

        event Action<GameState> OnGameStateChanged;
        event Action<GameResultData> OnGameOver;
        event Action<float> OnTimerUpdated; // Passa il tempo rimanente

        void StartNewMatch();
        void PauseMatch();
        void ResumeMatch();
        void EndMatchPrematurely();

        void Tick(float deltaTime); // Metodo per l'aggiornamento del timer
    }
}
