using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core.GameLoop;
using Code.Core.Save;
using UnityEngine;

namespace Code.Core.ServiceLocator
{
    public class Container : MonoBehaviour
    {
        public static Container Instance;
        
        [SerializeField] private List<ScriptableObject> _configs;

        private MonoBehaviour[] _allObjects;
        private List<IService> _services = new();

        private Type[] _cachedOrderedTypes;
        private IAssemblyInstaller[] _cachedInstallers;
        private HashSet<Type> _cachedAllTypes;


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;

            _cachedInstallers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IAssemblyInstaller).IsAssignableFrom(t) 
                            && t.IsClass && !t.IsAbstract)
                .Select(t => (IAssemblyInstaller)Activator.CreateInstance(t))
                .OrderBy(a => a.Order)
                .ToArray();
            
            _cachedOrderedTypes = _cachedInstallers
                .SelectMany(i => i.GetServiceOrder())
                .ToArray();
            
            _cachedAllTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract
                                      && !typeof(MonoBehaviour).IsAssignableFrom(t))
                .ToHashSet();
            
            _allObjects = FindObjectsOfType<MonoBehaviour>();

            CreateTypes(ref _services);
        }
        
        private void CreateTypes<T>(ref List<T> collection)
        {
            foreach (Type type in CollectOrderedTypes<T>())
            {
                if (Activator.CreateInstance(type) is T item)
                {
                    collection.Add(item);
                }
            }
            
            IEnumerable<T> ofType = _allObjects.OfType<T>();
            IEnumerable<T> enumerable = ofType as T[] ?? ofType.ToArray();
            
            if (enumerable.Any())
            {
                collection.AddRange(enumerable);
            }
        }

        private List<Type> CollectOrderedTypes<T>()
        {
            Type targetType = typeof(T);
    
            List<Type> ordered = _cachedOrderedTypes
                .Where(t => targetType.IsAssignableFrom(t))
                .ToList();

            HashSet<Type> orderedSet = new(ordered); // O(1) lookup

            IEnumerable<Type> rest = _cachedAllTypes
                .Where(t => targetType.IsAssignableFrom(t) && !orderedSet.Contains(t));
    
            ordered.AddRange(rest);

            return ordered;
        }

        public T GetConfig<T>() where T : ScriptableObject
        {
            foreach (ScriptableObject scriptableObject in _configs)
            {
                if (scriptableObject is T findConfig)
                {
                    return findConfig;
                }
            }

            return null;
        }

        public T GetService<T>() where T : IService
        {
            foreach (IService service in _services)
            {
                if (service is T findService)
                {
                    return findService;
                }
            }

            return default;
        }
        
        public List<IGameListeners> GetGameListeners()
        {
            return GetContainerComponents<IGameListeners>();
        }

        public List<IProgressReader> GetProgressReaders()
        {
            return GetContainerComponents<IProgressReader>();
        }

        private List<T> GetContainerComponents<T>()
        {
            List<T> list = new();

            list.AddRange(_services.OfType<T>().ToList());

            IEnumerable<T> enumerable = _allObjects.OfType<T>();
       
            foreach (T value in enumerable)
            {
                if (!list.Contains(value))
                {
                    list.Add(value);
                }
            }

            return list;
        }
    }
}