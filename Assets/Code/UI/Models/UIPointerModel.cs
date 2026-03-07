using System;
using UnityEngine;

namespace Code.UI.Models
{
    [Serializable]
    public class UIPointerModel
    {
        public Color Default = Color.white;
        public Color Enter = new(0.8f,0.8f,0.8f);
        public Color Up = new(0.8f,0.8f,0.8f);
        public Color Down = new(0.6f,0.6f,0.6f);
    }
}