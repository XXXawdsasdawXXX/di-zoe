using Code.Core.GameLoop;
using Code.Game.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Windows.Timers
{
    public class TimerEntryView: UIComponent
    {
        [Header("Header")]
        [SerializeField] private TMP_Text  _badgeText;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Button    _deleteButton;

        [Header("Dial — shown when Idle")]
        [SerializeField] private GameObject      _dialRoot;
        [SerializeField] private TMP_InputField  _hoursInput;
        [SerializeField] private TMP_InputField  _minutesInput;
        [SerializeField] private TMP_InputField  _secondsInput;

        [Header("Display — shown when Running/Paused/Done")]
        [SerializeField] private GameObject _displayRoot;
        [SerializeField] private TMP_Text   _displayText;

        [Header("Controls")]
        [SerializeField] private Button  _toggleButton;
        [SerializeField] private TMP_Text _toggleLabel;
        [SerializeField] private Button  _resetButton;

        [Header("Colors")]
        [SerializeField] private Color _colorTimer     = new Color(0.09f, 0.37f, 0.65f);
        [SerializeField] private Color _colorAlarm     = new Color(0.73f, 0.46f, 0.07f);
        [SerializeField] private Color _colorStopwatch = new Color(0.23f, 0.43f, 0.07f);
        [SerializeField] private Color _colorDone      = new Color(0.64f, 0.18f, 0.18f);


        private TimerEntry _entry;
        private TimersService _service;

        
        public override void Disable()
        {
            _unbind();
            gameObject.SetActive(false);
        }

        public void Bind(TimerEntry entry, TimersService service)
        {
            _unbind();
            _entry   = entry;
            _service = service;

            
            
            _nameInput.text = entry.Name;
            _nameInput.onEndEdit.AddListener(_onNameChanged);

            _deleteButton.onClick.AddListener(_onDelete);
            _toggleButton.onClick.AddListener(_onToggle);
            _resetButton.onClick.AddListener(_onReset);

   

            
            _hoursInput.onEndEdit.AddListener(_onHoursChanged);
            _minutesInput.onEndEdit.AddListener(_onMinutesChanged);
            _secondsInput.onEndEdit.AddListener(_onSecondsChanged); 

            
            _hoursInput.onValueChanged.AddListener(val =>
            {
                if (val.Length >= 2) _minutesInput.Select();
            });

            _minutesInput.onValueChanged.AddListener(val =>
            {
                if (val.Length >= 2) _secondsInput.Select();
            });

            entry.OnChanged += _refresh;
            _refresh();
        }

        private void _unbind()
        {
            if (_entry == null) return;
            _entry.OnChanged -= _refresh;
            _nameInput.onEndEdit.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();
            _toggleButton.onClick.RemoveAllListeners();
            _resetButton.onClick.RemoveAllListeners();
            _hoursInput.onEndEdit.RemoveAllListeners();
            _hoursInput.onValueChanged.RemoveAllListeners();
            _minutesInput.onEndEdit.RemoveAllListeners();
            _entry = null;
        }

        private void _refresh()
        {
            if (_entry == null) return;

            bool isIdle = _entry.State == ETimerState.Idle;
            bool isStopwatch = _entry.Type == ETimerType.Stopwatch;

            // Badge
            _badgeText.text  = _entry.IsDone ? "done" : _entry.Type.ToString().ToLower();
            _badgeText.color = _entry.IsDone ? _colorDone : _entry.Type switch
            {
                ETimerType.Timer     => _colorTimer,
                ETimerType.Alarm     => _colorAlarm,
                ETimerType.Stopwatch => _colorStopwatch,
                _                    => _colorTimer
            };

            // Dial vs Display
            bool showDial = isIdle && !isStopwatch;
            _dialRoot.SetActive(showDial);
            _displayRoot.SetActive(!showDial);

            if (showDial)
            {
                int h = Mathf.FloorToInt(_entry.TargetSeconds / 3600);
                int m = Mathf.FloorToInt((_entry.TargetSeconds % 3600) / 60);
                int s = Mathf.FloorToInt(_entry.TargetSeconds % 60);
                _hoursInput.text   = h.ToString("D2");
                _minutesInput.text = m.ToString("D2");
                _secondsInput.text = s.ToString("D2");
            }
            else
            {
                _displayText.text  = _entry.FormatDisplay();
                _displayText.color = _entry.IsDone ? _colorDone : Color.white;
            }

            // Toggle button
            bool canToggle = !_entry.IsDone && (isStopwatch || _entry.TargetSeconds > 0);
            _toggleButton.gameObject.SetActive(canToggle);
            _toggleLabel.text = _entry.IsRunning ? "Pause" : (_entry.State == ETimerState.Paused ? "Resume" : "Start");
        }

        private void _onNameChanged(string val)
        {
            if (_entry != null) _entry.Name = string.IsNullOrWhiteSpace(val) ? _entry.Name : val;
        }

        private void _onDelete()
        {
            if (_entry != null) _service.Remove(_entry.Id);
        }

        private void _onToggle()
        {
            _entry?.Toggle();
        }

        private void _onReset()
        {
            _entry?.Reset();
            _refresh();
        }

        private void _onHoursChanged(string val)
        {
            if (_entry == null) return;
            int h = int.TryParse(val, out int r) ? Mathf.Clamp(r, 0, 23) : 0;
            int m = Mathf.FloorToInt((_entry.TargetSeconds % 3600) / 60);
            int s = Mathf.FloorToInt(_entry.TargetSeconds % 60);
            _entry.TargetSeconds = h * 3600 + m * 60 + s;
            _hoursInput.text = h.ToString("D2");
        }

        private void _onMinutesChanged(string val)
        {
            if (_entry == null) return;
            int m = int.TryParse(val, out int r) ? Mathf.Clamp(r, 0, 59) : 0;
            int h = Mathf.FloorToInt(_entry.TargetSeconds / 3600);
            int s = Mathf.FloorToInt(_entry.TargetSeconds % 60);
            _entry.TargetSeconds = h * 3600 + m * 60 + s;
            _minutesInput.text = m.ToString("D2");
        }
        
        private void _onSecondsChanged(string val)
        {
            if (_entry == null) return;
            int s = int.TryParse(val, out int r) ? Mathf.Clamp(r, 0, 59) : 0;
            int h = Mathf.FloorToInt(_entry.TargetSeconds / 3600);
            int m = Mathf.FloorToInt((_entry.TargetSeconds % 3600) / 60);
            _entry.TargetSeconds   = h * 3600 + m * 60 + s;
            _secondsInput.text = s.ToString("D2");
        }
    }
}