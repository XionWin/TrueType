using TrueType.Extension;
using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTFAtlas: Dictionary<TTFIndex, TTFGlyph>
    {
        private TTFAtlas() { }

        private static TTFAtlas _Instance = new TTFAtlas();
        public static TTFAtlas Instance = _Instance;

        public TTFGlyph GetGlyph(TTFIndex ttfIndex, TTFRaw raw, TTFVector vector)
        {
            var character = ttfIndex.Character;
            var size = ttfIndex.Size;
            var blur = ttfIndex.Blur;


            // Find code point and size.
            var h = TTFExtension.HashInt(character) & (Consts.FONS_HASH_LUT_SIZE - 1);

            if (size < 2)
                throw new Exception("Unsupported size");
            if (blur > 20)
                blur = 20;
            var pad = blur + 2;

            var scaleValue = raw.GetPixelHeightScale(size);
            var scale = new PointF(scaleValue, scaleValue);

            var shift = new PointF();

            var index = raw.GetGlyphIndex((int)character);
            var (advanceWidth, leftSideBearing, x0, y0, x1, y1) = raw.BuildGlyphBoxSettings(index, size, scale, shift);

            var renderSize = new Size(x1 - x0, y1 - y0);
            var glyphSize = new Size(renderSize.Width + pad * 2, renderSize.Height + pad * 2);

            // Location-related
            //AtlasAddRect(Atlas.Instance, this._raw, glyphSize);

            var xadv = (short)(scaleValue * advanceWidth * 10.0f);
            var off = new Point(x0, y0);

            var bitmap = vector.Rasterize(ttfIndex, renderSize, scale, shift, off);

            var glyph = new TTFGlyph()
            {
                Index = index,
                Scale = scaleValue,
                Shift = shift,
                AdvanceWidth = advanceWidth,
                LeftSideBearing = leftSideBearing,
                XAdvanceWidth = xadv,
                Rect = new Rect(x0, y0, x1 - x0, y1 - y0),
                Offset = off,
                Bitmap = bitmap,
            };
            return glyph;
        }
        
    }


}
