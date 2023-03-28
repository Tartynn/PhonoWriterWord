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

        public void LoadImage(Database.Models.Language lan, String str)
        {
            var wc = new WordsController(dbc);
            var ic = new ImagesController(dbc);
            var wordObj = wc.ResearchByText(lan, str);
            String path = "";
            pictureBox.Source = null;
            if (wordObj != null)
            {
                var img = ic.ResearchByWord(wordObj);
                if (img != null)
                {
                    System.Diagnostics.Debug.WriteLine(img.FileName);
                    path = Constants.IMAGES + "\\" + img.FileName;
                    LoadImage(path);
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
        }

        private void ListViewItem_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                var fr = new Database.Models.Language(1, "fr");
                LoadImage(fr, item.Content.ToString());
            }
        }
    }
}
