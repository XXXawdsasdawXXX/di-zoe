using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioTrackTab : UIRadioButton
    {
        public struct Model
        {
            public string Artist;
            public string Title;
            public bool IsFavorite;
        }

        [field: SerializeField] public UIRadioButton UIRadioButton_Fav { get; private set; }
        
        [SerializeField] private UIText _uiText_Artist;
        [SerializeField] private UIText _uiText_TrackName;
        
        
        public void SetModel(Model model)
        {
            _uiText_Artist.SetText(model.Artist);
            _uiText_TrackName.SetText(model.Title);
            UIRadioButton_Fav.SetValueWithoutNotify(model.IsFavorite);
        }
    }
}