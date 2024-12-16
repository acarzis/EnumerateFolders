using EnumerateFolders.Entities;
using EnumerateFolders.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DialogResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace EnumerateGUI
{
    public partial class ConfigWindow : Window
    {
        private void CategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            // refresh the extensions and folders config listbox
            ListBox item = sender as ListBox;
            if (item == lbCategories)
            {
                // categoryList
                string category = item.SelectedItem.ToString();

                if (categoryList.ContainsKey(category))
                {
                    Tuple<List<string>, List<string>> temp = categoryList[category];
                    lbExtensions.ItemsSource = temp.Item1;
                    lbFolders.ItemsSource = temp.Item2;
                };
            }
        }

        private void ExtensionsChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void FoldersChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public void Add_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            string type = string.Empty;

            if (item == CategoryAdd)
            {
                type = "Category";
            }
            else if (item == ExtensionAdd)
            {
                type = "Extension";
            }
            else if (item == FolderAdd)
            {
                type = "Folder";
            }

            if (type == "Folder")
            {
                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                folderBrowserDialog1.ShowNewFolderButton = false;
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result.ToString() == "OK")
                {
                    if (lbCategories.SelectedItem == null)
                        return;

                    string folderName = folderBrowserDialog1.SelectedPath;
                    string categoryName = lbCategories.SelectedItem.ToString();
                    FolderInfoRepository repo = new FolderInfoRepository();
                    Category category = new Category();

                    if (repo.CategoryExists(categoryName, out category))
                    {
                        if (string.IsNullOrEmpty(category.FolderLocations))
                            category.FolderLocations += folderName;
                        else
                            category.FolderLocations += "," + folderName;
                        repo.UpdateCategory(category);
                        Init();
                    }
                }
            }
            else
            {
                string inputBoxTitleText = "Add New " + type;
                string inputBoxText = "Enter a New " + type + ":";
                string inputRead = new InputBox(inputBoxText, inputBoxTitleText, String.Empty).ShowDialog();

                if (inputRead != String.Empty)
                {
                    FolderInfoRepository repo = new FolderInfoRepository();
                    Category category = new Category();
                    if (type == "Category")
                    {
                        category.Name = inputRead;
                        repo.AddCategory(category);
                    }
                    else if (type == "Extension")
                    {
                        if (lbCategories.SelectedItem == null)
                            return;

                        string categoryName = lbCategories.SelectedItem.ToString();

                        if (repo.CategoryExists(categoryName, out category))
                        {
                            if (string.IsNullOrEmpty(category.FolderLocations))
                                category.Extensions += inputRead;
                            else
                                category.Extensions += "," + inputRead;
                            repo.UpdateCategory(category);
                        }
                    }
                    Init();
                }
            }
        }

        public void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            string selection = string.Empty;
            string type = string.Empty;

            if (item == CategoryDelete)
            {
                if (lbCategories.SelectedItem == null)
                    return;

                type = "Category";
                selection = lbCategories.SelectedItem.ToString();
            }
            else if (item == ExtensionDelete)
            {
                if (lbExtensions.SelectedItem == null)
                    return;

                type = "Extension";
                selection = lbExtensions.SelectedItem.ToString();
            }
            else if (item == FolderDelete)
            {
                if (lbFolders.SelectedItem == null)
                    return;

                type = "Folder";
                selection = lbFolders.SelectedItem.ToString();
            }

            string messageBoxText = "Do you want to delete " + type + " " + selection + "?" ;
            string caption = type + " Deletion";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result;
            result = System.Windows.MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

            if (result == MessageBoxResult.Yes)
            {

            }
        }
    }
}
