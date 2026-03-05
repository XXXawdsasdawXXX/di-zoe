using System;
using Code.Core.ServiceLocator;
using Code.UI.Models;

namespace Code.UI
{
    [Serializable]
    public abstract class UIButtonImpact
    {
        protected UIConfiguration uiConfiguration;
        
        public virtual void Initialize()
        {
            uiConfiguration = Container.Instance.GetConfig<UIConfiguration>();
        }
        
        public abstract void OnEnter();
        
        public abstract void OnExit();

        public abstract void OnDown();

        public abstract void OnUp();
    }
}