using TrueType.Domain.Cache.Vector;
using TrueType.Extension;
using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTF : IDisposable
    {
        public string Name { get; set; }
        public string Path { get; set; }

        private Cache.Vector.Cache _vectorCache = Cache.Vector.Cache.Instance;
        private TTFAtlas _atlas = TTFAtlas.Instance;
        private TTFRaw _raw;

        public TTF(string name, string path)
        {
            Name = name;
            Path = path;

            if (_vectorCache.ContainsKey(name) is false)
                _vectorCache.Add(name, new FontCache(new TTFRaw(name, File.ReadAllBytes(path))));


            this._raw = _vectorCache[name].Raw is TTFRaw ttfRaw ? ttfRaw : throw new ArgumentException();

            var lineGap = 0;
            var vMetrics = this._raw.GetFontVMetrics();
            var fontHeight = vMetrics.ascent - vMetrics.descent;
            var fontascender = (float)vMetrics.ascent / fontHeight;
            var fontdescender = (float)vMetrics.descent / fontHeight;
            var fontLineHeight = (float)(fontHeight + lineGap) / fontHeight;

        }

        public TTFGlyph GetGlyph(char character, int size, int blur, char? pervious)
        {
            var index = new TTFIndex(character, size, blur);
            var vector = this._vectorCache[this.Name].TryGet(character);

            return TTFAtlas.Instance.GetGlyph(index, this._raw, vector);
        }

        public void Dispose()
        {
            this._vectorCache.Clear();
        }
    }
}