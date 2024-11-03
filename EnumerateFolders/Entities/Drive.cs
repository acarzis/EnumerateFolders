using System.ComponentModel.DataAnnotations;

namespace EnumerateFolders.Entities
{
    public class Drive
    {
        [Key]
        public string LogicalDrive { get; set; }
        public string Name { get; set; }
        public int ScanPriority { get; set; }
    }
}
