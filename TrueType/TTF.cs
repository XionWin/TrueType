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

    public class TTFRaw
    {
        private byte[] _raw;
        public byte[] Raw => this._raw;
        public ReadOnlySpan<byte> Span => this._raw;

        public TTFRaw(byte[] raw)
        {
            this._raw = raw;
            this.Tables = this.LoadTables(0);
        }

        public Dictionary<string, uint> Tables { get; private set; }

        public T GetNumber<T>(int position) where T : struct, INumber<T> => this.Span.GetNumber<T>(position);
    }

    public class TTF
    {
        public object? UserData { get; set; }
        /// <summary>
        /// Pointer to .ttf file
        /// </summary>
        public TTFRaw Raw { get; init; }

        /// <summary>
        /// Offset of start of font
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Number of glyphs, needed for range checking
        /// </summary>
        public int GlyphsCount { get; set; }

        public int Cmap { get; set; }

        public int Glyf { get; set; }
        public int Head { get; set; }
        public int Loca { get; set; }
        public int Maxp { get; set; }
        public int Hmtx { get; set; }
        public int Hhea { get; set; }
        public int Kern { get; set; }

        public TTF(byte[] raw)
        {
            this.Raw = new TTFRaw(raw);
        }

    }

    internal static class TrueTypeFontInfoExtension
    {
        public static T GetNumber<T>(this ReadOnlySpan<byte> data, int position)
            where T : struct, INumber<T>
        {
            var span = new Span<byte>(data.Slice(position, Marshal.SizeOf<T>()).ToArray());
            span.Reverse();
            return MemoryMarshal.Read<T>(span);
        }


        internal static Dictionary<string, uint> LoadTables(this TTFRaw raw, int start)
        {
            var span = raw.Span;
            var tableCount = span.GetNumber<ushort>(start + TTFC.TABLE_COUNT_OFFSET);
            var tableDir = start + TTFC.TABLE_DIR_OFFSET;

            var result = new Dictionary<string, uint>();
            for (int i = 0; i < tableCount; i++)
            {
                var location = tableDir + TTFC.TABLE_DIR_STEP_LEN * i;
                var nameData = span.Slice(location, 4);
                result.Add(Encoding.Default.GetString(nameData), span.GetNumber<uint>(location + 8));
            }
            return result;
        }

    }
}
