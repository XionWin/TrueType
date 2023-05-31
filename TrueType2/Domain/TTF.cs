using Extension;
using TrueType2.Domain.Support;
using TrueType2.Extension;

namespace TrueType2.Domain
{
    public class TTF
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public TTFRaw Raw { get; private set; }

        private TTFVectorCache _cache = new TTFVectorCache();

        public TTF(string name, string path)
        {
            Name = name;
            Path = path;

            this.Raw = 
                TTFRawCache.Instance.ContainsKey(name) ? 
                    TTFRawCache.Instance[name] 
                    : File.Exists(path) ? 
                        new TTFRaw(name, File.ReadAllBytes(path)).With(x => this.Raw = x).With(x => TTFRawCache.Instance.Add(name, x)) 
                        : throw new Exception($"Font {path} not found");


            foreach (var c in "我")
            {
                var index = Raw.GetGlyphIndex((int)c);
                var vector = this.Raw.GetVector(index);
            }

            //var bitmap = vector.GetBitmap();
        }
    }
}