using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Code.Game.Radio
{
    [Serializable, Preserve]
    public class RadioSongListModel
    {
        public List<RadioSongModel> Songs;
    }
}