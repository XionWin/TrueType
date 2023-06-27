using TrueType.Extension;
using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTFRaw
    {
        public string Name { get; set; }

        private byte[] _data;
        public byte[] Data => _data;
        public ReadOnlySpan<byte> Span => _data;

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
            _data = raw;
            _rawTables = new TTFRawTable(this.LoadTables());

            var (indexMap, indexLocFormat) = this.LoadCMap();
            this.IndexMap = indexMap;
            this.IndexLocFormat = indexLocFormat;
            this.GlyphCount = this.GetNumber<ushort>(this.Table.Maxp + Consts.TABLE_MAXP_GLYPHS_OFFSET);

        }
    }

}
