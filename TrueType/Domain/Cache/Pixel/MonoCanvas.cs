using System.Numerics;
using TrueType.Mode;

namespace TrueType.Domain.Cache.Pixel
{
    public class MonoCanvas
    {
        public Size Size { get; init; }

        public byte[] Pixels { get; init; }

        private static MonoCanvas _Instance = new MonoCanvas(new Size(512, 512));
        public static MonoCanvas Instance = _Instance;

        private MonoCanvas(Size size)
        {
            Size = size;
            Pixels = new byte[size.Width * size.Height];
        }

        public Point Location { get; private set; }

        internal TTFBitmap LocateCharacter(char character, int size, byte[] data, Size renderSize, int lineHeight)
        {
            var location = Location;
            if (Location.X + renderSize.Width > Size.Width)
            {
                location.X = 0;
                location.Y += lineHeight;
            }

            var steps = renderSize.Height;
            for (int i = 0; i < steps; i++)
            {
                Array.Copy(data, i * renderSize.Width, Pixels, location.X + location.Y * Size.Width + i * Size.Width, renderSize.Width);
            }
            var bitmap = new TTFBitmap(character, size, new Rect(location.X, location.Y, renderSize.Width, renderSize.Height));

            location.X += renderSize.Width;
            Location = location;

            return bitmap;
        }
    }
}
