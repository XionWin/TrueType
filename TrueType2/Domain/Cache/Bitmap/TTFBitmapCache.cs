namespace TrueType2.Domain.Cache.Bitmap
{
    public class TTFBitmapCache
    {

        private static TTFBitmapCache _Instance = new TTFBitmapCache();
        public static TTFBitmapCache Instance = _Instance;

        public byte[]? Scanline { get; set; }

        public MonoCanvas Canvas => MonoCanvas.Instance;

        public byte[] RequestScanline(int len)
        {
            if (Scanline is null)
            {
                Scanline = new byte[len];
            }

            if (Scanline.Length < len)
            {
                var scanline = Scanline;
                Array.Resize(ref scanline, len);
                Scanline = scanline;
            }
            return Scanline;
        }
    }
}
