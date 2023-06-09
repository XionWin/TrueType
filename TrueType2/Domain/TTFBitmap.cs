using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrueType2.Mode;

namespace TrueType2.Domain
{
    public class TTFBitmap
    {
        public MonoCanvas Canvas { get; init; }
        public Rect Rectangle { get; init; }

        public Point Offset { get; init; }

        public TTFBitmap(MonoCanvas canvas, Rect rectangle, Point offset)
        {
            Canvas = canvas;
            Rectangle = rectangle;
            Offset = offset;
        }
    }
}
