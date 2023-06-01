using Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType2.Domain.Support
{
    internal class TTFCache : Dictionary<string, TTFRawCache>
    {

        private static TTFCache _Instance = new TTFCache();
        public static TTFCache Instance = _Instance;

    }
}
