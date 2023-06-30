using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using TrueType.Domain;
using TrueType.Mode;

namespace TrueType.Extension
{
    public static class TTFRawExtension
    {
        internal static Dictionary<string, uint> LoadTables(this TTFRaw raw)
        {
            var tableCount = raw.GetNumber<ushort>(raw.FontStart + Consts.TABLE_COUNT_OFFSET);
            var tableDir = raw.FontStart + Consts.TABLE_DIR_OFFSET;

            var result = new Dictionary<string, uint>();
            for (int i = 0; i < tableCount; i++)
            {
                var location = tableDir + Consts.TABLE_DIR_STEP_LEN * i;
                var nameData = raw.Span.Slice(location, Consts.TABLE_DIR_NAME_LEN);
                result.Add(Encoding.Default.GetString(nameData), raw.GetNumber<uint>(location + Consts.TABLE_DIR_DATA_OFFSET));
            }
            return result;
        }

        internal static (int indexMap, int indexLocFormat) LoadCMap(this TTFRaw raw)
        {
            var cmapTables = raw.GetNumber<ushort>(raw.Table.Cmap + Consts.TABLE_CMAP_TABLES_OFFSET);

            var encoding_record = Enumerable.Range(0, cmapTables).Select(x => (int)(raw.Table.Cmap + 4 + 8 * x)).First(x =>
                (STBTT_PLATFORM_ID)raw.GetNumber<ushort>(x) == STBTT_PLATFORM_ID.STBTT_PLATFORM_ID_MICROSOFT ?
                        new[] { STBTT_PLATFORM_ID_MICROSOFT.STBTT_MS_EID_UNICODE_FULL, STBTT_PLATFORM_ID_MICROSOFT.STBTT_MS_EID_UNICODE_BMP }.Contains((STBTT_PLATFORM_ID_MICROSOFT)raw.GetNumber<ushort>(x + 2)) :
                        false
            );

            var indexMap = raw.Table.Cmap + raw.GetNumber<uint>(encoding_record + 4);
            var indexLocFormat = raw.GetNumber<ushort>(raw.Table.Head + 50);
            return ((int)indexMap, indexLocFormat);
        }

        internal static (int ascent, int descent, int lineGap) GetFontVMetrics(this TTFRaw raw)
        {
            return (raw.GetNumber<short>(raw.Table.Hhea + 4), raw.GetNumber<short>(raw.Table.Hhea + 6), raw.GetNumber<short>(raw.Table.Hhea + 8));
        }

        internal static int GetGlyphOffset(this TTFRaw raw, int index)
        {
            if (index >= raw.GlyphCount)
                throw new Exception("Glyph index out of range");
            if (raw.IndexLocFormat >= 2)
                throw new Exception("Unknown glyph map format");

            int g1, g2;
            if (raw.IndexLocFormat == 0)
            {
                g1 = raw.Table.Glyf + raw.GetNumber<ushort>(raw.Table.Loca + index * 2) * 2;
                g2 = raw.Table.Glyf + raw.GetNumber<ushort>(raw.Table.Loca + index * 2 + 2) * 2;
            }
            else
            {
                g1 = (int)(raw.Table.Glyf + raw.GetNumber<uint>(raw.Table.Loca + index * 4));
                g2 = (int)(raw.Table.Glyf + raw.GetNumber<uint>(raw.Table.Loca + index * 4 + 4));
            }
            return g1 == g2 ? -1 : g1; // if length is 0, return -1
        }

        internal static int GetGlyphIndex(this TTFRaw raw, int code)
        {
            var format = raw.GetNumber<ushort>(raw.IndexMap);
            if (format == 0)
            { // apple byte encoding
                int bytes = raw.GetNumber<ushort>(raw.IndexMap + 2);
                if (code < bytes - 6)
                    return raw.GetNumber<byte>(raw.IndexMap + 6 + code);
                return 0;
            }
            else if (format == 2)
            {
                //STBTT_assert(0); // @TODO: high-byte mapping for japanese/chinese/korean
                throw new NotImplementedException();
            }
            else if (format == 4)
            { // standard mapping for windows fonts: binary search collection of ranges
                ushort segcount = (ushort)(raw.GetNumber<ushort>(raw.IndexMap + 6) >> 1);
                ushort searchRange = (ushort)(raw.GetNumber<ushort>(raw.IndexMap + 8) >> 1);
                ushort entrySelector = raw.GetNumber<ushort>(raw.IndexMap + 10);
                ushort rangeShift = (ushort)(raw.GetNumber<ushort>(raw.IndexMap + 12) >> 1);
                ushort item, offset, start, end;

                // do a binary search of the segments
                int endCount = raw.IndexMap + 14;
                int search = endCount;


                // they lie from endCount .. endCount + segCount
                // but searchRange is the nearest power of two, so...
                if (code >= raw.GetNumber<ushort>(search + rangeShift * 2))
                    search += (rangeShift * 2);

                // now decrement to bias correctly to find smallest
                search -= 2;
                while (entrySelector != 0)
                {
                    //ushort start, end;
                    searchRange >>= 1;
                    start = raw.GetNumber<ushort>(search + 2 + segcount * 2 + 2);
                    end = raw.GetNumber<ushort>(search + 2);
                    start = raw.GetNumber<ushort>(search + searchRange * 2 + segcount * 2 + 2);
                    end = raw.GetNumber<ushort>(search + searchRange * 2);
                    if (code > end)
                        search += searchRange * 2;
                    --entrySelector;
                }
                search += 2;

                item = (ushort)((search - endCount) >> 1);

                //STBTT_assert(unicode_codepoint <= ttUSHORT(data + endCount + 2*item));
                start = raw.GetNumber<ushort>(raw.IndexMap + 14 + segcount * 2 + 2 + 2 * item);
                end = raw.GetNumber<ushort>(raw.IndexMap + 14 + 2 + 2 * item);
                if (code < start)
                    return 0;

                offset = raw.GetNumber<ushort>(raw.IndexMap + 14 + segcount * 6 + 2 + 2 * item);
                if (offset == 0)
                    return (ushort)(code + raw.GetNumber<short>(raw.IndexMap + 14 + segcount * 4 + 2 + 2 * item));

                return raw.GetNumber<ushort>(offset + (code - start) * 2 + raw.IndexMap + 14 + segcount * 6 + 2 + 2 * item);
            }
            else if (format == 6)
            {
                int first = raw.GetNumber<ushort>(raw.IndexMap + 6);
                int count = raw.GetNumber<ushort>(raw.IndexMap + 8);
                if ((int)code >= first && (int)code < first + count)
                    return raw.GetNumber<ushort>(raw.IndexMap + 10 + (code - first) * 2);
                else
                    throw new Exception("Not found");
            }
            else if (format == 12 || format == 13)
            {
                uint ngroups = raw.GetNumber<uint>(raw.IndexMap + 12);
                int low, high;
                low = 0;
                high = (int)ngroups;
                // Binary search the right group.
                while (low < high)
                {
                    int mid = low + ((high - low) >> 1); // rounds down, so low <= mid < high
                    uint start_char = raw.GetNumber<uint>(raw.IndexMap + 16 + mid * 12);
                    uint end_char = raw.GetNumber<uint>(raw.IndexMap + 16 + mid * 12 + 4);
                    if ((uint)code < start_char)
                        high = mid;
                    else if ((uint)code > end_char)
                        low = mid + 1;
                    else
                    {
                        uint start_glyph = raw.GetNumber<uint>(raw.IndexMap + 16 + mid * 12 + 8);
                        if (format == 12)
                            return (int)(start_glyph + code - start_char);
                        else // format == 13
                            return (int)start_glyph;
                    }
                }
                throw new Exception("Not found");
            }
            return default;
        }

        internal static float GetPixelHeightScale(this TTFRaw raw, float height)
        {
            int fheight = raw.GetNumber<short>(raw.Table.Hhea + 4) - raw.GetNumber<short>(raw.Table.Hhea + 6);
            return (float)height / fheight;
        }

        internal static T GetNumber<T>(this TTFRaw raw, int position)
            where T : struct, INumber<T>
        {
            var span = raw.Span.Slice(position, Marshal.SizeOf<T>()).ToArray().AsSpan();
            span.Reverse();
            return MemoryMarshal.Read<T>(span);
        }

        internal static int GetGlyphKernAdvance(this TTFRaw raw, int glyph1, int glyph2)
        {
            int kern = raw.Table.Kern;
            int needle, straw;
            int l, r, m;

            // we only look at the first table. it must be 'horizontal' and format 0.
            if (!(kern != 0))
                return 0;
            if (raw.GetNumber<ushort>(2 + kern) < 1) // number of tables, need at least 1
                return 0;
            if (raw.GetNumber<ushort>(8 + kern) != 1) // horizontal flag must be set in format
                return 0;

            l = 0;
            r = raw.GetNumber<ushort>(10 + kern) - 1;
            needle = glyph1 << 16 | glyph2;
            while (l <= r)
            {
                m = (l + r) >> 1;
                straw = (int)raw.GetNumber<uint>(18 + (m * 6) + kern); // note: unaligned read
                if (needle < straw)
                    r = m - 1;
                else if (needle > straw)
                    l = m + 1;
                else
                    return raw.GetNumber<short>(22 + (m * 6) + kern);
            }
            return 0;
        }

        internal static (int x0, int y0, int x1, int y1) GetGlyphBox(this TTFVector vector, int index, PointF scale)
        {
            var x0 = vector.Vertices?.Min(x => x.X) ?? 0;
            var y0 = vector.Vertices?.Min(x => x.Y) ?? 0;
            var x1 = vector.Vertices?.Max(x => x.X) ?? 0;
            var y1 = vector.Vertices?.Max(x => x.Y) ?? 0;


            var ix0 = (int)Math.Floor(x0 * scale.X);
            var iy0 = (int)-Math.Ceiling(y1 * scale.Y);
            var ix1 = (int)Math.Ceiling(x1 * scale.X);
            var iy1 = (int)-Math.Floor(y0 * scale.Y);
            return (ix0, iy0, ix1, iy1);
        }

        internal static (short advanceWidth, short leftSideBearing) GetGlyphHMetrics(this TTFRaw raw, int index)
        {
            var numOfLongHorMetrics = raw.GetNumber<ushort>(raw.Table.Hhea + 34);
            if (index < numOfLongHorMetrics)
            {
                var advanceWidth = raw.GetNumber<short>(raw.Table.Hmtx + 4 * index);
                var leftSideBearing = raw.GetNumber<short>(raw.Table.Hmtx + 4 * index + 2);
                return (advanceWidth, leftSideBearing);
            }
            else
            {
                var advanceWidth = raw.GetNumber<short>(raw.Table.Hmtx + 4 * (numOfLongHorMetrics - 1));
                var leftSideBearing = raw.GetNumber<short>(raw.Table.Hmtx + 4 * numOfLongHorMetrics + 2 * (index - numOfLongHorMetrics));
                return (advanceWidth, leftSideBearing);
            }
        }

        #region Vector related
        internal static TTFVector GetVector(this TTFRaw raw, char character) =>
            raw.GetGlyphIndex(character) is var glyphIndex ?
            new TTFVector(character, raw.GetShape(glyphIndex)) :
            throw new ArgumentException();

        private static Vertex[] GetShape(this TTFRaw raw, int glyphIndex)
        {
            var glyphIndexOffset = raw.GetGlyphOffset(glyphIndex);
            short numberOfContours = raw.GetNumber<short>(glyphIndexOffset);

            return numberOfContours switch
            {
                0 => throw new ArgumentException(),    // Do nothing.
                > 0 => raw.GetSimpleShape(glyphIndexOffset, numberOfContours, glyphIndex),
                < 0 => raw.GetCompositeShape(glyphIndexOffset, numberOfContours, glyphIndex),     // Composite Glyph == -1
            };
        }

        private static Vertex[] GetSimpleShape(this TTFRaw raw, int glyphIndexOffset, int numberOfContours, int glyphIndex)
        {
            int iData = glyphIndexOffset + 10;

            var ins = raw.GetNumber<short>(iData + numberOfContours * 2);

            int iPoints = iData + numberOfContours * 2 + 2 + ins;

            var n = 1 + raw.GetNumber<ushort>(numberOfContours * 2 - 2 + iData);
            var off = 2 * numberOfContours;
            var m = n + off;
            var vertices = new Vertex[m];
            // first load flags
            var flagcount = 0;
            var points = raw.Data;
            byte flags = 0;
            for (var i = 0; i < n; ++i)
            {
                if (flagcount == 0)
                {
                    flags = points[iPoints++];
                    if ((flags & 8) != 0)
                        flagcount = points[iPoints++];
                }
                else
                    --flagcount;
                vertices[off + i].Type = (VertexType)flags;
            }

            // now load x coordinates
            var x = 0;
            for (var i = 0; i < n; ++i)
            {
                flags = (byte)vertices[off + i].Type;
                if ((flags & 2) != 0)
                {
                    short dx = points[iPoints++];
                    x += (flags & 16) != 0 ? dx : -dx; // ???
                }
                else
                {
                    if (!((flags & 16) != 0))
                    {
                        x = x + (short)((points[0 + iPoints] * 256) + points[1 + iPoints]);
                        iPoints += 2;
                    }
                }
                vertices[off + i].X = (short)x;
            }


            // now load y coordinates
            var y = 0;
            for (var i = 0; i < n; ++i)
            {
                flags = (byte)vertices[off + i].Type;
                if ((flags & 4) != 0)
                {
                    short dy = points[iPoints++];
                    y += (flags & 32) != 0 ? dy : -dy; // ???
                }
                else
                {
                    if (!((flags & 32) != 0))
                    {
                        y = y + (short)((points[0 + iPoints] * 256) + points[1 + iPoints]);
                        iPoints += 2;
                    }
                }
                vertices[off + i].Y = (short)y;
            }


            // now convert them to our format
            var num_vertices = 0;

            int cx, cy, sx, sy, scx, scy;
            sx = sy = cx = cy = scx = scy = 0;
            var next_move = 0;
            var was_off = 0;
            var start_off = 0;
            var j = 0;

            for (var i = 0; i < n; ++i)
            {
                flags = (byte)vertices[off + i].Type;
                x = (short)vertices[off + i].X;
                y = (short)vertices[off + i].Y;

                if (next_move == i)
                {
                    if (i != 0)
                        num_vertices = VertexsCloseShape(vertices, num_vertices, was_off, start_off, sx, sy, scx, scy, cx, cy);

                    // now start the new one               
                    start_off = 0 == (flags & 1) ? 1 : 0;
                    if (start_off != 0)
                    {
                        // if we start off with an off-curve point, then when we need to find a point on the curve
                        // where we can start, and we need to save some state for when we wraparound.
                        scx = x;
                        scy = y;
                        if (!(((int)vertices[off + i + 1].Type & 1) != 0))
                        {
                            // next point is also a curve point, so interpolate an on-point curve
                            sx = (x + (int)vertices[off + i + 1].X) >> 1;
                            sy = (y + (int)vertices[off + i + 1].Y) >> 1;
                        }
                        else
                        {
                            // otherwise just use the next point as our start point
                            sx = (int)vertices[off + i + 1].X;
                            sy = (int)vertices[off + i + 1].Y;
                            ++i; // we're using point i+1 as the starting point, so skip it
                        }
                    }
                    else
                    {
                        sx = x;
                        sy = y;
                    }
                    SetVertex(ref vertices[num_vertices++], num_vertices, VertexType.MoveTo, sx, sy, 0, 0);
                    was_off = 0;
                    next_move = 1 + raw.GetNumber<ushort>(iData + j * 2);
                    ++j;
                }
                else
                {
                    if (!((flags & 1) != 0))
                    { // if it's a curve
                        if (was_off != 0) // two off-curve control points in a row means interpolate an on-curve midpoint
                            SetVertex(ref vertices[num_vertices++], num_vertices, VertexType.CurveTo, (cx + x) >> 1, (cy + y) >> 1, cx, cy);
                        cx = x;
                        cy = y;
                        was_off = 1;
                    }
                    else
                    {
                        if (was_off != 0)
                            SetVertex(ref vertices[num_vertices++], num_vertices, VertexType.CurveTo, x, y, cx, cy);
                        else
                            SetVertex(ref vertices[num_vertices++], num_vertices, VertexType.LineTo, x, y, 0, 0);
                        was_off = 0;
                    }
                }
            }

            num_vertices = VertexsCloseShape(vertices, num_vertices, was_off, start_off, sx, sy, scx, scy, cx, cy);
            return vertices.Take(num_vertices).ToArray();
        }

        private static Vertex[] GetCompositeShape(this TTFRaw raw, int glyphIndexOffset, int numberOfContours, int glyphIndex)
        {
            int more = 1;
            byte[] compositeData = raw.Data;
            int iCompositeData = glyphIndexOffset + 10;
            var num_vertices = 0;
            Vertex[]? vertices = null;

            while (more != 0)
            {
                ushort flags, gidx;
                int comp_num_verts = 0, i;
                Vertex[]? comp_verts = null, tmp = null;
                float[] mtx = { 1, 0, 0, 1, 0, 0 };
                float m, n;

                flags = raw.GetNumber<ushort>(iCompositeData);
                iCompositeData += 2;
                gidx = raw.GetNumber<ushort>(iCompositeData);
                iCompositeData += 2;

                if ((flags & 2) != 0)
                { // XY values
                    if ((flags & 1) != 0)
                    { // shorts
                        mtx[4] = raw.GetNumber<short>(iCompositeData);
                        iCompositeData += 2;
                        mtx[5] = raw.GetNumber<short>(iCompositeData);
                        iCompositeData += 2;
                    }
                    else
                    {
                        mtx[4] = raw.GetNumber<short>(iCompositeData);
                        iCompositeData += 1;
                        mtx[5] = raw.GetNumber<short>(iCompositeData);
                        iCompositeData += 1;
                    }
                }
                else
                {
                    // @TODO handle matching point
                    throw new NotImplementedException();
                }
                if ((flags & (1 << 3)) != 0)
                { // WE_HAVE_A_SCALE
                    mtx[0] = mtx[3] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[1] = mtx[2] = 0;
                }
                else if ((flags & (1 << 6)) != 0)
                { // WE_HAVE_AN_X_AND_YSCALE
                    mtx[0] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[1] = mtx[2] = 0;
                    mtx[3] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                }
                else if ((flags & (1 << 7)) != 0)
                { // WE_HAVE_A_TWO_BY_TWO
                    mtx[0] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[1] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[2] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[3] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                }

                // Find transformation scales.
                m = (float)Math.Sqrt(mtx[0] * mtx[0] + mtx[1] * mtx[1]);
                n = (float)Math.Sqrt(mtx[2] * mtx[2] + mtx[3] * mtx[3]);

                // Get indexed glyph.
                comp_verts = raw.GetShape(gidx);
                comp_num_verts = comp_verts?.Length ?? 0;
                if (comp_num_verts > 0)
                {
                    // Transform vertices.
                    for (i = 0; i < comp_num_verts; ++i)
                    {
                        Vertex v = comp_verts![i];
                        short x, y; // stbtt_vertex_type = short;
                        x = v.X;
                        y = v.Y;
                        v.X = (short)(m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                        v.Y = (short)(n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                        x = v.CenterX;
                        y = v.CenterY;
                        v.CenterX = (short)(m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                        v.CenterY = (short)(n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                    }

                    // Append vertices.
                    //tmp = (stbtt_vertex*)STBTT_malloc((num_vertices+comp_num_verts)*sizeof(stbtt_vertex), info->userdata);
                    tmp = new Vertex[num_vertices + comp_num_verts];
                    if (tmp == null)
                    {
                        //if (vertices) STBTT_free(vertices, info->userdata);
                        //if (comp_verts) STBTT_free(comp_verts, info->userdata);
                        throw new ArgumentException();
                    }
                    if (num_vertices > 0)
                        //memcpy(tmp, vertices, num_vertices*sizeof(stbtt_vertex));
                        Array.Copy(vertices!, tmp, num_vertices);
                    //memcpy(tmp+num_vertices, comp_verts, comp_num_verts*sizeof(stbtt_vertex));
                    Array.Copy(comp_verts!, 0, tmp, num_vertices, comp_num_verts);
                    //if (vertices) 
                    //	STBTT_free(vertices, info->userdata);
                    vertices = tmp;
                    //STBTT_free(comp_verts, info->userdata);
                    num_vertices += comp_num_verts;
                }
                // More components ?
                more = flags & (1 << 5);
            }
            return vertices!;
        }

        private static int VertexsCloseShape(Vertex[] vertices, int num_vertices, int was_off, int start_off,
                                    int sx, int sy, int scx, int scy, int cx, int cy)
        {
            if (start_off != 0)
            {
                if (was_off != 0)
                    SetVertex(ref vertices[num_vertices++], num_vertices,
                        VertexType.CurveTo, (cx + scx) >> 1, (cy + scy) >> 1, cx, cy);
                SetVertex(ref vertices[num_vertices++], num_vertices,
                    VertexType.CurveTo, sx, sy, scx, scy);
            }
            else
            {
                if (was_off != 0)
                    SetVertex(ref vertices[num_vertices++], num_vertices,
                        VertexType.CurveTo, sx, sy, cx, cy);
                else
                    SetVertex(ref vertices[num_vertices++], num_vertices,
                        VertexType.LineTo, sx, sy, 0, 0);
            }
            return num_vertices;
        }

        private static void SetVertex(ref Vertex v, int numVert, VertexType type, int x, int y, int cx, int cy)
        {
            if (numVert == 15)
            {
                numVert = 15;
            }

            v.Type = type;
            v.X = (short)x;
            v.Y = (short)y;
            v.CenterX = (short)cx;
            v.CenterY = (short)cy;
        }
        #endregion Vector related

    }
}
