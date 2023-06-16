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

        public static (int advanceWidth, int leftSideBearing, int x0, int y0, int x1, int y1) BuildGlyphBoxSettings(this TTFRaw raw, int index, int size, PointF scale, PointF shift)
        {
            var (advanceWidth, leftSideBearing) = raw.GetGlyphHMetrics(index);
            var (x0, y0, x1, y1) = raw.GetGlyphBox(index, scale, shift);
            return (advanceWidth, leftSideBearing, x0, y0, x1, y1);
        }

        private static (int x0, int y0, int x1, int y1) GetGlyphBox(this TTFRaw raw, int index, PointF scale, PointF shift)
        {
            var offset = raw.GetGlyphOffset(index);
            if (offset == 0)
                throw new Exception("Not found");

            var x0 = (int)raw.GetNumber<short>(offset + 2);
            var y0 = (int)raw.GetNumber<short>(offset + 4);
            var x1 = (int)raw.GetNumber<short>(offset + 6);
            var y1 = (int)raw.GetNumber<short>(offset + 8);


            var ix0 = (int)Math.Floor(x0 * scale.X + shift.X);
            var iy0 = (int)-Math.Ceiling(y1 * scale.Y + shift.Y);
            var ix1 = (int)Math.Ceiling(x1 * scale.X + shift.X);
            var iy1 = (int)-Math.Floor(y0 * scale.Y + shift.Y);
            return (ix0, iy0, ix1, iy1);
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

        private static (short advanceWidth, short leftSideBearing) GetGlyphHMetrics(this TTFRaw raw, int index)
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
    }
}
