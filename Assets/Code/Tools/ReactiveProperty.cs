using System;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Code.Tools
{
    [Serializable]
    public class ReactiveProperty<T>
    {
        private event Action<T> Changed;

        [ShowNativeProperty] public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    Changed?.Invoke(_value);
                }
            }
        }

        private T _value;

        public ReactiveProperty(T value)
        {
            _value = value;
        }

        public void Subscribe(Action<T> action)
        {
            Changed += action;
        }

        public void Unsubscibe(Action<T> action)
        {
            Changed -= action;
        }
    }
}