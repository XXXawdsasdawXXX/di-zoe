using System;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using NAudio.Wave;
using UnityEngine;

namespace Code.Game.Radio
{
    public class RadioPlayer : MonoBehaviour, IService, IInitializeListener, ISubscriber, IExitListener
    {
        private MediaFoundationReader mediaFoundationReader;
        private WaveOutEvent waveOut;
        private RadioTranslation _radioModels;

        private Coroutine _coroutine;


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
        }

        public float GetVolume()
        {
            return waveOut?.Volume ?? 0;
        }

        public void StopRadio()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
            }
        }

        private void _setVolume(float volume)
        {
            if (waveOut != null)
            {
                waveOut.Volume = Mathf.Clamp01(volume);
            }
        }

        private void _playCurrentStream()
        {
            string url = _radioModels.GetCurrentStreamUrl();
            
            try
            {
                mediaFoundationReader = new MediaFoundationReader(url);
                waveOut = new WaveOutEvent();
                waveOut.Init(mediaFoundationReader);
                waveOut.Play();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error playing radio: {ex.Message}. url = {url}");
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

        private async UniTask _changeRadioStation()
        {
            StopRadio();
            
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            
            _playCurrentStream();
        }
    }
}