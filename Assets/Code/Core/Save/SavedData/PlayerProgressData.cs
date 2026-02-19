using System;

namespace Code.Core.Save.SavedData
{
    [Serializable]
    public class PlayerProgressData
    {
   public DateTime GameExitTime;
        public DateTime GameEnterTime;

        public PlayerProgressData()
        {

        }
    }
}