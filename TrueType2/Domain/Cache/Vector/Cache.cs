using Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType2.Domain.Cache.Vector
{
    internal class Cache : Dictionary<string, FontCache>
    {

        private static Cache _Instance = new Cache();
        public static Cache Instance = _Instance;

    }
}
