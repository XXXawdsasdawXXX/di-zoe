using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Windows.Radio
{
    public class UIVolumeElement : UIButton
    {
        [SerializeField] private Image _image;


        public override bool IsEnabled()
        {
            return _image.color == Color.white;
        }

        public override void Enable()
        {
            _image.color = Color.white;    
        }

        public override void Disable()
        {
            _image.color = Color.clear;    
        }

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();

            if (_image == null)
            {
                TryGetComponent(out _image);
            }
        }
#endif
    }
}