using System;
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
    public class FolderInfoRepository : IFolderInfoRepository
    {
        private SqlSrvCtx _context;

        public FolderInfoRepository()
        {
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

        public bool GetAllFolders(out IEnumerable<Folder> folders, string namesearchpattern = "", string pathsearchpattern = "")
        {
            folders = null;

            try
            {
                _context = new SqlSrvCtx();

                if ((!String.IsNullOrEmpty(namesearchpattern)) && (!String.IsNullOrEmpty(pathsearchpattern)))
                {
                    folders = _context.Folders.Where(x => x.Name.ToLower().Contains(namesearchpattern.ToLower()) || x.Path.ToLower().Contains(pathsearchpattern.ToLower())).Include(c => c.Category).ToList();
                }
                else if (!String.IsNullOrEmpty(namesearchpattern))
                {
                    folders = _context.Folders.Where(x => x.Name.ToLower().Contains(namesearchpattern.ToLower())).Include(c => c.Category).ToList();
                }
                else if (!String.IsNullOrEmpty(pathsearchpattern))
                {
                    folders = _context.Folders.Where(x => x.Path.ToLower().Contains(pathsearchpattern.ToLower())).Include(c => c.Category).ToList();
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

                    AddFolder(folderpath, foldercat);
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
    }
}
