using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Core.GameLoop
{
    public interface  IPoolEntity
    {
        public int Index { get; }
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
            T entity = _getDisabledEntity() ?? _addNewEntity();
            _enabled.Add(entity);
            entity.Enable();
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
            List<T> ordered = fromEnd 
                ? _all.OrderByDescending(e => e.Index).ToList()
                : _all.OrderBy(e => e.Index).ToList();

            _enabled.Clear();
            
            for (int i = 0; i < ordered.Count; i++)
            {
                T entity = ordered[i];
                if (i < count)
                {
                    entity.Enable();
                    _enabled.Add(entity);
                }
                else
                {
                    entity.Disable();
                }
            }
        }

        public T GetByIndex(int index)
        {
            return _all.FirstOrDefault(a => a.Index == index);
        }

        public int PoolCount()
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

        public void SetRoot(Transform root)
        {
            _root = root;
        }
        
        private T _getDisabledEntity()
        {
            return _all.FirstOrDefault(entity => entity != null && !entity.IsEnabled());
        }

        private T _addNewEntity()
        {
            T entity = Spawner.Instantiate(_prefab, _root);
            _all.Add(entity);
            return entity;
        }
    }
}