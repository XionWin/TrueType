﻿using TrueType2.Mode;

namespace TrueType2.Domain
{
    public class TTFGlyph
    {
        public int Index { get; set; }
        public int Size { get; set; }
        public int Blur { get; set; }
        public float Scale { get; set; }
        public PointF Shift { get; set; }
        public int AdvanceWidth { get; set; }
        public int LeftSideBearing { get; set; }
        public Rect Rect { get; set; }
        public Point Offset { get; set; }
        public required TTFBitmap Bitmap { get; set; }
    }
}
