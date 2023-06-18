using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrueType.Mode;

namespace TrueType.Support
{
    public struct TTFEdge : IComparable
    {
        public PointF P0 { get; set; }
        public PointF P1 { get; set; }

        public bool IsInvented { get; set; }

        public TTFEdge(PointF p0, PointF p1, bool IsInvent)
        {
            this.P0 = p0;
            this.P1 = p1;
            this.IsInvented = IsInvent;
        }

        public int CompareTo(object? obj)
        {
            if (obj is TTFEdge edge)
            {
                if (this.P0.Y < edge.P0.Y)
                    return -1;
                if (this.P0.Y > edge.P0.Y)
                    return 1;
                return 0;
            }
            else
                return 1;
        }
    }
}
