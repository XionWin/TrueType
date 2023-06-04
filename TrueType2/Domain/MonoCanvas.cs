using TrueType2.Domain.Cache.Pixel;
using TrueType2.Mode;

namespace TrueType2.Domain
{
    public class MonoCanvas
    {
        public Size Size { get; init; }
        public int FontSize { get; init; }

        public byte[] Pixels { get; init; }

        internal MonoCanvas(int fontSize, Size size)
        {
            FontSize = fontSize;
            Size = size;
            Pixels = new byte[size.Width * size.Height];
        }

        public Point Location { get; private set; }


        internal void DrawScanline(Scanline scanline, int lineIndex, Size renderSize)
        {
            Array.Copy(scanline.Data!, 0, this.Pixels, this.Location.X + this.Location.Y * this.Size.Width + (lineIndex * this.Size.Width), renderSize.Width);
        }

        public void UpdateLocation( Size renderSize)
        {
            var padding = 2;
            var location = this.Location;
            if (this.Location.X + renderSize.Width + padding * 2  > this.Size.Width)
            {
                location.X = 0;
                location.Y += this.FontSize + padding * 2;
            }
            else
            {
                location.X += renderSize.Width + padding * 2;
            }
            this.Location = location;
        }


    }
}
