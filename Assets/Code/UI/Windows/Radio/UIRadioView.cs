using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UIRadioView : UIView
    {
        [field: SerializeField] public UIText UIText_currentTrack { get; private set; }
        [field: SerializeField] public UIText UIText_previousTracks { get; private set; }
        [field: SerializeField] public UIText UIText_listenerCount { get; private set; }
     
        [field: SerializeField] public UISlider UISlider_volume { get; private set; }
        
        [field: SerializeField] public UIDropDown UIDropDown_channels { get; private set; }
        
        
        
        public RawImage albumCoverImage;        // Обложка текущего трека
        public RawImage channelLogoImage;       // Логотип канала

        
    }
}