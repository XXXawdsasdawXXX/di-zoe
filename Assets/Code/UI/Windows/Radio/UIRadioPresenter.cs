using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Game.Radio;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Windows.Radio
{
    public class UIRadioPresenter : UIPresenter<UIRadioView>, IInitializeListener, ISubscriber, IStartListener
    {
        private RadioService _radioModels;
        private RadioConfiguration _radioConfiguration;
        private RadioFavoriteStuff _radioFavoriteStuff;


        #region Life

        public UniTask GameInitialize()
        {
            Container.Instance.GetService<RadioPlayer>();
            _radioModels = Container.Instance.GetService<RadioService>();
            _radioConfiguration = Container.Instance.GetConfiguration<RadioConfiguration>();
            _radioFavoriteStuff = Container.Instance.GetService<RadioFavoriteStuff>();

            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioModels.State.Channels)
            {
                channel.Value.SubscribeToValue(_tryUpdateListenersCountView);
            }

            _radioModels.State.CurrentSong.SubscribeToValue(_updateCurrentSongView);
            _radioModels.State.PreviousSongs.SubscribeToValue(_updatePreviousSongsView);
            _radioModels.State.CurrentChannelIndex.SubscribeToValue(_updateChannelView);
            _radioModels.State.RadioVolume.SubscribeToValue(_updateVolume);

            view.UIButton_randomChannel.SubscribeToClicked(_setRandomChannel);
            view.UISlider_volume.SubscribeToElement(_radioModels.SetVolume);
            view.UIDropDown_channels.SubscribeToDropDown(_radioModels.SetChannel);
        }

        public async UniTask GameStart()
        {
            await _createChannelList();
        }
        
        public void Unsubscribe()
        {
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioModels.State.Channels)
            {
                channel.Value.UnsubscibeFromValue(_tryUpdateListenersCountView);
            }
            
            _radioModels.State.CurrentSong.UnsubscibeFromValue(_updateCurrentSongView);
            _radioModels.State.PreviousSongs.UnsubscibeFromValue(_updatePreviousSongsView);
            _radioModels.State.CurrentChannelIndex.UnsubscibeFromValue(_updateChannelView);
            _radioModels.State.RadioVolume.UnsubscibeFromValue(_updateVolume);

            view.UIButton_randomChannel.UnsubscribeFromClicked(_setRandomChannel);
            view.UISlider_volume.UnsubscribeFromElement(_radioModels.SetVolume);
            view.UIDropDown_channels.UnsubscribeFromDropDown(_radioModels.SetChannel);
        }

        #endregion

        #region Channel

        private async Task _createChannelList()
        {
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioModels.State.Channels)
            {
                UIRadioChannelTab tab = view.UIDropDown_channels.AddElement() as UIRadioChannelTab;

                if (tab != null)
                {
                    tab.SetModel(new UIRadioChannelTab.Model
                    {
                        Name = channel.Key,
                        Genre = channel.Value.PropertyValue.genre,
                        IsFavorite = false
                    });
                }
            }

            _updateChannelView(_radioModels.State.CurrentChannelIndex.PropertyValue);

            await UniTask.DelayFrame(1);
            RectTransform[] layouts = view.Rect.GetComponentsInChildren<RectTransform>();
            Array.Reverse(layouts);
            foreach (RectTransform rect in layouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(view.Rect);
        }
        
        private void _setRandomChannel()
        {
            int index = UnityEngine.Random.Range(0, _radioModels.State.Channels.Count);
            view.UIDropDown_channels.SetCurrentValueWithoutNotify(index);
            _radioModels.SetChannel(index);
        }
        
        private void _tryUpdateListenersCountView(RadioChannelModel model)
        {
            view.UIText_listenerCount.SetText($"Listeners: {model.listeners}");
        }

        private async void _updateChannelView(int channelIndex)
        {
            RadioChannelModel channelModel = _radioModels.State.GetCurrentChannel();

            string logoUrl = channelModel.image.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                ? channelModel.largeimage
                : channelModel.image;
            
            Texture2D texture2D = await _radioModels.GetChannelLogoAsync(logoUrl);

            view.UIRawImage_channelLogo.SetTexture(texture2D);
            view.UIText_channel_name.SetText(channelModel.title);
            view.UIText_channel_description.SetText(channelModel.description);
            view.UIText_channel_genre.SetText(channelModel.genre);

            UIRadioChannelTab mainTab = view.UIDropDown_channels.UIRadioButton_main as UIRadioChannelTab;
            if (mainTab != null)
            {
                mainTab.SetModel(new UIRadioChannelTab.Model
                {
                    Name = channelModel.title,
                    Genre = channelModel.genre,
                    IsFavorite = false
                });
            }
            
            _updateCurrentSongView(_radioModels.State.CurrentSong.PropertyValue);
        }

        #endregion

        #region Tracks

        private void _updateCurrentSongView(RadioSongModel model)
        {
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
                IsFavorite = _radioFavoriteStuff.IsFavoriteTrack(track)
            });

            tab.UIRadioButton_Fav.IsChecked.SubscribeToValue(value =>
            {
                if (value) {_radioFavoriteStuff.AddTrack(track);}
                else _radioFavoriteStuff.RemoveTrack(track);
            });
        }

        #endregion
        
        private void _updateVolume(float volume)
        {
            view.UISlider_volume.SetValueWithoutNotify(volume);
        }
    }
}