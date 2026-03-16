using System;
using System.Collections.Generic;
using System.Threading;
using Code.Core.GameLoop;
using Code.Core.Pools;
using Cysharp.Threading.Tasks;
using TriInspector;
using UnityEditor;
using UnityEngine;

namespace Code.UI
{
    public class UIDropDown : UIComponent, ISubscriber, IInitializeListener
    {
        private event Action<int> _changed;

        [SerializeField] private UIRadioButton _uiRadioButton_main;
        [SerializeField] private MonoPool<UIRadioButton> _pool;
        [SerializeField] private RectTransform _listView;

        private int _current;
        private Camera _camera;
        private CancellationTokenSource _cts;

        
        #region Life
        
        public UniTask GameInitialize()
        {
            _pool.DisableAll();
            
            _listView.gameObject.SetActive(false);
            
            _camera = Camera.main;
            
            return UniTask.CompletedTask;
        }

        
        public void Subscribe()
        {
            _uiRadioButton_main.SubscribeToClicked(_setListViewState);

            IReadOnlyList<UIRadioButton> all = _pool.GetAll();
           
            for (int i = 0; i < all.Count; i++)
            {
                int index = i;
          
                all[i].SubscribeToClicked(() => _setSelected(index));
            }
        }

        public void Unsubscribe()
        {
            _uiRadioButton_main.UnsubscribeFromClicked(_setListViewState);

            IReadOnlyList<UIRadioButton> all = _pool.GetAll();

            foreach (UIRadioButton t in all)
            {
                t.ClearSubscriptions();
            }
        }

        #endregion

        public void SubscribeToDropDown(Action<int> change)
        {
            _changed += change;
        }

        public void UnsubscribeFromDropDown(Action<int> change)
        {
            _changed -= change;
        }

        public UIRadioButton SetCurrentValueWithoutNotify(int index)
        {
            if (index == _current)
            {
                return _uiRadioButton_main;
            }
            
            _pool.GetByIndex(_current).UnCheck();
      
            _current = index;
        
            _pool.GetByIndex(_current).Check();
            
            return _uiRadioButton_main;
        }

        public UIRadioButton AddElement()
        {
            UIRadioButton element = _pool.GetNext();

            int index = _pool.GetIndex(element);

            element.SubscribeToClicked(() => _setSelected(index));
            
            return element;
        }

        private void _setSelected(in int index)
        {
            SetCurrentValueWithoutNotify(index);
            
            _invokeChanged(index);
            
            _listView.gameObject.SetActive(false);
        }

        private void _invokeChanged(in int index)
        {
            _changed?.Invoke(index);
        }

        private async void _setListViewState()
        {
            _listView.gameObject.SetActive(true);
                
            _cts?.Cancel();
            
            _cts = new CancellationTokenSource();

            await UniTask.WaitUntil(()=> (Input.GetMouseButtonDown(0) && !_isPointerOverList()) 
                                         || !_listView.gameObject.activeSelf);
            
            _listView.gameObject.SetActive(false);

            _cts?.Cancel();
        }

        private bool _isPointerOverList()
        {
            return RectTransformUtility.RectangleContainsScreenPoint(
                _listView,
                Input.mousePosition,
               _camera
            );
        }

        #region Editor
#if UNITY_EDITOR
        
        [Button]
        private void _showListView()
        {
            _listView.gameObject.SetActive(true);
            EditorUtility.SetDirty(this);
        }
        
        [Button]
        private void _hideListView()
        {
            _listView.gameObject.SetActive(false);
            EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }
}