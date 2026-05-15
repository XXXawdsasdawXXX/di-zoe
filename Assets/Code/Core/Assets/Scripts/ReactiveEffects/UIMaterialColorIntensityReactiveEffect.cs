using Code.Core.Assets.Scripts.ReactiveEffects.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Core.Assets.Scripts.ReactiveEffects
{
    public class UIMaterialColorIntensityReactiveEffect : VisualizationEffectBase
    {
        #region Private Member Variables

        private Image _renderer;
        private Color _initialColor;
        private Color _initialEmissionColor;

        #endregion

        #region Public Properties

        public float MinIntensity;
        public float IntensityScale;
        public float MinEmissionIntensity;
        public float EmissionIntensityScale;

        #endregion

        #region Startup / Shutdown

        public override void Start()
        {
            base.Start();

            _renderer = GetComponent<Image>();
            _initialColor = _renderer.color;
        }

        #endregion

        #region Render

        public void Update()
        {
            float audioData = GetAudioData();
            float scaledAmount = Mathf.Clamp(MinIntensity + (audioData * IntensityScale), 0.0f, 1.0f);
            Color scaledColor = _initialColor * scaledAmount;

            _renderer.color =  scaledColor;
        }

        #endregion

        #region Public Methods

        public void UpdateColor(Color color)
        {
            _initialColor = color;
            _initialEmissionColor = color;
        }

        #endregion
    }
}