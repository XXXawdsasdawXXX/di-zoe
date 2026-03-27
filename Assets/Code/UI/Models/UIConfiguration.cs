using DG.Tweening;
using UnityEngine;

namespace Code.UI.Models
{
    [CreateAssetMenu(fileName = "UIConfiguration", menuName = "Configuration/UI")]
    public class UIConfiguration : ScriptableObject
    {
        public const  float ANIMATION_DURATION_SHORT  = 0.3f;
        public const  float ANIMATION_DURATION_LONG  = 0.7f;
        public const  float TYPE_WRITE_DELAY  = 0.02f;
        public const  float CLICK_COOLDOWN  = 0.05f;
        public const Ease TWEEN_EASY = Ease.Flash; 
        
        [field: SerializeField] public UIPointerModel DefaultButtonsImpactColor { get; private set; }
    }
}