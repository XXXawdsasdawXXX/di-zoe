using System.Collections;
using UnityEngine;

namespace Code.Game.Effects
{
    public class NimbusParticleFacade : ParticleSystemFacade
    {
        private Coroutine _disableRoutine;

        public override void On()
        {
            _audio.On();

            _trails.lifetimeMultiplier = _defaultSettings.TrailLiveTime;
            _main.startLifetimeMultiplier = _defaultSettings.LiveTime;

            _emission.enabled = true;
        }

        public override void Off()
        {
            _audio?.Off();

            /*_trails.lifetimeMultiplier = 0;
            _main.startLifetimeMultiplier =0;*/

            if (_disableRoutine != null)
            {
                StopCoroutine(_disableRoutine);
            }

            _disableRoutine = StartCoroutine(DisableRoutine());
        }

        private IEnumerator DisableRoutine()
        {
            yield return new WaitUntil(_audio.IsSleep);
            _emission.enabled = false;
        }
    }
}