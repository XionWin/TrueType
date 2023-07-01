using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTFGlyph
    {
        public int Index { get; set; }
        public float Scale { get; set; }
        public int AdvanceWidth { get; set; }
        public int LeftSideBearing { get; set; }
        public float XAdvanceWidth { get; set; }
        public Size Size { get; set; }
        public Point Offset { get; set; }
        public required TTFBitmap Bitmap { get; set; }
    }
}
