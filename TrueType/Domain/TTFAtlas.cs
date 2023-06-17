using TrueType.Domain.Cache.Pixel;
using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTFAtlas
    {

        public List<TTFGlyph> _glyphs = new List<TTFGlyph>();

        private TTFAtlas() { }

        private static TTFAtlas _Instance = new TTFAtlas();
        public static TTFAtlas Instance = _Instance;

        public TTFGlyph GetGlyph(char character, int size)
        {


            return default;
        }
        
    }
}
