using System.Threading;
using Code.Core.GameLoop;
using Code.Core.Save;
using Code.Core.Save.SavedData;
using Code.Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Timer = Code.Tools.Timer;

namespace Code.Game.Radio
{
    [Preserve]
    public class RadioTranslation : IService, IInitializeListener, IUpdateListener, IProgressWriter
    {
        public RadioModel Model { get; private set; }

        private RadioConfiguration _config;
        private RadioRepository _repository;

        private Timer _channelsTimer;
        private Timer _songsTimer;

        private bool _isFetchingChannels;
        private bool _isFetchingSongs;


        public async UniTask GameInitialize()
        {
            Model = Container.Instance.GetService<RadioModel>();

            _repository = Container.Instance.GetService<RadioRepository>();
            _config = Container.Instance.GetConfiguration<RadioConfiguration>();

            _channelsTimer = new Timer(_config.ChannelsUpdateInterval);
            _songsTimer = new Timer(_config.TrackUpdateInterval);
            
            await _refreshChannelsAsync();
        }

        public async UniTask LoadProgress(PlayerProgressData playerProgress)
        {
            Model.CurrentChannelIndex.PropertyValue = playerProgress.RadioChanel;
            Model.RadioVolume.PropertyValue = playerProgress.RadioVolume;

            await _refreshSongsAsync();
        }

        public void SaveProgress(PlayerProgressData playerProgress)
        {
            playerProgress.RadioChanel = Model.CurrentChannelIndex.PropertyValue;
            playerProgress.RadioVolume = Model.RadioVolume.PropertyValue;
        }

        public void GameUpdate()
        {
            float dt = Time.deltaTime;

            if (_channelsTimer.Update(dt))
            {
                _refreshChannelsAsync().Forget(Debug.LogException);
            }

            if (_songsTimer.Update(dt))
            {
                _refreshSongsAsync().Forget(Debug.LogException);
            }
        }

        public void SetChannel(int channelIndex)
        {
            Model.CurrentChannelIndex.PropertyValue = channelIndex;

            _songsTimer.Finish();
        }

        public void SetVolume(float volume)
        {
            Debug.Log($"translation set volume -> {volume}");
            Model.RadioVolume.PropertyValue = volume;
        }

        public string GetCurrentStreamUrl()
        {
            RadioChannelModel channel = Model.GetCurrentChannel();
            return string.Format(RadioConfiguration.STREAM_URL_TEMPLATE, channel.id);
        }

        public UniTask<Texture2D> GetChannelLogoAsync(string logoUrl, CancellationToken ct)
        {
            return _repository.FetchLogoAsync(logoUrl, ct);
        }

        private async UniTask _refreshChannelsAsync()
        {
            if (_isFetchingChannels)
            {
                return;
            }

            _isFetchingChannels = true;

            try
            {
                RadioChannelModel[] channels = await _repository.FetchChannelsAsync();
                Model.ApplyChannels(channels);
            }
            finally
            {
                _isFetchingChannels = false;
            }
        }

        private async UniTask _refreshSongsAsync()
        {
            if (_isFetchingSongs)
            {
                return;
            }

            RadioChannelModel current = Model.GetCurrentChannel();

            if (string.IsNullOrEmpty(current.id))
            {
                return;
            }

            _isFetchingSongs = true;

            try
            {
                RadioSongListModel songs = await _repository.FetchSongsAsync(current.id);
                Model.ApplySongs(songs);
            }
            finally
            {
                _isFetchingSongs = false;
            }
        }
    }
}