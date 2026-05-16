using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Code.Core.GameLoop;
using Code.Core.Save;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Game.Timers
{
    [Preserve]
    public class TimersService: IProgressWriter, IUpdateListener, IService
    {
        private readonly List<TimerEntry> _entries = new();
        private int _timerCounter;
        private int _alarmCounter;
        private int _stopwatchCounter;

        public ReadOnlyCollection<TimerEntry> Entries => _entries.AsReadOnly();

        public event Action OnListChanged;
        public event Action<TimerEntry> OnTimerDone;

        // ── IProgressWriter ───────────────────────────────────────────────────

        public async UniTask LoadProgress(PlayerProgressData data)
        {
            _entries.Clear();

            if (string.IsNullOrEmpty(data.Timers))
            {
                OnListChanged?.Invoke();
                await UniTask.CompletedTask;
                return;
            }

            TimersSaveData saveData = data.Timers.ToDeserialized<TimersSaveData>();

            if (saveData == null)
            {
                OnListChanged?.Invoke();
                await UniTask.CompletedTask;
                return;
            }

            _timerCounter     = saveData.TimerCounter;
            _alarmCounter     = saveData.AlarmCounter;
            _stopwatchCounter = saveData.StopwatchCounter;

            foreach (TimerEntryData entryData in saveData.Entries)
            {
                TimerEntry entry = TimerEntry.FromData(entryData);
                _subscribe(entry);
                _entries.Add(entry);
            }

            OnListChanged?.Invoke();
            await UniTask.CompletedTask;
        }

        public void SaveProgress(PlayerProgressData data)
        {
            var saveData = new TimersSaveData
            {
                TimerCounter     = _timerCounter,
                AlarmCounter     = _alarmCounter,
                StopwatchCounter = _stopwatchCounter
            };

            foreach (TimerEntry entry in _entries)
                saveData.Entries.Add(entry.ToData());

            data.Timers = JsonUtility.ToJson(saveData);
        }

        // ── IUpdateListener ───────────────────────────────────────────────────

        public void GameUpdate()
        {
            foreach (TimerEntry entry in _entries)
                entry.Tick(Time.deltaTime);
        }

        // ── Public API ────────────────────────────────────────────────────────

        public TimerEntry Add(ETimerType type)
        {
            string id   = Guid.NewGuid().ToString();
            string name = _generateName(type);
            float target = type == ETimerType.Stopwatch ? 0f : 5 * 60f;

            var entry = new TimerEntry(id, name, type, target);
            _subscribe(entry);
            _entries.Add(entry);
            OnListChanged?.Invoke();
            return entry;
        }

        public void Remove(string id)
        {
            TimerEntry entry = _entries.Find(e => e.Id == id);
            if (entry == null) return;

            _entries.Remove(entry);
            OnListChanged?.Invoke();
        }

        public void RemoveAll()
        {
            _entries.Clear();
            OnListChanged?.Invoke();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void _subscribe(TimerEntry entry)
        {
            entry.OnDone += () => OnTimerDone?.Invoke(entry);
        }

        private string _generateName(ETimerType type)
        {
            return type switch
            {
                ETimerType.Timer     => $"timer_{++_timerCounter:D2}",
                ETimerType.Alarm     => $"alarm_{++_alarmCounter:D2}",
                ETimerType.Stopwatch => $"stopwatch_{++_stopwatchCounter:D2}",
                _                    => "timer_01"
            };
        }
    }
}