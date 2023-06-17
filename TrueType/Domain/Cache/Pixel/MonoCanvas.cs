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

        internal void DrawScanline(Scanline scanline, int lineIndex, Size renderSize)
        {
            Array.Copy(scanline.Data!, 0, Pixels, Location.X + Location.Y * Size.Width + lineIndex * Size.Width, renderSize.Width);
        }

        public void TryLocate(Size renderSize, int lightHeight)
        {
            var location = Location;
            if (Location.X + renderSize.Width > Size.Width)
            {
                location.X = 0;
                location.Y += lightHeight;
            }
            Location = location;
        }

        public void UpdateLocation(Size renderSize, int lightHeight)
        {
            var location = Location;
            if (Location.X + renderSize.Width > Size.Width)
            {
                location.X = 0;
                location.Y += lightHeight;
            }
            else
            {
                location.X += renderSize.Width;
            }
            Location = location;
        }


    }
}
