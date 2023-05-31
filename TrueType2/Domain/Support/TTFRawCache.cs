using Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType2.Domain.Support
{
    internal class TTFRawCache : Dictionary<string, TTFVectorCache>
    {

        private static TTFRawCache _Instance = new TTFRawCache();
        public static TTFRawCache Instance = _Instance;

    }
}
