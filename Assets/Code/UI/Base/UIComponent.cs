using Code.Core.Pools;
using UnityEngine;

namespace Code.UI
{
    public class UIComponent : MonoBehaviour, IPoolEntity
    {
        [field: SerializeField] public RectTransform Rect { get; private set; }

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