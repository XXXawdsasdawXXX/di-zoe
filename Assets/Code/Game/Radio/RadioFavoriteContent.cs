using System.Collections.Generic;
using Code.Core.Save;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Game.Radio
{
    [Preserve]
    public class RadioFavoriteContent : IService, IProgressWriter
    {
        public List<RadioSongModel> Songs { get; private set; } = new();

        private List<int> _channelList = new();


        public UniTask LoadProgress(PlayerProgressData playerProgress)
        {
            Debug.Log("favorite content LoadProgress");
            _channelList = playerProgress.FavoriteRadioChannels ?? new List<int>();
            Songs = playerProgress.FavoriteRadioTracks
                             .ToDeserialized<List<RadioSongModel>>() ?? new List<RadioSongModel>();

            return UniTask.CompletedTask;
        }

        public void SaveProgress(PlayerProgressData playerProgress)
        {
            playerProgress.FavoriteRadioChannels = _channelList;
            playerProgress.FavoriteRadioTracks = Songs.ToJson();
        }

        public void AddChannel(int index)
        {
            Debug.Log($"add channel {index} {!_channelList.Contains(index)}");
       
            if (_channelList.Contains(index))
            {
                return;
            }

            _channelList.Add(index);
        }

        public void AddSong(RadioSongModel song)
        {
            if (Songs.Contains(song))
            {
                return;
            }

            Songs.Add(song);
            Debug.Log($"Fav stuff: add track {song}");
        }

        public bool IsFavoriteChannel(int index)
        {
            return _channelList.Contains(index);
        }

        public bool IsFavoriteSong(RadioSongModel song)
        {
            return Songs.Contains(song);
        }
        
        public void RemoveSong(RadioSongModel song)
        {
            if (Songs.Contains(song))
            {
                Songs.Remove(song);

                Debug.Log($"Fav stuff: remove song {song}");
            }
        }

        public void RemoveChannel(int index)
        {
            Debug.Log($"remove channel {index} {_channelList.Contains(index)}");

            if (_channelList.Contains(index))
            {
                _channelList.Remove(index);
            }
        }
    }
}