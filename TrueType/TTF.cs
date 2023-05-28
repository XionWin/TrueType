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

            var scaleValue = raw.GetPixelHeightScale(size);
            var scale = new PointF(scaleValue, scaleValue);

            var shift = new PointF();
        
            var index = raw.GetGlyphIndex((int)code);
            var(advanceWidth, leftSideBearing, x0, y0, x1, y1) = raw.BuildGlyphBitmap(index, size, scale, shift);

            var renderSize = new Size(x1 - x0, y1 - y0);
            var glyphSize = new Size(renderSize.Width + pad * 2, renderSize.Height + pad * 2);

            var off = new Point(x0, y0);

            AtlasAddRect(Atlas.Instance, raw, glyphSize);

            var vertices = raw.GetGlyphShape(index);

            raw.Rasterize(vertices!, renderSize, scale, shift, off);
            return vertices;
        }

        private static void Rasterize(this TTFRaw raw, Vertex[] vertices, Size renderSize, PointF scale, PointF shift, Point off)
        {
            var flatness_in_pixels = 0.35f;
            int vsubsample = renderSize.Height < 8 ? 15 : 5;
            var windings = vertices.FlattenCurves(flatness_in_pixels / Math.Min(scale.X, scale.Y));

            var data = System.Text.Json.JsonSerializer.Serialize(windings);

            var edges = windings!.stbtt__rasterize(vsubsample, scale, shift, off, true);

            edges!.stbtt__rasterize_sorted_edges(renderSize, vsubsample, off);
        }

        static void stbtt__rasterize_sorted_edges(this Edge[] edges, Size renderSize, int vsubsample, Point off)
        {
            var pixels = new byte[230400];

            var activeIsNext = new ActiveEdge();
            
            // int y, j = 0, eIndex = 0;
            int max_weight = (255 / vsubsample);        // weight per vertical scanline
            byte[] scanline_data = new byte[480], scanline;

            if (renderSize.Width > 480)
            {
                // scanline = (byte[])userdata;
                // Array.Resize(ref scanline, result.w);
                throw new NotImplementedException();
            }
            else
                scanline = scanline_data;

            var y = off.Y * vsubsample;

            edges = edges.Append(new Edge(new PointF(0, (off.Y + renderSize.Height) * (float)vsubsample + 1), new PointF(), false)).ToArray();

            var j = 0;
            var eIndex = 0;

            while (j < renderSize.Height)
            {
                Array.Clear(scanline, 0, scanline.Length);
                // vertical subsample index
                for (var s = 0; s < vsubsample; ++s)
                {
                    // find center of pixel for this scanline
                    float scan_y = y + 0.5f;
                    ActiveEdge stepIsNext = activeIsNext;

                    // update all active edges;
                    // remove all active edges that terminate before the center of this scanline
                    while (stepIsNext.Next is not null)
                    {
                        var z = stepIsNext.Next;
                        if (z.EY <= scan_y)
                        {
                            stepIsNext.Next = z.Next; // delete from list
                                                      //STBTT_assert(z->valid);
                            z.Valid = 0;
                            //STBTT_free(z, userdata);
                        }
                        else
                        {
                            z.X += z.DX; // advance to position for current scanline
                            stepIsNext = stepIsNext.Next; // advance through list
                        }
                    }

                    for (; ; )
                    {
                        bool changed = false;
                        stepIsNext = activeIsNext;
                        while (stepIsNext.Next != null && stepIsNext.Next.Next != null)
                        {
                            if (stepIsNext.Next.X > stepIsNext.Next.Next.X)
                            {
                                ActiveEdge t = stepIsNext.Next;
                                ActiveEdge q = t.Next;

                                t.Next = q.Next;
                                q.Next = t;
                                stepIsNext.Next = q;
                                changed = true;
                            }
                            stepIsNext = stepIsNext.Next;
                        }
                        if (!changed)
                            break;
                    }

                    // insert all edges that start before the center of this scanline -- omit ones that also end on this scanline
                    while (edges[eIndex].P0.Y <= scan_y)
                    {
                        if (edges[eIndex].P1.Y > scan_y)
                        {
                            ActiveEdge z = edges[eIndex].new_active(off.X, scan_y);
                             // find insertion point
                            if (activeIsNext.Next == null)
                                activeIsNext.Next = z;
                            else if (z.X < activeIsNext.Next.X)
                            {
                                // insert at front
                                z.Next = activeIsNext.Next;
                                activeIsNext.Next = z;
                            }
                            else
                            {
                                // find thing to insert AFTER
                                var p = activeIsNext.Next;
                                while (p.Next != null && p.Next.X < z.X)
                                    p = p.Next;
                                // at this point, p->next->x is NOT < z->x
                                z.Next = p.Next;
                                p.Next = z;
                            }
                        }
                        ++eIndex;
                    }

                    // now process all active edges in XOR fashion
                    if (activeIsNext.Next != null)
                        stbtt__fill_active_edges(scanline, renderSize.Width, activeIsNext.Next, max_weight);

                    ++y;
                }

                Array.Copy(scanline, 0, pixels, 240 + 0 * 480 + (j * 480), renderSize.Width);

                ++j;

            }

            System.IO.File.Delete(@"raw.dat");
            using (var fileStream = new System.IO.FileStream(@"raw.dat", FileMode.CreateNew, FileAccess.Write))
            {
                using (var writer = new System.IO.BinaryWriter(fileStream))
                {
                    writer.Write(pixels);
                }
            }

        }

        static void stbtt__fill_active_edges(byte[] scanline, int len, ActiveEdge? edge, int max_weight)
        {
            byte ab = 0;
            // non-zero winding fill
            int x0 = 0, w = 0;

            while (edge != null)
            {
                if (w == 0)
                {
                    // if we're currently at zero, we need to record the edge start point
                    x0 = edge.X;
                    w += edge.Valid;
                }
                else
                {
                    int x1 = edge.X;
                    w += edge.Valid;
                    // if we went to zero, we need to draw
                    if (w == 0)
                    {
                        int i = x0 >> FIXSHIFT;
                        int j = x1 >> FIXSHIFT;

                        if (i < len && j >= 0)
                        {
                            if (i == j)
                            {
                                // x0,x1 are the same pixel, so compute combined coverage
                                ab = (byte)(scanline[i] + (byte)((x1 - x0) * max_weight >> FIXSHIFT));
                                scanline[i] = ab;
                            }
                            else
                            {
                                if (i >= 0) // add antialiasing for x0
                                {
                                    ab = (byte)(scanline[i] + (byte)(((FIX - (x0 & FIXMASK)) * max_weight) >> FIXSHIFT));
                                    scanline[i] = ab;
                                }
                                else
                                    i = -1; // clip

                                if (j < len) // add antialiasing for x1
                                {
                                    ab = (byte)(scanline[j] + (byte)(((x1 & FIXMASK) * max_weight) >> FIXSHIFT));
                                    scanline[j] = ab;
                                }
                                else
                                    j = len; // clip

                                for (++i; i < j; ++i) // fill pixels between x0 and x1
                                {
                                    ab = (byte)(scanline[i] + (byte)max_weight);
                                    scanline[i] = ab;
                                }
                            }
                        }
                    }
                }
                edge = edge.Next;
            }
        }

        const int FIXSHIFT = 10;
        const int FIX = (1 << FIXSHIFT);
        const int FIXMASK = (FIX - 1);
        static ActiveEdge new_active(this Edge edge, int off_x, float start_point)
        {
            //stbtt__active_edge z = (stbtt__active_edge *) STBTT_malloc(sizeof(*z), userdata); // @TODO: make a pool of these!!!
            var z = new ActiveEdge();
            float dxdy = (edge.P1.X - edge.P0.X) / (edge.P1.Y - edge.P0.Y);
            //STBTT_assert(e->y0 <= start_point);
            if (z == null)
                return z;
            // round dx down to avoid going too far
            if (dxdy < 0)
                z.DX = -(int)Math.Floor(FIX * -dxdy);
            else
                z.DX = (int)Math.Floor(FIX * dxdy);
            z.X = (int)Math.Floor(FIX * (edge.P0.X + dxdy * (start_point - edge.P0.Y)));
            z.X -= off_x * FIX;
            z.EY = edge.P1.Y;
            z.Next = null;
            z.Valid = edge.IsInvented ? 1 : -1;
            return z;
        }

        static Edge[]? stbtt__rasterize(this PointF[][] windings, int vsubsample, PointF scale, PointF shift, Point off, bool isInvented)
        {
            float y_scale_inv = isInvented ? -scale.Y : scale.Y;
            // vsubsample should divide 255 evenly; otherwise we won't reach full opacity
            
            //e = (stbtt__edge) STBTT_malloc(sizeof(*e) * (n+1), userdata); // add an extra one as a sentinel
            var edges = new List<Edge>();

            for (int i = 0; i < windings.Length; i++)
            {
                var points = windings[i];

                for (int j = 1; j < points.Length; j++)
                {
                    var p0 = points[j];
                    var p1 = points[j - 1];

                    // skip the edge if horizontal
                    if (p0.Y == p1.Y)
                        continue;

                    var inventedFlag = false;

                    if(isInvented)
                    {
                        if(p1.Y > p0.Y)
                        {
                            inventedFlag = true;
                            var temp = p0;
                            p0 = p1;
                            p1 = temp;
                        }
                    }
                    else
                    {
                        if(p0.Y < p1.Y)
                        {
                            inventedFlag = true;
                            var temp = p0;
                            p0 = p1;
                            p1 = temp;
                        }
                    }

                    var edge = new Edge();
                    edge.P0 = new PointF(p0.X * scale.X + shift.X,
                                    p0.Y * y_scale_inv * vsubsample + shift.Y);
                    edge.P1 = new PointF(p1.X * scale.X + shift.X, 
                                    p1.Y * y_scale_inv * vsubsample + shift.Y);
                    edge.IsInvented = inventedFlag;
                    edges.Add(edge);
                }
            }
            edges.Sort();

            // // now, traverse the scanlines and find the intersections on each scanline, use xor winding rule
            // stbtt__rasterize_sorted_edges(ref result, e, n, vsubsample, off_x, off_y, userdata);

            //STBTT_free(e, userdata);

            return edges.ToArray();
        }

        private static PointF[][]? FlattenCurves(this Vertex[] vertices, float objspace_flatness)
        {
            var objspace_flatness_pow_2 = (float)Math.Pow(objspace_flatness, 2);

            var pointsList = new List<PointF[]>();
            List<PointF>? currentPoints = null;

            float x = 0, y = 0;
            for(var i = 0; i < vertices.Length; ++i)
            {
                switch (vertices[i].Type)
                {
                    case VertexType.MoveTo:
                        // start the next contour
                        if(currentPoints is not null)
                        {
                            pointsList.Add(currentPoints.ToArray());
                        }
                        currentPoints = new List<PointF>();

                        x = vertices[i].X;
                        y = vertices[i].Y;
                        currentPoints.Add(new PointF(x, y));
                        break;
                    case VertexType.LineTo:
                        x = vertices[i].X;
                        y = vertices[i].Y;
                        currentPoints!.Add(new PointF(x, y));
                        break;
                    case VertexType.CurveTo:
                        currentPoints!.stbtt__tesselate_curve(x, y,
                            vertices[i].CenterX, vertices[i].CenterY,
                            vertices[i].X, vertices[i].Y, objspace_flatness_pow_2);
                        x = vertices[i].X;
                        y = vertices[i].Y;
                        break;
                }
            }
            if(currentPoints is not null)
            {
                pointsList.Add(currentPoints.ToArray());
            }
            return pointsList.ToArray();
        }

        static int stbtt__tesselate_curve(this List<PointF> points, float x0, float y0, float x1, float y1, float x2, float y2, float objspace_flatness_squared, int n = 0)
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
            { 
                // half-pixel error allowed... need to be smaller if AA
                stbtt__tesselate_curve(points, x0, y0, (x0 + x1) / 2.0f, (y0 + y1) / 2.0f, mx, my, objspace_flatness_squared, n + 1);
                stbtt__tesselate_curve(points, mx, my, (x1 + x2) / 2.0f, (y1 + y2) / 2.0f, x2, y2, objspace_flatness_squared, n + 1);
            }
            else
            {
                points.Add(new PointF(x2, y2));
            }
            return 1;
        }


        private static void AtlasAddRect(this Atlas atlas, TTFRaw raw, Size glyphSize)
        {
            atlas.FitAtlas(glyphSize);
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
