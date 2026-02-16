using System;

namespace Code.Infrastructure.Save
{
    [Serializable]
    public class CooldownSavedData
    {
        public int AppleRemainingTick;
        public int SleepRemainingTick;
    }
}