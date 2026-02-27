using UnityEngine;

namespace Code.UI
{
    public class UIPresenter<View> : UIComponent where View : UIView
    {
        [SerializeField] protected View view;

        
        #region Editor
#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (view == null)
            {
                if (!TryGetComponent(out view))
                {
                    view = gameObject.AddComponent<View>();
                }
            }
        }

#endif
        #endregion
        
    }
}