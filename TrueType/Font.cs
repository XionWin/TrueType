using Extension;

namespace TrueType
{
    public class Font
    {

        public Font(string name, string path)
        {
            if (File.Exists(path))
                this.TTF = new TTFRaw(name, File.ReadAllBytes(path));
            else
                throw new Exception($"Font {path} not found");
        }

        public TTFRaw TTF { get; private set; }

    }
}
