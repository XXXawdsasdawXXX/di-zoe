using System;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Effects.Data
{
    public class ParticleSystemFacade : MonoBehaviour, IInitializeListener
    {
        [field: SerializeField] public EParticleType Type { get; private set; }
     
        [SerializeField] private ParticleSystem _particleSystem;

        [Header("Optional modules")] 
        [SerializeField] protected AudioParticleModule _audio;

        [Header("Modules")]
        protected ParticleSystem.EmissionModule _emission;
        protected ParticleSystem.MainModule _main;
        protected ParticleSystem.TrailModule _trails;
        protected ParticleSystem.NoiseModule _noise;
        protected ParticleSystem.VelocityOverLifetimeModule _velocityOverLifetime;
        protected ParticleSystem.ColorOverLifetimeModule _colorOverLifetime;

        [Header("Services")]
        protected GradientsConfiguration _gradientsConfiguration;

        protected readonly FacadeSettings _defaultSettings = new FacadeSettings();

        [Serializable]
        protected class FacadeSettings
        {
            public float TrailLiveTime;
            public float LiveTime;
        }

        public UniTask GameInitialize()
        {
            _emission = _particleSystem.emission;
            _main = _particleSystem.main;
            _trails = _particleSystem.trails;
            _noise = _particleSystem.noise;
            _velocityOverLifetime = _particleSystem.velocityOverLifetime;
            _colorOverLifetime = _particleSystem.colorOverLifetime;
            _defaultSettings.TrailLiveTime = _trails.lifetimeMultiplier;
            _defaultSettings.LiveTime = _main.startLifetimeMultiplier;

            _gradientsConfiguration = Container.Instance.GetConfiguration<GradientsConfiguration>();
            
            return UniTask.CompletedTask;
        }

        public virtual void On()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (_audio != null)
            {
                _audio.On();
            }

            _trails.lifetimeMultiplier = _defaultSettings.TrailLiveTime;
            _main.startLifetimeMultiplier = _defaultSettings.LiveTime;

            _emission.enabled = true;
        }

        public virtual void Off()
        {
            if (_audio != null)
            {
                _audio.Off();
            }

            _trails.lifetimeMultiplier = 0;
            _main.startLifetimeMultiplier = 0;

            _emission.enabled = false;
        }


        public void SetTrailWidthOverTrail(float value)
        {
            _trails.widthOverTrailMultiplier = value;
        }

        public void SetMainStartSizeMultiplier(float value)
        {
            _main.startSizeMultiplier = value;
        }

        public void SetVelocitySpeed(float value)
        {
            _velocityOverLifetime.speedModifierMultiplier = value;
        }

        public void SetNoiseSize(float value)
        {
            _noise.frequency = value;
        }

        public void SetTrailsLifetimeMultiplier(float value)
        {
            _trails.lifetimeMultiplier = value;
        }

        public void SetMainLifetime(float getValue)
        {
            _main.startLifetimeMultiplier = getValue;
        }

        public void SetTrailsGradientValue(float getValue, GradientType gradientType)
        {
            if (_gradientsConfiguration.TryGetGradient(gradientType, out Gradient gradientData))
            {
                GradientColorKey[] colors = new GradientColorKey[gradientData.colorKeys.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new GradientColorKey(gradientData.colorKeys[i].color,
                        gradientData.colorKeys[i].time * Mathf.Clamp(getValue, 0, 1));
                }

                GradientAlphaKey[] alphas = new GradientAlphaKey[gradientData.alphaKeys.Length];
                for (int i = 0; i < alphas.Length; i++)
                {
                    alphas[i] = new GradientAlphaKey(gradientData.alphaKeys[i].alpha, i / alphas.Length);
                }

                Gradient newGradient = new Gradient();
                
                newGradient.SetKeys(colors, alphas);
                
                ParticleSystem.MinMaxGradient gradient = new ParticleSystem.MinMaxGradient()
                {
                    gradient = newGradient,
                    mode = ParticleSystemGradientMode.Gradient
                };
                
                _trails.colorOverLifetime = gradient;
            }
        }

        public void SetLifetimeColor(float getValue, GradientType gradientType)
        {
            if (_gradientsConfiguration.TryGetGradient(gradientType, out Gradient gradientData))
            {
                GradientColorKey[] colors = new GradientColorKey[gradientData.colorKeys.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new GradientColorKey(gradientData.colorKeys[i].color,
                        gradientData.colorKeys[i].time * Mathf.Clamp(getValue, 0, 1));
                }

                GradientAlphaKey[] alphas = gradientData.alphaKeys;
             
                Gradient newGradient = new Gradient();
                
                newGradient.SetKeys(colors, alphas);
                
                ParticleSystem.MinMaxGradient minMaxGradient = new ParticleSystem.MinMaxGradient()
                {
                    gradient = newGradient,
                    mode = ParticleSystemGradientMode.Gradient
                };
           
                _colorOverLifetime.color = minMaxGradient;
            }
        }

        public float GetValue(EParticleParamType paramType)
        {
            switch (paramType)
            {
                case EParticleParamType.None:
                case EParticleParamType.TrailGradient:
                case EParticleParamType.ColorLiveTime:
                default:
                    return 0;
              
                case EParticleParamType.SizeMultiplier:
                    return _main.startSizeMultiplier;
                
                case EParticleParamType.TrailWidthOverTrail:
                    return _trails.widthOverTrailMultiplier;
                
                case EParticleParamType.VelocitySpeed:
                    return _velocityOverLifetime.speedModifierMultiplier;
                
                case EParticleParamType.NoiseSize:
                    return _noise.sizeAmount.constant;
                
                case EParticleParamType.TrailLiveTime:
                    return _trails.lifetimeMultiplier;
                
                case EParticleParamType.LiveTime:
                    return _main.startLifetimeMultiplier;
            }
        }

        public bool TryGetAudioModule(out AudioParticleModule audioModule)
        {
            audioModule = _audio;
            
            return audioModule != null;
        }
    }
}