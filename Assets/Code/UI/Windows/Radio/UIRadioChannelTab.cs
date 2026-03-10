using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioChannelTab : UIComponent
    {
        [field: SerializeField] public UIText UIText_Name { get; private set; }
        [field: SerializeField] public UIText UIText_Genre { get; private set; }
        [field: SerializeField] public UIRadioButton UIRadioButton_Favorite { get; private set; }
    }
}