using System;
using System.Collections.Generic;
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
        private RadioTranslation _radioTranslation;
        private RadioConfiguration _radioConfiguration;
        private RadioFavoriteContent _radioFavoriteContent;
        
        
        public UniTask GameInitialize()
        {
            Container.Instance.GetService<RadioPlayer>();
            _radioTranslation = Container.Instance.GetService<RadioTranslation>();
            _radioConfiguration = Container.Instance.GetConfiguration<RadioConfiguration>();
            _radioFavoriteContent = Container.Instance.GetService<RadioFavoriteContent>();

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
            view.UIRadioChannelDropdown.SubscribeToChangeChannel(_radioTranslation.SetChannel);
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
            view.UIRadioChannelDropdown.UnsubscribeFromChangeChannel(_radioTranslation.SetChannel);
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

        private async void _updateCurrentChannelView(int channelIndex)
        {
            RadioChannelModel channelModel = _radioTranslation.Model.GetCurrentChannel();

            string logoUrl = channelModel.image.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                ? channelModel.largeimage
                : channelModel.image;
            
            Texture2D texture2D = await _radioTranslation.GetChannelLogoAsync(logoUrl);

            view.UIRawImage_channelLogo.SetTexture(texture2D);
            view.UIText_channel_name.SetText(channelModel.title);
            view.UIText_channel_description.SetText(channelModel.description);
            view.UIText_channel_genre.SetText(channelModel.genre);

            _updateCurrentSongView(_radioTranslation.Model.CurrentSong.PropertyValue);
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