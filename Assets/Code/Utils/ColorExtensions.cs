using System;
using UnityEngine;

namespace Code.Utils
{
    public static class ColorExtensions
    {
        public static bool Equal(this Color32 color1, Color32 color2)
        {
            return color1.r == color2.r && color1.g == color2.g && color1.b == color2.b && color1.a == color2.a;
        }

        public static bool Equal(this Color32 color1, Color32 color2, byte sensitivity)
        {
            if (sensitivity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sensitivity));
            }
            
            byte rDiff = (byte)Mathf.Abs(color1.r - color2.r);
            byte gDiff = (byte)Mathf.Abs(color1.g - color2.g);
            byte bDiff = (byte)Mathf.Abs(color1.b - color2.b);
            byte aDiff = (byte)Mathf.Abs(color1.a - color2.a);

            return rDiff <= sensitivity && gDiff <= sensitivity && bDiff <= sensitivity && aDiff <= sensitivity;
        }
    }
}