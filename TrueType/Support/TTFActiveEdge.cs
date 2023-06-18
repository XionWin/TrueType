using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType.Support
{
    public class TTFActiveEdge
    {
        public int X { get; set; }
        public int DX { get; set; }
        public float EY { get; set; }
        public int Valid { get; set; }
        public TTFActiveEdge? Next { get; set; }
    }
}
