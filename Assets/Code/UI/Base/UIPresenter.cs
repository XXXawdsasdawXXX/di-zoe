using UnityEngine;

namespace Code.UI
{
    public class UIPresenter<View> : MonoBehaviour where View : UIView
    {
        [SerializeField] protected View view;

        
        #region Editor
#if UNITY_EDITOR

        protected void OnValidate()
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