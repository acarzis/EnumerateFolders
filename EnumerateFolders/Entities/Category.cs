using System.ComponentModel.DataAnnotations;

namespace EnumerateFolders.Entities
{
    public class Category
    {
        [Key]
        public string Name { get; set; }
        public string Extensions { get; set; }
        public string FolderLocations { get; set; }
    }
}
