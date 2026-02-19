using System.Collections.Generic;
using Code.Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Kirurobo;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Code.Core.GameLoop
{
    public class GameEventDispatcher : MonoBehaviour, IService
    {
        private enum State
        {
            None,
            Initialize,
            Subscribe,
            Load,
            Start,
            Update,
            Unsubscribe,
            Exit
        }

        private UniWindowController _controller;

        private readonly List<IInitializeListener> _initListeners = new();
        private readonly List<ILoadListener> _loadListeners = new();
        private readonly List<IStartListener> _startListeners = new();
        private readonly List<IUpdateListener> _updateListeners = new();
        private readonly List<IExitListener> _exitListeners = new();
        private readonly List<ISubscriber> _subscribers = new();

        private readonly Dictionary<IUpdateListener, string> _updateListenerName = new();

        private ProfilerMarker _updateMarker = new("update");

        private State _currentState;

        private void Awake()
        {
            _controller = Container.Instance.GetUniWindowController();

            _initializeListeners();

            if (Application.isEditor)
            {
                _controller.gameObject.SetActive(false);
              
                _bootGame();
                
            }
            else
            {
                _controller.OnStateChanged += _onWindowInitialized;
                
            }
        }

        private async void Start()
        {
            await UniTask.WaitUntil(() => _currentState is State.Start);

            await _notifyGameStart();
            
            _currentState = State.Update;
        }

        private void Update()
        {
            if (_currentState is State.Update)
            {
                _notifyGameUpdate();
            }
        }

        private void OnApplicationQuit()
        {
            if (_currentState > State.Subscribe)
            {
                _notifyGameExit();
            }

        }

        public async void AddRuntimeListener(IGameListeners listener)
        {
#if UNITY_EDITOR
            ProfilerMarker marker = new($"RuntimeListener: {listener.GetType().Name}");
            marker.Begin();
#endif
            if (listener is IInitializeListener initListener) await initListener.GameInitialize();

            if (listener is ISubscriber subscriber)
            {
                subscriber.Subscribe();
                _subscribers.Add(subscriber);
            }

            if (listener is ILoadListener loadListener) await loadListener.GameLoad();

            if (listener is IStartListener startListener) await startListener.GameStart();

            if (listener is IUpdateListener tickListener) _updateListeners.Add(tickListener);

            if (listener is IExitListener exitListener) _exitListeners.Add(exitListener);

#if UNITY_EDITOR
            marker.End();
#endif
        }

        public void RemoveRuntimeListener(IGameListeners listener)
        {
            if (listener is IUpdateListener tickListener) _updateListeners.Remove(tickListener);

            if (listener is IExitListener exitListener) _exitListeners.Remove(exitListener);

            if (listener is ISubscriber subscriber) subscriber.Unsubscribe();
        }

        private void _onWindowInitialized(UniWindowController.WindowStateEventType type)
        {
            _controller.OnStateChanged -= _onWindowInitialized;

            _bootGame();
        }

        private async void _bootGame()
        {
#if UNITY_EDITOR
            ProfilerMarker marker = new("_bootGame");
          
            marker.Begin();
            
            _currentState = State.Initialize;
            await _notifyGameInitialize();

            _currentState = State.Subscribe;
            await _notifySubscribe();

            _currentState = State.Load;
            await _notifyGameLoad();

            _currentState = State.Start;
            
            marker.End();

#else
            _currentState = State.Initialize;
            await _notifyGameInitialize();

            _currentState = State.Subscribe;
            await _notifySubscribe();

            _currentState = State.Load;
            await _notifyGameLoad();

            _currentState = State.Start;
#endif

        }

        private void _initializeListeners()
        {
            List<IGameListeners> gameListeners = Container.Instance.GetGameListeners();

            foreach (IGameListeners listener in gameListeners)
            {
                if (listener is IInitializeListener initListener) _initListeners.Add(initListener);

                if (listener is ISubscriber subscriber) _subscribers.Add(subscriber);

                if (listener is ILoadListener loadListener) _loadListeners.Add(loadListener);

                if (listener is IStartListener startListener) _startListeners.Add(startListener);

                if (listener is IUpdateListener tickListener) _updateListeners.Add(tickListener);

                if (listener is IExitListener exitListener) _exitListeners.Add(exitListener);
            }
        }

        private async UniTask _notifyGameInitialize()
        {
#if UNITY_EDITOR
            ProfilerMarker marker = new("_notifyGameInitialize");
            marker.Begin();
            
            foreach (IInitializeListener listener in _initListeners)
            {
                await listener.GameInitialize();
            }
            
            marker.End();
#else
            foreach (IInitializeListener listener in _initListeners)
            {
                await listener.GameInitialize();
            }
#endif
        }

        private async UniTask _notifyGameLoad()
        {
#if UNITY_EDITOR
            ProfilerMarker marker = new("_notifyGameLoad");
            marker.Begin();
          
            foreach (ILoadListener listener in _loadListeners)
            {
                await listener.GameLoad();
            }
     
            marker.End();
#else
            foreach (ILoadListener listener in _loadListeners)
            {
                await listener.GameLoad();
            }
#endif
        }

        private UniTask _notifySubscribe()
        {
#if UNITY_EDITOR
            ProfilerMarker marker = new("_notifySubscribe");
            marker.Begin();

            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Subscribe();
            }

            marker.End();
#else
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Subscribe();
            }
#endif
            return UniTask.CompletedTask;
        }

        private async UniTask _notifyGameStart()
        {
#if UNITY_EDITOR
            ProfilerMarker marker = new("_notifyGameStart");
            marker.Begin();

            foreach (IStartListener listener in _startListeners)
            {
                await listener.GameStart();
            }

            marker.End();
#else
            foreach (IStartListener listener in _startListeners)
            {
                await listener.GameStart();
            }
#endif
        }

        private void _notifyGameUpdate()
        {
#if UNITY_EDITOR
            using (_updateMarker.Auto())
            {
                foreach (IUpdateListener listener in _updateListeners)
                {
                    if (!_updateListenerName.ContainsKey(listener))
                    {
                        _updateListenerName.Add(listener, listener.GetType().Name);
                    }

                    Profiler.BeginSample(_updateListenerName[listener]);
                    listener.GameUpdate();
                    Profiler.EndSample();
                }
            }
#else
            foreach (IUpdateListener listener in _updateListeners)
            {
                listener.GameUpdate();
            }
#endif
        }

        private void _notifyGameExit()
        {
#if UNITY_EDITOR
            ProfilerMarker marker = new("_notifyGameExit");
            marker.Begin();
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Unsubscribe();
            }

            foreach (IExitListener listener in _exitListeners)
            {
                listener.GameExit();
            }
            marker.End();
#else
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Unsubscribe();
            }

            foreach (IExitListener listener in _exitListeners)
            {
                listener.GameExit();
            }
#endif
        }
    }
}