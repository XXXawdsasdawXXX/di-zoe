using System;
using System.Threading;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using NAudio.Wave;
using UnityEngine;

namespace Code.Game.Radio
{
    public class RadioPlayer : MonoBehaviour, IService, IInitializeListener,IStartListener ,ISubscriber, IExitListener
    {
        private MediaFoundationReader mediaFoundationReader;
        private WaveOutEvent waveOut;
        private RadioTranslation _radioModels;

        private CancellationTokenSource _streamCts;
       
        private CancellationTokenSource _watchdogCts;
        private const float WATCHDOG_INTERVAL = 5f;
        private const int MAX_RETRY_ATTEMPTS = 3;
        
        
        public UniTask GameInitialize()
        {
            _radioModels = Container.Instance.GetService<RadioTranslation>();
            
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _radioModels.Model.CurrentChannelIndex.SubscribeToValue(_onChangeRadioStation);
            _radioModels.Model.RadioVolume.SubscribeToValue(_setVolume);
        }

        public UniTask GameStart()
        {
            _watchdogCts = new CancellationTokenSource();
            _watchdogLoop(_watchdogCts.Token).Forget();
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _radioModels.Model.CurrentChannelIndex.UnsubscibeFromValue(_onChangeRadioStation);
            _radioModels.Model.RadioVolume.UnsubscibeFromValue(_setVolume);
        }

        public void GameExit()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
            }

            if (mediaFoundationReader != null)
            {
                mediaFoundationReader.Dispose();
            }
            
            _watchdogCts?.Cancel();
            _watchdogCts?.Dispose();
            _watchdogCts = null;
        }
        
        public void StopRadio()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
            }
        }

        private void _onChangeRadioStation(int index)
        {
            if (index < 0 || index >= _radioModels.Model.Channels.Count)
            {
                Debug.LogError("Invalid chanel index");
                return;
            }

            _changeRadioStation().Forget();
        }

        private void _setVolume(float volume)
        {
            if (waveOut != null)
            {
                Debug.Log($"set volume {volume} -> {Mathf.Clamp01(volume)}");
                waveOut.Volume = Mathf.Clamp01(volume);
            }
        }

        private async UniTask _changeRadioStation()
        {
            // Отменяем предыдущее подключение
            _streamCts?.Cancel();
            _streamCts?.Dispose();
            _streamCts = new CancellationTokenSource();
            var ct = _streamCts.Token;

            if (waveOut != null) { waveOut.Stop(); waveOut.Dispose(); waveOut = null; }
            if (mediaFoundationReader != null) { mediaFoundationReader.Dispose(); mediaFoundationReader = null; }

            await UniTask.Delay(TimeSpan.FromMilliseconds(300), cancellationToken: ct); // debounce

            if (ct.IsCancellationRequested) return;

            string url = _radioModels.GetCurrentStreamUrl();

            await UniTask.RunOnThreadPool(() =>
            {
                if (ct.IsCancellationRequested) return;
        
                try
                {
                    var reader = new MediaFoundationReader(url);
                    var output = new WaveOutEvent();
                    output.Init(reader);

                    if (ct.IsCancellationRequested)
                    {
                        output.Dispose();
                        reader.Dispose();
                        return;
                    }

                    UniTask.Post(() =>
                    {
                        if (ct.IsCancellationRequested)
                        {
                            output.Dispose();
                            reader.Dispose();
                            return;
                        }
                        mediaFoundationReader = reader;
                        waveOut = output;
                        waveOut.Volume = Mathf.Clamp01(_radioModels.Model.RadioVolume.PropertyValue);
                        waveOut.Play();
                    });
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { Debug.LogError($"Error playing radio: {ex.Message}"); }
            });
        }
        
        private async UniTaskVoid _watchdogLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(WATCHDOG_INTERVAL),
                    cancellationToken: ct);

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                // Если стрим активен но waveOut остановился — перезапускаем
                bool shouldBePlaying = _radioModels.Model.CurrentChannelIndex.PropertyValue >= 0;
                bool isPlaying = waveOut?.PlaybackState == PlaybackState.Playing;

                if (shouldBePlaying && !isPlaying)
                {
                    Debug.LogWarning("[RadioPlayer] Watchdog: playback stopped unexpectedly, retrying...");
                    await _retryWithBackoff(ct);
                }
            }
        }

        private async UniTask _retryWithBackoff(CancellationToken ct)
        {
            for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                Debug.Log($"[RadioPlayer] Retry attempt {attempt}/{MAX_RETRY_ATTEMPTS}");

                await _changeRadioStation();

                // Ждём немного и проверяем успех
                await UniTask.Delay(
                    TimeSpan.FromSeconds(2),
                    cancellationToken: ct);

                if (waveOut?.PlaybackState == PlaybackState.Playing)
                {
                    Debug.Log("[RadioPlayer] Playback restored successfully");
                    return;
                }

                // Экспоненциальная задержка между попытками: 2s, 4s, 8s
                float backoff = Mathf.Pow(2, attempt);
                Debug.LogWarning($"[RadioPlayer] Attempt {attempt} failed, waiting {backoff}s...");

                await UniTask.Delay(
                    TimeSpan.FromSeconds(backoff),
                    cancellationToken: ct);
            }

            Debug.LogError("[RadioPlayer] All retry attempts failed");
        }
    }
}