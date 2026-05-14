using System.Collections.Generic;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Game.Radio;
using Cysharp.Threading.Tasks;
using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI.Windows.Radio
{
    public class UIVolumeSlider : UISlider, IStartListener, IInitializeListener, 
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private MonoPool<UIButton> _volumePool;
        private RadioTranslation _radioTranslation;

        private int _lastDragIndex = -1;


        public UniTask GameInitialize()
        {
            _radioTranslation = Container.Instance.GetService<RadioTranslation>();
            return UniTask.CompletedTask;
        }

        public UniTask GameStart()
        {
            IReadOnlyList<UIButton> all = _volumePool.GetAll();

            
            for (int i = 0; i < all.Count; i++)
            {
                UIButton button = all[i];
                int index = i;
                button.Index = index;
                button.SubscribeToClicked(() =>
                {
                    _updateVolume(_volumePool.PoolCount() - index);
                });
            }


            int sliderVolume =
                Mathf.RoundToInt(_radioTranslation.Model.RadioVolume.PropertyValue * _volumePool.GetAll().Count);

            SetValueWithoutNotify(sliderVolume);

            return UniTask.CompletedTask;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _handlePointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _handlePointer(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) 
        {
            _lastDragIndex = -1;
        }

        private void _handlePointer(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Rect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint
            );

            Rect rect = Rect.rect;

            float normalized = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);

            int total = _volumePool.PoolCount();
            int index = Mathf.Clamp(Mathf.FloorToInt((1f - normalized) * total), 0, total);

            if (index != _lastDragIndex)
            {
                _lastDragIndex = index;
                _updateVolume(index);
            }
        }

        [Button]
        public override void SetValueWithoutNotify(float value)
        {
            int count = Mathf.RoundToInt(value);

            _volumePool.EnableCount(count, true);
        }

        public override float GetValue()
        {
            if (_volumePool.PoolCount() == 0)
            {
                return 0f;
            }

            return (float)_volumePool.EnabledCount() / _volumePool.PoolCount();
        }

        private void _updateVolume(int index)
        {
            int total = _volumePool.PoolCount();

            SetValueWithoutNotify(index);

            _radioTranslation.SetVolume(1 - (index / (float)total));
        }
    }
}