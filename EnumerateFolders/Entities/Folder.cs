using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnumerateFolders.Entities
{
    public class Folder
    {
        [Key]
        public string FullPathHash { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime LastChecked { get; set; }
        public Int64 FolderSize { get; set; }

        [ForeignKey("CategoryName")]
        public virtual Category Category { get; set; }
    }
}
