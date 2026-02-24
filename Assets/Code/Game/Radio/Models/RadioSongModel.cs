using System;

namespace Code.Game.Radio
{
    [Serializable]
    public struct RadioSongModel
    {
        public string title;
        public string artist;
        public string album;
        public string image;    // URL обложки (может отсутствовать)
        public string date;     // Unix timestamp в виде строки
    }
}