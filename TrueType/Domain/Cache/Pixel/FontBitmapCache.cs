using Extension;
using TrueType.Domain.Cache.Vector;

namespace TrueType.Domain.Cache.Pixel
{
    public class FontBitmapCache : Dictionary<int, MonoCanvas>
    {
        public string FontName { get; init; }


        public FontBitmapCache(string name)
        {
            this.FontName = name;
        }

        public MonoCanvas TryGet(int fontSize) =>
            this.ContainsKey(fontSize) ?
                this[fontSize]
                : new MonoCanvas(fontSize, new Mode.Size(512, 512)).With(x => Add(fontSize, x));
    }
}
