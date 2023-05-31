namespace TrueType2.Domain.Support
{
    internal class TTFBitmapCache
    {
        private TTFBitmapCache()
        {
            Pixels = new byte[480 * 480];
        }

        private static TTFBitmapCache _Instance = new TTFBitmapCache();
        public static TTFBitmapCache Instance = _Instance;

        public byte[]? ScanLine { get; set; }

        public byte[] Pixels { get; init; }
    }
}
