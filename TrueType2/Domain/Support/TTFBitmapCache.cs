namespace TrueType2.Domain.Support
{
    public class TTFBitmapCache
    {

        private static TTFBitmapCache _Instance = new TTFBitmapCache();
        public static TTFBitmapCache Instance = _Instance;

        public byte[]? Scanline { get; set; }

        public MonoCanvas Canvas => MonoCanvas.Instance;

        public byte[] RequestScanline(int len)
        {
            if (this.Scanline is null)
            {
                this.Scanline = new byte[len];
            }

            if (this.Scanline.Length < len)
            {
                var scanline = this.Scanline;
                Array.Resize(ref scanline, len);
                this.Scanline = scanline;
            }
            return this.Scanline;
        }
    }
}
