using TrueType2.Domain.Support;
using TrueType2.Extension;

namespace TrueType2.Domain
{
    public class TTF
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public TTFRaw Raw { get; private set; }

        private TTFCache _cache = new TTFCache();

        public TTF(string name, string path)
        {
            Name = name;
            Path = path;
            if (File.Exists(path))
                this.Raw = new TTFRaw(name, File.ReadAllBytes(path));
            else
                throw new Exception($"Font {path} not found");


            foreach (var c in "我")
            {
                var vector = this.Raw.GetVector(c);
            }

            //var bitmap = vector.GetBitmap();
        }
    }
}