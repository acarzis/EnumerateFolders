using EnumerateFolders.Domain.Models;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnumerateFolders.Database
{
    class PathCtx : DbContext
    {
        string connectionstring = "Server=zeus;Database=FolderEF;Trusted_Connection=True;";

        /*
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string FullPathName { get; set; }

        public int PathType { get; set; }
        */


        public DbSet<Path> Paths { get; set; }                          // Path contains PathEntry Value

    //    public DbSet<PathEntry> PathEntries { get; set;  }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
            modelBuilder.Entity<File>().HasBaseType<PathEntry>();
            modelBuilder.Entity<Folder>().HasBaseType<PathEntry>();
            modelBuilder.Entity<Folder>().ToTable<Folder>("Folder");
            modelBuilder.Entity<File>().ToTable<File>("File");
            */

            modelBuilder.Entity<Folder>();
            modelBuilder.Entity<File>();
//            modelBuilder.Entity<Path>();




            //            modelBuilder.Entity<Path>().ToTable<Path>("Path");
            //modelBuilder.Ignore<PathEntry>();


            //   modelBuilder.Entity<Path>().Ignore(b => b.Value.);

        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionstring);
        }

    }

}

