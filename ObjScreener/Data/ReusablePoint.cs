using OpenTK;

namespace ObjScreener.Data
{
    public struct ReusablePoint
    {
        public Vector3 Vertex { get; private set; }
        public Vector3 Normal { get; private set; }
        public Vector2 TexCoord { get; private set; }

        public ReusablePoint(Vector3 vertex, Vector3 normal, Vector2 texCoord)
        {
            Vertex = vertex;
            Normal = normal;
            TexCoord = texCoord;
        }
    }
}
