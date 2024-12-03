using EnumerateFolders.Entities;
using EnumerateFolders.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;


namespace EnumerateGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            FolderInfoRepository repo = new FolderInfoRepository();
            rows = new List<SearchResultRow>();
            searchFileCount = 0;
            searchFolderCount = 0;

            // search the folders
            lastSearchResultFolderList = new List<Folder>();
            repo.GetAllFolders(out lastSearchResultFolderList, textSearch, textSearch);

            if (category != "All")
            {
                lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category != null);
                lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category.Name != String.Empty);
            }
            else
            {
                lastSearchResultFolderList = lastSearchResultFolderList.Select(x => x);
            }

            foreach (Folder f in lastSearchResultFolderList)
            {
                SearchResultRow row = new SearchResultRow();
                row.Name = f.Name;
                row.Path = f.Path;
                if (row.CategoryName != null)
                {
                    row.CategoryName = f.Category.Name;
                }
                row.FileSize = f.FolderSize;
                rows.Add(row);
                searchFolderCount++;
            }

            // search the files    
            lastSearchResultFileList = new List<EnumerateFolders.Entities.File>();
            lastSearchResultFileList = repo.GetAllFiles();

            if (category != "All")
            {
                lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Category != null);
                lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Category.Name == category);
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
                rows.Add(row);
                searchFileCount++;
            }

            resultsDataGrid.RowBackground = Brushes.LightGreen;
            resultsDataGrid.ItemsSource = rows;

            searchQueueCount = repo.GetQueueSize();
            statusText.Text = "Folders: " + searchFolderCount + "  Files: " + searchFileCount + "  To Be Processed: " + searchQueueCount;
        }


        private void SearchFromCache(string textSearch, string category, bool includeEmptyCategory = true)
        {
            searchFileCount = 0;
            searchFolderCount = 0;
            rows = new List<SearchResultRow>();


            IEnumerable<Folder> copySearchResultFolderList = new List<Folder>();
            IEnumerable<EnumerateFolders.Entities.File> copySearchResultFileList = new List<EnumerateFolders.Entities.File>();

            copySearchResultFolderList = lastSearchResultFolderList;
            copySearchResultFileList = lastSearchResultFileList;

            if (category != "All")
            {
                copySearchResultFolderList = copySearchResultFolderList.Where(x => x.Category != null);
                copySearchResultFolderList = copySearchResultFolderList.Where(x => x.Category.Name != String.Empty);
            }
            else
            {
                copySearchResultFolderList = copySearchResultFolderList.Select(x => x);
            }

            foreach (Folder f in copySearchResultFolderList)
            {
                SearchResultRow row = new SearchResultRow();
                row.Name = f.Name;
                row.Path = f.Path;
                if (row.CategoryName != null)
                {
                    row.CategoryName = f.Category.Name;
                }
                row.FileSize = f.FolderSize;
                rows.Add(row);
                searchFolderCount++;
            }

            if (category != "All")
            {
                copySearchResultFileList = copySearchResultFileList.Where(x => x.Category != null);
                copySearchResultFileList = copySearchResultFileList.Where(x => x.Category.Name == category);
            }
            else
            {
                if (includeEmptyCategory)
                {
                    copySearchResultFileList = copySearchResultFileList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()));
                }
                else
                {
                    copySearchResultFileList = copySearchResultFileList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()) && (x.Category != null));
                }
            }

            foreach (EnumerateFolders.Entities.File f in copySearchResultFileList)
            {
                SearchResultRow row = new SearchResultRow();
                row.Name = f.Name;
                row.Path = Path.Combine(f.Folder.Path, f.Folder.Name);
                if (f.Category != null)
                {
                    row.CategoryName = f.Category.Name;
                }
                row.FileSize = f.FileSize;
                rows.Add(row);
                searchFileCount++;
            }

            resultsDataGrid.RowBackground = Brushes.LightGreen;
            resultsDataGrid.ItemsSource = rows;

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
