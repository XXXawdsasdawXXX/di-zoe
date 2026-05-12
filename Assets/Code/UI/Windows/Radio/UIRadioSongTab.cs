using Code.Game.Radio;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioSongTab : UIRadioButton
    {
        public struct Model
        {
            public string Artist;
            public string Title;
            public bool IsFavorite;
        }

        [field: SerializeField] public UIRadioButton UIRadioButton_Favorite { get; private set; }
        
        [SerializeField] private UIText _uiText_Artist;
        [SerializeField] private UIText _uiText_TrackName;
        
        
        public void SetModel(Model model)
        {
            _uiText_Artist.SetText(model.Artist);
            _uiText_TrackName.SetText(model.Title);
            UIRadioButton_Favorite.SetValueWithoutNotify(model.IsFavorite);
        }
        
        public void SetModel(RadioSongModel model, bool isFavorite)
        {
            _uiText_Artist.SetText(model.artist);
            _uiText_TrackName.SetText(model.title);
            UIRadioButton_Favorite.SetValueWithoutNotify(isFavorite);
        }
    }
}