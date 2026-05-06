using Code.Core.Pools;
using TriInspector;
using UnityEngine;

namespace Code.UI
{
    public class UIComponent : MonoBehaviour, IPoolEntity
    {
        [field: SerializeField, ReadOnly] public int Index { get; set; }
        [field: SerializeField] public RectTransform Rect { get; private set; }

        
        public virtual bool IsEnabled()
        {
            return Rect.gameObject.activeSelf;
        }

        public virtual void Enable()
        {
            Rect.gameObject.SetActive(true);
        }

        public virtual void Disable()
        {
            Rect.gameObject.SetActive(false);
        }
        
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Rect == null)
            {
                Rect = GetComponent<RectTransform>();
            }
        }
#endif
    }
}