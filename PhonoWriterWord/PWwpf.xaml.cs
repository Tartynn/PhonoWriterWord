﻿using Microsoft.Office.Interop.Word;
using PhonoWriterWord.Database;
using PhonoWriterWord.Database.Controllers;
using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Enumerations;
using PhonoWriterWord.Exceptions;
using PhonoWriterWord.Managers;
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
        public LanguagesManager lm;
        private ThisAddIn _app;

        public PWwpf()
        {
            InitializeComponent();
            this.dbc = dbc;
            this.lm = lm;
            this._app = _app;
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
                // below the only way atm when it doesn't crash when list item clicked
                var fr = new Database.Models.Language(1, "fr");

                // _app is null => crash
                //var language = _app.LanguagesManager.CurrentLanguage;

                // most promising, but LanguagesManager is null if not initialized first
                // but LanguagesManager.Initialize() returns hardcoded language ("CurrentLanguage = _languages[0]; //_app.Configuration.Language - 1];" on line 109 @LanguagesManager)
                //var lang = new Database.Models.Language(lm.CurrentLanguage.Id, lm.CurrentLanguage.Iso);
                //System.Diagnostics.Debug.WriteLine("Current language in listview " + language.Id + language.Iso);
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = sender as ComboBox;
            ThisAddIn.LanguageChanged(selection.Text);
        }
    }
}
