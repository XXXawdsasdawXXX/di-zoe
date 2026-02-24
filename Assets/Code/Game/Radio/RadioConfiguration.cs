using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Code.Game.Radio
{
    [CreateAssetMenu(fileName = "RadioConfiguration", menuName = "Configuration/Radio")]
    public class RadioConfiguration : ScriptableObject
    {
        [Serializable]
        public struct ChannelData
        {
            public string Id;
            public string Path;
        }

        public const string ChannelModelsURL = "https://somafm.com/channels.json";
        
        [field: MaxValue(10)] public int PreviousTracksCount { get; }  = 5;
        public  float ChannelsUpdateInterval  { get; }  = 60f;
        public  float TrackUpdateInterval { get; } = 30f;
        
        
        public static string GetTrackModelURL(string channel)
        {
            return $"https://somafm.com/songs/{channel}.json";
        }

        public static string GetStreamURL(string channel)
        {
            return $"//ice1.somafm.com/{channel}-256-mp3";
        }
    }
}