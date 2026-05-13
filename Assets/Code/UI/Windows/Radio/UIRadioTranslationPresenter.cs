using System;
using System.Collections.Generic;
using System.Threading;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Game.Radio;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioTranslationPresenter : UIPresenter<UIRadioTranslationView>, IInitializeListener, ISubscriber, IStartListener
    {
        private CancellationTokenSource _logoCts;
        private RadioTranslation _radioTranslation;

        public UniTask GameInitialize()
        {
            Container.Instance.GetService<RadioPlayer>();
            _radioTranslation = Container.Instance.GetService<RadioTranslation>();
 
            return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioTranslation.Model.Channels)
            {
                channel.Value.SubscribeToValue(_updateListenersCountView);
            }

            _radioTranslation.Model.CurrentSong.SubscribeToValue(_updateCurrentSongView);
            _radioTranslation.Model.CurrentChannelIndex.SubscribeToValue(_updateCurrentChannelView);
            _radioTranslation.Model.RadioVolume.SubscribeToValue(_updateVolume);

            view.UIButton_randomChannel.SubscribeToClicked(_setRandomChannel);
            view.UISlider_volume.SubscribeToElement(_radioTranslation.SetVolume);
        }

        public UniTask GameStart()
        {
            _updateCurrentChannelView(_radioTranslation.Model.CurrentChannelIndex.PropertyValue);
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioTranslation.Model.Channels)
            {
                channel.Value.UnsubscibeFromValue(_updateListenersCountView);
            }
            
            _radioTranslation.Model.CurrentSong.UnsubscibeFromValue(_updateCurrentSongView);
            _radioTranslation.Model.CurrentChannelIndex.UnsubscibeFromValue(_updateCurrentChannelView);
            _radioTranslation.Model.RadioVolume.UnsubscibeFromValue(_updateVolume);
            
            view.UIButton_randomChannel.UnsubscribeFromClicked(_setRandomChannel);
            view.UISlider_volume.UnsubscribeFromElement(_radioTranslation.SetVolume);
            
            _logoCts?.Cancel();
            _logoCts?.Dispose();
        }

        private async void _updateCurrentChannelView(int channelIndex)
        {
            _logoCts?.Cancel();
            _logoCts?.Dispose();
            _logoCts = new CancellationTokenSource();
          
            CancellationToken ct = _logoCts.Token;

            RadioChannelModel channelModel = _radioTranslation.Model.GetCurrentChannel();

            view.UIText_channel_name.SetText(channelModel.title);
            view.UIText_channel_description.SetText(channelModel.description);
            view.UIText_channel_genre.SetText(channelModel.genre);
            _updateCurrentSongView(_radioTranslation.Model.CurrentSong.PropertyValue);

            string logoUrl = channelModel.image.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                ? channelModel.largeimage
                : channelModel.image;

            try
            {
                Texture2D texture2D = await _radioTranslation.GetChannelLogoAsync(logoUrl, ct);
        
                if (ct.IsCancellationRequested)
                {
                    return; 
                }   
        
                view.UIRawImage_channelLogo.SetTexture(texture2D);
            }
            catch (OperationCanceledException) { /* норм, канал сменился */ }
        }

        private void _setRandomChannel()
        {
            int index = UnityEngine.Random.Range(0, _radioTranslation.Model.Channels.Count);
            _radioTranslation.SetChannel(index);
        }

        private void _updateListenersCountView(RadioChannelModel model)
        {
            view.UIText_listenerCount.SetText($"Listeners: {model.listeners}");
        }

        private void _updateCurrentSongView(RadioSongModel model)
        {
            if (model == null)
            {
                view.UIText_currentTrack.SetText(string.Empty);
                return;
            }
            view.UIText_currentTrack.SetText($"{model.artist} - {model.title}");
        }
        
        private void _updateVolume(float volume)
        {
            view.UISlider_volume.SetValueWithoutNotify(volume);
        }
    }
}