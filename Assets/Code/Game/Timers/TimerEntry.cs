using System;
using UnityEngine;

namespace Code.Game.Timers
{
    public class TimerEntry
    {
        public string Id { get; }
        public string Name { get; set; }
        public ETimerType Type { get; }
        public ETimerState State { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public float TargetSeconds { get; set; }

        public bool IsDone => State == ETimerState.Done;
        public bool IsRunning => State == ETimerState.Running;

        public float Remaining => Mathf.Max(0f, TargetSeconds - ElapsedSeconds);
        public float DisplaySeconds => Type == ETimerType.Stopwatch
            ? ElapsedSeconds
            : (State == ETimerState.Idle ? TargetSeconds : Remaining);

        public event Action OnDone;
        public event Action OnChanged;

        public TimerEntry(string id, string name, ETimerType type, float targetSeconds = 0)
        {
            Id            = id;
            Name          = name;
            Type          = type;
            TargetSeconds = targetSeconds;
            State         = ETimerState.Idle;
        }

        public void Tick(float dt)
        {
            if (State != ETimerState.Running || IsDone) return;

            if (Type == ETimerType.Stopwatch)
            {
                ElapsedSeconds += dt;
                OnChanged?.Invoke();
            }
            else
            {
                ElapsedSeconds += dt;
                if (ElapsedSeconds >= TargetSeconds)
                {
                    ElapsedSeconds = TargetSeconds;
                    State          = ETimerState.Done;
                    OnChanged?.Invoke();
                    OnDone?.Invoke();
                }
                else
                {
                    OnChanged?.Invoke();
                }
            }
        }

        public void Start()
        {
            if (IsDone) return;
            if (Type != ETimerType.Stopwatch && TargetSeconds <= 0) return;
            State = ETimerState.Running;
            OnChanged?.Invoke();
        }

        public void Pause()
        {
            if (State != ETimerState.Running) return;
            State = ETimerState.Paused;
            OnChanged?.Invoke();
        }

        public void Toggle()
        {
            if (IsRunning) Pause();
            else           Start();
        }

        public void Reset()
        {
            State          = ETimerState.Idle;
            ElapsedSeconds = 0f;
            OnChanged?.Invoke();
        }

        public TimerEntryData ToData() => new()
        {
            Id             = Id,
            Name           = Name,
            Type           = Type,
            State          = State == ETimerState.Running ? ETimerState.Paused : State,
            ElapsedSeconds = ElapsedSeconds,
            TargetSeconds  = TargetSeconds,
            PausedAtTicks  = DateTime.Now.Ticks
        };

        public static TimerEntry FromData(TimerEntryData data)
        {
            var entry = new TimerEntry(data.Id, data.Name, data.Type, data.TargetSeconds)
            {
                ElapsedSeconds = data.ElapsedSeconds
            };
            if (data.State == ETimerState.Done)
                entry.State = ETimerState.Done;
            return entry;
        }
        
        public string FormatDisplay()
        {
            float secs = DisplaySeconds;
            int h = Mathf.FloorToInt(secs / 3600);
            int m = Mathf.FloorToInt((secs % 3600) / 60);
            int s = Mathf.FloorToInt(secs % 60);
            return $"{h:D2}:{m:D2}:{s:D2}";
        }
    }
}