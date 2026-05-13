using Code.Core.GameLoop;
using Code.Core.Save;
using Code.Core.Save.SavedData;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Game.Radio
{
    [Preserve]
    public class RadioTranslation : IService, IInitializeListener, IUpdateListener, IProgressWriter
    {
        public RadioModel Model => _radioModel;
        
        private RadioConfiguration _config;
        private RadioRepository    _repository;
        private RadioModel         _radioModel;
 
        private Timer _channelsTimer;
        private Timer _tracksTimer;
 
        private bool _isFetchingChannels;
        private bool _isFetchingSongs;
        
        
        public async UniTask GameInitialize()
        {
            _config     = Container.Instance.GetConfiguration<RadioConfiguration>();
            _repository = Container.Instance.GetService<RadioRepository>();
            _radioModel      = Container.Instance.GetService<RadioModel>();
 
            _channelsTimer = new Timer(_config.ChannelsUpdateInterval);
            _tracksTimer   = new Timer(_config.TrackUpdateInterval);
 
            await _refreshChannelsAsync();
        }
 
        public async UniTask LoadProgress(PlayerProgressData playerProgress)
        {
            Debug.Log("translation load progress");
            
            _radioModel.CurrentChannelIndex.PropertyValue = playerProgress.RadioChanel;
            _radioModel.RadioVolume.PropertyValue         = playerProgress.RadioVolume;
 
            await _refreshSongsAsync();
        }
 
        public void SaveProgress(PlayerProgressData playerProgress)
        {
            playerProgress.RadioChanel  = _radioModel.CurrentChannelIndex.PropertyValue;
            playerProgress.RadioVolume  = _radioModel.RadioVolume.PropertyValue;
        }
 
        public void GameUpdate()
        {
            float dt = Time.deltaTime;
 
            if (_channelsTimer.Update(dt))
                _refreshChannelsAsync().Forget(Debug.LogException);
 
            if (_tracksTimer.Update(dt))
                _refreshSongsAsync().Forget(Debug.LogException);
        }
 
        // ── Публичное API для UI ──────────────────────────────────────────────
 
        public void SetChannel(int channelIndex)
        {
            _radioModel.CurrentChannelIndex.PropertyValue = channelIndex;
 
            // Форсируем немедленное обновление треков при смене канала
            _tracksTimer.Finish();
        }
 
        public void SetVolume(float volume)
        {
            _radioModel.RadioVolume.PropertyValue = volume;
        }
 
        public string GetCurrentStreamUrl()
        {
            RadioChannelModel channel = _radioModel.GetCurrentChannel();
            return string.Format(RadioConfiguration.STREAM_URL_TEMPLATE, channel.id);
        }
 
        public UniTask<Texture2D> GetChannelLogoAsync(string logoUrl)
        {
            return _repository.FetchLogoAsync(logoUrl);
        }
        
        private async UniTask _refreshChannelsAsync()
        {
            // Не запускаем параллельный запрос если предыдущий ещё идёт
            if (_isFetchingChannels) return;
 
            _isFetchingChannels = true;
            try
            {
                RadioChannelModel[] channels = await _repository.FetchChannelsAsync();
                _radioModel.ApplyChannels(channels);
            }
            finally
            {
                _isFetchingChannels = false;
            }
        }
 
        private async UniTask _refreshSongsAsync()
        {
            if (_isFetchingSongs) return;
 
            RadioChannelModel current = _radioModel.GetCurrentChannel();
            if (string.IsNullOrEmpty(current.id)) return;
 
            _isFetchingSongs = true;
            try
            {
                RadioSongListModel songs = await _repository.FetchSongsAsync(current.id);
                _radioModel.ApplySongs(songs);
            }
            finally
            {
                _isFetchingSongs = false;
            }
        }
    }
}