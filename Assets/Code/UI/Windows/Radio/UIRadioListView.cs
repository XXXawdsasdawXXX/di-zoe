using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioListView : UIView
    {
        [field: Space, Header("Channels")]
        [field: SerializeField] public UIRadioChannelDropDown DropDownChannels { get; private set; }
        [field: SerializeField] public UIImpactComponent ImpactAllChannels { get; private set; }
        [field: SerializeField] public UIImpactComponent ImpactFavChannels { get; private set; }
        [field: SerializeField] public UIRadioButton ButtonAllChannels { get; private set; }
        [field: SerializeField] public UIRadioButton ButtonFavChannels { get; private set; }
        
        
        [field: Space, Header("Tracks")]
        [field: SerializeField] public UIDropDown DropDownTracks { get; private set; }
        [field: SerializeField] public UIImpactComponent ImpactTracks { get; private set; }
        [field: SerializeField] public UIRadioButton ButtonAllTracks { get; private set; }
        [field: SerializeField] public UIRadioButton ButtonFavTracks { get; private set; }
    }
}