using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Game.Radio;
using Code.Tools;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioPresenter : UIPresenter<UIRadioView>, IInitializeListener, ISubscriber, IStartListener
    {
        private RadioPlayer _radioPlayer;
        private RadioModelService _radioModels;
        private RadioConfiguration _radioConfiguration;


        #region Life
        
        public UniTask GameInitialize()
        {
            _radioPlayer = Container.Instance.GetService<RadioPlayer>();
            _radioModels = Container.Instance.GetService<RadioModelService>();
            _radioConfiguration = Container.Instance.GetConfig<RadioConfiguration>();
            
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioModels.Channels)
            {
                channel.Value.SubscribeToValue(_tryUpdateListenersCountView);
            }
            
            _radioModels.CurrentSong.SubscribeToValue(_updateCurrentSongView);
            _radioModels.PreviousSongs.SubscribeToValue(_updatePreviousSongsView);
            _radioModels.CurrentChannel.SubscribeToValue(_updateChannelView);
            
            view.UISlider_volume.SubscribeToElement(_radioPlayer.SetVolume);
            view.UIDropDown_channels.SubscribeToElement(_radioModels.SetCurrentChannel);
        }
        
        public UniTask GameStart()
        {
            List<TMP_Dropdown.OptionData> channels = new();

            foreach (KeyValuePair<string,ReactiveProperty<RadioChannelModel>> channel in _radioModels.Channels)
            {
                channels.Add(new TMP_Dropdown.OptionData($"<color=#000000>{channel.Value.PropertyValue.genre}:</color> \n{channel.Key}"));
            }

            view.UIDropDown_channels.SetValues(channels);
            view.UIDropDown_channels.SetCurrentValueWithoutNotify(_radioModels.CurrentChannel.PropertyValue);
            _updateChannelView(_radioModels.CurrentChannel.PropertyValue);
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioModels.Channels)
            {
                channel.Value.UnsubscibeFromValue(_tryUpdateListenersCountView);
            }
            
            _radioModels.CurrentSong.UnsubscibeFromValue(_updateCurrentSongView);
            _radioModels.PreviousSongs.UnsubscibeFromValue(_updatePreviousSongsView);
            _radioModels.CurrentChannel.UnsubscibeFromValue(_updateChannelView);
            
            view.UISlider_volume.UnsubscribeFromElement(_radioPlayer.SetVolume);
            view.UIDropDown_channels.UnsubscribeFromElement(_radioModels.SetCurrentChannel);
        }

        #endregion

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
            if (songs.Songs == null || songs.Songs.Count == 0)
            {
                view.UIText_previousTracks.SetText(string.Empty);

                return;
            }
            
            StringBuilder stringBuilder = new();

            stringBuilder.Append("Recently Played");
            stringBuilder.AppendLine();
            
            for (int i = 0; i < _radioConfiguration.PreviousTracksCount; i++)
            {
                stringBuilder.Append(songs.Songs[i].artist);
                stringBuilder.Append(" - ");
                stringBuilder.Append(songs.Songs[i].title);
                stringBuilder.AppendLine();
            }

            view.UIText_previousTracks.SetText(stringBuilder.ToString());
        }

        private async void _updateChannelView(int channelIndex)
        {
            RadioChannelModel channelModel = _radioModels.GetCurrentChannelModel();
            
            Texture2D texture2D = await _radioModels.GetChannelLogo(channelModel.image);
            
            view.UIRawImage_channelLogo.SetTexture(texture2D);
            
            view.UIText_channel_name.SetText(channelModel.title);
            
            view.UIText_channel_description.SetText(channelModel.description);

            view.UIText_channel_genre.SetText(channelModel.genre);
            
            _updateCurrentSongView(_radioModels.CurrentSong.PropertyValue);
            
            _updatePreviousSongsView(_radioModels.PreviousSongs.PropertyValue);
        }
    }
}