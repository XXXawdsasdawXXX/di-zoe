using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Tools
{
    [Serializable]
    public class ReactiveProperty<T>
    {
        private event Action<T> Changed;

        public T PropertyValue
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

        [SerializeField] private T _propertyValue;

        public ReactiveProperty(T propertyValue)
        {
            _propertyValue = propertyValue;
        }

        public void SetValueWithoutNotify(T value)
        {
            _propertyValue = value;
        }

        public void UnsubscibeFromValue(Action<T> action)
        {
            Changed -= action;
        }

        public void SubscribeToValue(Action<T> action)
        {
            Changed += action;
        }

        public void ClearSubscription()
        {
            Changed = null;
        }
    }
}