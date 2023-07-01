using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrueType.Domain;
using TrueType.Mode;

namespace TrueType.Extension
{
    internal static class TTFGlyphExtension
    {
        internal static (Point, RectF) GetQuad(this TTFGlyph glyph, float adv,
                                  float scale, float spacing, FONSflags flags)
        {
            int x = 0, y = 0;
            float rx, ry, xoff, yoff, x0, y0, x1, y1;

            if (adv != 0)
            {
                x += (int)(adv + spacing + 0.5f);
            }

            // Each glyph has 2px border to allow good interpolation,
            // one pixel to prevent leaking, and one to allow good interpolation for rendering.
            // Inset the texture region by one pixel for corret interpolation.
            xoff = (short)(glyph.Offset.X + 1);
            yoff = (short)(glyph.Offset.Y + 1);
            x0 = (float)glyph.Offset.X;
            y0 = (float)glyph.Offset.Y;
            x1 = (float)(glyph.Offset.X + glyph.Size.Width);
            y1 = (float)(glyph.Offset.Y + glyph.Size.Height);


            float rx0, ry0, rx1, ry1;

            if (flags == FONSflags.FONS_ZERO_TOPLEFT)
            {
                rx = (float)(int)(x + xoff);
                ry = (float)(int)(y + yoff);

                rx0 = rx;
                ry0 = ry;
                rx1 = rx + x1 - x0;
                ry1 = ry + y1 - y0;

            }
            else
            {
                rx = (float)(int)(x + xoff);
                ry = (float)(int)(y - yoff);

                rx0 = rx;
                ry0 = ry;
                rx1 = rx + x1 - x0;
                ry1 = ry - y1 + y0;

            }

            x += (int)(glyph.XAdvanceWidth / 10.0f + 0.5f);

            return (new Point(x, y), new RectF(rx0, ry0, rx1 - rx0, ry1 - ry0));
        }
    }
}
