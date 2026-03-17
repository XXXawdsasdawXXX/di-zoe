using System;
using System.Collections.Generic;

namespace Code.Core.Save.SavedData
{
    [Serializable]
    public class PlayerProgressData
    {
        public DateTime GameExitTime;
        public DateTime GameEnterTime;

        public int RadioChanel;
        public float RadioVolume;

        public List<int> FavoriteRadioChannels;

        
        public PlayerProgressData()
        {
            RadioChanel = 0;
            RadioVolume = 0.5f;
            FavoriteRadioChannels = new List<int>();
        }
    }
}