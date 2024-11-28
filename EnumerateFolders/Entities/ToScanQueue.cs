using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnumerateFolders.Entities
{
    public class ToScanQueue
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string FullPathHash { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int Priority { get; set; }
    }
}
