using TrueType2.Domain.Cache.Pixel;
using TrueType2.Mode;

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

        public Point Location { get; private set; }


        internal void DrawScanline(Scanline scanline, int lineIndex, Size renderSize)
        {
            Array.Copy(scanline.Data!, 0, this.Pixels, this.Location.X + this.Location.Y * this.Width + (lineIndex * this.Width), renderSize.Width);
        }

        public void UpdateLocation( Size renderSize)
        {
            var padding = 2;
            var location = this.Location;
            if (this.Location.X + renderSize.Width + padding * 2  > this.Width)
            {
                location.X = 0;
                location.Y += 64 + padding * 2;
            }
            else
            {
                location.X += renderSize.Width + padding * 2;
            }
            this.Location = location;
        }


    }
}
