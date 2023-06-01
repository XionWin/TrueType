using Extension;
using TrueType2.Domain.Support;
using TrueType2.Extension;

namespace TrueType2.Domain
{
    public class TTF : IDisposable
    {
        public string Name { get; set; }
        public string Path { get; set; }

        private TTFVectorCache? _cache;

        public TTF(string name, string path)
        {
            Name = name;
            Path = path;

            this._cache =
                TTFRawCache.Instance.ContainsKey(name) ?
                    TTFRawCache.Instance[name]
                    : new TTFVectorCache(new TTFRaw(name, File.ReadAllBytes(path))).With(x => TTFRawCache.Instance.Add(name, x));
        }

        public void GetGlyph(char c)
        {
            var v = this._cache!.TryGet(c);
        }

        public void Dispose()
        {
            this._cache = null;
        }
    }
}