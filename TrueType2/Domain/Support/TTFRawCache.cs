using Extension;
using System;
using TrueType2.Extension;

namespace TrueType2.Domain.Support
{
    internal class TTFRawCache : Dictionary<int, TTFVector>
    {
        public TTFRaw Raw { get; private set; }
        public TTFRawCache(TTFRaw raw)
        {
            Raw = raw;
        }

        public TTFVector TryGet(char c) =>
            this.Raw.GetGlyphIndex((int)c) is var index && this.ContainsKey(index) ?
                this[index]
                : this.Raw.GetVector(index).With(x => this.Add(index, x));
    }

}
