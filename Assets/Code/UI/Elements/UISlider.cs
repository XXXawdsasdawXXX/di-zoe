using System;
using Code.Core.GameLoop;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UISlider : UIComponent, ISubscriber
    {
        private event Action<float> _changed;
        
        [SerializeField] private Slider _slider;
        
        
        #region Life
        
        public void Subscribe()
        {
            _slider.onValueChanged.AddListener(_invokeChanged);            
        }

        public void Unsubscribe()
        {
            _slider.onValueChanged.RemoveListener(_invokeChanged);
        }
        
        #endregion

        public void SubscribeToElement(Action<float> action)
        {
            _changed += action;
        }
        
        public void UnsubscribeFromElement(Action<float> action)
        {
            _changed -= action;
        }
        
        public void SetValueWithoutNotify(float value)
        {
            _slider.SetValueWithoutNotify(value);
        }
        
        private void _invokeChanged(float value)
        {
            _changed?.Invoke(value);
        }

        #region Editor

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (_slider == null)
            {
                TryGetComponent(out _slider);
            }
        }

#endif
        #endregion
    }
}