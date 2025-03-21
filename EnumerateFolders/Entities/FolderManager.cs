using EnumerateFolders.Services;
using EnumerateFolders.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace EnumerateFolders.Entities
{
    public class FolderManager
    {
        /*
        we want:  folder - count of files, count of folders

        map <folderhash, file count/list>	-->		all files with that folder hash
        map <folderhash, folder count/list>	-->		sub-folders with same parent folder hash

        */

        Dictionary<string, List<string>> _filelist;     // folderfullpath hash - filenamehash  hash --> filefullpath hash
        Dictionary<string, List<string>> _folderlist;   // folderfullpath hash - subfoldername hash --> subfolder fullpath hash 

        public FolderManager() 
        {
            _filelist = new Dictionary<string, List<string>>();
            _folderlist = new Dictionary<string, List<string>>();
        }


        /*
	        int64_t GetFolderCount(string folderfullpath);
	        int64_t GetFileCount(string folderfullpath);

	        void AddFile(string folderfullpath, string filename);
	        void AddChildFolder(string folderfullpath, string subfoldername);
	        void EnumerateFolders(string fullpath, list<string>& folderlistfullpath);
	        void EnumerateFiles(string fullpath, list<string>& filelistfullpath);
	        int64_t ComputeParentFolderSize(string fullpath);
	        void PopulateFolderData(set<Folder> &folderset);
	        void PopulateFileData(set<File> &fileset);          
         */

        long GetFolderCount(string folderfullpath)
        {
            long count = 0;
            string hash = Hash.getHashSha256(folderfullpath);

            if (_folderlist.ContainsKey(hash))
            {
                List<string> temp = _folderlist[hash];
                count = temp.Count;
            }
            return count;
        }

        long GetFileCount(string folderfullpath)
        {
            long count = 0;
            string hash = Hash.getHashSha256(folderfullpath);
            if (_filelist.ContainsKey(hash))
            {
                List<string> temp = _filelist[hash];
                count = temp.Count;
            }
            return count;
        }

        public void AddFile(string folderfullpath, string filename)
        {
            string folderfullpathhash = Hash.getHashSha256(folderfullpath);
            string absolutePath = Path.Combine(folderfullpath, filename);
            string filehash = Hash.getHashSha256(absolutePath);

            if (_filelist.ContainsKey(folderfullpathhash))
            {
                List<string> temp = _filelist[folderfullpathhash];
                temp.Add(filehash);
                _filelist[folderfullpathhash] = temp;
            }
            else
            {
                List<string> temp = new List<string>();
                temp.Add(filehash);
                _filelist[folderfullpathhash] = temp;
            }
        }

        public void AddChildFolder(string folderfullpath, string subfoldername)
        {
            string hash = Hash.getHashSha256(folderfullpath);
            string absolutePath = Path.Combine(folderfullpath, subfoldername);
            string subfolderhash = Hash.getHashSha256(absolutePath);

            if (_folderlist.ContainsKey(hash))
            {
                List<string> temp = _folderlist[hash];
                if (temp.Find(c => c == subfolderhash) == null)
                {
                    temp.Add(subfolderhash);
                    _folderlist[hash] = temp;
                }
            }
            else
            {
                List<string> temp = new List<string>();
                temp.Add(subfolderhash);
                _folderlist[hash] = temp;
            }
        }

        void EnumerateFolders(string fullpath, List<string> folderlistfullpath)
        {
            FolderInfoRepository repo = new FolderInfoRepository();
            string hash = Hash.getHashSha256(fullpath);

            // Files are stored as:  folderfullpath hash - subfolder fullpath hash
            if (_folderlist.ContainsKey(hash))
            {
                List<string> subfolders = _folderlist[hash];
                foreach (string subfoldernamehash in subfolders)
                {
                    // get subfolder name from sub-foldername hash
                    if (_folderlist.ContainsKey(subfoldernamehash))
                    {
                        string subfoldername = repo.GetFullPath(subfoldernamehash);
                        string absolutePath = Path.Combine(fullpath, subfoldername);
                        folderlistfullpath.Add(absolutePath);
                    }
                }
            }
        }

        void EnumerateFiles(string fullpath, out List<string> filelistfullpath)
        {
            filelistfullpath = new List<string>();
            FolderInfoRepository repo = new FolderInfoRepository();
            string hash = Hash.getHashSha256(fullpath);

            // folderfullpath hash - filefullpath hash
            if (_filelist.ContainsKey(hash))
            {
                List<string> files = _filelist[hash];
                foreach (string fullpathhash in files)
                {
                    string filename = repo.GetFileName(fullpathhash);
                    string absolutePath = Path.Combine(fullpath, filename);

                    if (!filelistfullpath.Contains(absolutePath))
                    {
                        filelistfullpath.Add(absolutePath);
                    }
                }
            }
        }

        public void PopulateFolderData()
        {
            FolderInfoRepository repo = new FolderInfoRepository();
            IEnumerable<Folder> folderlist = new List<Folder>();
            repo.GetAllFolders(out folderlist, "", "");

            foreach (Folder f in folderlist)
            {
                if (f.Name != string.Empty)
                {
                    AddChildFolder(f.Path, f.Name);
                }
            }
        }

        public void PopulateFileData()
        {
            FolderInfoRepository repo = new FolderInfoRepository();
            List<File> filelist = (List<File>)repo.GetAllFiles();

            foreach (File f in filelist)
            {
                string folderfullpath = repo.GetFullPath(f.FolderHash);

                // temp
                if (folderfullpath == "G:\\My Drive\\EBooks\\C#")
                {
                    int xc = 0;
                }
                AddFile(folderfullpath, f.Name);
            }
        }


        public long ComputeFolderSize(string fullpath)
        {
            /*
                1. Get list of subfolders
                2. Compute size of each subfolder:
                    get size of each subfolder from FolderManager
                    get size of each file from FolderManager
            */


            long size = 0;
            FolderInfoRepository repo = new FolderInfoRepository();

            IEnumerable<Folder> folderlist = new List<Folder>();
            List<Folder> folderlist_filtered = new List<Folder>();
            repo.GetAllFolders(out folderlist, "", fullpath);       // this is recursive

            // remove non-base folders
            foreach (Folder folder in folderlist)
            {
                if (folder.Path != fullpath)
                    folderlist_filtered.Add(folder);
            }


            string parentpath = fullpath; 
            string parentpathhash = Hash.getHashSha256(parentpath);

            if (_folderlist.ContainsKey(parentpathhash))
            {
                List<string> subfolders = _folderlist[parentpathhash];
                foreach (string subfoldernamehash in subfolders)
                {
                    string subfoldername = repo.GetFullPath(subfoldernamehash);
                    Folder foldertemp;
                    if (repo.FolderExists(subfoldername, out foldertemp))
                    {
                        size += foldertemp.FolderSize;
                    }
                }

                if (_filelist.ContainsKey(parentpathhash))
                {
                    List<string> files = _filelist[parentpathhash];
                    foreach (string filefullpathhash in files)
                    {
                        string parentfolder = parentpath;
                        string absolutePath = Path.Combine(parentfolder, repo.GetFileName(filefullpathhash));

                        File temp;
                        if (repo.FileExists(absolutePath, out temp))
                        {
                            size += temp.FileSize;
                        }
                    }
                }
            }

            // now handle all the subfolders which were not part of the root
            foreach (Folder f in folderlist_filtered)
            {
                parentpath = f.Path;
                parentpathhash = Hash.getHashSha256(parentpath);

                if (_folderlist.ContainsKey(parentpathhash))
                {
                    List<string> subfolders = _folderlist[parentpathhash];
                    foreach (string subfoldernamehash in subfolders)
                    {
                        string subfoldername = repo.GetFullPath(subfoldernamehash);
                        Folder foldertemp;
                        if (repo.FolderExists(subfoldername, out foldertemp))
                        {
                            size += foldertemp.FolderSize;
                        }
                    }

                    if (_filelist.ContainsKey(parentpathhash))
                    {
                        List<string> files = _filelist[parentpathhash];
                        foreach (string filefullpathhash in files)
                        {
                            string parentfolder = parentpath;
                            string absolutePath = Path.Combine(parentfolder, repo.GetFileName(filefullpathhash));

                            File temp;
                            if (repo.FileExists(absolutePath, out temp))
                            {
                                size += temp.FileSize;
                            }
                        }
                    }
                }
            }
            return size;
        }
    }
}

