using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Code.Core.GameLoop;
using Code.Core.Save;
using Kirurobo;
using UnityEngine;

namespace Code.Core.ServiceLocator
{
    public class Container : MonoBehaviour
    {
        public static Container Instance;

        [SerializeField] private UniWindowController _uniWindowController;
        [SerializeField] private List<ScriptableObject> _configs;

        private MonoBehaviour[] _allObjects;
        
        private List<IService> _services = new();
        private List<IStorage> _storages = new();
        private List<IMono> _mono = new();
        private List<IView> _getters = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;

            _allObjects = FindObjectsOfType<MonoBehaviour>();
            InitList(ref _services);
            InitList(ref _storages);
            InitList(ref _mono);
            InitList(ref _getters);
        }

        private void InitList<T>(ref List<T> list)
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            IEnumerable<Type> serviceTypes = types.Where(t =>
                typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract &&
                !typeof(MonoBehaviour).IsAssignableFrom(t));

            foreach (Type serviceType in serviceTypes)
            {
                if (Activator.CreateInstance(serviceType) is T service)
                {
                    list.Add(service);
                }
            }

            IEnumerable<T> mbServices = _allObjects.OfType<T>();
            if (mbServices.Any())
            {
                list.AddRange(mbServices);
            }

        }

        public UniWindowController GetUniWindowController()
        {
            return _uniWindowController;
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


        public T GetView<T>() where T : class
        {
            foreach (IView getter in _getters)
            {
                if (getter is T findGetter)
                {
                    return findGetter;
                }
            }

            return default;
        }

        public T FindStorage<T>() where T : IStorage
        {
            foreach (IStorage storage in _storages)
            {
                if (storage is T typedStorage)
                {
                    return typedStorage;
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
            list.AddRange(_storages.OfType<T>().ToList());
            list.AddRange(_mono.OfType<T>().ToList());

            IEnumerable<T> mbListeners = _allObjects.OfType<T>();
            foreach (T mbListener in mbListeners)
            {
                if (!list.Contains(mbListener))
                {
                    list.Add(mbListener);
                }
            }

            return list;
        }
    }
}