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

            raw.Rasterize(vertices!, renderWidth, renderHeight, scale);
            return vertices;
        }

        private static void Rasterize(this TTFRaw raw, Vertex[] vertices, int width, int height, float scale)
        {
            var flatness_in_pixels = 0.35f;
            var windings = vertices.FlattenCurves(flatness_in_pixels / scale);
        }

        private static VertexPoint[]? FlattenCurves(this Vertex[] vertices, float objspace_flatness)
        {
            // count how many "moves" there are to get the contour count
            var n = vertices.Count(x => x.Type is VertexType.MoveTo);
            var num_contours = n;
            if(n == 0)
                return null;
            var contour_lengths = new int[n];
            
            VertexPoint[]? points = null;
            var num_points = 0;
            var start = 0;
            // make two passes through the points so we don't need to realloc
            for (var pass = 0; pass < 2; ++pass)
            {
                float x = 0, y = 0;
                if (pass == 1)
                {
                    //points = (stbtt__point *) STBTT_malloc(num_points * sizeof(points[0]), userdata);
                    if (num_points == 0)
                    {
                        contour_lengths = null;
                        num_contours = 0;

                        return null;
                    }
                    points = new VertexPoint[num_points];
                }
                num_points = 0;
                n = -1;
                for (var i = 0; i < vertices.Length; ++i)
                {
                    switch (vertices[i].Type)
                    {
                        case VertexType.MoveTo:
                            // start the next contour
                            if (n >= 0)
                                contour_lengths[n] = num_points - start;
                            ++n;
                            start = num_points;

                            x = vertices[i].X;
                            y = vertices[i].Y;
                            stbtt__add_point(points!, num_points++, x, y);
                            break;
                        case VertexType.LineTo:
                            x = vertices[i].X;
                            y = vertices[i].Y;
                            stbtt__add_point(points!, num_points++, x, y);
                            break;
                        case VertexType.CurveTo:
                            stbtt__tesselate_curve(points!, ref num_points, x, y,
                                vertices[i].CenterX, vertices[i].CenterY,
                                vertices[i].X, vertices[i].Y,
                                (float)Math.Pow(objspace_flatness, 2), 0);
                            x = vertices[i].X;
                            y = vertices[i].Y;
                            break;
                    }
                }
                contour_lengths[n] = num_points - start;
            }
            return points;
        }

        static int stbtt__tesselate_curve(VertexPoint[] points, ref int num_points, float x0, float y0, float x1, float y1, float x2, float y2, float objspace_flatness_squared, int n)
        {
            // midpoint
            float mx = (x0 + 2 * x1 + x2) / 4;
            float my = (y0 + 2 * y1 + y2) / 4;
            // versus directly drawn line
            float dx = (x0 + x2) / 2 - mx;
            float dy = (y0 + y2) / 2 - my;
            if (n > 16) // 65536 segments on one curve better be enough!
                return 1;
            if (dx * dx + dy * dy > objspace_flatness_squared)
            { // half-pixel error allowed... need to be smaller if AA
                stbtt__tesselate_curve(points, ref num_points, x0, y0, (x0 + x1) / 2.0f, (y0 + y1) / 2.0f, mx, my, objspace_flatness_squared, n + 1);
                stbtt__tesselate_curve(points, ref num_points, mx, my, (x1 + x2) / 2.0f, (y1 + y2) / 2.0f, x2, y2, objspace_flatness_squared, n + 1);
            }
            else
            {
                stbtt__add_point(points, num_points, x2, y2);
                num_points = num_points + 1;
            }
            return 1;
        }

        static void stbtt__add_point(VertexPoint[] points, int n, float x, float y)
        {
            if (points == null)
                return; // during first pass, it's unallocated
            points[n].X = x;
            points[n].Y = y;
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
