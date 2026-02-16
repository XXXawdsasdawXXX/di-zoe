using System;


namespace Code.Infrastructure.Save
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