using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Extension;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TrueType
{

    public class TTFRawTable
    {
        public int Cmap { get; set; }
        public int Glyf { get; set; }
        public int Head { get; set; }
        public int Hhea { get; set; }
        public int Hmtx { get; set; }
        public int Kern { get; set; }
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
                Kern = (int)this._rawTables["kern"],
                Loca = (int)this._rawTables["loca"],
                Name = (int)this._rawTables["name"],
            };
            var (indexMap, indexLocFormat) = this.LoadCMap();
            this.IndexMap = indexMap;
            this.IndexLocFormat = indexLocFormat;

            //var glyphs = this.LoadTableValue<ushort>("maxp", TTFC.TABLE_MAXP_GLYPHS_OFFSET);
            //var cmapTables = this.LoadTableValue<ushort>("cmap", TTFC.TABLE_CMAP_TABLES_OFFSET);
            var lineGap = 0;
            var vMetrics = this.GetFontVMetrics();
            var fontHeight = vMetrics.ascent - vMetrics.descent;
            var fontascender = (float)vMetrics.ascent / fontHeight;
            var fontdescender = (float)vMetrics.descent / fontHeight;
            var fontLineHeight = (float)(fontHeight + lineGap) / fontHeight;

            this.GetGlyph((byte)'T', 124, 0);
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
        internal static void GetGlyph(this TTFRaw raw, byte code, short size, short blur)
        {

            // Find code point and size.
            var h = HashInt(code) & (FONS_HASH_LUT_SIZE - 1);

            var scale = raw.GetPixelHeightScale(size);
            "abcdefg".ToArray().Select(x => (byte)x).ToList().ForEach(x =>
            {
                var index = raw.GetGlyphIndex(x);
            });
        }

        private static int GetGlyphIndex(this TTFRaw raw, byte code)
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

        private static float GetPixelHeightScale(this TTFRaw raw, float height)
        {
            int fheight = raw.GetNumber<short>(raw.Table.Hhea + 4) - raw.GetNumber<short>(raw.Table.Hhea + 6);
            return (float)height / fheight;
        }

        private static uint HashInt(byte data)
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
