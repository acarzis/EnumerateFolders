using EnumerateFolders.Services;
using System.Windows;

namespace EnumerateGUI
{
    /// <summary>
    /// Interaction logic for DBConfigWindow.xaml
    /// </summary>
    public partial class DBConfigWindow : Window
    {
        public DBConfigWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            FolderInfoRepository repo = new FolderInfoRepository();
            string dbFilePath = repo.GetConnectionString();
            int start = dbFilePath.IndexOf("DataSource=");
            if (start >= 0)
            {
                dbFilePath = dbFilePath.Substring(start+11);
            }
            DBLocation.Text = dbFilePath;
        }
    }
}
