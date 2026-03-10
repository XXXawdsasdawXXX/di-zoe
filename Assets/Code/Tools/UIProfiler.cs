using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Code.Tools
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

        private float _timer;
        private float _deltaTime;

        private Recorder _cpuRecorder;
        private Recorder _gpuRecorder;

        void OnEnable()
        {
            _cpuRecorder = Recorder.Get("PlayerLoop");
            _cpuRecorder.enabled = true;

            _gpuRecorder = Recorder.Get("Camera.Render");
            _gpuRecorder.enabled = true;
        }

        void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _timer += Time.unscaledDeltaTime;

            if (_timer < 1f / UPDATE_RATE) return;
            _timer = 0f;

            float fps = 1f / _deltaTime;
            textFPS.text = $"FPS: {Mathf.RoundToInt(fps)}";

            float cpuTime = _cpuRecorder.elapsedNanoseconds / 1_000_000f;
            textCPU.text = $"CPU: {cpuTime:0.00} ms";

            float gpuTime = _gpuRecorder.elapsedNanoseconds / 1_000_000f;
            textGPU.text = $"GPU: {gpuTime:0.00} ms";

            float ram = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            textRAM.text = $"RAM: {Mathf.RoundToInt(ram)} MB";

            float mono = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
            textMono.text = $"Mono: {Mathf.RoundToInt(mono)} MB";

            long gc = System.GC.GetTotalMemory(false) / (1024 * 1024);
            textGC.text = $"GC: {gc} MB";

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            textDrawCalls.text = $"Draw Calls: {UnityStats.drawCalls}";
#endif
        }
        
        // Нормальные значения  для 2D игры ориентиры примерно такие:
        // Метрика	      Хорошо
        // FPS	          60
        // CPU	        < 5 ms
        // GPU	        < 5 ms 
        // Draw Calls	< 100
    }
}