using System;
using System.Collections.Generic;
using Code.Core.GameLoop;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIDropDown : UIComponent, ISubscriber
    {
        private event Action<int> _changed;

        [SerializeField] private TMP_Dropdown _dropdown;

        
        #region Life
        
        public void Subscribe()
        {
            _dropdown.onValueChanged.AddListener(_invokeChanged);
        }

        public void Unsubscribe()
        {
            _dropdown.onValueChanged.RemoveListener(_invokeChanged);
        }

        #endregion

        public void SubscribeToElement(Action<int> change)
        {
            _changed += change;
        }
        
        public void UnsubscribeFromElement(Action<int> change)
        {
            _changed -= change;
        }

        public void SetCurrentValueWithoutNotify(int value)
        {
            _dropdown.SetValueWithoutNotify(value);
        }

        public void SetValues(List<TMP_Dropdown.OptionData> options)
        {
            _dropdown.options = options;
        }

        private void _invokeChanged(int index)
        {
            _changed?.Invoke(index);
        }
    }
}