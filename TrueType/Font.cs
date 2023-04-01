using Extension;

namespace TrueType
{
    public class Font
    {

        public Font(string path)
        {
            this.TTF = new TTF(File.ReadAllBytes(path));
        }

        public TTF TTF { get; private set; }

    }
}
