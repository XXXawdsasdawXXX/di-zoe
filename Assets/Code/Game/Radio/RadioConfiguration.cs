using UnityEngine;

namespace Code.Game.Radio
{
    [CreateAssetMenu(fileName = "RadioConfiguration", menuName = "Configuration/Radio")]
    public class RadioConfiguration : ScriptableObject
    {
        public const string CHANNEL_MODELS_URL = "https://somafm.com/channels.json";
        
        [field: SerializeField] public int PreviousTracksCount { get; private set; }  = 5; //max 10
        [field: SerializeField] public float ChannelsUpdateInterval  { get; private set;  }  = 60f;
        [field: SerializeField] public float TrackUpdateInterval { get; private set; } = 30f;
        
        [field: SerializeField] public Texture2D DefaultChannelLogo { get; private set; }
        
        
        public static string GetTrackModelURL(string channel)
        {
            return $"https://somafm.com/songs/{channel}.json";
        }
    }
}