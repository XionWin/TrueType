namespace TrueType2.Domain
{
    public class MonoCanvas
    {
        public int Width { get; init; }
        public int Height { get; init; }

        public byte[] Pixels { get; init; }

        internal MonoCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new byte[Width * Height];
        }
    }
}
