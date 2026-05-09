using System.Collections.Generic;
using Code.Core.Save;
using Code.Core.Save.SavedData;
using Code.Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Game.Radio
{
    [Preserve]
    public class RadioFavoriteStuff : IService, IProgressWriter
    {
        private List<string> _trackList = new();
        private List<int> _channelList = new();


        public UniTask LoadProgress(PlayerProgressData playerProgress)
        {
            _channelList = playerProgress.FavoriteRadioChannels ?? new List<int>();
            _trackList = playerProgress.FavoriteRadioTracks ?? new List<string>();

            return UniTask.CompletedTask;
        }

        public void SaveProgress(PlayerProgressData playerProgress)
        {
            playerProgress.FavoriteRadioChannels = _channelList;
            playerProgress.FavoriteRadioTracks = _trackList;
        }

        public void AddChannel(int index)
        {
            if (_channelList.Contains(index))
            {
                return;
            }

            _channelList.Add(index);
        }

        public void AddTrack(string track)
        {
            if (_trackList.Contains(track))
            {
                return;
            }

            _trackList.Add(track);
            Debug.Log($"Fav stuff: add track {track}");
        }
        
        public void AddTrack(string artist, string title)
        {
            AddTrack(RadioConfiguration.FormatTrack(artist, title));
        }

        public bool IsFavoriteChannel(int index)
        {
            return _channelList.Contains(index);
        }

        public bool IsFavoriteTrack(string track)
        {
            return _trackList.Contains(track);
        }
        
        public bool IsFavoriteTrack(string artist, string title)
        {
            return _trackList.Contains(RadioConfiguration.FormatTrack(artist, title));
        }


        public void RemoveTrack(string track)
        {
            if (_trackList.Contains(track))
            {
                _trackList.Remove(track);
                
                Debug.Log($"Fav stuff: remove track {track}");
            }
        }
    }
}