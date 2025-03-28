﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnumerateFolders.Database;
using EnumerateFolders.Entities;
using EnumerateFolders.Utils;
using Microsoft.EntityFrameworkCore;
using File = EnumerateFolders.Entities.File;


namespace EnumerateFolders.Services
{
    public class FolderInfoRepository : IFolderInfoRepository, IDatabase
    {
        private SqlSrvCtx _context;
        private string _serviceAssemblyPath = string.Empty;

        // called from EnumerateService
        public void SetAssemblyLocation(string servicepath)
        {
            _serviceAssemblyPath = servicepath;

            _context = new SqlSrvCtx();
            _context.SetAssemblyLocation(_serviceAssemblyPath);
        }

        public string GetAssemblyLocation()
        {
            _context = new SqlSrvCtx();
            return _context.GetServiceAssemblyLocation();
        }


        // IDatabase methods
        public string GetConnectionString()
        {
            _context = new SqlSrvCtx();
            return _context.GetConnectionString();
        }

        public void SetConnectionString(string fulldbfilepath)
        {
            _context = new SqlSrvCtx();
            _context.SetConnectionString(fulldbfilepath);
        }
        // end IDatabase methods


        public string GetDBFilePath()
        {
            _context = new SqlSrvCtx();
            return _context.GetConnectionString();
        }

        public void SetDBFilePath(string path)
        {
            _context = new SqlSrvCtx();
            _context.SetConnectionString(path);
        }

        public IEnumerable<Category> GetCategories()
        {
            _context = new SqlSrvCtx();
            return _context.Categories.ToList();
        }

        public bool CategoryExists(string category, out Category cat)
        {
            _context = new SqlSrvCtx();

            var qry = _context.Categories.Where(c => c.Name == category).FirstOrDefault();
            if (qry != null)
            {
                cat = qry;
                return true;
            }
            else
            {
                cat = null;
                return false;
            }
        }

        public bool AddCategory(Category category)
        {
            try
            {
                _context = new SqlSrvCtx();

                _context.Categories.Add(category);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateCategory(Category category)
        {
            try
            {
                _context = new SqlSrvCtx();
                Category cat = _context.Categories.FirstOrDefault(f => f.Name == category.Name);
                if (cat != null)
                {
                    cat.Extensions = category.Extensions;
                    cat.FolderLocations = category.FolderLocations;
                    _context.Categories.Update(cat);
                    _context.SaveChanges();
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool AddFolder(string folderpath, string category = null, long foldersize = 0)
        {
            try
            {
                if (!Directory.Exists(folderpath))
                {
                    throw new ArgumentException("Invalid Folderpath");
                }

                _context = new SqlSrvCtx();

                Folder folder = new Folder();
                Category cat = new Category();

                if (!string.IsNullOrEmpty(category))
                {
                    if (CategoryExists(category, out cat))
                    {
                        folder.Category = cat;
                    }
                }

                folder.Name = Path.GetFileName(folderpath);
                folder.Path = Path.GetDirectoryName(folderpath);

                // below is to accomodate for mapped drives
                if (string.IsNullOrEmpty(folder.Path))
                {
                    folder.Path = Path.GetPathRoot(folderpath);
                    folder.Name = String.Empty;
                }

                folder.FullPathHash = Hash.getHashSha256(folderpath);
                folder.FolderSize = foldersize;
                _context.Folders.Add(folder);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddFolderDetails(string folderpath, string category = "", long foldersize = 0, DateTime lastmodified = new DateTime(), bool updateLastChecked = false)
        {
            try
            {
                _context = new SqlSrvCtx();

                if (lastmodified == DateTime.MinValue)
                    lastmodified = DateTime.UtcNow;

                Category cat = new Category();
                Folder folder = _context.Folders.Include(p => p.Category).FirstOrDefault(f => f.FullPathHash == Hash.getHashSha256(folderpath));
                if (folder != null)
                {
                    if (!string.IsNullOrEmpty(category))
                    {
                        if (CategoryExists(category, out cat))
                        {
                            folder.Category = cat;
                        }
                    }
                    if (foldersize > 0)
                    {
                        folder.FolderSize = foldersize;
                    }

                    if (updateLastChecked)
                    {
                        folder.LastChecked = DateTime.UtcNow;
                    }

                    if (lastmodified != DateTime.MinValue)
                    {
                        folder.LastModified = lastmodified;
                    }

                    _context.Update(folder);
                    _context.SaveChanges();
                    return true;
                }
                else
                {
                    throw new ArgumentException("Invalid Folderpath");
                }
            }

            catch (Exception)
            {
                return false;
            }
        }

        public long GetFolderSize(string folderpath)
        {
            try
            {
                _context = new SqlSrvCtx();
                Folder folder = _context.Folders.FirstOrDefault(f => f.FullPathHash == Hash.getHashSha256(folderpath));
                if (folder != null)
                {
                    return folder.FolderSize;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return 0;
        }

        public string GetFolderCategory(string folderpath)
        {
            try
            {
                _context = new SqlSrvCtx();
                Folder folder = _context.Folders.Include(p => p.Category).FirstOrDefault(f => f.FullPathHash == Hash.getHashSha256(folderpath));
                if (folder != null)
                {
                    return folder.Category.Name;
                }
            }
            catch (Exception)
            {
                return String.Empty;
            }
            return String.Empty;
        }

        public bool GetFolderDetails(string folderpath, out Category cat, out long foldersize, out DateTime lastchecked, out DateTime lastmodified)
        {
            cat = null;
            foldersize = 0;
            lastchecked = DateTime.MinValue;
            lastmodified = DateTime.MinValue;

            try
            {
                _context = new SqlSrvCtx();
                Folder folder = _context.Folders.Include(p => p.Category).FirstOrDefault(f => f.FullPathHash == Hash.getHashSha256(folderpath));
                foldersize = folder.FolderSize;
                lastchecked = folder.LastChecked;
                lastmodified = folder.LastModified;
                if (folder != null)
                {
                    cat = folder.Category;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool GetAllFolders(out IEnumerable<Folder> folders, string namesearchpattern = "", string pathbeginswith = "")
        {
            // sub-folders are also returned because this is method is based on a path search  
            folders = null;

            try
            {
                _context = new SqlSrvCtx();

                if ((!String.IsNullOrEmpty(namesearchpattern)) && (!String.IsNullOrEmpty(pathbeginswith)))
                {
                    folders = _context.Folders.Where(x => x.Name.ToLower().Contains(namesearchpattern.ToLower()) || x.Path.ToLower().Contains(pathbeginswith.ToLower())).Include(c => c.Category).ToList();
                }
                else if (!String.IsNullOrEmpty(namesearchpattern))
                {
                    folders = _context.Folders.Where(x => x.Name.ToLower().Contains(namesearchpattern.ToLower())).Include(c => c.Category).ToList();
                }
                else if (!String.IsNullOrEmpty(pathbeginswith))
                {
                    folders = _context.Folders.Where(x => x.Path.ToLower().Contains(pathbeginswith.ToLower())).Include(c => c.Category).ToList();
                }
                else
                {
                    folders = _context.Folders.Include(c => c.Category).ToList();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool AddFile(string folderpath, string filepath, string foldercategory = "", string filecategory = "", long filesize = 0)
        {
            try
            {
                _context = new SqlSrvCtx();
                filepath = Path.Combine(folderpath, filepath);

                Category cat = new Category();
                File file = _context.Files.Include(p => p.Category).Include(o => o.Folder).FirstOrDefault(f => f.FullPathHash == Hash.getHashSha256(filepath));
                Folder folder = new Folder();
                bool categoryExists = false;

                if (file == null)
                {
                    file = new File();
                }

                if (!string.IsNullOrEmpty(filecategory))
                {
                    if (CategoryExists(filecategory, out cat))
                    {
                        file.Category = cat;
                        categoryExists = true;
                    }
                }

                if (!FolderExists(folderpath, out folder))
                {
                    string foldercat = String.Empty;
                    if (!string.IsNullOrEmpty(foldercategory))
                    {
                        if (CategoryExists(foldercategory, out cat))
                        {
                            foldercat = cat.Name;
                        }
                    }

                    DirectoryInfo di = new DirectoryInfo(folderpath);
                    long totalFolders = 0;
                    long totalFiles = 0;
                    AddFolder(folderpath, foldercat, DriveOperations.GetFolderSize(di, ref totalFiles, ref totalFolders,  1));
                    FolderExists(folderpath, out folder);
                }
                file.Folder = folder;

                string filename = Path.GetFileName(filepath);
                if (string.IsNullOrEmpty(filename))
                {
                    throw new ArgumentException("Invalid Filepath");
                }
                file.Name = filename;
                file.FullPathHash = Hash.getHashSha256(filepath);
                file.FileSize = filesize;
                _context.Files.Add(file);

                if (categoryExists)
                {
                    _context.Entry(file.Category).State = EntityState.Unchanged;
                }
                _context.Entry(file.Folder).State = EntityState.Unchanged;
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddFileDetails(string filepath, string category = "", long filesize = 0)
        {
            try
            {
                _context = new SqlSrvCtx();

                Category cat = new Category();
                File file = _context.Files.FirstOrDefault(f => f.FullPathHash == Hash.getHashSha256(filepath));
                if (file != null)
                {
                    if (!string.IsNullOrEmpty(category))
                    {
                        if (CategoryExists(category, out cat))
                        {
                            file.Category = cat;
                        }
                    }
                    if (filesize > 0)
                    {
                        file.FileSize = filesize;
                    }
                    _context.Update(file);
                    _context.SaveChanges();
                    return true;
                }
                else
                {
                    throw new ArgumentException("Invalid Folderpath");
                }
            }

            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<File> GetAllFiles()
        {
            try
            {
                _context = new SqlSrvCtx();
                return _context.Files.Include(c => c.Category).Include(c => c.Folder).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool GetDriveList(out IEnumerable<Drive> drives)
        {
            drives = null;

            try
            {
                _context = new SqlSrvCtx();
                drives = _context.Drives.ToList();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool AddDrive(string drive, string name, int priority = 0)
        {
            try
            {
                _context = new SqlSrvCtx();

                Drive d = _context.Drives.FirstOrDefault(f => f.LogicalDrive == drive);
                if (d != null)
                {
                    d.Name = name;
                    d.ScanPriority = priority;
                    _context.Update(d);
                }
                else
                {
                    d = new Drive();
                    d.Name = name;
                    d.LogicalDrive = drive;
                    d.ScanPriority = priority;
                    _context.Drives.Add(d);
                }
                _context.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool GetFileCategory(string fileExtension, out Category cat)
        {
            cat = null;

            try
            {
                if (fileExtension.Length > 0)
                {
                    _context = new SqlSrvCtx();
                    bool match = false;
                    if (fileExtension.StartsWith("."))
                        fileExtension = fileExtension.Substring(1);

                    IEnumerable<Category> categories = GetCategories();
                    foreach (Category category in categories)
                    {
                        string[] words = category.Extensions.Split(',');
                        foreach (string word in words)
                        {
                            if (fileExtension == word)
                            {
                                match = true;
                                cat = category;
                                break;
                            }
                        }

                        if (match)
                            break;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool FolderExists(string folderpath, out Folder folder)
        {
            _context = new SqlSrvCtx();

            var qry = _context.Folders.Where(c => c.FullPathHash == Hash.getHashSha256(folderpath)).FirstOrDefault();
            if (qry != null)
            {
                folder = qry;
                return true;
            }
            else
            {
                folder = null;
                return false;
            }
        }

        public void AddPathToScanQueue(string fullpath, int priority)
        {
            try
            {
                _context = new SqlSrvCtx();

                ToScanQueue toScanQueue = new ToScanQueue();
                toScanQueue.FullPathHash = Hash.getHashSha256(fullpath);
                toScanQueue.Name = Path.GetFileName(fullpath);
                string directoryname = Path.GetDirectoryName(fullpath);
                toScanQueue.Path = String.IsNullOrEmpty(directoryname) ? Path.GetPathRoot(fullpath) : directoryname;  // this will remove any trailing '\'
                toScanQueue.Priority = priority;
                _context.ToScanQueue.Add(toScanQueue);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool PathExistsinScanQueue(string fullpath)
        {
            var qry = _context.ToScanQueue.Where(c => c.FullPathHash == Hash.getHashSha256(fullpath)).FirstOrDefault();
            if (qry != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ToScanQueue GetNextQueueItem()
        {
            try
            {
                _context = new SqlSrvCtx();
                return _context.ToScanQueue.OrderBy(s => s.Id).OrderByDescending(s => s.Priority).FirstOrDefault();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public long GetQueueSize()
        {
            try
            {
                _context = new SqlSrvCtx();
                return _context.ToScanQueue.Count();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void RemoveQueueItem(long queueId)
        {
            try
            {
                _context = new SqlSrvCtx();

                var itemToRemove = _context.ToScanQueue.SingleOrDefault(x => x.Id == queueId);

                if (itemToRemove != null)
                {
                    _context.ToScanQueue.Remove(itemToRemove);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public long ComputeFolderSize(string folderpath)
        {
            // the number of subfolders and root files in the repo must match what the file system detects in order for size to be computed.

            long result = 0;

            try
            {
                _context = new SqlSrvCtx();
                IEnumerable<Folder> folders = _context.Folders.Where(x => x.Path.ToLower() == folderpath.ToLower());

                List<string> folderList = new List<string>();
                DriveOperations.EnumerateFolders(folderpath, "*.*", ref folderList);
                if (folderList.Count == folders.Count()) // TO DO: think about this. this may not imply we have folder counts for all the folders 
                {
                    foreach (Folder folder in folders)
                    {
                        if (folder.FolderSize != 0)
                            result += folder.FolderSize;
                        else
                            return 0;
                    }

                    // get the size of all the files
                    string debughash = Hash.getHashSha256(folderpath);
                    IEnumerable<File> files = _context.Files.Where(x => x.FolderHash == Hash.getHashSha256(folderpath)).Include(c => c.Category).Include(c => c.Folder).ToList();

                    List<string> fileList = new List<string>();
                    DriveOperations.EnumerateFiles(folderpath, "*.*", ref fileList);
                    if (fileList.Count == files.Count())
                    {
                        foreach (File file in files)
                        {
                            result += file.FileSize;
                        }
                    }
                    else
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        public void GetFolderExclusions(out IEnumerable<FolderExclusions> folderexclusions)
        {
            try
            {
                _context = new SqlSrvCtx();
                folderexclusions = _context.FolderExclusions.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string GetFileName(string filepathhash)
        {
            string result = String.Empty;

            try
            {
                _context = new SqlSrvCtx();
                IEnumerable<File> file = _context.Files.Where(x => x.FullPathHash == filepathhash);
                if (file != null)
                {
                    if (file.Count() > 1)
                    {
                        throw new FileNotFoundException("File not found - filepathhash: " + filepathhash);
                    }
                    foreach (File f in file)
                    {
                        result = f.Name;
                    }
                }
                else
                {
                    throw new FileNotFoundException("File not found - filepathhash: " + filepathhash);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public string GetFullPath(string folderhash)
        {
            string result = string.Empty;

            try
            {
                _context = new SqlSrvCtx();
                IEnumerable<Folder> folder = _context.Folders.Where(x => x.FullPathHash == folderhash);
                if (folder != null)
                {
                    if (folder.Count() > 1)
                    {
                        throw new FileNotFoundException("Folder not found - hash: " + folderhash);
                    }
                    foreach (Folder f in folder)
                    {
                        result = Path.Combine(f.Path, f.Name);
                    }
                }
                else
                {
                    throw new FileNotFoundException("Folder not found - hash: " + folderhash);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public bool FileExists(string filepath, out File file)
        {
            _context = new SqlSrvCtx();

            var qry = _context.Files.Where(c => c.FullPathHash == Hash.getHashSha256(filepath)).FirstOrDefault();
            if (qry != null)
            {
                file = qry;
                return true;
            }
            else
            {
                file = null;
                return false;
            }
        }

        public bool GetAllFolders(out IEnumerable<Folder> folders, string folderpath)
        {
            folders = null;

            try
            {
                _context = new SqlSrvCtx();
                if (!String.IsNullOrEmpty(folderpath))
                {
                    folders = _context.Folders.Where(x => x.Path.ToLower().Equals(folderpath.ToLower())).Include(c => c.Category).ToList();
                }
                else
                {
                    folders = _context.Folders.Include(c => c.Category).ToList();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        public long RecomputeFolderSize(string path)
        {
            long foldersize = 0;
            List<string> folders = new List<string>();
            Folder temp = new Folder();
            File tempfile = new File();
            bool recurse = false;


            if (Directory.GetParent(path) == null)
                return 0;

            try
            {
                DriveOperations.EnumerateFolders(path, "*.*", ref folders, recurse);
                FolderInfoRepository repo = new FolderInfoRepository();
                IEnumerable<Folder> folderlist = new List<Folder>();
                repo.GetAllFolders(out folderlist, path);

                foreach (string folder in folders)
                {
                    if (repo.FolderExists(folder, out temp))
                    {
                        foldersize += temp.FolderSize;
                    }
                    else
                    {
                        DirectoryInfo di = new DirectoryInfo(folder);
                        long totalFolders = 0;
                        long totalFiles = 0;
                        long totalfoldersize = DriveOperations.GetFolderSize(di, ref totalFiles, ref totalFolders, 1);

                        repo.AddFolder(folder);
                        repo.AddFolderDetails(folder, "", totalfoldersize, di.LastWriteTimeUtc, false);
                        foldersize += totalfoldersize;
                    }
                }

                List<string> files = new List<string>();
                DriveOperations.EnumerateFiles(path, "*.*", ref files);
                foreach (string file in files)
                {
                    if (repo.FileExists(file, out tempfile))
                    {
                        foldersize += tempfile.FileSize;
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(file);
                        foldersize += fi.Length;
                    }
                }

                repo.AddFolder(path);
                repo.AddFolderDetails(path, String.Empty, foldersize, DateTime.UtcNow, false);
            }

            catch (Exception) 
            {
                throw;
            }
            return foldersize;
        }
    }
}