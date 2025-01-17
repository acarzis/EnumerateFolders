using EnumerateFolders.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace EnumerateGUI
{
    public partial class DBConfigWindow : Window
    {
        public void ChangeDB_OnClick(object sender, RoutedEventArgs e)
        {
            FolderInfoRepository repo = new FolderInfoRepository();
            string dbFilePath = repo.GetConnectionString();
            string newDBFilePath = string.Empty;
            int start = dbFilePath.IndexOf("DataSource=");
            if (start >= 0)
            {
                dbFilePath = dbFilePath.Substring(start + 11);
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select database file";
            openFileDialog.Filter = "SQLite db files (*.db)|*.db";
            openFileDialog.CheckFileExists = false;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(dbFilePath);

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                newDBFilePath = openFileDialog.FileName;
            }

            if (!String.Equals(dbFilePath, newDBFilePath, StringComparison.OrdinalIgnoreCase))
            {
                DBLocation.Text = newDBFilePath;
                repo.SetConnectionString(newDBFilePath);
            }
        }
    }
}
