using System;
using System.Collections;
using System.Linq;
using Code.Core.GameLoop;
using Code.Core.Save;
using Code.Core.Save.SavedData;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using NAudio.Wave;
using UnityEngine;

namespace Code.Game.Radio
{
    /// <summary>
    /// Manages radio functionality, including volume control and channel selection.
    /// </summary>
    public class RadioPlayer : MonoBehaviour, IService, IInitializeListener, ISubscriber, IExitListener
    {
        private RadioConfiguration _configuration;
        private MediaFoundationReader mediaFoundationReader;
        private WaveOutEvent waveOut;
        private RadioModelService _radioModels;

        private Coroutine _coroutine;


        public UniTask GameInitialize()
        {
            _configuration = Container.Instance.GetConfig<RadioConfiguration>();

            _radioModels = Container.Instance.GetService<RadioModelService>();

            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _radioModels.CurrentChannel.SubscribeToValue(_onChangeRadioStation);
        }
        
        public void Unsubscribe()
        {
            _radioModels.CurrentChannel.UnsubscibeFromValue(_onChangeRadioStation);
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

        private void _playChannel(int channelIndex)
        {
            string url = _radioModels.GetCurrentStreamURL();
            
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
        
        public void SetVolume(float volume)
        {
            if (waveOut != null)
            {
                waveOut.Volume = Mathf.Clamp01(volume);
            }
        }

        public float GetVolume()
        {
            return waveOut != null ? waveOut.Volume : 0;
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
            if (index < 0 || index >= _radioModels.Channels.Count)
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
            
            _playChannel(_radioModels.CurrentChannel.PropertyValue);
        }

  
    }
}