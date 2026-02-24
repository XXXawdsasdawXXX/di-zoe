using System;
using System.Collections.Generic;

namespace Code.Game.Radio
{
    [Serializable]
    public struct RadioSongListModel
    {
        public List<RadioSongModel> Songs;
    }
}