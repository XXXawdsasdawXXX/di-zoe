using Code.Tools;
using Code.UI.RadioButtonImpact;
using UnityEngine;

namespace Code.UI
{
    public class UIRadioButton : UIButton
    {
        [field: SerializeField] public ReactiveProperty<bool> IsChecked { get; private set; }

        [SerializeReference] private UIRadioButtonImpact[] _radioButtonImpacts;
        
        
        public void SetValueWithoutNotify(bool value)
        {
            IsChecked.SetValueWithoutNotify(value);
        }
        
        protected override void onClick()
        {
            base.onClick();

            if (IsChecked.PropertyValue)
            {
                _unCheck();
            }
            else
            {
                _check();
            }
        }

        private void _check()
        {
            IsChecked.PropertyValue = true;

            foreach (UIRadioButtonImpact buttonImpact in _radioButtonImpacts)
            {
                buttonImpact.Check();
            }
        }

        private void _unCheck()
        {
            IsChecked.PropertyValue = false;
            
            foreach (UIRadioButtonImpact buttonImpact in _radioButtonImpacts)
            {
                buttonImpact.Uncheck();
            }
        }
    }
}