using EnumerateFolders.Entities;
using EnumerateFolders.Services;
using EnumerateFolders.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace EnumerateFolders
{
    class Program
    {
        static void Main(string[] args)
        {
            FolderInfoRepository repo = new FolderInfoRepository();

            // let's add some categories
            Category cat = new Category();
            cat.Name = "Music";
            cat.Extensions = "mp3,wav,flac,m4a,vob";
            cat.FolderLocations = "Y:\\";
            repo.AddCategory(cat);

            cat = new Category();
            cat.Name = "Documents";
            cat.Extensions = "txt,doc,docx,pdf,htm,html,xls,xlsx,odt,ppt,pptx,rtf,wpd,log";
            repo.AddCategory(cat);

            cat = new Category();
            cat.Name = "Pictures";
            cat.Extensions = "jpeg,jpg,png,gif,tiff,bmp,webp,psd,raw";
            repo.AddCategory(cat);

            cat = new Category();
            cat.Name = "Videos";
            cat.Extensions = "mp4,mov,avi,wmv,flv,f4v,mkv,webm,avchd,mpeg,mpg,ogv,m4v";
            repo.AddCategory(cat);

            List<Category> categories = (List<Category>)repo.GetCategories();
            foreach (var c in categories)
            {
                Console.WriteLine("{0} - {1} ", c.Name, c.Extensions);
            }

            // add the drive list to the repo.
            string[] drives = DriveOperations.EnumerateDrives();
            foreach (string drive in drives)
            {
                DriveInfo d = new DriveInfo(drive);
                Console.WriteLine("Drive {0} added to the repo", drive);
                repo.AddDrive(drive, d.VolumeLabel);
            }

            Environment.Exit(0);



            string[] x = DriveOperations.EnumerateDrives();
            List<string> folders = new List<string>();
            List<string> files = new List<string>();

            try
            {
                DriveOperations.EnumerateFolders("c:\\", "*.*", ref folders);
                DriveOperations.EnumerateFiles("c:\\", "*.*", ref files);
            }
            catch (Exception)
            {
            }

            foreach (string f in folders)
            {
                repo.AddFolder(f);
            }

            foreach (string f in files)
            {
                repo.AddFile("c:\\", f);
            }


            repo.AddFolderDetails("AMD", "Music");
            DirectoryInfo di = new DirectoryInfo("c:\\AMD");
            long totalFolders = 0;
            long totalFiles = 0;
            long foldersize = DriveOperations.GetFolderSize(di, ref totalFiles, ref totalFolders, 1);

            Console.WriteLine("{0} - {1} ", totalFolders, totalFiles);
            repo.AddFolderDetails("c:\\AMD", "Music", foldersize);


            Console.WriteLine("AMD Folder Size: {0}    Category: {1}", repo.GetFolderSize("c:\\AMD"), repo.GetFolderCategory("c:\\AMD"));

            Category c1;
            DateTime lastchecked;
            DateTime lastmodified;

            repo.GetFolderDetails("c:\\AMD", out c1, out foldersize, out lastchecked, out lastmodified);
            Console.WriteLine("AMD Folder Size: {0}    Category: {1}", foldersize, c1.Name);

            IEnumerable<Folder> folders1 = new List<Folder>();
            repo.GetAllFolders(out folders1);

            foreach (Folder folder in folders1)
            {
                Console.WriteLine("Folder: {0}   Size: {1}    Category: {2}", string.IsNullOrEmpty(folder.Name) ? String.Empty : folder.Name,
                    folder == null ? 0 : folder.FolderSize, folder.Category == null ? String.Empty : folder.Category.Name);
            }

            repo.AddFileDetails("c:\\pagefile.sys", "Music", 123456789);


            foreach (string drive in x)
            {
                DriveInfo d = new DriveInfo(drive);
                string name = String.Empty;
                try
                {
                    Directory.GetFiles(drive); // to make drive ready
                    name = d.VolumeLabel;
                }
                catch { }

                repo.AddDrive(drive, name, 0);
            }

            IEnumerable<Drive> drives1 = new List<Drive>();
            repo.GetDriveList(out drives1);
            foreach (Drive drive in drives1)
            {
                Console.WriteLine("Drive {0}   Priority {1}", string.IsNullOrEmpty(drive.LogicalDrive) ? String.Empty : drive.LogicalDrive,
                    drive.ScanPriority);
            }

            Environment.Exit(0);
        }
    }
}


