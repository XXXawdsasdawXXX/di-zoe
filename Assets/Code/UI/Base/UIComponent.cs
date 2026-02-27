using UnityEngine;

namespace Code.UI
{
    public class UIComponent : MonoBehaviour
    {
        [field: SerializeField] public RectTransform Rect { get; private set; }

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