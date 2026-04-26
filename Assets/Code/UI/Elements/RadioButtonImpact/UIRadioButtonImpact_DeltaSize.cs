using System;
using Code.UI.Models;
using DG.Tweening;
using TriInspector;
using UnityEngine;

namespace Code.UI.RadioButtonImpact
{
    [Serializable]
    public class UIRadioButtonImpact_DeltaSize : UIRadioButtonImpact
    {
        [Serializable]
        public struct ImpactModel
        {
            public RectTransform Rect;
            public Vector2 CheckSize;
            public Vector2 UncheckSize;
            public bool IsDefaultAnimationDuration;

            [ShowIf(nameof(IsDefaultAnimationDuration))]
            public float AnimationDuration;
        }

        [SerializeField] private ImpactModel[] _impactModels;

        private Sequence _sequence;

        
        public override void Check()
        {
            _playAnimation(true);
        }

        public override void Uncheck()
        {
            _playAnimation(false);
        }

        private void _playAnimation(bool isChecked)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            foreach (ImpactModel impactModel in _impactModels)
            {
                if (impactModel.Rect == null)
                {
                    continue;
                }

                float duration = impactModel.IsDefaultAnimationDuration
                    ? UIConfiguration.ANIMATION_DURATION_SHORT
                    : impactModel.AnimationDuration;

                Vector2 size = isChecked
                    ? impactModel.CheckSize
                    : impactModel.UncheckSize;

                _sequence.Append(impactModel.Rect.DOSizeDelta(size, duration));
            }

            _sequence.Play();
        }
    }
}