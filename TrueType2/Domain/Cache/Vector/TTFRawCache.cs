using Extension;
using System;
using TrueType2.Extension;

namespace TrueType2.Domain.Cache.Vector
{
    internal class TTFRawCache : Dictionary<int, TTFVector>
    {
        public TTFRaw Raw { get; private set; }
        public TTFRawCache(TTFRaw raw)
        {
            Raw = raw;
        }

        public TTFVector TryGet(char c) =>
            Raw.GetGlyphIndex(c) is var index && ContainsKey(index) ?
                this[index]
                : Raw.GetVector(index).With(x => Add(index, x));
    }

}
