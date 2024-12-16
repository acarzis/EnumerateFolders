using EnumerateFolders.Entities;
using EnumerateFolders.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EnumerateGUI
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        Dictionary<string, Tuple<List<string>, List<string>>> categoryList = new Dictionary<string, Tuple<List<string>, List<string>>>();

        public ConfigWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            categoryList.Clear();
            FolderInfoRepository repo = new FolderInfoRepository();

            IEnumerable<Category> categories = repo.GetCategories();
            foreach (Category category in categories)
            {
                string[] extensions = { };
                string[] locations = { };

                if (category.Extensions != null)
                    extensions = category.Extensions.Split(',');

                if (category.FolderLocations != null)
                    locations = category.FolderLocations.Split(',');

                Tuple<List<string>, List<string>> temp = new Tuple<List<string>, List<string>>(extensions.ToList(), locations.ToList());
                categoryList.Add(category.Name, temp);
            }

            lbCategories.ItemsSource = categoryList.Keys;

            if (lbCategories.Items.Count > 0)
                lbCategories.SelectedItem = lbCategories.Items.GetItemAt(0);
        }

    }
}

