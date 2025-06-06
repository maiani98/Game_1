using UnityEngine;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.GameManagement;
using ChaosCosmos.Gameplay.GameLogic; // Per GameState
using UnityEngine.SceneManagement; // Per SceneManager
using ChaosCosmos.Core.Constants; // Per SceneNames

namespace ChaosCosmos.Core.MonoBehaviours
{
    // Questo MonoBehaviour ospita il GameManagerService e ne chiama il Tick.
    // Può essere aggiunto a un GameObject persistente (es. _Bootstrapper) o
    // a un GameObject specifico della scena di gioco (es. _GameSceneManager).
    // Se è specifico della scena, StartNewMatch potrebbe essere chiamato nel suo Start().
    public class GameManagerMB : MonoBehaviour
    {
        private IGameManagerService _gameManagerService;

        void Start()
        {
            if (!ServiceLocator.IsRegistered<IGameManagerService>())
            {
                Debug.LogError("GameManagerMB: IGameManagerService non registrato in ServiceLocator! GameManagerMB non funzionerà.");
                enabled = false; // Disabilita l'Update se il servizio non c'è
                return;
            }

            _gameManagerService = ServiceLocator.Get<IGameManagerService>();

            // Decide se avviare automaticamente una partita in base alla scena corrente
            // Questo è un esempio; la logica di avvio partita potrebbe essere più centralizzata
            // o controllata dalla UI (es. LobbyManager).
            if (SceneManager.GetActiveScene().name == SceneNames.GAMEPLAY_SCENE_PLACEHOLDER) // O il nome della tua scena di gioco
            {
                // Potrebbe essere desiderabile non avviare la partita qui se LobbyManager lo fa già.
                // Per ora, lo commentiamo, assumendo che LobbyManager.StartGame() chiami _gameManagerService.StartNewMatch()
                // dopo il caricamento della scena. Se GameManagerMB è solo nella scena di gioco, questo Start
                // verrebbe chiamato dopo il caricamento della scena.
                // _gameManagerService.StartNewMatch();
                Debug.Log("GameManagerMB: Nella scena di gioco. StartNewMatch dovrebbe essere chiamato da chi ha caricato la scena (es. LobbyManager) o da un gestore di flusso.");
            }
        }

        void Update()
        {
            // Se il servizio non è stato recuperato o se il gioco non è in uno stato che richiede Tick, non fare nulla.
            if (_gameManagerService == null ||
                _gameManagerService.CurrentGameState == GameState.Pregame ||
                _gameManagerService.CurrentGameState == GameState.GameOver)
            {
                return;
            }

            _gameManagerService.Tick(Time.deltaTime);
        }

        // Esempio di come la UI potrebbe interagire per mettere in pausa/riprendere,
        // anche se questi metodi potrebbero essere in un UIInputController o simile.
        public void RequestPause()
        {
            _gameManagerService?.PauseMatch();
        }

        public void RequestResume()
        {
            _gameManagerService?.ResumeMatch();
        }

        public void RequestEndMatch()
        {
            _gameManagerService?.EndMatchPrematurely();
        }
    }
}
