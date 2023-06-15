using Extension;
using System;
using TrueType.Extension;

namespace TrueType.Domain.Cache.Vector
{
    internal class FontCache : Dictionary<int, TTFVector>
    {
        public string FontName => this.Raw.Name;
        public TTFRaw Raw { get; private set; }
        public FontCache(TTFRaw raw)
        {
            Raw = raw;
        }

        public TTFVector TryGet(char c) =>
            Raw.GetGlyphIndex(c) is var index && ContainsKey(index) ?
                this[index]
                : Raw.GetVector(index).With(x => Add(index, x));
    }

}
