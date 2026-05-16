using System;

namespace Code.Game.Timers 
{
    public enum ETimerType { Timer, Alarm, Stopwatch }
    public enum ETimerState { Idle, Running, Paused, Done }

    [Serializable]
    public class TimerEntryData
    {
        public string Id;
        public string Name;
        public ETimerType Type;
        public ETimerState State;
        public float ElapsedSeconds;
        public float TargetSeconds;
        public long PausedAtTicks; // DateTime.Ticks когда поставили на паузу
    }
}