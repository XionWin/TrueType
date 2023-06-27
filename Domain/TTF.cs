using TrueType.Extension;
using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTF : IDisposable
    {
        public string Name { get; set; }
        public string Path { get; set; }


        private TTFAtlas _atlas = TTFAtlas.Instance;
        private TTFRaw Raw { get; init; }

        public TTF(string name, string path)
        {
            Name = name;
            Path = path;

            this.Raw = new TTFRaw(name, File.ReadAllBytes(path));

            var lineGap = 0;
            var vMetrics = this.Raw.GetFontVMetrics();
            var fontHeight = vMetrics.ascent - vMetrics.descent;
            var fontascender = (float)vMetrics.ascent / fontHeight;
            var fontdescender = (float)vMetrics.descent / fontHeight;
            var fontLineHeight = (float)(fontHeight + lineGap) / fontHeight;

        }

        public TTFGlyph GetGlyph(char character, int size, int blur, char? pervious)
        {
            var index = new TTFIndex(character, size, blur);

            return TTFAtlas.Instance.GetGlyph(index, this.Raw);
        }

        public void Dispose()
        {

        }
    }
}