namespace ObjScreener.Data
{
    public struct ModelPoint
    {
        public uint VertexIndex { get; }
        public uint TexIndex { get; }
        public uint NormalIndex { get; }

        public ModelPoint(uint? vertexIndex, uint? texIndex, uint? normalIndex)
        {
            VertexIndex = vertexIndex.Value;
            TexIndex = texIndex.Value;
            NormalIndex = normalIndex.Value;
        }
    }
}
