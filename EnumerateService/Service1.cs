#define DEBUG

using System.ServiceProcess;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using EnumerateFolders.Database;
using EnumerateFolders.Services;
using System.Collections.Generic;
using EnumerateFolders.Entities;
using System.Linq;
using EnumerateFolders.Utils;
using System.IO;
using Microsoft.Win32;

namespace EnumerateService
{
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    public partial class Service1 : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        private System.Diagnostics.EventLog eventLog1;

        bool running = false;
        bool shuttingDown = false;

        SqlSrvCtx ctx = null;
        FolderInfoRepository repo = null;
        ManualResetEvent oSignalEvent = new ManualResetEvent(false);
        System.Timers.Timer timer = new System.Timers.Timer();

        public Service1()
        {
            InitializeComponent();

            try
            {
                eventLog1 = new System.Diagnostics.EventLog();
                if (!System.Diagnostics.EventLog.SourceExists("EnumerateService"))
                {
                    System.Diagnostics.EventLog.CreateEventSource("EnumerateService", "EnumerateServiceLog");
                }
                eventLog1.Source = "EnumerateService";
                eventLog1.Log = "EnumerateServiceLog";
            }
            catch
            { }
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStart");

            if (Start())
            {
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            }
            else
            {
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            }

            oSignalEvent.Set();
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            eventLog1.WriteEntry("In OnStart Completed");
        }

        public bool Start()
        {
            try
            {
                eventLog1.WriteEntry("Service Startup");

                Init();

                timer.Interval = 100; // in milliseconds, 1/10 sec
                timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
                timer.AutoReset = false;
                timer.Start();

                eventLog1.WriteEntry("Startup Success");
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Startup Exception: " + e.Message);
                return false;
            }
            return true;
        }

        protected override void OnStop()
        {
            shuttingDown = true;
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            oSignalEvent.WaitOne();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }


        /*            
        What do we want this service to do ?

        1. Get the category list
        2. Get the drive list
        3. Scan each folder for files that match something in the Category List.
                Add/Update a file in the repo with category,
           Record/update the folder size, last checked date, last modified date  
        4. Do not scan any folder which has been assigned a Category.
        5. Do not scan any folder who's last checked date > last modified date.
        6. Root folder:  Always record/update the folder size, last checked date, last modified date
        */

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // get next queue item and start processing it.

            oSignalEvent.Reset();

            if ((running == false) && (shuttingDown == false))
            {
                running = true;
                try
                {
                    Category cat;
                    DateTime lastchecked = DateTime.UtcNow, lastmodified = DateTime.UtcNow;
                    long fldrsize;

                    repo = new FolderInfoRepository();
                    ToScanQueue nextitemtoprocess = repo.GetNextQueueItem();

                    IEnumerable<Category> categories = repo.GetCategories();

                    List<Tuple<string, string>> categoryPaths = new List<Tuple<string, string>>();          // fullpath, category name
                    List<Tuple<string, string>> categoryExtensions = new List<Tuple<string, string>>();     // extension, category name
                    string[] locations = { };
                    string[] extensions = { };

                    foreach (Category category in categories)
                    {
                        extensions = category.Extensions.Split(',');
                        foreach (string extension in extensions)
                        {
                            if (extension != String.Empty)
                            {
                                Tuple<string, string> temp = new Tuple<string, string>(extension, category.Name);
                                categoryExtensions.Add(temp);
                            }
                        }
                    }

                    categories = categories.Where(c => !string.IsNullOrEmpty(c.FolderLocations)).ToList();
                    foreach (Category category in categories)
                    {
                        locations = category.FolderLocations.Split(',');
                        foreach (string location in locations)
                        {
                            Tuple<string, string> temp = new Tuple<string, string>(UNCPath(location), category.Name);
                            categoryPaths.Add(temp);
                        }
                    }


                    // process all the files from this folder, and add all the sub-folders for future processing
                    string fullpath = Path.Combine(nextitemtoprocess.Path, nextitemtoprocess.Name);
                    DirectoryInfo di = new DirectoryInfo(fullpath);
                    List<string> filelist = new List<string>();

                    bool exists = false;
                    if (repo.FolderExists(fullpath, out Folder tmpfolder))
                    {
                        repo.GetFolderDetails(fullpath, out cat, out fldrsize, out lastchecked, out lastmodified);
                        exists = true;
                    }

                    if ((di.LastWriteTimeUtc >= lastchecked) || (!exists))
                    {
                        try
                        {
                            DriveOperations.EnumerateFiles(fullpath, "*.*", ref filelist);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Access exception, directory: " + Path.Combine(nextitemtoprocess.Path, nextitemtoprocess.Name));
                            repo.RemoveQueueItem(nextitemtoprocess.Id);
                        }

                        long filelistSize = 0;
                        foreach (string f in filelist)
                        {
                            FileInfo fileInfo = new FileInfo(f);
                            Category fileCategory = new Category();
                            string catstr = String.Empty;

                            filelistSize += fileInfo.Length;

                            Tuple<string, string> matched = categoryExtensions.Find(t => fileInfo.Extension.Contains(t.Item1));  // fileInfo.Extension has a leading '.'
                            if (matched != null)
                            {
                                catstr = matched.Item2;
                            }

                            // below will create a folder in the repo if required
                            repo.AddFile(Path.GetDirectoryName(f), Path.GetFileName(f), String.Empty, catstr, fileInfo.Length);

#if DEBUG
                            Console.WriteLine("Adding file, directory: " + Path.GetDirectoryName(f) + ", filename: " + Path.GetFileName(f) + " to the repo");
#endif

                        }

                        // add all the sub-folders to the scan queue for future processing
                        List<string> folderlist = new List<string>();

                        try
                        {
                            DriveOperations.EnumerateFolders(Path.Combine(nextitemtoprocess.Path, nextitemtoprocess.Name), "*.*", ref folderlist);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Access exception, directory: " + Path.Combine(nextitemtoprocess.Path, nextitemtoprocess.Name));
                            repo.RemoveQueueItem(nextitemtoprocess.Id);
                        }

                        if (folderlist.Count == 0)
                        {
                            repo.AddFolderDetails(fullpath, String.Empty, filelistSize, new DateTime(), false);

                            // check the sub-folders of the parent folder. we are checking for folder size presence for all sub-folders
                            DirectoryInfo parent = Directory.GetParent(fullpath);
                            long foldersize = repo.ComputeFolderSize(parent.FullName);
                            if (foldersize > 0)
                            {
                                repo.AddFolderDetails(parent.FullName, String.Empty, foldersize, new DateTime(), false);
                            }
                        }

                        foreach (string f in folderlist)
                        {
                            lastchecked = DateTime.UtcNow;
                            lastmodified = DateTime.UtcNow;
                            if (repo.FolderExists(f, out Folder tempfolder))
                            {
                                repo.GetFolderDetails(f, out cat, out fldrsize, out lastchecked, out lastmodified);
                            }

                            if (lastmodified >= lastchecked)
                            {
                                if (!repo.PathExistsinScanQueue(f))
                                    repo.AddPathToScanQueue(f, (int)ScanPriority.MED);
#if DEBUG
                                Console.WriteLine("Adding sub-folder location: " + f + " to the scan queue");
#endif
                            }
                        }
                    }
                    repo.RemoveQueueItem(nextitemtoprocess.Id);

                    // get the category name if it exists.
                    string categoryname = String.Empty;
                    string tmp = fullpath;
                    if (!tmp.EndsWith("\\"))
                        tmp += "\\";

                    Tuple<string, string> found = categoryPaths.Find(t => tmp.Contains(t.Item1));
                    if (found != null)
                    {
                        categoryname = found.Item2;
                    }
                    repo.AddFolderDetails(fullpath, categoryname, 0, di.LastWriteTimeUtc, true); // true = update lastchecked date


                    // let's scan the sub-folders where a category is specified
                    foreach (string location in locations)
                    {
                        di = new DirectoryInfo(location);
                        lastchecked = DateTime.UtcNow;
                        lastmodified = DateTime.UtcNow;
                        exists = false;

                        string path = UNCPath(location);
                        if (path.EndsWith("\\"))
                            path = path.Remove(path.Length - 1);

                        if (repo.FolderExists(path, out Folder tempfolder))
                        {
                            repo.GetFolderDetails(path, out cat, out fldrsize, out lastchecked, out lastmodified);
                            exists = true;
                        }

                        if ((di.LastWriteTimeUtc >= lastchecked) || (!exists))          // TO DO: handle folder sizes
                        {
                            if (!repo.PathExistsinScanQueue(UNCPath(location)))
                                repo.AddPathToScanQueue(UNCPath(location), (int)ScanPriority.HIGH);

#if DEBUG
                            Console.WriteLine("Adding category folder location: " + location + " (" + UNCPath(location) + ") to the scan queue");
#endif
                        }
                    }
                }

                catch (Exception e)
                {
                    eventLog1.WriteEntry("OnTimer Exception:  " + e.ToString());
                }

                running = false;

                object waitTimeOut = new object();
                lock (waitTimeOut)
                {
                    Monitor.Wait(waitTimeOut, TimeSpan.FromMilliseconds(Globals.OnTimerWaitDelayMSecs));
                }

                timer.Start();
            }

            oSignalEvent.Set();
        }

        private bool Init()
        {
            // returns true on error
            bool result = false;

            try
            {
                repo = new FolderInfoRepository();

                IEnumerable<Drive> drives = new List<Drive>();
                repo.GetDriveList(out drives);
                drives = drives.OrderByDescending(s => s.ScanPriority);

                foreach (Drive d in drives)
                {
                    d.LogicalDrive = UNCPath(d.LogicalDrive);

                    if (d.ScanPriority >= 0)    // '<0' signifies disabled/don't scan it
                    {
#if DEBUG
                        Console.WriteLine("Adding drive to processing queue:  " + d.LogicalDrive);
#endif
                        if (!repo.PathExistsinScanQueue(d.LogicalDrive))
                            repo.AddPathToScanQueue(d.LogicalDrive, (int)ScanPriority.MEDHIGH); // trailing '\' is not added

                    }
                }
            }

            catch (Exception e)
            {
                result = true;
            }
            return result;
        }

        // as per :  https://stackoverflow.com/questions/2067075/how-do-i-determine-a-mapped-drives-actual-path
        private static string UNCPath(string path)
        {
            if (!path.StartsWith(@"\\"))
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Network\\" + path[0]))
                {
                    if (key != null)
                    {
                        return key.GetValue("RemotePath").ToString() + path.Remove(0, 2).ToString();
                    }
                }
            }
            return path;
        }

    }
}
