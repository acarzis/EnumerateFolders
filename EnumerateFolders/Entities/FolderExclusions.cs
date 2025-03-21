using System.ComponentModel.DataAnnotations;

namespace EnumerateFolders.Entities
{
    public class FolderExclusions
    {
        [Key]
        public string FullPath { get; set; }
    }
}
