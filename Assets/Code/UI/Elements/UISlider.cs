using System;

namespace Code.UI
{
    public abstract class UISlider : UIComponent
    {
        private event Action<float> _changed;
        
        
        public void SubscribeToElement(Action<float> action)
        {
            _changed += action;
        }
        
        public void UnsubscribeFromElement(Action<float> action)
        {
            _changed -= action;
        }
        
        public abstract float GetValue();
        
        public abstract void SetValueWithoutNotify(float value);

        public void SetValue(float value)
        {
            SetValueWithoutNotify(value);
            
            _changed?.Invoke(value);
        }
    }
}