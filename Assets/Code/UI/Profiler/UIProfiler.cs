using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Code.UI.Profiler
{
    public class UIProfiler : MonoBehaviour
    {
        private const float UPDATE_RATE = 4f;

        [SerializeField] private Text textFPS;
        [SerializeField] private Text textCPU;
        [SerializeField] private Text textGPU;
        [SerializeField] private Text textRAM;
        [SerializeField] private Text textMono;
        [SerializeField] private Text textGC;
        [SerializeField] private Text textDrawCalls;
        [SerializeField] private Button buttonCopyToClipboard;

        private float _timer;
        private float _deltaTime;

        private Recorder _cpuRecorder;
        private Recorder _gpuRecorder;

        private float _fps;
        private float _cpu;
        private float _gpu;
        private float _ram;
        private float _mono;
        private long _gc;
        private int _drawCalls;

        void OnEnable()
        {
            _cpuRecorder = Recorder.Get("PlayerLoop");
            _cpuRecorder.enabled = true;

            _gpuRecorder = Recorder.Get("Camera.Render");
            _gpuRecorder.enabled = true;
            
            buttonCopyToClipboard.onClick.RemoveAllListeners();
            buttonCopyToClipboard.onClick.AddListener(CopyStatsToClipboard);

#if !UNITY_EDITOR
            textDrawCalls.gameObject.SetActive(false);            
#endif
        }

        void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _timer += Time.unscaledDeltaTime;

            if (_timer < 1f / UPDATE_RATE) return;
            _timer = 0f;

            _fps = 1f / _deltaTime;
            textFPS.text = $"FPS: {Mathf.RoundToInt(_fps)}";

            _cpu = _cpuRecorder.elapsedNanoseconds / 1_000_000f;
            textCPU.text = $"CPU: {_cpu:0.00} ms";

            _gpu = _gpuRecorder.elapsedNanoseconds / 1_000_000f;
            textGPU.text = $"GPU: {_gpu:0.00} ms";

            _ram = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            textRAM.text = $"RAM: {Mathf.RoundToInt(_ram)} MB";

            _mono = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
            textMono.text = $"Mono: {Mathf.RoundToInt(_mono)} MB";

            _gc = System.GC.GetTotalMemory(false) / (1024 * 1024);
            textGC.text = $"GC: {_gc} MB";

#if UNITY_EDITOR
            _drawCalls = UnityStats.drawCalls;
            textDrawCalls.text = $"Draw Calls: {_drawCalls}";
#endif
        }

        private void CopyStatsToClipboard()
        {
            string stats =
            $@"==== GAME PROFILER ====

FPS: {Mathf.RoundToInt(_fps)}
CPU Frame: {_cpu:0.00} ms
GPU Frame: {_gpu:0.00} ms

RAM: {Mathf.RoundToInt(_ram)} MB
Mono: {Mathf.RoundToInt(_mono)} MB
GC: {_gc} MB

Draw Calls: {_drawCalls}

=======================";

            GUIUtility.systemCopyBuffer = stats;

            Debug.Log("Profiler stats copied to clipboard:\n" + stats);
        }


        // Нормальные значения  для 2D игры ориентиры примерно такие:
        // Метрика	      Хорошо
        // FPS	          60
        // CPU	        < 5 ms
        // GPU	        < 5 ms 
        // Draw Calls	< 100
    }
}