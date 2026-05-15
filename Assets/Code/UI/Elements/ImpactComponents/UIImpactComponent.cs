using System;
using Cysharp.Threading.Tasks;

namespace Code.UI.ImpactComponents
{
    public abstract class UIImpactComponent : UIComponent
    {
        public bool IsActivated { get; private set; }
        
        private event Action<bool> _changed;

        public virtual UniTask InvokeActiveImpact()
        {
            IsActivated = true;
            
            _changed?.Invoke(true);
            
            return UniTask.CompletedTask;
        }

        public virtual UniTask InvokeDisableImpact()
        {
            IsActivated = false;
            
            _changed?.Invoke(false);
            
            return UniTask.CompletedTask;
        }

        public void SubscribeToChanged(Action<bool> changed)
        {
            if (changed != null)
            {
                _changed += changed;
            }
        }

        public void UnsubscribeFromChanged(Action<bool> changed)
        {
            if (changed != null)
            {
                _changed -= changed;
            }
        }
    }
}