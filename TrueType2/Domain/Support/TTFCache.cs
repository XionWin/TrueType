namespace TrueType2.Domain.Support
{
    internal class TTFCache: Dictionary<int, TTFCacheItem>
    {

    }

    internal class TTFCacheItem
    {
        public TTFVector? Vector { get; set; }
        public TTFBitmap? Bitmap { get; set; }
    }
}
