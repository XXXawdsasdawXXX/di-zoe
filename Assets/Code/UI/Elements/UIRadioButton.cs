using Code.Tools;
using Code.UI.RadioButtonImpact;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI
{
    public class UIRadioButton : UIButton
    {
        [field: SerializeField] public ReactiveProperty<bool> IsChecked { get; private set; }
        [field: SerializeField] public int Index { get; private set; }

        [SerializeField] private bool _initializeOnStart;
        [SerializeField] private bool _isAutonomous;
        
        [SerializeReference] private UIRadioButtonImpact[] _radioButtonImpacts;

        
        public override async UniTask GameInitialize()
        {
            await base.GameInitialize();

            if (_initializeOnStart)
            {
                if (IsChecked.PropertyValue)
                {
                    UnCheck();
                }
                else
                {
                    Check();
                }
            }
        }

        public void SetValueWithoutNotify(bool value)
        {
            IsChecked.SetValueWithoutNotify(value);

            if (IsChecked.PropertyValue)
            {
                foreach (UIRadioButtonImpact buttonImpact in _radioButtonImpacts)
                {
                    buttonImpact?.Check();
                }
            }
            else
            {
                foreach (UIRadioButtonImpact buttonImpact in _radioButtonImpacts)
                {
                    buttonImpact?.Uncheck();
                }
            }
        }

        public void SetIndex(int i)
        {
            Index = i;
        }

        public void Check()
        {
            if (IsChecked.PropertyValue)
            {
                return;
            }
            
            IsChecked.PropertyValue = true;

            foreach (UIRadioButtonImpact buttonImpact in _radioButtonImpacts)
            {
                buttonImpact.Check();
            }
        }

        public void UnCheck()
        {
            if (!IsChecked.PropertyValue)
            {
                return;
            }
            
            IsChecked.PropertyValue = false;
            
            foreach (UIRadioButtonImpact buttonImpact in _radioButtonImpacts)
            {
                buttonImpact.Uncheck();
            }
        }

        public void UpdateImpactState()
        {
            if (IsChecked.PropertyValue)
            {
                foreach (UIRadioButtonImpact buttonImpact in _radioButtonImpacts)
                {
                    buttonImpact.Check();
                }
            }
            else
            {
                foreach (UIRadioButtonImpact buttonImpact in _radioButtonImpacts)
                {
                    buttonImpact.Uncheck();
                }
            }
        }

        public override void ClearSubscriptions()
        {
            base.ClearSubscriptions();

            IsChecked.ClearSubscription();
        }

        protected override void onClick()
        {
            base.onClick();

            if (!_isAutonomous)
            {
                UpdateImpactState();
                
                return;
            }

            if (IsChecked.PropertyValue)
            {
                UnCheck();
            }
            else
            {
                Check();
            }
        }
    }
}