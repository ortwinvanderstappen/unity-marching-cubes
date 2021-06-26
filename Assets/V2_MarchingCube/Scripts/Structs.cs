
using UnityEngine;

namespace V2
{
    public struct NoisePoint
    {
        public Vector3 position;
        public float noiseValue;

        public NoisePoint(Vector3 position, float noiseValue)
        {
            this.position = position;
            this.noiseValue = noiseValue;
        }
    }

    public struct Triangle
    {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;
    }

    public struct IndexStruct
    {
        public int x,y,z;
        public int index;

        public override string ToString()
        {
            return $"[{x}, {y}, {z}], {index}";
        }
    }
}