using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core.GameLoop;
using Code.Core.Save;
using Code.Core.Save.SavedData;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

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

        public ReactiveProperty<int> CurrentChannel { get; }  = new(-1);
        
        private RadioConfiguration _radioConfiguration;
        private RadioPlayer _radioPlayer;

        private Timer _channelsUpdateTimer;
        private Timer _tracksUpdateTimes;

        
        public async UniTask GameInitialize()
        {
            _radioPlayer = Container.Instance.GetService<RadioPlayer>();
            _radioConfiguration = Container.Instance.GetConfig<RadioConfiguration>();

            _channelsUpdateTimer = new Timer(_radioConfiguration.ChannelsUpdateInterval);
            _tracksUpdateTimes = new Timer(_radioConfiguration.TrackUpdateInterval);
            
            await _updateChannels();
        }
        
        public UniTask LoadProgress(PlayerProgressData playerProgress)
        {
            CurrentChannel.PropertyValue = playerProgress.RadioChanel;
            
            return UniTask.CompletedTask;
        }

        public void GameUpdate()
        {
            float deltaTime = Time.deltaTime;

            if (_channelsUpdateTimer.Update(deltaTime))
            {
                _updateChannels().Forget(Debug.LogException);
            }

            if (_tracksUpdateTimes.Update(deltaTime))
            {
                _updateSongs().Forget(Debug.LogException);
            }
        }

        public void SaveProgress(PlayerProgressData playerProgress)
        {
            playerProgress.RadioChanel = CurrentChannel.PropertyValue;
        }
        
        public void SetCurrentChannel(int channel)
        {
            CurrentChannel.PropertyValue = channel;
        }
        
        public async UniTask<Texture2D> GetChannelLogo(string logoUrl)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(logoUrl);
    
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"GetCurrentChannelLogo failed: {request.error}");
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            return texture;
        }

        
        public RadioChannelModel GetCurrentChannelModel()
        {
            return Channels.ElementAt(CurrentChannel.PropertyValue).Value.PropertyValue;
        }

        public string GetCurrentStreamURL()
        {
            return $"https://ice1.somafm.com/{GetCurrentChannelModel().id}-128-mp3";
        }
        
        private async UniTask _updateChannels()
        {
            using UnityWebRequest request = UnityWebRequest.Get(RadioConfiguration.ChannelModelsURL);

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
        
        private void _updateCurrentChanelModel(string chanelID)
        {
            _updateSongs().Forget();
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