using NaughtyAttributes;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioView : UIView
    {
        [field: SerializeField, BoxGroup("Chanel")] public UIText UIText_channel_name { get; private set; }
        [field: SerializeField, BoxGroup("Chanel")] public UIText UIText_channel_description { get; private set; }
        [field: SerializeField, BoxGroup("Chanel")] public UIText UIText_channel_genre { get; private set; }
        [field: SerializeField, BoxGroup("Chanel")] public UISlider UISlider_volume { get; private set; }
        [field: SerializeField, BoxGroup("Chanel")] public UIRawImage UIRawImage_channelLogo { get; private set; }
        [field: SerializeField, BoxGroup("Chanel")] public UIText UIText_listenerCount { get; private set; }
        [field: SerializeField, BoxGroup("Chanel")] public UIDropDown UIDropDown_channels { get; private set; }
        
        [field: Space]
        [field: SerializeField, BoxGroup("Tracks")] public UIText UIText_currentTrack { get; private set; }
        
        [field: Space]
        [field: SerializeField, BoxGroup("Optional")] public UIButton UIButton_previousTracks { get; private set; }
        [field: SerializeField, BoxGroup("Optional")] public RectTransform Rect_Background { get; private set; }
        [field: SerializeField, BoxGroup("Optional")] public UIText UIText_previousTracks { get; private set; }
    }
}