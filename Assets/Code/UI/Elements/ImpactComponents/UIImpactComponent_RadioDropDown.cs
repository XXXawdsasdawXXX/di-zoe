using System;
using Code.Core.GameLoop;
using Code.UI.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Kirurobo;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UIImpactComponent_RadioDropDown : UIImpactComponent, IStartListener
    {
        [SerializeField] private RectTransform _child;
        [SerializeField] private UIDropDown _dropDown;
        [SerializeField] private RectTransform _root;
        [SerializeField] private GameObject _disableObject;
        [SerializeField] private UIRadioGroup _radioGroupButtons;
        [Space]
        [SerializeField] private Vector2 _shownDeltaSize;
        [SerializeField] private Vector2 _hiddenDeltaSize;
        [SerializeField] private bool _selfOff;
        [SerializeField] private float _offCooldown = 1;
        [SerializeField] private bool _disableOnStart;
        
        [SerializeField, ReadOnly] private bool _isShowing;
        private Tween _tween;
        
        
        public UniTask GameStart()
        {
            if (_disableOnStart)
            {
                Rect.sizeDelta = _hiddenDeltaSize;
            }
            
            return UniTask.CompletedTask;
        }

        public override async UniTask InvokeActiveImpact()
        {
            if (_isShowing)
            {
                return;
            }
            
            _isShowing = true;

            _tween?.Kill();

            Rect.sizeDelta = _hiddenDeltaSize;
            
            if (_disableObject != null)
            {
                _disableObject.SetActive(true);
            }
            
            Vector2 size = _child.sizeDelta; 
            _child.anchorMin = Vector2.zero;
            _child.anchorMax = Vector2.one; 
            _child.offsetMin = Vector2.zero;
            _child.offsetMax = Vector2.zero;
            
            _tween = Rect.DOSizeDelta(_shownDeltaSize, UIConfiguration.ANIMATION_DURATION_SHORT)
                .SetEase(UIConfiguration.TWEEN_EASY)
                .SetLink(Rect.gameObject, LinkBehaviour.CompleteOnDisable)
                .OnComplete(() =>
                {
                    _child.anchorMin = new Vector2(0.5f, 1f);
                    _child.anchorMax = new Vector2(0.5f, 1f);
                    _child.sizeDelta = size;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_root.GetComponent<RectTransform>());
                });

            await _tween.AsyncWaitForCompletion();
            
            if (_selfOff)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_offCooldown));
                await InvokeDisableImpact();
            }
            
            _tween?.Kill();

            await base.InvokeActiveImpact();
        }

        public override async UniTask InvokeDisableImpact()
        {
            if (!_isShowing)
            {
                return;
            }

            await _tween.AsyncWaitForCompletion();
            await UniTask.Delay(TimeSpan.FromSeconds(_offCooldown));
            await _dropDown.HideListView();
            
            _isShowing = false;
            _tween?.Kill();
            
            Vector2 size = _child.sizeDelta;

            _child.anchorMin = Vector2.zero;
            _child.anchorMax = Vector2.one; 
            _child.offsetMin = Vector2.zero;
            _child.offsetMax = Vector2.zero;
            
            _tween = Rect.DOSizeDelta(_hiddenDeltaSize, UIConfiguration.ANIMATION_DURATION_SHORT)
                .SetEase(UIConfiguration.TWEEN_EASY)
                .SetLink(Rect.gameObject, LinkBehaviour.CompleteOnDisable)
                .OnComplete(() =>
                {
                    _child.anchorMin = new Vector2(0.5f, 1f);
                    _child.anchorMax = new Vector2(0.5f, 1f);
                    _child.sizeDelta = size;
                    _isShowing = false;
                    _disableObject.SetActive(false);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_root.GetComponent<RectTransform>());
                });

            _radioGroupButtons.SetCheckedWithoutNotify(-1);
            
            await _tween.AsyncWaitForCompletion();
            
            _tween?.Kill();
            
            await base.InvokeDisableImpact();
        }
    }
}