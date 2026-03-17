using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Code.Core.GameLoop;
using Code.Core.Save;
using Code.Core.Save.SavedData;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using Timer = Code.Tools.Timer;

namespace Code.Game.Radio
{
    [Preserve]
    public class RadioModelService : IService, IInitializeListener, IUpdateListener, IProgressWriter
    {
        [Serializable]
        private class ChannelListResponse
        {
            public RadioChannelModel[] channels;
        }
        
        public Dictionary<string, ReactiveProperty<RadioChannelModel>> Channels { get; } = new();
        public ReactiveProperty<RadioSongModel> CurrentSong { get; } = new(default);
        public ReactiveProperty<RadioSongListModel> PreviousSongs { get; } = new(default);
        public ReactiveProperty<int> CurrentChannelIndex { get; }  = new(-1);
        
        public ReactiveProperty<float> RadioVolume { get; }  = new(-1);
        
        private RadioConfiguration _radioConfiguration;

        private Timer _channelsUpdateTimer;
        private Timer _tracksUpdateTimer;


        public async UniTask GameInitialize()
        {
            _radioConfiguration = Container.Instance.GetConfig<RadioConfiguration>();

            _channelsUpdateTimer = new Timer(_radioConfiguration.ChannelsUpdateInterval);
            _tracksUpdateTimer = new Timer(_radioConfiguration.TrackUpdateInterval);
            
            await _updateChannels();
        }
        
        public async UniTask LoadProgress(PlayerProgressData playerProgress)
        {
            CurrentChannelIndex.PropertyValue = playerProgress.RadioChanel;
            RadioVolume.PropertyValue = playerProgress.RadioVolume;
            
            await  _updateSongs();
        }

        public void SaveProgress(PlayerProgressData playerProgress)
        {
            playerProgress.RadioChanel = CurrentChannelIndex.PropertyValue;
            playerProgress.RadioVolume = RadioVolume.PropertyValue;
        }

        public void GameUpdate()
        {
            float deltaTime = Time.deltaTime;

            if (_channelsUpdateTimer.Update(deltaTime))
            {
                _updateChannels().Forget(Debug.LogException);
            }

            if (_tracksUpdateTimer.Update(deltaTime))
            {
                _updateSongs().Forget(Debug.LogException);
            }
        }

        public void SetChannel(int channel)
        {
            CurrentChannelIndex.PropertyValue = channel;
           
            _tracksUpdateTimer.Finish();
            _channelsUpdateTimer.Finish();
        }
        
        public void SetVolume(float volume)
        {
            RadioVolume.PropertyValue = volume;
        }
        
        public async UniTask<Texture2D> GetChannelLogo(string logoUrl)
        {
            if (string.IsNullOrEmpty(logoUrl))
            {
                return _radioConfiguration.DefaultChannelLogo;
            }

            try
            {
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(logoUrl);
    
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Failed to load logo from {logoUrl}: {request.error}");
              
                    return _radioConfiguration.DefaultChannelLogo;
                }
                
                return DownloadHandlerTexture.GetContent(request);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load logo from {logoUrl}: {e.Message}");
                
                return _radioConfiguration.DefaultChannelLogo;
            }
        }
        
        public RadioChannelModel GetCurrentChannelModel()
        {
            return Channels.ElementAt(CurrentChannelIndex.PropertyValue).Value.PropertyValue;
        }

        public string GetCurrentStreamURL()
        {
            return $"https://ice1.somafm.com/{GetCurrentChannelModel().id}-128-mp3";
        }

        private async UniTask _updateChannels()
        {
            using UnityWebRequest request = UnityWebRequest.Get(RadioConfiguration.CHANNEL_MODELS_URL);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"FetchListenerCount failed: {request.error}");
                return;
            }

            try
            {
                ChannelListResponse allChannels = request.downloadHandler.text.ToDeserialized<ChannelListResponse>();

                foreach (RadioChannelModel channel in allChannels.channels)
                {
                    if (Channels.ContainsKey(channel.id))
                    {
                        Channels[channel.id].PropertyValue = channel;
                    }
                    else
                    {
                        Channels.Add(channel.id, new ReactiveProperty<RadioChannelModel>(channel));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(new Exception(e.ToString()));
            }
        }
        

        private async UniTask _updateSongs()
        {
            if (Channels.Count == 0)
            {
                return;
            }
            
            string url = RadioConfiguration.GetTrackModelURL(GetCurrentChannelModel().id);

            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"FetchCurrentTrack failed: {request.error}");
                return;
            }

            try
            {
                RadioSongListModel data = request.downloadHandler.text.ToDeserialized<RadioSongListModel>();

                if (data.Songs == null || data.Songs.Count == 0)
                {
                    return;
                }

                PreviousSongs.PropertyValue = data;
                
                CurrentSong.PropertyValue = data.Songs[0];
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FetchCurrentTrack parse error: {e.Message}");
            }
        }
    }
}