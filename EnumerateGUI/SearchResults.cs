using System;

namespace EnumerateGUI
{
    internal class SearchResultRow
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string CategoryName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeStr { get; set; }
        public bool IsDirectory { get; set; }
    }
}
