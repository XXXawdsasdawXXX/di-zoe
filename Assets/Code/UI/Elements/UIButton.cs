using System;
using System.Collections.Generic;
using Code.Core.GameLoop;
using Code.UI.Models;
using Cysharp.Threading.Tasks;
using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI
{
    public class UIButton : UIComponent, IInitializeListener,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [ShowInInspector] protected List<UIButtonImpact> buttonImpacts = new List<UIButtonImpact>() 
        {
            new UIButtonImpact_ImagesColor(),
        };
        
        private Action _clicked;
        private double _lastClickTime;
        
        
        public UniTask GameInitialize()
        {
            foreach (UIButtonImpact buttonImpact in buttonImpacts)
            {
                buttonImpact.Initialize();
            }
            
            return UniTask.CompletedTask;
        }
        
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

        protected virtual void onClick()
        {
            
        }
    }
}