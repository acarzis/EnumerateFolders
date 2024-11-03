using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EnumerateFolders.Domain.Models
{
    class Path
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string FullPathName { get; set; }

        public int PathType { get; set; }
        public virtual PathEntry PathEntry { get; set; }

    }
}
