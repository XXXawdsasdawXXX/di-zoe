using System;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Code.Tools
{
    [Serializable]
    public class ReactiveProperty<T>
    {
        private event Action<T> Changed;

        [ShowNativeProperty] public T PropertyValue
        {
            get => _propertyValue;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_propertyValue, value))
                {
                    _propertyValue = value;
                    Changed?.Invoke(_propertyValue);
                }
            }
        }

        private T _propertyValue;

        public ReactiveProperty(T propertyValue)
        {
            _propertyValue = propertyValue;
        }

        public void SubscribeToValue(Action<T> action)
        {
            Changed += action;
        }

        public void UnsubscibeFromValue(Action<T> action)
        {
            Changed -= action;
        }

        public void SetValueWithoutNotify(T value)
        {
            _propertyValue = value;
        }
    }
}