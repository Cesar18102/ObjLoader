using System.Drawing;

namespace ObjScreener.Data
{
    public class Mesh
    {
        public Geometry Geometry { get; private set; }
        public Bitmap Texture { get; private set; }

        public Mesh(Geometry geometry, Bitmap texture)
        {
            Geometry = geometry;
            Texture = texture;
        }
    }
}
