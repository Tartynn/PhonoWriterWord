using System.Windows.Controls;
using System.Windows.Input;

namespace PhonoWriterWord
{
    /// <summary>
    /// Interaction logic for PWwpf.xaml
    /// </summary>
    public partial class PWwpf : UserControl
    {
        public PWwpf()
        {
            InitializeComponent();
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                System.Diagnostics.Debug.WriteLine(item.ToString());
            }
        }
        private void ListViewItem_EnterPressed(object sender, KeyEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected && e.Key == Key.Return)
            {
                ThisAddIn.KeyReturnPressed(item);
            }
            if (e.Key == Key.Space)
            {
                myList.Items.Add("uwu");
            }
        }
    }
}
