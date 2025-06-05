using UnityEngine;
using System;
using System.Collections;

namespace ChaosCosmos.Core.Utils
{
    public class DelayExecutor : MonoBehaviour
    {
        // Metodo pubblico per avviare l'esecuzione ritardata
        public void ExecuteAfterDelay(Action action, float delay, Action onCompleteAction = null)
        {
            StartCoroutine(DoExecute(action, delay, onCompleteAction));
        }

        private IEnumerator DoExecute(Action action, float delay, Action onCompleteAction)
        {
            yield return new WaitForSeconds(delay);
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"DelayExecutor: Eccezione durante l'esecuzione dell'azione ritardata: {e}");
            }
            finally
            {
                try
                {
                   onCompleteAction?.Invoke(); // Chiamato anche in caso di eccezione nell'action principale
                }
                catch (Exception e)
                {
                    Debug.LogError($"DelayExecutor: Eccezione durante l'esecuzione di onCompleteAction: {e}");
                }

                // Auto-distruzione del GameObject che ospita questo DelayExecutor
                if (gameObject != null)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
