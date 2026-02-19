using System.Linq;
using Code.Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Core.GameLoop
{
    [Preserve]
    public class Spawner : IService, IInitializeListener
    {
        private GameEventDispatcher _gameEventDispatcher;

        public UniTask GameInitialize()
        {
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();
            
            return UniTask.CompletedTask;
        }
        
        public T Instantiate<T>(T prefab) where T : Object
        {
            T instance = Object.Instantiate(prefab);

            GameObject gameInstance = instance as GameObject;

            if (gameInstance != null)
            {
                IGameListeners[] listeners = gameInstance.GetComponentsInChildren<IGameListeners>(true).ToArray();

                foreach (IGameListeners listener in listeners)
                {
                    _gameEventDispatcher.AddRuntimeListener(listener);
                }
            }

            return instance;
        }

        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Object
        {
            T instance = Object.Instantiate(prefab, position, rotation);

            GameObject gameInstance = instance as GameObject;

            if (gameInstance != null)
            {
                IGameListeners[] listeners = gameInstance.GetComponentsInChildren<IGameListeners>(true).ToArray();

                foreach (IGameListeners listener in listeners)
                {
                    _gameEventDispatcher.AddRuntimeListener(listener);
                }
            }

            return instance;
        }

        public void Destroy(GameObject instance)
        {
            IGameListeners[] listeners = instance.GetComponentsInChildren<IGameListeners>(true).ToArray();

            foreach (IGameListeners listener in listeners)
            {
                _gameEventDispatcher.RemoveRuntimeListener(listener);
            }
            
            Object.Destroy(instance);
        }
    }
}