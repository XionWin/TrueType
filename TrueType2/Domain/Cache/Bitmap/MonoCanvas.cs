namespace TrueType2.Domain.Cache.Bitmap
{
    public class MonoCanvas
    {


        public int Width { get; init; }
        public int Height { get; init; }

        private static MonoCanvas _Instance = new MonoCanvas(512, 512);
        public static MonoCanvas Instance = _Instance;

        public byte[] Pixels { get; init; }

        private MonoCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new byte[Width * Height];
        }
    }
}
