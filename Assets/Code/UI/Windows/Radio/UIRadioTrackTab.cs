using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioTrackTab : UIRadioButton
    {
        public struct Model
        {
            public string Artist;
            public string Title;
        }

        [SerializeField] private UIText _uiText_Artist;
        [SerializeField] private UIText _uiText_TrackName;
        
        
        public void SetModel(Model model)
        {
            _uiText_Artist.SetText(model.Artist);
            _uiText_TrackName.SetText(model.Title);
        }
    }
}