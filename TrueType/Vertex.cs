namespace TrueType;
public enum MoveType
{
    STBTT_vmove = 1,
    STBTT_vline,
    STBTT_vcurve
}

public struct Vertex
{
    public short X { get; set; }
    public short Y { get; set; }
    public short CX { get; set; }
    public short CY { get; set; }
    public MoveType Type { get; set; }
    public byte Padding { get; set; }
}