namespace TrueType2.Domain.Support
{

    public static class TTFDefine
    {
        public const int TABLE_COUNT_OFFSET = 4;
        public const int TABLE_DIR_OFFSET = 12;
        public const int TABLE_DIR_STEP_LEN = 16;
        public const int TABLE_DIR_NAME_LEN = 4;
        public const int TABLE_DIR_DATA_OFFSET = 8;
        public const int TABLE_MAXP_GLYPHS_OFFSET = 4;
        public const int TABLE_CMAP_TABLES_OFFSET = 2;
    }
}
