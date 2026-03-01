using System;
using UnityEngine.Scripting;

namespace Code.Game.Radio
{
    [Serializable, Preserve]
    public class RadioSongModel
    {
        public string title;
        public string artist;
        public string album;
        public string image;    // URL обложки (может отсутствовать)
        public string date;     // Unix timestamp в виде строки
    }
}