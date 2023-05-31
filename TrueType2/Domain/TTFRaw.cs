using TrueType2.Domain.Support;
using TrueType2.Extension;

namespace TrueType2.Domain
{
    public class TTFRaw
    {
        public string Name { get; set; }

        private byte[] _raw;
        public byte[] Raw => _raw;
        public ReadOnlySpan<byte> Span => _raw;

        /// <summary>
        /// Offset of start of font
        /// </summary>
        public int FontStart { get; set; }
        public int IndexMap { get; set; }
        public int IndexLocFormat { get; set; }

        public int GlyphCount { get; set; }

        public TTFRawTable _rawTables;
        public TTFRawTable Table => _rawTables;
        public TTFRaw(string name, byte[] raw)
        {
            Name = name;
            _raw = raw;
            _rawTables = new TTFRawTable(this.LoadTables());

            var (indexMap, indexLocFormat) = this.LoadCMap();
            this.IndexMap = indexMap;
            this.IndexLocFormat = indexLocFormat;
            this.GlyphCount = this.LoadTableValue<ushort>(this.Table.Maxp, TTFDefine.TABLE_MAXP_GLYPHS_OFFSET);


            var lineGap = 0;
            var vMetrics = this.GetFontVMetrics();
            var fontHeight = vMetrics.ascent - vMetrics.descent;
            var fontascender = (float)vMetrics.ascent / fontHeight;
            var fontdescender = (float)vMetrics.descent / fontHeight;
            var fontLineHeight = (float)(fontHeight + lineGap) / fontHeight;


        }
    }

}
