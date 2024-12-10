using EnumerateFolders.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnumerateFolders.Services
{
    public class RepositoryCache : IRepositoryCache
    {
        FolderInfoRepository repo = new FolderInfoRepository();
        IEnumerable<Folder> folderCache = new List<Folder>();
        IEnumerable<EnumerateFolders.Entities.File> fileCache = new List<EnumerateFolders.Entities.File>();
        long toScanQueueSize = 0;

        public RepositoryCache()
        {
            repo.GetAllFolders(out folderCache, String.Empty, String.Empty);
            fileCache = repo.GetAllFiles();
            toScanQueueSize = repo.GetQueueSize();
        }

        public IEnumerable<File> GetAllFiles()
        {
            return fileCache;
        }

        public void GetAllFolders(out IEnumerable<Folder> folders, string namesearchpattern, string pathsearchpattern)
        {
            folders = folderCache;

            if ((!String.IsNullOrEmpty(namesearchpattern)) && (!String.IsNullOrEmpty(pathsearchpattern)))
            {
                folders = folders.Where(x => x.Name.ToLower().Contains(namesearchpattern.ToLower()) || x.Path.ToLower().Contains(pathsearchpattern.ToLower())).ToList();
            }
            else if (!String.IsNullOrEmpty(namesearchpattern))
            {
                folders = folders.Where(x => x.Name.ToLower().Contains(namesearchpattern.ToLower())).ToList();
            }
            else if (!String.IsNullOrEmpty(pathsearchpattern))
            {
                folders = folders.Where(x => x.Path.ToLower().Contains(pathsearchpattern.ToLower())).ToList();
            }
            else
            {
                folders = folders.ToList();
            }
        }

        public long GetQueueSize()
        {
            return toScanQueueSize;
        }
    }
}
