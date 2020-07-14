using System.Linq;
using System.Collections.Generic;

using OpenTK;

namespace ObjScreener.Data
{
    public class Geometry
    {
        public ReusablePoint[] Points { get; }
        public uint[] Indexes { get; }

        private Box boundingBox = null;
        public Box BoundingBox
        {
            get
            {
                if (boundingBox == null)
                    boundingBox = ComputeBoundingBox();
                return boundingBox;
            }
        }

        public Geometry(IEnumerable<ReusablePoint> points, IEnumerable<uint> indexes) 
        {
            Points = points.ToArray();
            Indexes = indexes.ToArray();
        }

        private Box ComputeBoundingBox()
        {
            Vector3 min = new Vector3(
                Points.Min(point => point.Vertex.X),
                Points.Min(point => point.Vertex.Y),
                Points.Min(point => point.Vertex.Z)
            );

            Vector3 max = new Vector3(
                Points.Max(point => point.Vertex.X),
                Points.Max(point => point.Vertex.Y),
                Points.Max(point => point.Vertex.Z)
            );

            return new Box(min, max);
        }
    }
}
