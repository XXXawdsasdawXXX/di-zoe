using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using AppTracker;
using Code.Core.GameLoop;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.AppTracker
{
    public class WindowTracker : MonoBehaviour, IExitListener
    {
        private readonly Dictionary<uint, string> _processNameCache = new();
        private float _cacheCleanupTimer;
        private const float CACHE_CLEANUP_INTERVAL = 30f;

        
        // Windows API для получения активного окна
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [SerializeField] private float _pollInterval = 1f; // опрос каждую секунду

        // Название приложения → суммарное время в секундах
        public Dictionary<string, float> AppTime { get; } = new();

        private string _currentApp;
        private float  _sessionStart;
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            TrackLoop(_cts.Token).Forget();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            FlushCurrentApp(); // сохранить время текущего приложения при выключении
        }

        private async UniTaskVoid TrackLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                _cacheCleanupTimer += _pollInterval;
                if (_cacheCleanupTimer >= CACHE_CLEANUP_INTERVAL)
                {
                    _processNameCache.Clear();
                    _cacheCleanupTimer = 0f;
                }

                string active = GetActiveAppName();

                UnityEngine.Debug.Log($"[WindowTracker] active='{active}' current='{_currentApp}' appTime count={AppTime.Count}");

            
                if (!string.IsNullOrEmpty(active))
                {
                    if (active != _currentApp)
                    {
                        _currentApp = active;
                        UnityEngine.Debug.Log($"[WindowTracker] switched to '{active}'");
                    }

                    // Накапливаем каждый тик — не ждём смены приложения
                    if (!AppTime.ContainsKey(_currentApp))
                        AppTime[_currentApp] = 0f;

                    AppTime[_currentApp] += _pollInterval;
                }


                await UniTask.Delay(
                    TimeSpan.FromSeconds(_pollInterval),
                    cancellationToken: ct);
            }
        }
        
        private void FlushCurrentApp()
        {
            if (string.IsNullOrEmpty(_currentApp)) return;

            float elapsed = Time.realtimeSinceStartup - _sessionStart;
            if (!AppTime.ContainsKey(_currentApp))
                AppTime[_currentApp] = 0f;

            AppTime[_currentApp] += elapsed;
        }

        private string GetActiveAppName()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return null;

                GetWindowThreadProcessId(hwnd, out uint pid);
                if (pid == 0) return null;

                // Кешируем результат по pid — не читаем MainModule каждый раз
                if (_processNameCache.TryGetValue(pid, out string cached))
                    return cached;

                using var process = Process.GetProcessById((int)pid);

                // MainModule требует прав и падает на системных процессах
                // Используем просто ProcessName как fallback
                string name = process.ProcessName;

                try
                {
                    string productName = process.MainModule?.FileVersionInfo.ProductName;
                    if (!string.IsNullOrEmpty(productName))
                        name = productName;
                }
                catch
                {
                    // системный процесс — оставляем ProcessName
                }

                _processNameCache[pid] = name;
                return name;
            }
            catch
            {
                return null;
            }
        }

        
        public void GameExit()
        {
            AppTimeStorage.Save(AppTime);
        }
    }
}