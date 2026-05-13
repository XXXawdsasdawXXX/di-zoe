using System;
using System.Collections.Generic;
using System.Threading;
using Code.Core.GameLoop;
using Code.UI.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UIDropDown : UIComponent, ISubscriber, IInitializeListener
    {
        private const float SHOWN_SIZE_SCALER = 5;
        private event Action<int> _changed;
        public bool IsShownList { get; private set; }
        [field: SerializeField] public UIRadioButton UIRadioButton_main { get; private set; }

        [SerializeField] private MonoPool<UIRadioButton> _pool;
        [SerializeField] private RectTransform _listView;
        [SerializeField] private bool _isInteractable = true;
        [SerializeField] private bool _hideListWhenChanged;
        [SerializeField] private float _defaultSizeY;

        private int _current;
        private Camera _camera;
        private CancellationTokenSource _cts;
        private Tween _tween;

        #region Life

        public virtual UniTask GameInitialize()
        {
            _pool.DisableAll();

            _listView.gameObject.SetActive(false);

            _camera = Camera.main;

            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            UIRadioButton_main.SubscribeToClicked(_setListViewState);

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
            UIRadioButton_main.UnsubscribeFromClicked(_setListViewState);

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

        public void SetCurrentValueWithoutNotify(int index)
        {
            _pool.GetByIndex(_current)?.UnCheck();

            _current = index;

            _pool.GetByIndex(_current)?.Check();
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

        [Button]
        public void ShowListView()
        {
            if (IsShownList)
            {
                return;
            }
            
            _tween?.Kill();
            
            IsShownList = true;

            _listView.gameObject.SetActive(true);

            Vector2 size = new(Rect.sizeDelta.x, _defaultSizeY * SHOWN_SIZE_SCALER);
            _tween = Rect.DOSizeDelta(size, UIConfiguration.ANIMATION_DURATION_SHORT)
                .SetEase(UIConfiguration.TWEEN_EASY);
        }

        [Button]
        public async UniTask HideListView()
        {
            if (!IsShownList)
            {
                return;
            }
            
            _tween?.Kill();

            Vector2 size = new(Rect.sizeDelta.x, _defaultSizeY);
            _tween = Rect.DOSizeDelta(size, UIConfiguration.ANIMATION_DURATION_SHORT)
                .SetEase(UIConfiguration.TWEEN_EASY)
                .OnComplete(() =>
                {
                    _listView.gameObject.SetActive(false);
                    IsShownList = false;
                });
            
            await _tween.AsyncWaitForCompletion();
        }

        private void _setSelected(in int index)
        {
            SetCurrentValueWithoutNotify(index);

            _invokeChanged(_pool.GetAllEnabled()[index].Index);

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
                await HideListView();
                _cts.Cancel();
                return;
            }

            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            ShowListView();

            try
            {
                await UniTask.WaitUntil(
                    () => (Input.GetMouseButtonDown(0) && !_isPointerOverList())
                          || !_listView.gameObject.activeSelf,
                    cancellationToken: token
                );
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Debug.Log("Hide list view");
            await HideListView();
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
    }
}