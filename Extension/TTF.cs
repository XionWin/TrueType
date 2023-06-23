using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrueType.Domain;
using TrueType.Mode;

namespace TrueType.Extension
{
    internal static class TTFExtension
    {
        internal static uint HashInt(uint data)
        {
            uint a = data;
            a += ~(a << 15);
            a ^= (a >> 10);
            a += (a << 3);
            a ^= (a >> 6);
            a += ~(a << 11);
            a ^= (a >> 16);
            return a;
        }
    }
}
