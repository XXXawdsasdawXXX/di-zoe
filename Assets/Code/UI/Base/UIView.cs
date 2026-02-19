using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI
{
    public class UIView : UIComponent
    {
        [field: SerializeField] protected RectTransform root;
        
        
        public UniTask Open()
        {
            root.gameObject.SetActive(true);
            
            return UniTask.CompletedTask;
        }

        public UniTask Close()
        {
            root.gameObject.SetActive(false);
            
            return UniTask.CompletedTask;
        }
    }
}