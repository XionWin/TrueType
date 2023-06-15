using TrueType.Domain.Cache.Pixel;
using TrueType.Domain.Cache.Vector;
using TrueType.Extension;
using TrueType.Mode;
using static System.Formats.Asn1.AsnWriter;

namespace TrueType.Domain
{
    public class TTF : IDisposable
    {
        public string Name { get; set; } 
        public string Path { get; set; }

        private Cache.Vector.Cache _cache = Cache.Vector.Cache.Instance;
        private TTFRaw _raw;

        public TTF(string name, string path)
        {
            Name = name;
            Path = path;

            if (_cache.ContainsKey(name) is false)
                _cache.Add(name, new FontCache(new TTFRaw(name, File.ReadAllBytes(path))));


            if (BitmapCache.Instance.ContainsKey(name) is false)
                BitmapCache.Instance.Add(name, new FontBitmapCache(name));

            this._raw = _cache[name].Raw is TTFRaw ttfRaw ? ttfRaw : throw new ArgumentException();

            var lineGap = 0;
            var vMetrics = this._raw.GetFontVMetrics();
            var fontHeight = vMetrics.ascent - vMetrics.descent;
            var fontascender = (float)vMetrics.ascent / fontHeight;
            var fontdescender = (float)vMetrics.descent / fontHeight;
            var fontLineHeight = (float)(fontHeight + lineGap) / fontHeight;

        }

        public TTFGlyph GetGlyph(char c, int size, int blur)
        {
            var vector = this._cache[this.Name].TryGet(c);
            var canvas = BitmapCache.Instance[this.Name].TryGet(size);


            // Find code point and size.
            var h = TTFExtension.HashInt(c) & (Consts.FONS_HASH_LUT_SIZE - 1);

            if (size < 2)
                throw new Exception("Unsupported size");
            if (blur > 20)
                blur = 20;
            var pad = blur + 2;

            var scaleValue = this._raw.GetPixelHeightScale(size);
            var scale = new PointF(scaleValue, scaleValue);

            var shift = new PointF();

            var index = this._raw.GetGlyphIndex((int)c);
            var (advanceWidth, leftSideBearing, x0, y0, x1, y1) = this._raw.BuildGlyphBoxSettings(index, size, scale, shift);

            var renderSize = new Size(x1 - x0, y1 - y0);
            var glyphSize = new Size(renderSize.Width + pad * 2, renderSize.Height + pad * 2);


            // Location-related
            //AtlasAddRect(Atlas.Instance, this._raw, glyphSize);

            var xadv = (short)(scaleValue * advanceWidth * 10.0f);
            var off = new Point(x0, y0);


            var bitmap = vector.Rasterize(canvas, renderSize, scale, shift, off);

            var glyph = new TTFGlyph() {
                Index = index,
                Size = size,
                Blur = blur,
                Scale = scaleValue,
                Shift = shift,
                AdvanceWidth = advanceWidth,
                LeftSideBearing = leftSideBearing,
                Rect = new Rect(x0, y0, x1 - x0, y1 - y0),
                Offset = off,
                Bitmap = bitmap,
            };
            return glyph;
        }

        public void Dispose()
        {
            this._cache.Clear();
        }
    }
}