using PhonoWriterWord.Database;
using PhonoWriterWord.Database.Controllers;
using PhonoWriterWord.Values;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhonoWriterWord
{
    /// <summary>
    /// Interaction logic for PWwpf.xaml
    /// </summary>
    public partial class PWwpf : UserControl
    {
        public DatabaseController dbc;
        public PWwpf()
        {
            InitializeComponent();
            this.dbc = dbc;
        }

        private void 
            Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
                var ic = new ImagesController(dbc);
                var wc = new WordsController(dbc);
                var fr = new Database.Models.Language(1, "fr");
                var wordObj = wc.ResearchByText(fr, item.Content.ToString());
                String path="";
                pictureBox.Source = null;
                if (wordObj != null)
                {
                    var img = ic.ResearchByWord(wordObj);
                    if (img!= null)
                    {
                        System.Diagnostics.Debug.WriteLine(img.FileName);
                        path = Constants.IMAGES + "\\" + img.FileName;
                        LoadImage(path);
                    }
                }
                
            }
        }

        public void LoadImage(string imagePath)
        {

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmapImage.EndInit();

            pictureBox.Source = bitmapImage;
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
