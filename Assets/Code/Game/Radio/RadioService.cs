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
    public class RadioService : IService, IInitializeListener, IUpdateListener, IProgressWriter
    {
        private RadioConfiguration _config;
        private RadioRepository    _repository;
        private RadioState         _state;
 
        private Timer _channelsTimer;
        private Timer _tracksTimer;
 
        // Флаг защиты от параллельных запросов
        private bool _isFetchingChannels;
        private bool _isFetchingSongs;
 
        // Удобный публичный доступ к состоянию (фасад для UI)
        public RadioState State => _state;
 
        public async UniTask GameInitialize()
        {
            _config     = Container.Instance.GetConfiguration<RadioConfiguration>();
            _repository = Container.Instance.GetService<RadioRepository>();
            _state      = Container.Instance.GetService<RadioState>();
 
            _channelsTimer = new Timer(_config.ChannelsUpdateInterval);
            _tracksTimer   = new Timer(_config.TrackUpdateInterval);
 
            await RefreshChannelsAsync();
        }
 
        public async UniTask LoadProgress(PlayerProgressData playerProgress)
        {
            _state.CurrentChannelIndex.PropertyValue = playerProgress.RadioChanel;
            _state.RadioVolume.PropertyValue         = playerProgress.RadioVolume;
 
            await RefreshSongsAsync();
        }
 
        public void SaveProgress(PlayerProgressData playerProgress)
        {
            playerProgress.RadioChanel  = _state.CurrentChannelIndex.PropertyValue;
            playerProgress.RadioVolume  = _state.RadioVolume.PropertyValue;
        }
 
        public void GameUpdate()
        {
            float dt = Time.deltaTime;
 
            if (_channelsTimer.Update(dt))
                RefreshChannelsAsync().Forget(Debug.LogException);
 
            if (_tracksTimer.Update(dt))
                RefreshSongsAsync().Forget(Debug.LogException);
        }
 
        // ── Публичное API для UI ──────────────────────────────────────────────
 
        public void SetChannel(int channelIndex)
        {
            _state.CurrentChannelIndex.PropertyValue = channelIndex;
 
            // Форсируем немедленное обновление треков при смене канала
            _tracksTimer.Finish();
        }
 
        public void SetVolume(float volume)
        {
            _state.RadioVolume.PropertyValue = volume;
        }
 
        public string GetCurrentStreamUrl()
        {
            RadioChannelModel channel = _state.GetCurrentChannel();
            return string.Format(_config.StreamUrlTemplate, channel.id);
        }
 
        public UniTask<Texture2D> GetChannelLogoAsync(string logoUrl)
        {
            return _repository.FetchLogoAsync(logoUrl);
        }
 
        // ── Внутренние методы обновления ─────────────────────────────────────
 
        private async UniTask RefreshChannelsAsync()
        {
            // Не запускаем параллельный запрос если предыдущий ещё идёт
            if (_isFetchingChannels) return;
 
            _isFetchingChannels = true;
            try
            {
                RadioChannelModel[] channels = await _repository.FetchChannelsAsync();
                _state.ApplyChannels(channels);
            }
            finally
            {
                _isFetchingChannels = false;
            }
        }
 
        private async UniTask RefreshSongsAsync()
        {
            if (_isFetchingSongs) return;
 
            RadioChannelModel current = _state.GetCurrentChannel();
            if (string.IsNullOrEmpty(current.id)) return;
 
            _isFetchingSongs = true;
            try
            {
                RadioSongListModel songs = await _repository.FetchSongsAsync(current.id);
                _state.ApplySongs(songs);
            }
            finally
            {
                _isFetchingSongs = false;
            }
        }
    }
}