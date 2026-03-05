using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Code.UI.Models
{
    [Serializable]
    public class UIPointerModel
    {
        public Color Default = Color.white;
        public Color Enter = Color.white;
        public Color Up = Color.white;
        public Color Down = Color.white;
        
        [Button()]
        public void SetDefaultColor()
        {
            Default = Color.white;
            Enter = new Color(0.8f,0.8f,0.8f);
            Up = new Color(0.8f,0.8f,0.8f);
            Down = new Color(0.6f,0.6f,0.6f);
        }
    }
}