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
        public RectF Rectangle { get; init; }

        public TTFBitmap(MonoCanvas canvas, RectF rectangle)
        {
            Canvas = canvas;
            Rectangle = rectangle;
        }
    }
}
