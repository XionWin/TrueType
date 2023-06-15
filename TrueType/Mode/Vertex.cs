namespace TrueType.Mode
{
    public enum VertexType
    {
        MoveTo = 1,
        LineTo,
        CurveTo
    }

    public struct Vertex
    {
        public short CenterX { get; set; }
        public short CenterY { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public VertexType Type { get; set; }
        public byte Padding { get; set; }
    }
}
