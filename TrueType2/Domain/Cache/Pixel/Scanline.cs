using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType2.Domain.Cache.Pixel
{
    internal class Scanline
    {
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
