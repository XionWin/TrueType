namespace TrueType2.Domain.Support
{
    public class TTFRawTable
    {
        private int? _camp = null;
        private int? _glyf = null;
        private int? _head = null;
        private int? _hhea = null;
        private int? _hmtx = null;
        private int? _loca = null;
        private int? _name = null;
        private int? _maxp = null;
        public int Cmap => _camp!.Value;
        public int Glyf => _glyf!.Value;
        public int Head => _head!.Value;
        public int Hhea => _hhea!.Value;
        public int Hmtx => _hmtx!.Value;
        public int Loca => _loca!.Value;
        public int Name => _name!.Value;
        public int Maxp => _maxp!.Value;

        public TTFRawTable(Dictionary<string, uint> table)
        {
            _camp = table.ContainsKey("cmap") ? (int)table["cmap"] : null;
            _glyf = table.ContainsKey("glyf") ? (int)table["glyf"] : null;
            _head = table.ContainsKey("head") ? (int)table["head"] : null;
            _hhea = table.ContainsKey("hhea") ? (int)table["hhea"] : null;
            _hmtx = table.ContainsKey("hmtx") ? (int)table["hmtx"] : null;
            _loca = table.ContainsKey("loca") ? (int)table["loca"] : null;
            _name = table.ContainsKey("name") ? (int)table["name"] : null;
            _maxp = table.ContainsKey("maxp") ? (int)table["maxp"] : null;
        }
    }
}
