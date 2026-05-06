using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core.GameLoop;
using UnityEngine;

namespace Code.Core.Pools
{
    public interface  IPoolEntity
    {
        public bool IsEnabled();
        void Enable();
        void Disable();
    }
    
    [Serializable]
    public class MonoPool<T> where T: MonoBehaviour, IPoolEntity
    {
        [SerializeField] private Transform _root;
        [SerializeField] private T _prefab;
        [SerializeField] private List<T> _all = new List<T>();
        [SerializeField] private List<T> _enabled = new List<T>();

        public void Add(List<T> entities) 
        {
            foreach (T entity in entities)
            {
                if (_all.Contains(entity))
                {
                    continue;
                }

                _all.Add(entity);
                
                if (entity.enabled)
                {
                    _enabled.Add(entity);
                }
            }
        }

        public T GetNext()
        {
            T entity = GetDisabledEntity() ?? AddNewEntity();
            _enabled.Add(entity);
            entity.Enable();
            return entity;
        }

        private T GetDisabledEntity()
        {
            return _all.FirstOrDefault(entity => entity != null && !entity.IsEnabled());
        }

        private T AddNewEntity()
        {
            T entity = Spawner.Instantiate(_prefab, _root);
            _all.Add(entity);
            return entity;
        }

        public IReadOnlyList<T> GetAll()
        {
            return _all;
        }

        public IReadOnlyList<T> GetAllEnabled()
        {
            return _enabled;
        }

        public void Disable(T entity)
        {
            if (entity == null || !entity.gameObject.activeSelf)
            {
                return;
            }
            
            entity.Disable();
            
            _enabled.Remove(entity);
        }

        public void DisableAll()
        {
            foreach (T entity in _all)
            {
                entity.Disable();
            }

            _enabled.Clear();
        }

        public void EnableCount(int count, bool fromEnd = false)
        {
            
            Debug.Log($"mono pool set enabled count: {_enabled.Count} < {count} {_all.Count}");
            while (_enabled.Count < count)
            {
                T entity = GetDisabledEntity() ?? AddNewEntity();
                _enabled.Add(entity);
                entity.Enable();
            }

            while (_enabled.Count > count)
            {
                int index = fromEnd ? _enabled.Count - 1 : 0;
                T entity = _enabled[index];
                entity.Disable();
                _enabled.RemoveAt(index);
            }
        }

        public T GetByIndex(int tabIndex)
        {
            return _all[tabIndex];
        }

        public int Count()
        {
            return _all.Count;
        }

        public int EnabledCount()
        {
            return _enabled.Count;
        }

        public int GetIndex(T element)
        {
            return _enabled.IndexOf(element);
        }
    }
}