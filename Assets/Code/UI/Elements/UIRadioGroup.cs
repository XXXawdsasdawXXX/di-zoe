using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core.GameLoop;
using Cysharp.Threading.Tasks;
using TriInspector;
using UnityEditor;
using UnityEngine;

namespace Code.UI
{
    public class UIRadioGroup : UIComponent, IStartListener, ISubscriber
    {
        public event Action<int> Checked;

        [SerializeField] private List<UIRadioButton> _buttons;
        [SerializeField, Min(0)] private int _maxSelectedCount = 1;
        [SerializeField] private bool _checkedOnAwake;

        [SerializeField, Min(0), ShowIf(nameof(_checkedOnAwake))]
        private int _defaultSelectedIndex;
       
        private readonly Queue<UIRadioButton> _checked = new();


        public UniTask GameStart()
        {
            if (_checkedOnAwake)
            {
                SetCheckedWithoutNotify(_defaultSelectedIndex);
            }
         
            return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                UIRadioButton button = _buttons[i];
                
                button.SetValueWithoutNotify(false);
                int index = i;
              
                button.SubscribeToClicked(() =>
                {
                    if (!button.IsChecked.PropertyValue)
                    {
                        _setChecked(index);
                    }
                });
            }
        }

        public void Unsubscribe()
        {
            foreach (UIRadioButton button in _buttons)
            {
                button.ClearSubscriptions();
            }
        }
        
        public void AddElement(UIRadioButton button)
        {
            if (_buttons.Contains(button))
            {
                return;
            }

            _buttons.Add(button);

            int index = _buttons.Count - 1;
            button.SubscribeToClicked(() =>
            {
                _setChecked(index);
            });
        }

        public bool SetCheckedWithoutNotify(int index)
        {
            if (index >= _buttons.Count)
            {
                return false;
            }

            if (index < 0)
            {
                foreach (UIRadioButton uiRadioButton in _checked)
                {
                    uiRadioButton.SetValueWithoutNotify(false);
                }

                return false;
            }
            
            if (_checked.Count >= _maxSelectedCount && _checked.Count > 0)
            {
                UIRadioButton button = _checked.Dequeue();
                button.UnCheck();
            }

            _buttons[index].Check();
            _checked.Enqueue(_buttons[index]);

            return true;
        }

        private void _setChecked(int index)
        {
            if (index < 0)
            {
                foreach (UIRadioButton uiRadioButton in _checked)
                {
                    uiRadioButton.SetValueWithoutNotify(false);
                }

                return;
            }
            
            if (SetCheckedWithoutNotify(index))
            {
                Checked?.Invoke(index);
            }
        }

        #region Editor

#if UNITY_EDITOR

        [Button]
        private void _findButtonsInChildren()
        {
            _buttons = GetComponentsInChildren<UIRadioButton>().ToList();

            _updateIndex();
        }

        [Button]
        private void _updateIndex()
        {
            if (_buttons == null || _buttons.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i] == null)
                {
                    break;
                }

                _buttons[i].SetIndex(i);
                EditorUtility.SetDirty(_buttons[i]);
            }

            EditorUtility.SetDirty(this);
        }

        protected override void OnValidate()
        {
            _updateIndex();
            
            base.OnValidate();
        }

#endif
        #endregion

    }
}