using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTFGlyph
    {
        public char Character { get; set; }
        public int Size { get; set; }
        public int Blur { get; set; }
        public float Scale { get; set; }
        public PointF Shift { get; set; }
        public int AdvanceWidth { get; set; }
        public int LeftSideBearing { get; set; }
        public float XAdvanceWidth { get; set; }
        public Rect Rect { get; set; }
        public Point Offset { get; set; }

        public int KernAdvance { get; set; }
        public required TTFBitmap Bitmap { get; set; }
    }
}
