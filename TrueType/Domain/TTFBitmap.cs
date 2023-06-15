using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTFBitmap
    {
        public MonoCanvas Canvas { get; init; }
        public Rect TexRect { get; init; }

        public TTFBitmap(MonoCanvas canvas, Rect texRect)
        {
            Canvas = canvas;
            TexRect = texRect;
        }
    }
}
