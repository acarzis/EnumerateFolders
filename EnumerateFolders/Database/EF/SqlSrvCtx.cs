using EnumerateFolders.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnumerateFolders.Database
{
    public class SqlSrvCtx : DbContext
    {
        // string connectionstring = "Server=zeus\\sqlexpress;Database=FolderEF;Trusted_Connection=True;Encrypt=False";
        string connectionstring =    "Server=zeus\\sqlexpress;Database=FolderEF;Trusted_Connection=False;User Id=enumerate;Password=enumerate;Encrypt=False";

        // This code creates the DB objects:
        public DbSet<Category> Categories { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Drive> Drives { get; set; }
        public DbSet<ToScanQueue> ToScanQueue { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionstring);
        }
    }
}
