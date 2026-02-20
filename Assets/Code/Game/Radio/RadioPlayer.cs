using System;
using System.Collections;
using Code.Core.GameLoop;
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
    public class RadioPlayer : MonoBehaviour, IService, IInitializeListener, IStartListener
    {
        public ReactiveProperty<int> Channel { get; private set; } = new ReactiveProperty<int>(-1);

        private RadioConfiguration _configuration;
    
        /// <summary>
        /// The MediaFoundationReader for audio processing.
        /// </summary>
        private MediaFoundationReader mediaFoundationReader;

        /// <summary>
        /// The WaveOutEvent for audio output.
        /// </summary>
        private WaveOutEvent waveOut;

        private Coroutine _coroutine;
    
        public UniTask GameInitialize()
        {
            _configuration = Container.Instance.GetConfig<RadioConfiguration>();
        
            return UniTask.CompletedTask;
        }
    
        public UniTask GameStart()
        {
            /*
        if (volumeSlider != null)
        {
            volumeSlider.value = 1f;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }


        if (radioDropdown != null)
        {
            radioDropdown.onValueChanged.AddListener(ChangeRadioStation);
            radioDropdown.value = 0;
        }
        */


            Channel.Value = 0;
            
            StartCoroutine(PlayRadio(_configuration.Channels[Channel.Value].Path));
        
            return UniTask.CompletedTask;
        }
    

        private IEnumerator PlayRadio(string url)
        {
            yield return null;
            try
            {
                mediaFoundationReader = new MediaFoundationReader(url);
                waveOut = new WaveOutEvent();
                waveOut.Init(mediaFoundationReader);
                waveOut.Play();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error playing radio: {ex.Message}");
            }
        }

        void OnDestroy()
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

        public void StopRadio()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
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


        // Do this shit later
        public void ChangeRadioStation(int dropdownIndex)
        {
            if (dropdownIndex < 0 || dropdownIndex >= _configuration.Channels.Length)
            {
                Debug.LogError("Invalid dropdown index");
                return;
            }

            Channel.Value = dropdownIndex;
            // Stop the current radio station and then play the new one after a delay
            StartCoroutine(ChangeRadioStationWithDelay(Channel.Value));
        }

        private IEnumerator ChangeRadioStationWithDelay(int dropdownIndex)
        {
            // Stop the current radio station
            StopRadio();

            yield return new WaitForSeconds(0.1f); // Wait for 100 milliseconds

            // Start playing the new radio station
            StartCoroutine(PlayRadio(_configuration.Channels[dropdownIndex].Path));
        }


  
    }
}