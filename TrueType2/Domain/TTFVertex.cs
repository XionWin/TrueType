namespace TrueType2.Domain
{
    public enum VertexType
    {
        MoveTo = 1,
        LineTo,
        CurveTo
    }

    public struct TTFVertex
    {
        public short CenterX { get; set; }
        public short CenterY { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public VertexType Type { get; set; }
        public byte Padding { get; set; }
    }
}
