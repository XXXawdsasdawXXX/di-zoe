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
        [SerializeReference] private List<UIButtonImpact> _buttonImpacts = new() 
        {
            new UIButtonImpact_ImagesColor(),
        };
        
        private Action _clicked;
        private double _lastClickTime;
        
        
        public virtual UniTask GameInitialize()
        {
            foreach (UIButtonImpact buttonImpact in _buttonImpacts)
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
            foreach (UIButtonImpact buttonImpact in _buttonImpacts)
            {
                buttonImpact.OnEnter();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (UIButtonImpact buttonImpact in _buttonImpacts)
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
            
            foreach (UIButtonImpact buttonImpact in _buttonImpacts)
            {
                buttonImpact.OnDown();
            }
            
            onClick();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            foreach (UIButtonImpact buttonImpact in _buttonImpacts)
            {
                buttonImpact.OnUp();
            }
        }

        protected virtual void onClick()
        {
            
        }
    }
}