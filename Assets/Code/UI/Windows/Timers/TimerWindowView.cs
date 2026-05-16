using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Game.Timers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Windows.Timers
{
    public class TimerWindowView: UIView, IInitializeListener, IStartListener, ISubscriber
    {
        [Header("Add buttons")]
        [SerializeField] private Button _addTimerButton;
        [SerializeField] private Button _addAlarmButton;
        [SerializeField] private Button _addStopwatchButton;
        [SerializeField] private Button _deleteAllButton;

        [Header("List")]
        [SerializeField] private MonoPool<TimerEntryView> _pool;
        [SerializeField] private GameObject _emptyLabel;

        [Header("Sound")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip   _doneClip;

        private TimersService _service;

        
        public UniTask GameInitialize()
        {
            _service = Container.Instance.GetService<TimersService>();
            
            return UniTask.CompletedTask;
        }
        
        public UniTask GameStart()
        {
            _rebuildList();
            
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _addTimerButton.onClick.AddListener(() => _service.Add(ETimerType.Timer));
            _addAlarmButton.onClick.AddListener(() => _service.Add(ETimerType.Alarm));
            _addStopwatchButton.onClick.AddListener(() => _service.Add(ETimerType.Stopwatch));
            _deleteAllButton.onClick.AddListener(_service.RemoveAll);
            
            _service.OnListChanged  += _rebuildList;
            _service.OnTimerDone    += _onDone;
        }

        public void Unsubscribe()
        {
            _service.OnListChanged -= _rebuildList;
            _service.OnTimerDone   -= _onDone;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        private void _rebuildList()
        {
            _pool.DisableAll();

            foreach (TimerEntry entry in _service.Entries)
            {
                TimerEntryView view = _pool.GetNext();
                view.Bind(entry, _service);
            }

            _emptyLabel.SetActive(_service.Entries.Count == 0);
        }

        private void _onDone(TimerEntry entry)
        {
            if (_doneClip != null)
            {
                _audioSource.PlayOneShot(_doneClip);
            }
        }
    }
}