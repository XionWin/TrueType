namespace TrueType.Domain
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
        private int? _kern = null;
        public int Cmap => _camp ?? 0;
        public int Glyf => _glyf ?? 0;
        public int Head => _head  ?? 0;
        public int Hhea => _hhea ?? 0;
        public int Hmtx => _hmtx ?? 0;
        public int Loca => _loca ?? 0;
        public int Name => _name ?? 0;
        public int Maxp => _maxp ?? 0;
        public int Kern => _kern ?? 0;

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
            _kern = table.ContainsKey("kern") ? (int)table["kern"] : null;
        }
    }
}
