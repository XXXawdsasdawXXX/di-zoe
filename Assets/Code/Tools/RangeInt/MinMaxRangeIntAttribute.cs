using System;

namespace Code.Data
{
    public class MinMaxRangeIntAttribute : Attribute
    {
        public MinMaxRangeIntAttribute(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int Min { get; private set; }
        public int Max { get; private set; }
    }
}