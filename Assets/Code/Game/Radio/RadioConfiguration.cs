using UnityEngine;

namespace Code.Game.Radio
{
    [CreateAssetMenu(fileName = "RadioConfiguration", menuName = "Configuration/Radio")]
    public class RadioConfiguration : ScriptableObject
    {
        public const string CHANNEL_MODELS_URL = "https://somafm.com/channels.json";
        
        public const string STREAM_URL_TEMPLATE = "https://ice1.somafm.com/{0}-128-mp3";

        public const int PREVIOUS_TRACKS_COUNT  = 5;

        [field: SerializeField] public float ChannelsUpdateInterval  { get; private set;  }  = 60f;
        [field: SerializeField] public float TrackUpdateInterval { get; private set; } = 30f;
        [field: SerializeField] public Texture2D DefaultChannelLogo { get; private set; }
        
        
        public static string GetTrackModelURL(string channel)
        {
            return $"https://somafm.com/songs/{channel}.json";
        }
        
        public static string FormatTrack(string artist, string title)
        {
            return $"{artist} - {title}";
        }
    }
}