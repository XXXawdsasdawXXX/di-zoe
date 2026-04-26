using System;
using Code.UI.Models;
using DG.Tweening;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Code.UI
{
    
    [Serializable]
    public class UIButtonImpact_ImageColor : UIButtonImpact
    {
        [Serializable]
        public class ImpactModel
        {
            public Image Render;
            
            public bool IsCustomColors;
            
            [ShowIf(nameof(IsCustomColors))] public UIPointerModel PointerModel;
        }
        
        [SerializeField] private ImpactModel[] _models;

        private Sequence _sequence;


        public override void OnEnter()
        {
            if (_models == null)
            {
                return;
            }
            
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            foreach (ImpactModel model in _models)
            {
                if (model.Equals(default))
                {
                    Debug.Log("pizdec");
                    continue;
                }

                if (uiConfiguration == null)
                {
                    Debug.Log("pizdec #2");
                    break;
                }
                
                Color color = model.IsCustomColors
                    ? model.PointerModel.Enter
                    : uiConfiguration.DefaultButtonsImpactColor.Enter; 
                
                _sequence.Join(model.Render.DOColor(color, UIConfiguration.ANIMATION_DURATION_SHORT));
            }

            _sequence.Play();
        }

        public override void OnExit()
        {
            if (_models == null)
            {
                return;
            }
            
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            foreach (ImpactModel model in _models)
            {
                Color color = model.IsCustomColors
                    ? model.PointerModel.Default
                    : uiConfiguration.DefaultButtonsImpactColor.Default; 
                
                _sequence.Join(model.Render.DOColor(color, UIConfiguration.ANIMATION_DURATION_SHORT));
            }

            _sequence.Play();
        }

        public override void OnDown()
        {
            _sequence?.Kill();            
            _sequence = DOTween.Sequence();

            foreach (ImpactModel model in _models)
            {
                Color color = model.IsCustomColors
                    ? model.PointerModel.Down
                    : uiConfiguration.DefaultButtonsImpactColor.Down; 
                
                _sequence.Join(model.Render.DOColor(color, UIConfiguration.ANIMATION_DURATION_SHORT));
            }

            _sequence.Play();
        }

        public override void OnUp()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
 
            foreach (ImpactModel model in _models)
            {
                Color color = model.IsCustomColors
                    ? model.PointerModel.Up
                    : uiConfiguration.DefaultButtonsImpactColor.Up; 
                
                _sequence.Join(model.Render.DOColor(color, UIConfiguration.ANIMATION_DURATION_SHORT));
            }

            _sequence.Play();
        }
    }
}