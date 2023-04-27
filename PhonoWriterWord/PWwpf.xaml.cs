using Microsoft.Office.Interop.Word;
using PhonoWriterWord.Database;
using PhonoWriterWord.Database.Controllers;
using PhonoWriterWord.Database.Models;
using PhonoWriterWord.Enumerations;
using PhonoWriterWord.Exceptions;
using PhonoWriterWord.Managers;
using PhonoWriterWord.Sources.Classes;
using PhonoWriterWord.Values;
using System;
using System.Drawing;
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
        private bool button1Clicked = false;
        private bool button2Clicked = false;
        private bool button3Clicked = false;
        private bool button4Clicked = false;
        private bool button5Clicked = false;
        private bool button6Clicked = false;

        private ListViewItem last = null;
        PredictionConfig config = PredictionsConfigManager.Config;
        private TtsManager _ttsManager;
        public bool IsTtsEnabled { get; set; } = true;



        public PWwpf()
        {
            InitializeComponent();
            this.dbc = dbc;
            this.lm = lm;
            this._app = _app;
            _ttsManager = new TtsManager();
            ThisAddIn.LanguageChangedEvent += OnLanguageChanged; // Subscribe to the new event


        }

        private void
            Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && config.PictographicActive)
            {
                String path = "";
                pictureBox.Source = null;
                if (item.Equals(last))
                {
                    CallAddIn(item);
                    return;
                }
                System.Diagnostics.Debug.WriteLine(item);
                var ic = new ImagesController(dbc);
                var wc = new WordsController(dbc);
                var language = Globals.ThisAddIn.LanguagesManager.CurrentLanguage;
                var wordObj = wc.ResearchByText(language, item.Content.ToString());
                if (wordObj != null)
                {
                    if (IsTtsEnabled)
                    {
                        _ttsManager.Speak(item.Content.ToString());
                    }
                    var img = ic.ResearchByWord(wordObj);
                    if (img != null)
                    {
                        System.Diagnostics.Debug.WriteLine(img.FileName);
                        path = Constants.IMAGES + "\\" + img.FileName;
                        LoadImage(path);
                    }
                }
                last = item;

            }
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            if (_ttsManager != null)
            {
                _ttsManager.SetVoiceForCurrentLanguage();
            }
        }

            public void CallAddIn(ListViewItem item)
            {
                ThisAddIn.KeyReturnPressed(item);
                this.mySelection.Content = "";
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
                    CallAddIn(item);
                }
            }

            private void ComboBox_SelectionChanged(object sender, EventArgs e)
            {
                var selection = sender as ComboBox;
                // selection.Text doesn't work properly
                string selectedItem = selection.SelectedItem.ToString();
                // a bit hacky, but at least this sends the language that user clicks on..
                ThisAddIn.LanguageChanged(selectedItem.Replace("System.Windows.Controls.ComboBoxItem: ", ""));
            }

            private void Button1_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                button1Clicked = !button1Clicked;

                if (button1Clicked)
                {
                    Button1.Content = "Classic off";
                    //Globals.ThisAddIn.PredictionsManager.DisablePrediction(PredictionsManager.PredictionsEnum.Classic);
                    config.PredictionClassicActive = false;

                }
                else
                {
                    Button1.Content = "Classic on";
                    //Globals.ThisAddIn.PredictionsManager.EnablePrediction(PredictionsManager.PredictionsEnum.Classic);
                    config.PredictionClassicActive = true;
                }
            }

            private void Button2_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                button2Clicked = !button2Clicked;
                if (button2Clicked)
                {
                    Button2.Content = "Fuzzy off";
                    //Globals.ThisAddIn.PredictionsManager.DisablePrediction(PredictionsManager.PredictionsEnum.Fuzzy);
                    config.PredictionFuzzyActive = false;
                }
                else
                {
                    Button2.Content = "Fuzzy on";
                    //Globals.ThisAddIn.PredictionsManager.EnablePrediction(PredictionsManager.PredictionsEnum.Fuzzy);
                    config.PredictionFuzzyActive = true;
                }
            }

            private void Button3_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                button3Clicked = !button3Clicked;
                if (button3Clicked)
                {
                    Button3.Content = "Reading off";
                    config.PredictionPhoneticActive = false;

            }
            else
                {
                    Button3.Content = "Reading on";
                    config.PredictionPhoneticActive = true;

            }
            ThisAddIn.Current.ToggleWordTextTts();

        }

        private void Button5_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                button5Clicked = !button5Clicked;
                if (button5Clicked)
                {
                    Button5.Content = "Pictographic off";
                    config.PictographicActive = false;
                }
                else
                {
                    Button5.Content = "Pictographic on";
                    config.PictographicActive = true;
                }
            }

            private void SliderClassic_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
            {
                var slider = (Slider)sender;
                int value = (int)slider.Value;
                config.PredictionClassicAmount = value;
            }

            private void SliderFuzzy_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
            {
                var slider = (Slider)sender;
                int value = (int)slider.Value;
                config.PredictionFuzzyAmount = value;
            }

            private void SliderClassicChar_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
            {
                var slider = (Slider)sender;
                int value = (int)slider.Value;
                config.PredictionClassicChars = value;
            }

            // Below can be removed

            //private void SliderPhonetic_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
            //{
            //    var slider = (Slider)sender;
            //    int value = (int)slider.Value;
            //    config.PredictionPhoneticAmount = value;
            //}

            //private void SliderPhoneticChar_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
            //{
            //    var slider = (Slider)sender;
            //    int value = (int)slider.Value;
            //    config.PredictionPhoneticChars = value;
            //}

            //private void SliderPhoneticUntil_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
            //{
            //    var slider = (Slider)sender;
            //    int value = (int)slider.Value;
            //    config.PredictionPhoneticUntil = value;
            //}

            private void Button4_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                // This also needs PredictionsManager to work.

                button4Clicked = !button4Clicked;
                if (button4Clicked)
                {
                    Button4.Content = "Enabled";
                    config.HidePictureless = true;
                }
                else
                {
                    Button4.Content = "Disabled";
                    config.HidePictureless = false;
                }
            }

            private void Button6_Click(object sender, System.Windows.RoutedEventArgs e)
            {
            button6Clicked = !button6Clicked;
            if (button6Clicked)
            {
                Button6.Content = "Phonetic off";
                config.PictographicActive = false;
                IsTtsEnabled = false;


            }
            else
            {
                Button6.Content = "Phonetic on";
                config.PictographicActive = true;
                IsTtsEnabled = true;

            }

        }

    }
}
