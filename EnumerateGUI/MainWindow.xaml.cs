using EnumerateFolders.Entities;
using EnumerateFolders.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        bool isStartup = true;
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
            backgroundWork.DoWork += InitCache;
            backgroundWork.RunWorkerCompleted += InitCacheDone;
            backgroundWork.RunWorkerAsync();
        }

        private void InitCache(object sender, DoWorkEventArgs e)
        {
            try
            {
                repoCache = new RepositoryCache();
            }
            catch (Exception)
            {
            }
        }

        private void InitCacheDone(object sender, RunWorkerCompletedEventArgs e)
        {
            isStartup = false;
            statusText.Text = "READY";
            Search(searchTextBox.Text, categoryComboBox.Text, showEmptyCats.IsChecked ? true : false);
        }

        private void InitTimers()
        {
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void InitGui()
        {
            FolderInfoRepository repo = new FolderInfoRepository();
            IEnumerable<Category> categories = repo.GetCategories();
            List<string> categoryComboboxItemList = new List<string>();

            categoryComboboxItemList.Add("All");
            foreach (Category category in categories) {
                categoryComboboxItemList.Add(category.Name);
            }
            categoryComboBox.ItemsSource = categoryComboboxItemList;
            categoryComboBox.SelectedIndex = 0;
        }

        private void Search(string textSearch, string category, bool includeEmptyCategory = true)
        {
            if (isStartup)
                return;

            FolderInfoRepository repo = new FolderInfoRepository();
            rows = new List<SearchResultRow>();
            searchFileCount = 0;
            searchFolderCount = 0;

            // search the folders
            lastSearchResultFolderList = new List<Folder>();
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
                row.IsDirectory = true;
                rows.Add(row);
                searchFolderCount++;
            }

            // search the files    
            lastSearchResultFileList = new List<EnumerateFolders.Entities.File>();
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
                row.IsDirectory = false;
                rows.Add(row);
                searchFileCount++;
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
    }
}
