using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType
{
    public class Cache
    {
        private Cache() 
        {
            this.Pixels = new byte[480 * 480];
        }


        private static Cache _Instance = new Cache();
        public static Cache Instance = _Instance;

        public byte[]? ScanLine { get; set; }
        
        public byte[] Pixels { get; init; }
    }

    internal static class CacheExtension
    {
        public static byte[] RequestScanline(this Cache cache, int len)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));  

            if (cache.ScanLine is null)
            {
                cache.ScanLine = new byte[len];
            }

            if (cache.ScanLine.Length < len)
            {
                var scanline = cache.ScanLine;
                Array.Resize(ref scanline, len);
                cache.ScanLine = scanline;
            }

            return cache.ScanLine;
        }
    }

}
