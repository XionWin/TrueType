using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Extension;
namespace TrueType
{
    public class TTFRawTable
    {
        public int Cmap { get; set; }
        public int Glyf { get; set; }
        public int Head { get; set; }
        public int Hhea { get; set; }
        public int Hmtx { get; set; }
        public int Loca { get; set; }
        public int Name { get; set; }
    }

    public class TTFRaw
    {
        public string Name { get; set; }

        private byte[] _raw;
        public byte[] Raw => this._raw;
        public ReadOnlySpan<byte> Span => this._raw;

        /// <summary>
        /// Offset of start of font
        /// </summary>
        public int Offset { get; set; }
        public int IndexMap { get; set; }
        public int IndexLocFormat { get; set; }

        public int GlyphCount { get; set; }

        public Dictionary<string, uint> _rawTables;
        public TTFRaw(string name, byte[] raw)
        {
            this.Name = name;
            this._raw = raw;
            this._rawTables = this.LoadTables();
            this.Table = new TTFRawTable()
            {
                Cmap = (int)this._rawTables["cmap"],
                Glyf = (int)this._rawTables["glyf"],
                Head = (int)this._rawTables["head"],
                Hhea = (int)this._rawTables["hhea"],
                Hmtx = (int)this._rawTables["hmtx"],
                Loca = (int)this._rawTables["loca"],
                Name = (int)this._rawTables["name"],
            };
            var (indexMap, indexLocFormat) = this.LoadCMap();
            this.IndexMap = indexMap;
            this.IndexLocFormat = indexLocFormat;
            this.GlyphCount = this.LoadTableValue<ushort>("maxp", TTFC.TABLE_MAXP_GLYPHS_OFFSET);

            //var cmapTables = this.LoadTableValue<ushort>("cmap", TTFC.TABLE_CMAP_TABLES_OFFSET);
            var lineGap = 0;
            var vMetrics = this.GetFontVMetrics();
            var fontHeight = vMetrics.ascent - vMetrics.descent;
            var fontascender = (float)vMetrics.ascent / fontHeight;
            var fontdescender = (float)vMetrics.descent / fontHeight;
            var fontLineHeight = (float)(fontHeight + lineGap) / fontHeight;
            
            foreach(var c in "É")
            {
                var vertices = this.GetGlyph((uint)c, 18, 0);
                
            }
        }


        public TTFRawTable Table { get; init; }

        public T GetNumber<T>(int position) where T : struct, INumber<T> => this.Span.GetNumber<T>(position);
    }

    internal static class TrueTypeFontInfoExtension
    {
        internal static T GetNumber<T>(this ReadOnlySpan<byte> data, int position)
            where T : struct, INumber<T>
        {
            
            var span = data.Slice(position, Marshal.SizeOf<T>()).ToArray().AsSpan();
            span.Reverse();
            return MemoryMarshal.Read<T>(span);
        }

        internal static Dictionary<string, uint> LoadTables(this TTFRaw raw)
        {
            var span = raw.Span;
            var tableCount = span.GetNumber<ushort>(raw.Offset + TTFC.TABLE_COUNT_OFFSET);
            var tableDir = raw.Offset + TTFC.TABLE_DIR_OFFSET;

            var result = new Dictionary<string, uint>();
            for (int i = 0; i < tableCount; i++)
            {
                var location = tableDir + TTFC.TABLE_DIR_STEP_LEN * i;
                var nameData = span.Slice(location, TTFC.TABLE_DIR_NAME_LEN);
                result.Add(Encoding.Default.GetString(nameData), span.GetNumber<uint>(location + TTFC.TABLE_DIR_DATA_OFFSET));
            }
            return result;
        }

        internal static T LoadTableValue<T>(this TTFRaw raw, string tableName, int propOffset)
            where T : struct, INumber<T>
        {
            var span = raw.Span;
            var tablePos = raw._rawTables[tableName];
            var propValue = span.GetNumber<T>((int)tablePos + propOffset);
            return propValue;
        }


        internal static (int indexMap, int indexLocFormat) LoadCMap(this TTFRaw raw)
        {
            var span = raw.Span;
            var cmap = raw.Table.Cmap;
            var cmapTables = raw.LoadTableValue<ushort>("cmap", TTFC.TABLE_CMAP_TABLES_OFFSET);

            var encoding_record = Enumerable.Range(0, cmapTables).Select(x => (int)(cmap + 4 + 8 * x)).First(x =>
                raw.Span is var span && (STBTT_PLATFORM_ID)span.GetNumber<ushort>(x) == STBTT_PLATFORM_ID.STBTT_PLATFORM_ID_MICROSOFT ?
                        new[] { STBTT_PLATFORM_ID_MICROSOFT.STBTT_MS_EID_UNICODE_FULL, STBTT_PLATFORM_ID_MICROSOFT.STBTT_MS_EID_UNICODE_BMP }.Contains((STBTT_PLATFORM_ID_MICROSOFT)span.GetNumber<ushort>(x + 2)) :
                        false
            );

            var indexMap = cmap + span.GetNumber<uint>(encoding_record + 4);
            var indexLocFormat = span.GetNumber<ushort>((int)raw.Table.Head + 50);
            return ((int)indexMap, indexLocFormat);
        }

        internal static (int ascent, int descent, int lineGap) GetFontVMetrics(this TTFRaw raw)
        {
            var span = raw.Span;
            var hhea = raw.Table.Hhea;

            return (raw.GetNumber<short>((int)hhea + 4), raw.GetNumber<short>((int)hhea + 6), raw.GetNumber<short>((int)hhea + 8));
        }

        public const uint FONS_HASH_LUT_SIZE = 256;
        internal static Vertex[]? GetGlyph(this TTFRaw raw, uint code, short size, short blur)
        {

            // Find code point and size.
            var h = HashInt(code) & (FONS_HASH_LUT_SIZE - 1);

            if (size < 2)
                throw new Exception("Unsupported size");
            if (blur > 20)
                blur = 20;
            var pad = blur + 2;

            var scale = raw.GetPixelHeightScale(size);
        
            var index = raw.GetGlyphIndex((int)code);
            var(advanceWidth, leftSideBearing, x0, y0, x1, y1) = raw.BuildGlyphBitmap(index, size, scale);

            var renderWidth = x1 - x0;
            var renderHeight = y1 - y0;
            var glyphWidth = renderWidth + pad * 2;
            var glyphHeight = renderHeight + pad * 2;

            AtlasAddRect(Atlas.Instance, raw, glyphWidth, glyphHeight);

            var vertices = raw.GetGlyphShape(index);

            return vertices;
        }

        private static void Rasterize(this TTFRaw raw, Vertex[] vertex, int width, int height)
        {

        }

        private static void AtlasAddRect(this Atlas atlas, TTFRaw raw, int gw, int gh)
        {
            atlas.FitAtlas(gw, gh);
        }

        //private static (int x, int y) atlasAddRectBestPos(this TTFAtlas atlas, TTFRaw raw, int gw, int gh)
        //{
        //    int besth = atlas.Height, bestw = atlas.Width, besti = -1;
        //    int bestx = -1, besty = -1, i;

        //    // Bottom left fit heuristic.
        //    for (i = 0; i < atlas.nnodes; i++)
        //    {
        //        int y = GetAtlasRectFits(atlas, i, rw, rh);
        //        if (y != -1)
        //        {
        //            short nw = atlas.nodes[i].width;
        //            if (y + rh < besth || (y + rh == besth && nw < bestw))
        //            {
        //                besti = i;
        //                bestw = atlas.nodes[i].width;
        //                besth = y + rh;
        //                bestx = atlas.nodes[i].x;
        //                besty = y;
        //            }
        //        }
        //    }

        //    if (besti == -1)
        //        throw new Exception("Index error");

        //    // Perform the actual packing.
        //    if (fons__atlasAddSkylineLevel(ref atlas, besti, bestx, besty, rw, rh) == 0)
        //        throw new Exception("AddSkylineLevel error");


        //    return (bestx, besty);
        //}

        //static int GetAtlasRectFits(this TTFAtlas atlas, AtlasNode node, TTFRaw raw, int gw, int gh)
        //{
        //    // Checks if there is enough space at the location of skyline span 'i',
        //    // and return the max height of all skyline spans under that at that location,
        //    // (think tetris block being dropped at that position). Or -1 if no space found.
        //    int x = node.X;
        //    int y = node.Y;
        //    int spaceLeft;
        //    if (x + gw > atlas.Width)
        //        return -1;
        //    while (spaceLeft > 0)
        //    {
        //        if (i == atlas.Nodes.Count)
        //            return -1;
        //        y = Math.Max(y, node.Y);
        //        if (y + gh > atlas.Height)
        //            return -1;
        //        spaceLeft -= node.Width;
        //        ++i;
        //    }
        //    return y;
        //}

        private static float GetPixelHeightScale(this TTFRaw raw, float height)
        {
            int fheight = raw.GetNumber<short>(raw.Table.Hhea + 4) - raw.GetNumber<short>(raw.Table.Hhea + 6);
            return (float)height / fheight;
        }

        private static uint HashInt(uint data)
        {
            uint a = data;
            a += ~(a << 15);
            a ^= (a >> 10);
            a += (a << 3);
            a ^= (a >> 6);
            a += ~(a << 11);
            a ^= (a >> 16);
            return a;
        }


        internal static void GetGlyphShape(this TTFRaw raw)
        {

        }

    }
}
