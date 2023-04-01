using Extension;

namespace TrueType
{
    public class TrueTypeFont
    {
        public TrueTypeFont(string path)
        {
            this.Info = new TrueTypeFontInfo(File.ReadAllBytes(path));
        }

        public TrueTypeFontInfo Info { get; private set; }

    }
}
