using System.Collections.Generic;
using Code.Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Kirurobo;
using Unity.Profiling;
using UnityEngine;


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

        [SerializeField] private int _focusedFPS   = 30;
        [SerializeField] private int _unfocusedFPS = 10;
        
       [SerializeField] private UniWindowController _appWindowController;

        private readonly List<IInitializeListener> _initListeners = new();
        private readonly List<ILoadListener> _loadListeners = new();
        private readonly List<IStartListener> _startListeners = new();
        private readonly List<IUpdateListener> _updateListeners = new();
        private readonly List<IExitListener> _exitListeners = new();
        private readonly List<ISubscriber> _subscribers = new();

        private State _currentState;

        
        private void Awake()
        {
            Application.targetFrameRate = _focusedFPS;
            QualitySettings.vSyncCount  = 0; 
            
            _initializeListeners();

            if (Application.isEditor)
            {
                _appWindowController.gameObject.SetActive(false);

                _bootGame();
            }
            else
            {
                _appWindowController.OnStateChanged += _onAppWindowInitialized;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Application.targetFrameRate = hasFocus || _appWindowController.isTopmost
                ? _focusedFPS 
                : _unfocusedFPS;
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
        }

        public void RemoveRuntimeListener(IGameListeners listener)
        {
            if (listener is IUpdateListener tickListener) _updateListeners.Remove(tickListener);

            if (listener is IExitListener exitListener) _exitListeners.Remove(exitListener);

            if (listener is ISubscriber subscriber) subscriber.Unsubscribe();
        }

        private void _onAppWindowInitialized(UniWindowController.WindowStateEventType type)
        {
            _appWindowController.OnStateChanged -= _onAppWindowInitialized;

            _bootGame();
        }

        private async void _bootGame()
        {
            _currentState = State.Initialize;
            await _notifyGameInitialize();

            _currentState = State.Subscribe;
            await _notifySubscribe();

            _currentState = State.Load;
            await _notifyGameLoad();
            
            _currentState = State.Start;
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
            foreach (IInitializeListener listener in _initListeners)
            {
                await listener.GameInitialize();
            }
        }

        private async UniTask _notifyGameLoad()
        {
            foreach (ILoadListener listener in _loadListeners)
            {
                await listener.GameLoad();
            }
        }

        private UniTask _notifySubscribe()
        {
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Subscribe();
            }

            return UniTask.CompletedTask;
        }

        private async UniTask _notifyGameStart()
        {
            foreach (IStartListener listener in _startListeners)
            {
                await listener.GameStart();
            }
        }

        private void _notifyGameUpdate()
        {
            foreach (IUpdateListener listener in _updateListeners)
            {
                listener.GameUpdate();
            }
        }

        private void _notifyGameExit()
        {
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Unsubscribe();
            }

            foreach (IExitListener listener in _exitListeners)
            {
                listener.GameExit();
            }
        }
    }
}