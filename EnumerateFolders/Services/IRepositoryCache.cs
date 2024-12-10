using EnumerateFolders.Entities;
using System.Collections.Generic;

namespace EnumerateFolders.Services
{
    interface IRepositoryCache
    {
        // operations done in the Search method of the GUI
        void GetAllFolders(out IEnumerable<Folder> folders, string namesearchpattern = "", string pathsearchpattern = "");
        IEnumerable<File> GetAllFiles();
        long GetQueueSize();
    }
}
