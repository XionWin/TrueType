using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType.Domain
{
    internal class Scanline
    {
        private Scanline() { }
        private static Scanline _Instance = new Scanline();
        public static Scanline Instance = _Instance;
        public byte[]? Data { get; set; }

        public byte[] Request(int len)
        {
            if (Data is null)
            {
                Data = new byte[len];
            }

            if (Data.Length < len)
            {
                var scanline = Data;
                Array.Resize(ref scanline, len);
                Data = scanline;
            }
            return Data;
        }
    }

}
