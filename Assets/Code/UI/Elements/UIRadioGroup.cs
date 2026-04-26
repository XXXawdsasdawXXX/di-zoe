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
        public event Action<int> Unchecked;

        [SerializeField] private List<UIRadioButton> _buttons;
        [SerializeField, Min(0)] private int _maxSelectedCount = 1;
        [SerializeField] private bool _checkedOnAwake;

        [SerializeField, Min(0), ShowIf(nameof(_checkedOnAwake))]
        private int _defaultSelectedIndex;

        private readonly Queue<UIRadioButton> _checked = new();
        

        public UniTask GameStart()
        {
            Debug.Log("start");
            if (_checkedOnAwake)
            {
                SetCheckedWithoutNotify(_defaultSelectedIndex);
            }
         
            return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            Debug.Log($"{name} Subscribe");
            for (int i = 0; i < _buttons.Count; i++)
            {
                UIRadioButton button = _buttons[i];
                
                button.SetValueWithoutNotify(false);
                int index = i;
              
                button.SubscribeToClicked(() =>
                {
                    Debug.Log($"click {button.IsChecked.PropertyValue}");
                    if (!button.IsChecked.PropertyValue)
                    {
                        SetChecked(index);
                    }
                });
            }
        }

        public void Unsubscribe()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                UIRadioButton button = _buttons[i];
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
                SetChecked(index);
            });
        }

        public void SetChecked(int index)
        {
            if (SetCheckedWithoutNotify(index))
            {
                Checked?.Invoke(index);
            }
        }

        public bool SetCheckedWithoutNotify(int index)
        {
            if (index >= _buttons.Count)
            {
                return false;
            }

            if (_checked.Count >= _maxSelectedCount && _checked.Count > 0)
            {
                UIRadioButton button = _checked.Dequeue();
                button.UnCheck();
                Unchecked?.Invoke(button.Index);
            }

            _buttons[index].Check();
            _checked.Enqueue(_buttons[index]);

            return true;
        }

        #region Editor

#if UNITY_EDITOR

        [Button]
        private void FindButtonsInChildren()
        {
            _buttons = GetComponentsInChildren<UIRadioButton>().ToList();

            UpdateIndex();
        }

        [Button]
        private void UpdateIndex()
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
            UpdateIndex();
            
            base.OnValidate();
        }

#endif

        #endregion


  
    }
}