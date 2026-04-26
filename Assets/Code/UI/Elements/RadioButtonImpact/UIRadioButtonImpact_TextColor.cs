using System;
using Code.UI.Models;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.UI.RadioButtonImpact
{
    [Serializable]
    public class UIRadioButtonImpact_TextColor : UIRadioButtonImpact
    {
        [Serializable]
        private struct ImpactModel
        {
            public TMP_Text Render;
         
            public Color CheckedColor;
           
            public Color UncheckedColor;
        }
        
        [SerializeField] private ImpactModel[] _models;

        private Sequence _sequence;

        
        public override void Check()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            foreach (ImpactModel impactModel in _models)
            {
                _sequence.Join(impactModel.Render.DOColor(impactModel.CheckedColor,
                    UIConfiguration.ANIMATION_DURATION_SHORT));
            }

            _sequence.Play();
        }

        public override void Uncheck()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            
            foreach (ImpactModel impactModel in _models)
            {
                _sequence.Join(impactModel.Render.DOColor(impactModel.UncheckedColor,
                    UIConfiguration.ANIMATION_DURATION_SHORT));
            }

            _sequence.Play();
        }
    }
}