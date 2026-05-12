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
        private RadioFavoriteContent radioFavoriteContent;
        

        #region Life

        public UniTask GameInitialize()
        {
            Container.Instance.GetService<RadioPlayer>();
            _radioTranslation = Container.Instance.GetService<RadioTranslation>();
            _radioConfiguration = Container.Instance.GetConfiguration<RadioConfiguration>();
            radioFavoriteContent = Container.Instance.GetService<RadioFavoriteContent>();

            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioTranslation.Model.Channels)
            {
                channel.Value.SubscribeToValue(_updateListenersCountView);
            }

            _radioTranslation.Model.CurrentSong.SubscribeToValue(_updateCurrentSongView);
            _radioTranslation.Model.PreviousSongs.SubscribeToValue(_updatePreviousSongsView);
            _radioTranslation.Model.CurrentChannelIndex.SubscribeToValue(_updateCurrentChannelView);
            _radioTranslation.Model.RadioVolume.SubscribeToValue(_updateVolume);

            view.UIButton_randomChannel.SubscribeToClicked(_setRandomChannel);
            view.UISlider_volume.SubscribeToElement(_radioTranslation.SetVolume);
            view.UIDropDown_channels.SubscribeToChangeChannel(_radioTranslation.SetChannel);
        }

        public UniTask GameStart()
        {
            view.UIDropDown_channels.InitializeAllChannels();
            view.UIDropDown_channels.InitializeFavoriteChannels();
          
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
            _radioTranslation.Model.PreviousSongs.UnsubscibeFromValue(_updatePreviousSongsView);
            _radioTranslation.Model.CurrentChannelIndex.UnsubscibeFromValue(_updateCurrentChannelView);
            _radioTranslation.Model.RadioVolume.UnsubscibeFromValue(_updateVolume);

            view.UIButton_randomChannel.UnsubscribeFromClicked(_setRandomChannel);
            view.UISlider_volume.UnsubscribeFromElement(_radioTranslation.SetVolume);
            view.UIDropDown_channels.UnsubscribeFromChangeChannel(_radioTranslation.SetChannel);
        }

        #endregion

        #region Channel
        
        private void _setRandomChannel()
        {
            int index = UnityEngine.Random.Range(0, _radioTranslation.Model.Channels.Count);
            _radioTranslation.SetChannel(index);
            view.UIDropDown_channels.UpdateCurrentChannel();
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

            view.UIDropDown_channels.UpdateCurrentChannel();
      
            _updateCurrentSongView(_radioTranslation.Model.CurrentSong.PropertyValue);
        }

        #endregion

        #region Tracks

        private void _updateCurrentSongView(RadioSongModel model)
        {
            if (model == null)
            {
                view.UIText_currentTrack.SetText(string.Empty);
                return;
            }
            view.UIText_currentTrack.SetText($"{model.artist} - {model.title}");
        }
        
        private void _updatePreviousSongsView(RadioSongListModel songs)
        {
            int count = Math.Min(_radioConfiguration.PreviousTracksCount, songs.Songs.Count);
            
            if (songs.Songs == null || count == 0)
            {
                return;
            }

            view.UIDropDown_previousTracks.ClearElements();
            
            for (int i = 1; i < count; i++)
            {
                if (songs.Songs[i] == null)
                {
                    break;
                }
                
                UIRadioTrackTab tab = view.UIDropDown_previousTracks.AddElement() as UIRadioTrackTab;
                
                if (tab != null)
                {
                    _initializeTrackTab(songs, i, tab);
                }
            }
            
            UIRadioTrackTab mainTab = view.UIDropDown_previousTracks.UIRadioButton_main as UIRadioTrackTab;
           
            if (mainTab != null)
            {
                _initializeTrackTab(songs, 0, mainTab);
            }
        }

        private void _initializeTrackTab(RadioSongListModel songs, int i, UIRadioTrackTab tab)
        {
            string track = RadioConfiguration.FormatTrack(songs.Songs[i].artist, songs.Songs[i].title);

            tab.SetModel(new UIRadioTrackTab.Model
            {
                Artist = songs.Songs[i].artist,
                Title = songs.Songs[i].title,
                IsFavorite = radioFavoriteContent.IsFavoriteTrack(track)
            });

            tab.UIRadioButton_Fav.IsChecked.SubscribeToValue(value =>
            {
                if (value) {radioFavoriteContent.AddTrack(track);}
                else radioFavoriteContent.RemoveTrack(track);
            });
        }

        #endregion
        
        private void _updateVolume(float volume)
        {
            view.UISlider_volume.SetValueWithoutNotify(volume);
        }
    }
}