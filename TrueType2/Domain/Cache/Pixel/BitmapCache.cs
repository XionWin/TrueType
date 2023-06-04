using TrueType2.Domain.Cache.Vector;

namespace TrueType2.Domain.Cache.Pixel
{
    public class BitmapCache : Dictionary<string, FontBitmapCache>
    {

        private static BitmapCache _Instance = new BitmapCache();
        public static BitmapCache Instance = _Instance;

        //private MonoCanvas _canvas = new MonoCanvas(512, 512, 64);
        //public MonoCanvas Canvas => this._canvas;

        private Scanline _scanline = new Scanline();
        internal Scanline Scanline => this._scanline;
    }
}
