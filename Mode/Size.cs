using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType.Mode
{
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
    public struct SizeF
    {
        public float Width { get; set; }
        public float Height { get; set; }

        public SizeF(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}
