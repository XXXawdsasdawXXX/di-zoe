using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UIRawImage : UIComponent
    {
        [SerializeField] private RawImage _image;

        public void SetTexture(Texture2D texture2D)
        {
            _image.texture = texture2D;
        }

        #region Editor
#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (_image == null)
            {
                if (!TryGetComponent(out _image))
                {
                    _image = GetComponent<RawImage>();
                }
            }
        }

#endif
        #endregion
    }
}