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
                Microsoft.Office.Interop.Word.Selection selection = Globals.ThisAddIn.Application.Selection;

                //if (selection.Range.Words.Count > 0)
                //{
                //    // get the word on which the cursor is
                //    Microsoft.Office.Interop.Word.Range wordRange = selection.Range.Words[1];

                //    // replace the word in Word doc by the word clicked on the list
                //    wordRange.Text = item.Content.ToString();

                //    // get the position of the end of the word
                //    int endPosition = wordRange.End;
                    
                //    // move the cursor to the end of the word
                //    selection.Start = endPosition;
                //    selection.End = endPosition;

                //}

                //selection.Range.Text = item.Content.ToString();

                System.Diagnostics.Debug.WriteLine(item);
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
