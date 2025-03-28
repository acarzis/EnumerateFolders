﻿using EnumerateFolders.Entities;
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
        // FileExists(filepath, out file);

        // Non-repository operations:
        // EnumerateDrives()
        // EnumerateFiles(folder)
        // GetFolderSize(folder path) - uses the filesystem to get the folder size, recursively


        // for Category processing
        IEnumerable<Category> GetCategories();
        bool AddCategory(Category category);
        bool CategoryExists(string category, out Category cat);
        bool GetFileCategory(string fileExtension, out Category cat);

        // for Folder processing
        bool AddFolder(string folderpath, string category, long foldersize);
        bool AddFolderDetails(string folderpath, string category, long foldersize, DateTime lastmodified, bool updateLastChecked);
        long GetFolderSize(string folderpath);
        string GetFolderCategory(string folderpath);
        bool GetFolderDetails(string folderpath, out Category cat, out long foldersize, out DateTime lastchecked, out DateTime lastmodified);
        bool GetAllFolders(out IEnumerable<Folder> folders, string namesearchpattern = "", string pathsearchpattern = "");
        bool FolderExists(string folderpath, out Folder folder);

        // for File processing
        bool AddFile(string folderpath, string filepath, string foldercategory = "", string filecategory = "", long filesize = 0);
        bool AddFileDetails(string filepath, string category, long filesize);
        IEnumerable<File> GetAllFiles();
        bool FileExists(string filepath, out File file);

        // for Drive processing
        bool GetDriveList(out IEnumerable<Drive> drives);
        bool AddDrive(string drive, string name, int priority = 0);

        // for ScanQueue processing
        void AddPathToScanQueue(string fullpath, int priority);
        ToScanQueue GetNextQueueItem();
        long GetQueueSize();
        void RemoveQueueItem(long queueId);
        bool PathExistsinScanQueue(string fullpath);

        // for determining folder size
        long ComputeFolderSize(string folderpath);

        // for FolderExclusions
        void GetFolderExclusions(out IEnumerable<FolderExclusions> folderexclusions);
    }
}
