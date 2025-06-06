using UnityEngine;
using System;
using ChaosCosmos.Gameplay.GameLogic;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.Progression;
using ChaosCosmos.Gameplay;
using ChaosCosmos.Core.Constants;
using ChaosCosmos.Services.Tutorial; // Aggiunto per ITutorialService

namespace ChaosCosmos.Services.GameManagement
{
    public class GameManagerService : IGameManagerService
    {
        public GameState CurrentGameState { get; private set; } = GameState.Pregame;
        public float CurrentTimeElapsed { get; private set; }
        public float MatchDurationSeconds { get; set; } = 120f;
        public float TimeRemaining => Mathf.Max(0, MatchDurationSeconds - CurrentTimeElapsed);

        public event Action<GameState> OnGameStateChanged;
        public event Action<GameResultData> OnGameOver;
        public event Action<float> OnTimerUpdated;

        private IProgressService _progressService;
        private PlanetController _playerPlanetController;
        private bool _matchCanEndByTime = true;

        public GameManagerService(IProgressService progressService)
        {
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        }

        public void Tick(float deltaTime)
        {
            if (CurrentGameState == GameState.Playing)
            {
                CurrentTimeElapsed += deltaTime;
                OnTimerUpdated?.Invoke(TimeRemaining);

                if (_matchCanEndByTime && CurrentTimeElapsed >= MatchDurationSeconds)
                {
                    EndMatchByTime();
                }
            }
        }

        public void StartNewMatch()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag(GameTags.PLAYER_TAG);
            if (playerGO != null)
            {
                _playerPlanetController = playerGO.GetComponent<PlanetController>();
                if (_playerPlanetController == null)
                {
                     Debug.LogError("GameManagerService: GameObject con tag 'Player' non ha PlanetController!");
                }
            }
            else
            {
                Debug.LogError("GameManagerService: Giocatore (GameObject con tag 'Player') non trovato all'inizio della partita!");
            }

            CurrentTimeElapsed = 0f;
            _matchCanEndByTime = true;
            ChangeGameState(GameState.Playing);
            OnTimerUpdated?.Invoke(TimeRemaining);
            // TODO_MUSIC: Play_Gameplay_Music_Loop (o riprendi se era in pausa)
            // TODO_SFX: Match_Start_Stinger
            Debug.Log($"GameManagerService: Nuova partita iniziata. Durata: {MatchDurationSeconds}s");

            // Avvia il tutorial se non completato
            if (ServiceLocator.IsRegistered<ITutorialService>())
            {
                ServiceLocator.Get<ITutorialService>().StartTutorialFlowIfNotCompleted();
            }
            else
            {
                Debug.LogWarning("GameManagerService: ITutorialService non registrato, impossibile avviare il flusso tutorial.");
            }
        }

        public void PauseMatch()
        {
            if (CurrentGameState == GameState.Playing)
            {
                ChangeGameState(GameState.Paused);
                Debug.Log("GameManagerService: Partita in pausa.");
            }
        }

        public void ResumeMatch()
        {
            if (CurrentGameState == GameState.Paused)
            {
                ChangeGameState(GameState.Playing);
                Debug.Log("GameManagerService: Partita ripresa.");
            }
        }

        private void EndMatchByTime()
        {
            if (CurrentGameState != GameState.Playing) return;

            Debug.Log("GameManagerService: Partita terminata per fine tempo.");
            ProcessGameOver();
        }

        public void EndMatchPrematurely()
        {
             if (CurrentGameState != GameState.Playing) return;
             Debug.Log("GameManagerService: Partita terminata prematuramente.");
             _matchCanEndByTime = false;
             ProcessGameOver();
        }

        private void ProcessGameOver()
        {
            if (CurrentGameState == GameState.GameOver) return;

            ChangeGameState(GameState.GameOver);

            float finalMass = 0;
            if (_playerPlanetController != null)
            {
                finalMass = _playerPlanetController.CurrentMass;
            }
            else
            {
                Debug.LogWarning("GameManagerService: PlayerPlanetController nullo a fine partita, massa finale sarà 0.");
            }

            int xpEarned = Mathf.FloorToInt(finalMass * 10);
            xpEarned += Mathf.FloorToInt(CurrentTimeElapsed * 0.5f);

            if (_progressService != null)
            {
                _progressService.AddXP(xpEarned);
                Debug.Log($"GameManagerService: Aggiunti {xpEarned} XP a ProgressManager.");
            }
            else
            {
                 Debug.LogError("GameManagerService: IProgressService nullo. Impossibile aggiungere XP.");
            }

            GameResultData results = new GameResultData
            {
                finalMass = finalMass,
                xpEarned = xpEarned,
                timePlayed = CurrentTimeElapsed
            };
            OnGameOver?.Invoke(results);
            // TODO_MUSIC: Stop_Gameplay_Music_Loop_or_FadeOut
            // TODO_SFX: Match_End_Sound
            Debug.Log($"GameManagerService: Game Over processato. Massa Finale: {finalMass}, XP Guadagnati: {xpEarned}, Tempo Giocato: {CurrentTimeElapsed:F1}s");
        }

        private void ChangeGameState(GameState newState)
        {
            if (CurrentGameState == newState) return;

            CurrentGameState = newState;
            OnGameStateChanged?.Invoke(CurrentGameState);
            Debug.Log($"GameManagerService: Stato cambiato a {CurrentGameState}");
        }
    }
}
