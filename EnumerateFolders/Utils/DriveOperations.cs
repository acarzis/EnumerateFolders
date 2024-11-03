using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EnumerateFolders.Utils
{
    public class DriveOperations
    {
        static List<Tuple<string, long>> mRootSubFolderInfo = new List<Tuple<string, long>>();

        public struct MyFileInfo
        {
            public string name { get; set; }
            public string directoryName { get; set; }
            public string extension { get; set; }
            public long fileLength { get; set; }
            public string driveName { get; set; }
            public bool isDirectory { get; set; }
        }

        public static string[] EnumerateDrives()
        {
            return Directory.GetLogicalDrives();
        }

        public static void EnumerateFolders(string path, string searchString, ref List<string> result)
        {
            var files = from file in Directory.EnumerateDirectories(path, searchString, SearchOption.TopDirectoryOnly)
                        select new {
                            File = file
                        };

            try
            {
                foreach (var s in files) { 
                   result.Add(s.File);
                };
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void EnumerateFiles(string path, string searchString, ref List<string> result)
        {
            try
            {
                var files = from file in Directory.EnumerateFiles(path, searchString, SearchOption.TopDirectoryOnly)
                            select new
                            {
                                File = file
                            };

                foreach (var s in files)
                {
                    result.Add(s.File);
                };
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // TO DO: change input such that folderpath instead of di
        public static long GetFolderSize(DirectoryInfo di, ref long totalFiles, ref long totalFolders, int level)
        {
            long folderSize = 0;
            int currentLevel = level;

            try
            {
                FileInfo[] fis = di.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    folderSize += fi.Length;
                    totalFiles += 1;
                }
            }
            catch {
                return 0;
            }

            try
            {
                DirectoryInfo[] dis = di.GetDirectories();

                foreach (DirectoryInfo d in dis)
                {
                    totalFolders += 1;
                    currentLevel += 1;
                    long sz = GetFolderSize(d, ref totalFiles, ref totalFolders, currentLevel);
                    folderSize += sz;

                    if (level == 1)
                    {
                        mRootSubFolderInfo.Add(new Tuple<string, long>(d.Name, sz));
                    }
                }
            }
            catch {
                return 0;
            }

            return folderSize;
        }


        public void EnumerateDirectoryInfo(List<string> files, ref List<MyFileInfo> filesinfo)
        {
            foreach (var f in files)
            {
                System.IO.DirectoryInfo fi = null;
                try
                {
                    fi = new System.IO.DirectoryInfo(f);
                    MyFileInfo finfo = new MyFileInfo();
                    finfo.name = fi.Name;
                    finfo.directoryName = fi.Name;
                    finfo.extension = fi.Extension;
                    finfo.driveName = fi.Root.FullName;
                    finfo.isDirectory = true;
                    finfo.fileLength = 0;
                    filesinfo.Add(finfo);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    continue;
                }

            }
        }

        public void EnumerateFileInfo(List<string> files, ref List<MyFileInfo> filesinfo)
        {
            foreach (var f in files)
            {
                System.IO.FileInfo fi = null;
                try
                {
                    fi = new System.IO.FileInfo(f);
                    MyFileInfo finfo = new MyFileInfo();
                    finfo.name = fi.Name;
                    finfo.directoryName = fi.DirectoryName;
                    finfo.extension = fi.Extension;
                    finfo.fileLength = fi.Length;
                    finfo.driveName = fi.Directory.Root.FullName;
                    finfo.isDirectory = false;
                    filesinfo.Add(finfo);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    continue;
                }
            }
        }
    }
}
