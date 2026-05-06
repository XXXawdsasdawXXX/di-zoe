using System.Collections.Generic;
using Code.Core.GameLoop;
using Code.Core.Pools;
using Cysharp.Threading.Tasks;
using TriInspector;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIVolumeSlider : UISlider, IStartListener
    {
        
        [SerializeField] private MonoPool<UIButton> _volumePool;

        public UniTask GameStart()
        {
            IReadOnlyList<UIButton> all = _volumePool.GetAll();
            
            for (int i = 0; i < all.Count; i++)
            {
                UIButton button = all[i];
                int index = i;
                button.Index = index;
                button.SubscribeToClicked(() => _updateVolume(index));
            }
            
            return UniTask.CompletedTask;
        }

        private void _updateVolume(int volume)
        {
            Debug.Log($"click to volume {volume}");
            SetValue(volume);
        }

        [Button]
        public override void SetValueWithoutNotify(float value)
        {
            Debug.Log(value);
            int count = Mathf.RoundToInt(value);

            _volumePool.EnableCount(count, true);
        }

        public override float GetValue()
        {
            if (_volumePool.Count() == 0) return 0f;
            return (float)_volumePool.EnabledCount() / _volumePool.Count();
        }
    }
}