using System;
using Code.UI.Models;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [Serializable]
    public class UIButtonImpact
    {
        [SerializeField] private Image _render;
        
        [SerializeField] private UIPointerModel _pointerModel = new UIPointerModel
        {
            Default = Color.white,
            Enter = new Color(0.8f,0.8f,0.8f),
            Up = new Color(0.8f,0.8f,0.8f),
            Down = new Color(0.6f,0.6f,0.6f),
        };
        
        private Tween _tween;

      
        public void OnEnter()
        {
            _setTween(_render.DOColor(_pointerModel.Enter, UIConfiguration.ANIMATION_DURATION_SHORT));
        }

        public void OnExit()
        {
            _setTween(_render.DOColor(_pointerModel.Default, UIConfiguration.ANIMATION_DURATION_SHORT));
        }

        public void OnDown()
        {
            _setTween(_render.DOColor(_pointerModel.Down, UIConfiguration.ANIMATION_DURATION_SHORT));
        }

        public void OnUp()
        {
            _setTween(_render.DOColor(_pointerModel.Up, UIConfiguration.ANIMATION_DURATION_SHORT));
        }

        private void _setTween(Tween tween)
        {
            _tween?.Kill();

            _tween = tween;
        }
    }
}