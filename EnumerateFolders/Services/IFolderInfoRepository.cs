using EnumerateFolders.Entities;
using System;
using System.Collections.Generic;
using File = EnumerateFolders.Entities.File;


namespace EnumerateFolders.Services
{
    interface IFolderInfoRepository
    {
        // Defining our API. What operations do we need? ...

        // Repository operations:
        // GetCategories
        // AddCategory(name, extensionlist)
        // CategoryExists(category, out Category)
        // AddFolder(folder path, category, size) - category and size are optional
        // AddFolderDetails(folderpath, category, foldersize) - category and size are optional
        // GetFolderSize(folderpath)
        // GetFolderCategory(folderpath)
        // GetFolderDetails(folderpath) - retrieves category and size   
        // GetAllFolders(out folders)
        // AddFile(folderpath, filepath, category, size) - category and size are optional
        // AddFileDetails(string filepath, string category, long filesize)
        // GetAllFiles() - returns all files
        // GetDriveList(out drives)
        // AddDrive(drive, name, priority) - priority is optional
        // GetFileCategory(fileExtension, out category);
        // FolderExists(folderpath, out folder);


        // Non-repository operations (they belong in a different class):
        // EnumerateDrives()
        // EnumerateFiles(folder)
        // GetFolderSize(folder path)


        IEnumerable<Category> GetCategories();
        bool AddCategory(Category category);
        bool CategoryExists(string category, out Category cat);
        bool AddFolder(string folderpath, string category, long foldersize);
        bool AddFolderDetails(string folderpath, string category, long foldersize, DateTime lastmodified, bool updateLastChecked);
        long GetFolderSize(string folderpath);
        string GetFolderCategory(string folderpath);
        bool GetFolderDetails(string folderpath, out Category cat, out long foldersize, out DateTime lastchecked, out DateTime lastmodified);
        bool GetAllFolders(out IEnumerable<Folder> folders, string namesearchpattern = "", string pathsearchpattern = "");
        bool AddFile(string folderpath, string filepath, string foldercategory = "", string filecategory = "", long filesize = 0);
        bool AddFileDetails(string filepath, string category, long filesize);
        IEnumerable<File> GetAllFiles();
        bool GetDriveList(out IEnumerable<Drive> drives);
        bool AddDrive(string drive, string name, int priority = 0);
        bool GetFileCategory(string fileExtension, out Category cat);
        bool FolderExists(string folderpath, out Folder folder);

        // for support of adding a folder to the scan queue:
        void AddPathToScanQueue(string fullpath, int priority);
        ToScanQueue GetNextQueueItem();
        void RemoveQueueItem(long queueId);
    }
}
