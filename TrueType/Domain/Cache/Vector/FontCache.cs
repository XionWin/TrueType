using Extension;
using System;
using TrueType.Extension;

namespace TrueType.Domain.Cache.Vector
{
    internal class FontCache : Dictionary<char, TTFVector>
    {
        public string FontName => this.Raw.Name;
        public TTFRaw Raw { get; private set; }
        public FontCache(TTFRaw raw)
        {
            Raw = raw;
        }

        public TTFVector TryGet(char character) =>
            ContainsKey(character) ?
                this[character] :
                Raw.GetVector(character).With(x => Add(character, x));
    }

}
