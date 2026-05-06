using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioDropDownView : UIView
    {
        [field: Space, Header("Channels")]
        [field: SerializeField] public UIDropDown DropDownChannels { get; private set; }

        [field: SerializeField] public UIImpactComponent ImpactChannels { get; private set; }
        [field: SerializeField] public UIRadioButton ButtonAllChannels { get; private set; }
        [field: SerializeField] public UIRadioButton ButtonFavChannels { get; private set; }
        
        
        [field: Space, Header("Tracks")]
        [field: SerializeField] public UIDropDown DropDownTracks { get; private set; }
        [field: SerializeField] public UIImpactComponent ImpactTracks { get; private set; }
        [field: SerializeField] public UIRadioButton ButtonAllTracks { get; private set; }
        [field: SerializeField] public UIRadioButton ButtonFavTracks { get; private set; }
    }
}