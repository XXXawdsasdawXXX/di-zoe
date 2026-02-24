using System;

namespace Code.Game.Radio
{
    [Serializable]
    public struct RadioChannelModel
    {
        public string id;
        public string title;
        public string description;
        public string image; // маленький логотип
        public string largeimage; // средний логотип
        public string xlimage; // большой логотип
        public int listeners;
        public string genre;
    }
}