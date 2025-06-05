using UnityEngine;
using UnityEngine.Rendering; // Necessario per accedere a RenderPipelineManager
using UnityEngine.Rendering.Universal; // Necessario per UniversalRenderPipelineAsset

namespace ChaosCosmos.Core.Graphics
{
    public class DynamicRenderScaleManager : MonoBehaviour
    {
        // Range per la scala di rendering (es. 0.5 = 50%, 1.0 = 100%)
        public float minRenderScale = 0.7f;
        public float maxRenderScale = 1.0f;
        public float currentRenderScale = 1.0f;

        private UniversalRenderPipelineAsset _urpAsset;

        void Start()
        {
            // Tenta di ottenere l'asset URP corrente
            _urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (_urpAsset == null)
            {
                Debug.LogError("DynamicRenderScaleManager: UniversalRenderPipelineAsset non trovato. Assicurati che URP sia configurato correttamente.");
                enabled = false; // Disabilita lo script se URP non è attivo
                return;
            }
            SetRenderScale(currentRenderScale); // Imposta la scala iniziale
        }

        // Metodo per impostare la scala di rendering
        public void SetRenderScale(float scale)
        {
            if (_urpAsset == null) return;

            currentRenderScale = Mathf.Clamp(scale, minRenderScale, maxRenderScale);
            _urpAsset.renderScale = currentRenderScale; // Usa il campo rinominato
            Debug.Log($"DynamicRenderScaleManager: Render Scale impostato a {currentRenderScale}");
        }

        // Esempio di controllo manuale per il testing
        void Update()
        {
            if (_urpAsset == null) return;

            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                SetRenderScale(currentRenderScale + 0.05f);
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                SetRenderScale(currentRenderScale - 0.05f);
            }
        }
    }
}
