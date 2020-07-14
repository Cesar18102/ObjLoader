using OpenTK;

namespace ObjScreener.Data
{
    public class Box
    {
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }
        public Vector3 Center { get; private set; }

        public Box(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;

            Center = (Min + Max) / 2;
        }
    }
}
