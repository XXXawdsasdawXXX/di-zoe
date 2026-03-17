using System;
using System.Collections.Generic;
using System.Text;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Game.Radio;
using Code.Tools;
using Code.UI.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TriInspector;
using UnityEditor;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioPresenter : UIPresenter<UIRadioView>, IInitializeListener, ISubscriber, IStartListener
    {
        private RadioService _radioModels;
        private RadioConfiguration _radioConfiguration;

        private bool _isHiddenPreviousTracks = true;

        
        #region Life

        public UniTask GameInitialize()
        {
            Container.Instance.GetService<RadioPlayer>();
            _radioModels = Container.Instance.GetService<RadioService>();
            _radioConfiguration = Container.Instance.GetConfig<RadioConfiguration>();

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
            view.UIButton_previousTracks.SubscribeToClicked(_switchPreviousTracksView);
            view.UISlider_volume.SubscribeToElement(_radioModels.SetVolume);
            view.UIDropDown_channels.SubscribeToDropDown(_radioModels.SetChannel);
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
            view.UIButton_previousTracks.UnsubscribeFromClicked(_switchPreviousTracksView);
            view.UISlider_volume.UnsubscribeFromElement(_radioModels.SetVolume);
            view.UIDropDown_channels.UnsubscribeFromDropDown(_radioModels.SetChannel);
        }

        public UniTask GameStart()
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

            UIRadioChannelTab mainTab =  view.UIDropDown_channels
                .SetCurrentValueWithoutNotify(_radioModels.State.CurrentChannelIndex.PropertyValue) as UIRadioChannelTab;

            if (mainTab)
            {
                RadioChannelModel currentChannel = _radioModels.State.GetCurrentChannel();
                mainTab.SetModel(new UIRadioChannelTab.Model
                {
                    Name = currentChannel.title,
                    Genre = currentChannel.genre,
                    IsFavorite = false
                });
            }
            
            _updateChannelView(_radioModels.State.CurrentChannelIndex.PropertyValue);

            return UniTask.CompletedTask;
        }

        #endregion
        
        private void _setRandomChannel()
        {
            int index = UnityEngine.Random.Range(0, _radioModels.State.Channels.Count);
            view.UIDropDown_channels.SetCurrentValueWithoutNotify(index);
            _radioModels.SetChannel(index);
        }
        
        private void _updateVolume(float volume)
        {
            view.UISlider_volume.SetValueWithoutNotify(volume);
        }

        private void _tryUpdateListenersCountView(RadioChannelModel model)
        {
            view.UIText_listenerCount.SetText($"Listeners: {model.listeners}");
        }

        private void _updateCurrentSongView(RadioSongModel model)
        {
            view.UIText_currentTrack.SetText($"{model.artist} - {model.title}");
        }

        private void _updatePreviousSongsView(RadioSongListModel songs)
        {
            int count = Math.Min(_radioConfiguration.PreviousTracksCount, songs.Songs.Count);
            
            if (songs.Songs == null || count == 0 || _isHiddenPreviousTracks)
            {
                view.UIText_previousTracks.StopTypewrite();

                return;
            }

            StringBuilder stringBuilder = new();
            stringBuilder.Append("Recently Played");
            stringBuilder.AppendLine();
            
            for (int i = 0; i < count; i++)
            {
                if (songs.Songs[i] == null || stringBuilder.Length > 180)
                {
                    break;
                }
                
                stringBuilder.Append(songs.Songs[i].artist);
                stringBuilder.Append(" - ");
                stringBuilder.Append(songs.Songs[i].title);
                stringBuilder.AppendLine();
            }
            
            Debug.Log($"length {stringBuilder.Length}");
            
            stringBuilder.Append("...");
            
            view.UIText_previousTracks.StartTypewrite(stringBuilder.ToString()).Forget();
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

            _updateCurrentSongView(_radioModels.State.CurrentSong.PropertyValue);

            _updatePreviousSongsView(_radioModels.State.PreviousSongs.PropertyValue);
        }

        [Button]
        private void _switchPreviousTracksView()
        {
            const float hiddenHeight = 295;
            const float shownHeight = 415;

            Vector2 size = view.Rect_Background.sizeDelta;
            _isHiddenPreviousTracks = Math.Abs(size.y - shownHeight) < 1;
            size.y = _isHiddenPreviousTracks ? hiddenHeight : shownHeight;

            view.UIText_previousTracks.SetText("");
            view.UIText_previousTracks.gameObject.SetActive(!_isHiddenPreviousTracks);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                view.Rect_Background.sizeDelta = size;
                EditorUtility.SetDirty(gameObject);
                return;
            }
#endif
            Sequence sequence = DOTween.Sequence();

            Vector2 defaultButtonSize = view.UIButton_previousTracks.Rect.sizeDelta;

            sequence.Append(view.Rect_Background.DOSizeDelta(size, UIConfiguration.ANIMATION_DURATION_LONG));
            sequence.Join(
                view.UIButton_previousTracks.Rect.DOSizeDelta(Vector2.zero, UIConfiguration.ANIMATION_DURATION_SHORT));
            sequence.Append(view.UIButton_previousTracks.Rect.DORotate(new Vector3(0, 0, _isHiddenPreviousTracks ? 0 : 180), 0));
            sequence.AppendCallback(() => view.UIButton_previousTracks.OnPointerExit(null));
            sequence.Append(view.UIButton_previousTracks.Rect.DOSizeDelta(defaultButtonSize,
                UIConfiguration.ANIMATION_DURATION_SHORT));
            sequence.AppendCallback(() => _updatePreviousSongsView(_radioModels.State.PreviousSongs.PropertyValue));
            sequence.Play();
        }
    }
}