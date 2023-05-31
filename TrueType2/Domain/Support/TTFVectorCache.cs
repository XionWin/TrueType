namespace TrueType2.Domain.Support
{
    internal class TTFVectorCache: Dictionary<string, TTFRawCache>
    {
        private static TTFVectorCache _Instance = new TTFVectorCache();
        public static TTFVectorCache Instance = _Instance;
    }

}
