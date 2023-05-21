namespace TrueType;
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

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

public struct PointF
{
    public float X { get; set; }
    public float Y { get; set; }

    public PointF(float x, float y)
    {
        this.X = x;
        this.Y = y;
    }
}

public struct Size
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Size(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }
}

public struct Edge
{
    public PointF P0 { get; set; }
    public PointF P1 { get; set; }

    public bool IsInvented { get; set; }

    public Edge(PointF p0, PointF p1, bool IsInvent)
    {
        this.P0 = p0;
        this.P1 = p1;
        this.IsInvented = IsInvent;
    }
}