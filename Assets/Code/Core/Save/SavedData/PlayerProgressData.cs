using System;

namespace Code.Core.Save.SavedData
{
    [Serializable]
    public class PlayerProgressData
    {
        public DateTime GameExitTime;
        public DateTime GameEnterTime;

        public int RadioChanel;
        public float RadioVolume;

        public PlayerProgressData()
        {
            RadioChanel = 0;
            RadioVolume = 0.5f;
        }
    }
}