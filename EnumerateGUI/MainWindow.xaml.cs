using EnumerateFolders.Entities;
using EnumerateFolders.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace EnumerateGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IEnumerable<Folder> lastSearchResultFolderList = new List<Folder>();
        IEnumerable<EnumerateFolders.Entities.File> lastSearchResultFileList = new List<EnumerateFolders.Entities.File>();

        public MainWindow()
        {
            InitializeComponent();
            InitGui();
        }

        private void InitGui()
        {
            FolderInfoRepository repo = new FolderInfoRepository();
            IEnumerable<Category> categories = repo.GetCategories();
            List<string> categoryComboboxItemList = new List<string>();    

            foreach (Category category in categories) {
                categoryComboboxItemList.Add(category.Name);
            }
            categoryComboBox.ItemsSource = categoryComboboxItemList;
            categoryComboBox.SelectedIndex = 0;


        }


        private void categoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            ComboBox cmb = sender as ComboBox;

            switch (categoryComboBox.SelectedItem.ToString())
            {
                case "1":
                    break;
                case "2":
                    break;
                case "3":
                    break;
            }
            */
        }

        private void searchTextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // do a search 
                string textSearch = searchTextBox.Text;
                string category = categoryComboBox.Text;

                FolderInfoRepository repo = new FolderInfoRepository();
                List<SearchResultRow> rows = new List<SearchResultRow>();

                // search the folders
                lastSearchResultFolderList = new List<Folder>();
                repo.GetAllFolders(out lastSearchResultFolderList);

                lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Name.ToLower().Contains(textSearch.ToLower()) || x.Path.ToLower().Contains(textSearch.ToLower()));
                lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category != null);
                lastSearchResultFolderList = lastSearchResultFolderList.Where(x => x.Category.Name == category);

                foreach (Folder f in lastSearchResultFolderList)
                {
                    SearchResultRow row= new SearchResultRow();
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
                lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Category != null);
                lastSearchResultFileList = lastSearchResultFileList.Where(x => x.Category.Name == category);

                foreach (EnumerateFolders.Entities.File f in lastSearchResultFileList)
                {
                    SearchResultRow row = new SearchResultRow();
                    row.Name = f.Name;
                    row.Path = Path.Combine(f.Folder.Path, f.Folder.Name);
                    row.CategoryName = f.Category.Name;
                    row.FileSize = f.FileSize;
                    rows.Add(row);
                }

                resultsDataGrid.RowBackground = Brushes.LightGreen;
                resultsDataGrid.ItemsSource = rows;
            }
        }
    }
}
