using System;
using Code.UI.Models;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI
{
    public class UIButton : UIComponent, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] protected UIButtonImpact[] buttonImpacts = new[]
        {
            new UIButtonImpact(),
        };
     
        private Action _clicked;
        private double _lastClickTime;
        
          
        public void SubscribeToClicked(Action clicked)
        {
            _clicked += clicked;
        }

        public void UnsubscribeFromClicked(Action clicked)
        {
            _clicked -= clicked;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact.OnEnter();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact.OnExit();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
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
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact.OnUp();
            }
        }
    }
}