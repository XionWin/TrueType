using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTFBitmap
    {
        public char Character { get; set; }
        public int Size { get; set; }
        public Rect TexRect { get; init; }

        public TTFBitmap(char character, int fontSize, Rect texRect)
        {
            Character = character;
            Size = fontSize;
            TexRect = texRect;
        }
    }
}
