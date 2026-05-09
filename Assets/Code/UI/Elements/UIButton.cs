using System;
using Code.Core.GameLoop;
using Code.UI.Models;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI
{
    public class UIButton : UIComponent, IInitializeListener,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsEntered { get; private set; }
        
        [SerializeReference] protected UIButtonImpact[] buttonImpacts;

        [SerializeField, CanBeNull] private UIButton _parentButton;
        
        private Action _clicked;
        private Action<bool> _entered;
        private double _lastClickTime;

        
        public virtual UniTask GameInitialize()
        {
            if (buttonImpacts == null || buttonImpacts.Length == 0)
            {
                return UniTask.CompletedTask;
            }
            
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact?.Initialize();
            }

            return UniTask.CompletedTask;
        }
        
        public void SubscribeToClicked(Action clicked) => _clicked += clicked;

        public void UnsubscribeFromClicked(Action clicked) => _clicked -= clicked;

        public void SubscribeToEntered(Action<bool> entered) => _entered += entered;

        public void UnsubscribeFromEntered(Action<bool> entered) => _entered -= entered;


        public virtual void ClearSubscriptions()
        {
            _clicked = null;
            _entered = null;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject != gameObject)
            {
                return;
            }

            if (_parentButton != null)
            {
                _parentButton._onPointerExit();
            }
            
            _onPointerEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_parentButton != null)
            {
                _parentButton._onPointerEnter();
            }
      
            _onPointerExit();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject != gameObject)
            {
                return;
            }
            
            if (_lastClickTime + UIConfiguration.CLICK_COOLDOWN > DateTime.UtcNow.TimeOfDay.TotalSeconds)
            {
                return;
            }
            
            _clicked?.Invoke();

            _lastClickTime = DateTime.UtcNow.TimeOfDay.TotalSeconds;
            
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact.OnDown();
            }
            
            onClick();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject != gameObject)
            {
                return;
            }
            
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact.OnUp();
            }
        }

        protected virtual void onClick()
        {
            
        }
        
        protected virtual void onEntered(bool entered)
        {
            IsEntered = entered;
        }
        
        private void _onPointerEnter()
        {
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact.OnEnter();
            }

            onEntered(true);

            _entered?.Invoke(true);
        }

        private void _onPointerExit()
        {
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact.OnExit();
            }

            UIRadioButton radioButton = this as UIRadioButton;

            if (radioButton != null)
            {
                radioButton.UpdateImpactState();
            }

            onEntered(false);

            _entered?.Invoke(false);
        }
    }
}