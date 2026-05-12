using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioChannelTab : UIRadioButton
    {
        public struct Model
        {
            public string Name;
            public string Genre;
            public bool IsFavorite;
        }

        [field: SerializeField] public UIText UIText_Name { get; private set; }
        [field: SerializeField] public UIText UIText_Genre { get; private set; }
        [field: SerializeField] public UIRadioButton UIRadioButton_Favorite { get; private set; }
        

        public void SetModel(Model model)
        {
            UIText_Name.SetText(model.Name);
            UIText_Genre.SetText(model.Genre);
            UIRadioButton_Favorite.SetValueWithoutNotify(model.IsFavorite);
        }
    }
}