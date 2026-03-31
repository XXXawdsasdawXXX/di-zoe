using System;
using System.Collections.Generic;
using System.Threading;
using Code.Core.GameLoop;
using Code.Core.Pools;
using Code.UI.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TriInspector;
using UnityEngine;

namespace Code.UI
{
    public class UIDropDown : UIComponent, ISubscriber, IInitializeListener, IStartListener
    {
        private const float SHOWN_SIZE_SCALER = 5;
        
        private event Action<int> _changed;

        [SerializeField] private UIRadioButton _uiRadioButton_main;
        [SerializeField] private MonoPool<UIRadioButton> _pool;
        [SerializeField] private RectTransform _listView;
        [SerializeField] private bool _isInteractable = true;

        private int _current;
        private Camera _camera;
        private CancellationTokenSource _cts;
        private Tween _tween;
        private float _defaultSizeY;


        #region Life
        
        public UniTask GameInitialize()
        {
            _pool.DisableAll();
            
            _listView.gameObject.SetActive(false);
            
            _camera = Camera.main;
            
            return UniTask.CompletedTask;
        }
        
        public UniTask GameStart()
        {
            _defaultSizeY = _uiRadioButton_main.Rect.sizeDelta.y;
           return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            _uiRadioButton_main.SubscribeToClicked(_setListViewState);

            if (!_isInteractable)
            {
                return;
            }
            
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

            if (!_isInteractable)
            {
                return;
            }

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

            if (!_isInteractable)
            {
                return element;
            }
            
            element.SubscribeToClicked(() => _setSelected(index));
            
            return element;
        }

        public void ClearElements()
        {
            foreach (UIRadioButton t in _pool.GetAllEnabled())
            {
                t.ClearSubscriptions();
            }
            
            _pool.DisableAll();
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
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _hideListView();
                _cts.Cancel();
                return; // ← ключевой фикс
            }

            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            _showListView();

            try
            {
                await UniTask.WaitUntil(
                    () => (Input.GetMouseButtonDown(0) && !_isPointerOverList())
                          || !_listView.gameObject.activeSelf,
                    cancellationToken: token  // ← теперь WaitUntil реально отменяется
                );
            }
            catch (OperationCanceledException)
            {
                return; // отмена — выходим без лишних вызовов
            }

            _hideListView();
            _cts?.Cancel();
        }

        private bool _isPointerOverList()
        {
            return RectTransformUtility.RectangleContainsScreenPoint(
                Rect,
                Input.mousePosition,
               _camera
            );
        }
        
        [Button]
        private void _showListView()
        {
            _tween?.Kill();
            
            _listView.gameObject.SetActive(true);
            
            Vector2 size = new(Rect.sizeDelta.x, _defaultSizeY * SHOWN_SIZE_SCALER);
            _tween = Rect.DOSizeDelta(size, UIConfiguration.ANIMATION_DURATION_SHORT)
                .SetEase(UIConfiguration.TWEEN_EASY);
        }
        
        [Button]
        private void _hideListView()
        {
            _tween?.Kill();
            
            Vector2 size = new(Rect.sizeDelta.x, _defaultSizeY);
            _tween = Rect.DOSizeDelta(size, UIConfiguration.ANIMATION_DURATION_SHORT)
                .SetEase(UIConfiguration.TWEEN_EASY)
                .OnComplete(() =>
                {
                    _listView.gameObject.SetActive(false);
                })
                .OnKill(() =>
                {
                    _listView.gameObject.SetActive(false);
                });
        }
    }
}