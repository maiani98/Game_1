using UnityEngine;
using UnityEngine.Profiling; // Per Profiler API
// using TMPro; // Se si usasse TextMeshPro per la UI di debug

namespace ChaosCosmos.Core.DebugUtils
{
    public class MemoryMonitorDebug : MonoBehaviour
    {
        // public TextMeshProUGUI memoryStatsText; // Assegnare se si usa TMP Text
        public bool showOnGUIMonitor = true;
        private string _memoryInfo;

        void Update()
        {
            if (showOnGUIMonitor && Time.frameCount % 30 == 0) // Aggiorna info ogni 30 frame per non impattare troppo
            {
                RefreshMemoryInfo();
            }
        }

        void RefreshMemoryInfo()
        {
            long monoHeapSize = Profiler.GetMonoHeapSizeLong();
            long monoUsedSize = Profiler.GetMonoUsedSizeLong();
            long totalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong();
            long totalReservedMemory = Profiler.GetTotalReservedMemoryLong();
            // long totalUnusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong(); // Obsoleto in versioni recenti

            _memoryInfo = $"Mono Heap: {monoHeapSize / 1048576f:F2} MB\n" +
                         $"Mono Used: {monoUsedSize / 1048576f:F2} MB\n" +
                         $"Total Allocated: {totalAllocatedMemory / 1048576f:F2} MB\n" +
                         $"Total Reserved: {totalReservedMemory / 1048576f:F2} MB";

            // if (memoryStatsText != null) memoryStatsText.text = _memoryInfo;
        }

        // GUI Semplice per i pulsanti e info (non ideale per performance, solo per DEBUG)
        void OnGUI()
        {
            if (!showOnGUIMonitor)
            {
                return;
            }

            int padding = 10;
            int lineHeight = 25;
            int buttonWidth = 220; // Leggermente più largo per testo
            int boxHeight = 120; // Aumentato per più testo
            int currentY = padding;

            // Stile per GUI.Box e GUI.Label per migliore leggibilità
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.fontSize = 14; // Aumenta font size

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.richText = true; // Permette \n in Label

            GUI.Box(new Rect(padding, currentY, buttonWidth + 20, boxHeight), "<b>Memory Monitor</b>", boxStyle);
            currentY += lineHeight; // Spazio per il titolo del box

            if (string.IsNullOrEmpty(_memoryInfo)) RefreshMemoryInfo(); // Carica info se non ancora fatto

            GUI.Label(new Rect(padding + 5, currentY, buttonWidth + 10, boxHeight - lineHeight - 5), _memoryInfo ?? "Loading...", labelStyle);
            currentY += (boxHeight - lineHeight - 5) + padding; // Sposta sotto il testo delle stats

            if (GUI.Button(new Rect(padding, currentY, buttonWidth, lineHeight), "Log Current Memory Stats"))
            {
                RefreshMemoryInfo();
                Debug.Log("--- MEMORY STATS SNAPSHOT (DEBUG) ---\n" + _memoryInfo +
                                  $"\nTotal GameObjects: {FindObjectsOfType<GameObject>().Length}" + // Cambiato a GameObject per un conteggio più generale
                                  $"\nTotal MonoBehaviours: {FindObjectsOfType<MonoBehaviour>().Length}" +
                                  $"\nPlanetControllers: {FindObjectsOfType<ChaosCosmos.Gameplay.PlanetController>().Length}" +
                                  $"\nCollectibles (MassSource): {FindObjectsOfType<ChaosCosmos.Gameplay.MassSource>().Length}");
            }
            currentY += lineHeight + 5;

            if (GUI.Button(new Rect(padding, currentY, buttonWidth, lineHeight), "Force GC.Collect()"))
            {
                System.GC.Collect();
                Debug.Log("MemoryMonitorDebug: System.GC.Collect() chiamato.");
                RefreshMemoryInfo();
            }
        }
    }
}
