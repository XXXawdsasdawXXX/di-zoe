using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIText : UIComponent
    {
        [SerializeField] private TMP_Text _component;

        public void SetText(string text)
        {
            _component.text = text;
        }
    }
}