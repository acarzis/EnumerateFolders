using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnumerateGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void categoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;

            if (cmb == categoryComboBox)
            {
                Search(searchTextBox.Text, e.AddedItems[0].ToString(), showEmptyCats.IsChecked ? true : false);
            }
        }

        private void searchTextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search(searchTextBox.Text, "All");
                Search(searchTextBox.Text, categoryComboBox.Text, showEmptyCats.IsChecked ? true : false);
            }
        }

        private void SearchMenuItem_Select(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item == refreshSearch)
            {
                if (!searchIsBusy)
                {
                    refreshSearch.IsEnabled = false;
                    statusText.Text = "RETRIEVING DATA FROM THE DATABASE. PLEASE WAIT ...";
                    backgroundWork.RunWorkerAsync();
                }
            }
            else
            {
                Search(searchTextBox.Text, categoryComboBox.Text, showEmptyCats.IsChecked, showFolders.IsChecked, showFiles.IsChecked);
            }
        }

        private void CategoriesConfig_Select(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow();
            configWindow.Owner = this;
            configWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            configWindow.ShowDialog();
        }
    }
}
