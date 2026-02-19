using System;
using System.Collections.Generic;
using Code.Infrastructure.GameLoop;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIDropDown : UIComponent, ISubscriber
    {
        public event Action<int> OnChanged;

        [SerializeField] private TMP_Dropdown _dropdown;

        
        #region Life
        
        public void Subscribe()
        {
            _dropdown.onValueChanged.AddListener(value => OnChanged?.Invoke(value));
        }

        public void Unsubscribe()
        {
            _dropdown.onValueChanged.RemoveAllListeners();
        }

        #endregion

        
        public void SetCurrentValueWithoutNotify(int value)
        {
            _dropdown.SetValueWithoutNotify(value);
        }

        public void SetValues(List<TMP_Dropdown.OptionData> options)
        {
            _dropdown.options = options;
        }
    }
}