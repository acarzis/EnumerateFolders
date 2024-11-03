using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnumerateFolders.Entities
{
    public class File 
    {
        [Key, Column(Order = 0)]
        public string FullPathHash { get; set; }
        public string Name { get; set; }
        public Int64 FileSize { get; set; }

        public string FolderHash { get; set; }              // TO DO: Why do we need this? convenience? 
        [ForeignKey("FolderHash")]
        public virtual Folder Folder { get; set; }

        [ForeignKey("CategoryName")]
        public virtual Category Category { get; set; }
    }
}
