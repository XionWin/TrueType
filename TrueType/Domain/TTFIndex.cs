using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType.Domain
{
    public struct TTFIndex
    {
        public char Character { get; set; }
        public int Size { get; set; }
        public int Blur { get; set; }

        public TTFIndex(char character, int size, int blur) 
        {
            Character = character;
            Size = size;
            Blur = blur;
        }
    }
}
