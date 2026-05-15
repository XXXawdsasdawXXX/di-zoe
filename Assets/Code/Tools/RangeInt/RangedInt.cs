using System;
using Random = UnityEngine.Random;

namespace Code.Tools.RangeInt
{
    [Serializable]
    public struct RangedInt
    {
        public int MinValue;
        public int MaxValue;

        public readonly int GetRandomValue()
        {
            return Random.Range(MinValue, MaxValue);
        }

        public bool Contains(int value)
        {
            return MinValue <= value && MaxValue >= value;
        }
    }
}