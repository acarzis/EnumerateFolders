using System;

namespace EnumerateFolders.Domain.Models
{
    class File : PathEntry
    {
        public string Name { get; set; }
        public Int64 FileSize { get; set; }
        public string Extension { get; set; }
    }
}
