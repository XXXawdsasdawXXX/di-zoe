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
    public class UIImpactComponent_DeltaSize : UIImpactComponent, IStartListener
    {
        [SerializeField] private Vector2 _shownDeltaSize;
        [SerializeField] private Vector2 _hiddenDeltaSize;

        [SerializeField] private bool _selfOff;
        [SerializeField] private float _offCooldown = 1;
        [SerializeField] private bool _disableOnStart;
        [SerializeField] private GameObject _disableObject;
        
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
            
            _tween = Rect.DOSizeDelta(_shownDeltaSize, UIConfiguration.ANIMATION_DURATION_SHORT)
                .SetEase(UIConfiguration.TWEEN_EASY)
                .SetLink(Rect.gameObject, LinkBehaviour.CompleteOnDisable)
                .OnComplete(() =>
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(Rect.parent.GetComponent<RectTransform>());
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

            _isShowing = false;

            _tween?.Kill();

            _tween = Rect.DOSizeDelta(_hiddenDeltaSize, UIConfiguration.ANIMATION_DURATION_SHORT)
                .SetEase(UIConfiguration.TWEEN_EASY)
                .SetLink(Rect.gameObject, LinkBehaviour.CompleteOnDisable)
                .OnComplete(() =>
                {
                    _isShowing = false;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(Rect.parent.GetComponent<RectTransform>());
                    _disableObject.SetActive(false);
                });

            await _tween.AsyncWaitForCompletion();
            
            _tween?.Kill();

            await base.InvokeDisableImpact();
        }
    }
}