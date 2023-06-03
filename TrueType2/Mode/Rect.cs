using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType2.Mode
{
    public struct Rect
    {
        public Point Location { get; set; }

        public Size Size { get; set; }

        public Rect(int x, int y, int width, int height)
        {
            Location = new Point(x, y);
            Size = new Size(width, height);
        }
    }

    public struct RectF
    {
        public PointF Location { get; set; }

        public SizeF Size { get; set; }

        public RectF(float x, float y, float width, float height)
        {
            Location = new PointF(x, y);
            Size = new SizeF(width, height);
        }
    }
}
