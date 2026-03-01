using NaughtyAttributes;
using UnityEngine;

namespace Code.Game.Radio
{
    [CreateAssetMenu(fileName = "RadioConfiguration", menuName = "Configuration/Radio")]
    public class RadioConfiguration : ScriptableObject
    {
        public const string CHANNEL_MODELS_URL = "https://somafm.com/channels.json";
        
        [field: MaxValue(10), SerializeField] public int PreviousTracksCount { get; private set; }  = 5;
        [field: SerializeField] public  float ChannelsUpdateInterval  { get; private set;  }  = 60f;
        [field: SerializeField] public  float TrackUpdateInterval { get; private set; } = 30f;
        
        
        public static string GetTrackModelURL(string channel)
        {
            return $"https://somafm.com/songs/{channel}.json";
        }
    }
}