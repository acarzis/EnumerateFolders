using EnumerateFolders.Entities;
using EnumerateFolders.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Threading;


namespace EnumerateGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker backgroundWork = new BackgroundWorker();
        bool searchIsBusy = true;
        RepositoryCache repoCache;

        IEnumerable<Folder> lastSearchResultFolderList = new List<Folder>();
        IEnumerable<EnumerateFolders.Entities.File> lastSearchResultFileList = new List<EnumerateFolders.Entities.File>();
        DispatcherTimer timer = new DispatcherTimer();
        List<SearchResultRow> rows = new List<SearchResultRow>();
        long searchFileCount = 0;
        long searchFolderCount = 0;
        long searchQueueCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitGui();
            InitTimers();

            statusText.Text = "RETRIEVING DATA FROM THE DATABASE. PLEASE WAIT ...";
            refreshSearch.IsEnabled = false;
            backgroundWork.DoWork += InitCache;
            backgroundWork.RunWorkerCompleted += InitCacheDone;
            backgroundWork.RunWorkerAsync();
        }

        private void InitCache(object sender, DoWorkEventArgs e)
        {
            try
            {
                searchIsBusy = true;
                repoCache = new RepositoryCache();
            }
            catch (Exception)
            {
            }
        }

        private void InitCacheDone(object sender, RunWorkerCompletedEventArgs e)
        {
            searchIsBusy = false;
            statusText.Text = "READY";
            Search(searchTextBox.Text, categoryComboBox.Text, showEmptyCats.IsChecked ? true : false);
            refreshSearch.IsEnabled = true;
        }

        private void InitTimers()
        {
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void InitGui()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
            ManagementObjectCollection collection = searcher.Get();

            string servicepath = string.Empty;
            foreach (ManagementObject obj in collection)
            {
                string name = obj["Name"] as string;

                if (name == "EnumerateService")
                {
                    servicepath = obj["PathName"] as string;
                }
            }


            ServiceController sc = new ServiceController("EnumerateService");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                FolderInfoRepository repo = new FolderInfoRepository();
                repo.SetAssemblyLocation(servicepath);

                /*
                if (repo.GetAssemblyLocation() == string.Empty)
                {
                    // This condition should never be reached
                    string message = "Enumerate Service is Not Running";
                    MessageBox.Show(message, "Startup Error");

                    throw new ArgumentException("Service Assembly Location Not Set");
                }
                */

                IEnumerable<Category> categories = repo.GetCategories();
                List<string> categoryComboboxItemList = new List<string>();
                categoryComboboxItemList.Add("All");
                foreach (Category category in categories)
                {
                    categoryComboboxItemList.Add(category.Name);
                }
                categoryComboBox.ItemsSource = categoryComboboxItemList;
                categoryComboBox.SelectedIndex = 0;
            }
            else
            {
                string message = "Enumerate Service is Not Running";
                MessageBox.Show(message, "Startup Error");

                throw new ArgumentException("Service Assembly Location Not Set");
            }
        }

        private void Search(string textSearch, string category, bool includeEmptyCategory = true, bool includeFolders = true, bool includeFiles = true)
        {
            if (searchIsBusy)
                return;

            FolderInfoRepository repo = new FolderInfoRepository();
            rows = new List<SearchResultRow>();
            searchFileCount = 0;
            searchFolderCount = 0;

            // search the folders
            lastSearchResultFolderList = new List<Folder>();

            if (includeFolders)
            {
                repoCache.GetAllFolders(out lastSearchResultFolderList, textSearch, textSearch);

                if (category != "All")
                {
                    lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category != null);
                    lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category.Name == category);
                    lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()) || x.Path.ToLower().Contains(textSearch.ToLower()));
                }
                else
                {
                    if (includeEmptyCategory)
                    {
                        lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()) || x.Path.ToLower().Contains(textSearch.ToLower()));
                    }
                    else 
                    {
                        lastSearchResultFolderList = lastSearchResultFolderList.Where(x => (x.Name.ToLower().Contains(textSearch.ToLower()) || x.Path.ToLower().Contains(textSearch.ToLower())) && (x.Category != null));
                    }
                }

                foreach (Folder f in lastSearchResultFolderList)
                {
                    SearchResultRow row = new SearchResultRow();
                    row.Name = f.Name;
                    row.Path = f.Path;
                    if (f.Category != null)
                    {
                        row.CategoryName = f.Category.Name;
                    }
                    row.FileSize = f.FolderSize;
                    row.FileSizeStr = FileSize2String(f.FolderSize);
                    row.IsDirectory = true;
                    rows.Add(row);
                    searchFolderCount++;
                }
            }

            // search the files    
            lastSearchResultFileList = new List<EnumerateFolders.Entities.File>();

            if (includeFiles)
            {

                lastSearchResultFileList = repoCache.GetAllFiles();

                if (category != "All")
                {
                    lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Category != null);
                    lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Category.Name == category);
                    lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()));
                }
                else
                {
                    if (includeEmptyCategory)
                    {
                        lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()));
                    }
                    else
                    {
                        lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()) && (x.Category != null));
                    }
                }

                foreach (EnumerateFolders.Entities.File f in lastSearchResultFileList)
                {
                    SearchResultRow row = new SearchResultRow();
                    row.Name = f.Name;
                    row.Path = Path.Combine(f.Folder.Path, f.Folder.Name);
                    if (f.Category != null)
                    {
                        row.CategoryName = f.Category.Name;
                    }
                    row.FileSize = f.FileSize;
                    row.FileSizeStr = FileSize2String(f.FileSize);
                    row.IsDirectory = false;
                    rows.Add(row);
                    searchFileCount++;
                }
            }

            resultsDataGrid.ItemsSource = rows;
            resultsDataGrid.Columns[resultsDataGrid.Columns.Count - 1].Visibility = Visibility.Hidden;

            searchQueueCount = repoCache.GetQueueSize();
            statusText.Text = "Folders: " + searchFolderCount + "  Files: " + searchFileCount + "  To Be Processed: " + searchQueueCount;
        }

        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;

            statusTime.Text = time.ToString();
            timer.Start();
        }

        private string FileSize2String(long filesize)
        {
            string result = String.Empty;
            if (filesize > 1099511627776)
            {
                result = Convert.ToString(Math.Round(filesize/1099511627776.0, 3)) + " TB";
            }
            else if (filesize > 1073741824)
            {
                result = Convert.ToString(Math.Round(filesize / 1073741824.0, 3)) + " GB";
            }
            else if (filesize > 1048576)
            {
                result = Convert.ToString(Math.Round(filesize / 1048576.0, 3)) + " MB";
            }
            else if (filesize > 1024)
            {
                result = Convert.ToString(Math.Round(filesize / 1024.0, 3)) + " KB";
            }
            else
            {
                result = Convert.ToString(filesize);
            }
            return result;
        }
    }
}
