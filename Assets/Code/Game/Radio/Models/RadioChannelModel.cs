using System;
using UnityEngine.Scripting;

namespace Code.Game.Radio
{
    [Serializable, Preserve]
    public class RadioChannelModel
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