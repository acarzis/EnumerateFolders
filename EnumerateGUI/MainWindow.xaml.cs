using EnumerateFolders.Entities;
using EnumerateFolders.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void categoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;

            if (cmb == categoryComboBox)
            {
                Search(searchTextBox.Text, e.AddedItems[0].ToString());
            }
        }

        private void searchTextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search(searchTextBox.Text, categoryComboBox.Text);
            }
        }

        private void Search(string textSearch, string category)
        {
            FolderInfoRepository repo = new FolderInfoRepository();
            List<SearchResultRow> rows = new List<SearchResultRow>();

            // search the folders
            lastSearchResultFolderList = new List<Folder>();
            repo.GetAllFolders(out lastSearchResultFolderList, textSearch, textSearch);

            lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category != null);

            if (category != "All")
            {
                lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category.Name != String.Empty);
            }
            else
            {
                lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category.Name == category);
            }

            foreach (Folder f in lastSearchResultFolderList)
            {
                SearchResultRow row = new SearchResultRow();
                row.Name = f.Name;
                row.Path = f.Path;
                row.CategoryName = f.Category.Name;
                row.FileSize = f.FolderSize;
                rows.Add(row);
            }

            // search the files    
            lastSearchResultFileList = new List<EnumerateFolders.Entities.File>();
            lastSearchResultFileList = repo.GetAllFiles();

            lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()));

            if (category != "All")
            {
                lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Category != null);
                lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Category.Name == category);
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
            }

            resultsDataGrid.RowBackground = Brushes.LightGreen;
            resultsDataGrid.ItemsSource = rows;
        }

        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;

            statusTime.Text = time.ToString();
            timer.Start();
        }
    }
}
