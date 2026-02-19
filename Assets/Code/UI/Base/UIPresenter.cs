using System;
using UnityEngine;

namespace Code.UI
{
    public class UIPresenter<View> : UIComponent where View : UIView
    {
        [SerializeField] protected View view;


        #region Editor
#if UNITY_EDITOR

        private void OnValidate()
        {
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