using System;
using System.Collections.Generic;

namespace Code.Game.Timers
{
    [Serializable]
    public class TimersSaveData
    {
        public List<TimerEntryData> Entries = new();
        public int TimerCounter;
        public int AlarmCounter;
        public int StopwatchCounter;
    }
}