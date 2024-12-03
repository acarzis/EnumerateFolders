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
                SearchFromCache(searchTextBox.Text, e.AddedItems[0].ToString(), showEmptyCats.IsChecked ? true : false);
            }
        }

        private void searchTextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search(searchTextBox.Text, "All");
                SearchFromCache(searchTextBox.Text, categoryComboBox.Text, showEmptyCats.IsChecked ? true: false);
            }
        }

        private void ShowEmptyCats_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item == showEmptyCats)
            {
                if (item.IsChecked)
                    SearchFromCache(searchTextBox.Text, categoryComboBox.Text, true);
                else
                    SearchFromCache(searchTextBox.Text, categoryComboBox.Text, false);
            }
        }
    }
}
