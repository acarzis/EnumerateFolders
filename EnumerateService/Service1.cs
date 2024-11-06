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

                ctx = new SqlSrvCtx();
                repo = new FolderInfoRepository(ctx);

                timer.Interval = 15000; // in milliseconds, 15 secs
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
            oSignalEvent.Reset();

            if ((running == false) && (shuttingDown == false))
            {
                eventLog1.WriteEntry("Timer Operation Started");

                running = true;

                try
                {
                    ctx = new SqlSrvCtx();
                    repo = new FolderInfoRepository(ctx);

                    IEnumerable<Drive> drives = new List<Drive>();
                    repo.GetDriveList(out drives);
                    drives = drives.OrderByDescending(s => s.ScanPriority);
                    IEnumerable<Category> categories = repo.GetCategories();

                    foreach (Drive d in drives)
                    {
                        d.LogicalDrive = MappedDriveResolver.ResolveToUNC(d.LogicalDrive);

                        if (d.ScanPriority >= 0)
                        {
                            eventLog1.WriteEntry("Processing Drive:  " + d.LogicalDrive);

                            repo.AddFolder(d.LogicalDrive);
                            Category category = new Category();
                            long fldrsize = 0;
                            DateTime lastchecked;
                            DateTime lastmodified;
                            repo.GetFolderDetails(d.LogicalDrive, out category, out fldrsize, out lastchecked, out lastmodified);

                            if ((fldrsize == 0) || (lastchecked >= lastmodified))
                            {
                                eventLog1.WriteEntry("Updating Folder Details:  " + d.LogicalDrive);

                                DirectoryInfo di = new DirectoryInfo(d.LogicalDrive);
                                long totalFiles = 0;
                                long totalFolders = 0;
                                long foldersize = DriveOperations.GetFolderSize(di, ref totalFiles, ref totalFolders, 1);
                                repo.AddFolderDetails(d.LogicalDrive, String.Empty, foldersize, di.LastWriteTimeUtc, true);

                                List<string> folderlist = new List<string>();
                                DriveOperations.EnumerateFolders(d.LogicalDrive, "*.*", ref folderlist);

                                List<string> filelist = new List<string>();
                                DriveOperations.EnumerateFiles(d.LogicalDrive, "*.*", ref filelist);
                                
                                foreach (string f in folderlist)
                                {
                                    eventLog1.WriteEntry("Adding Folder:  " + f);
                                    repo.AddFolder(f);  // f = fullpath

                                    di = new DirectoryInfo(f);
                                    totalFolders = 0;
                                    totalFiles = 0;
                                    foldersize = DriveOperations.GetFolderSize(di, ref totalFiles, ref totalFolders, 1); // also gets subfolder info
                                    repo.AddFolderDetails(f, String.Empty, foldersize, di.LastWriteTimeUtc, true);
                                }

                                foreach (string f in filelist)
                                {
                                    FileInfo fileInfo = new FileInfo(f);
                                    Category fileCategory = new Category();
                                    string cat = String.Empty;

                                    if (repo.GetFileCategory(fileInfo.Extension, out fileCategory))
                                    {
                                        if (fileCategory != null)
                                            cat = fileCategory.Name;
                                    }
                                    repo.AddFile(Path.GetDirectoryName(f), Path.GetFileName(f), cat, fileInfo.Length);
                                }
                            }
                        }
                    }
                }

                catch (Exception e)
                {
                    eventLog1.WriteEntry("OnTimer Exception: " + e.Message);
                }

                running = false;
                eventLog1.WriteEntry("Timer Operation Stopped");

                object waitTimeOut = new object();
                lock (waitTimeOut)
                {
                    Monitor.Wait(waitTimeOut, TimeSpan.FromHours(6));
                }

                timer.Start();
            }

            oSignalEvent.Set();
        }
    }
}

