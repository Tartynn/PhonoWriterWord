using PhonoWriterWord.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Utils
{
    class RelationshipsLearner
    {
        private ThisAddIn _app;

        private Dictionary<string, int> _relationships = new Dictionary<string, int>();
        private int _token = 0;
        int _words = 0;


        void Store(Dictionary<string, int> relationships, string first, string second)
        {
            string key = first + " " + second;
            if (relationships.ContainsKey(key))
                relationships[key]++;
            else
                relationships.Add(key, 1);
        }

        public RelationshipsLearner()
        {

            _app = (ThisAddIn)ThisAddIn.Current;

            cboLanguage.ItemsSource = _app.LanguagesManager.Languages;
            cboLanguage.SelectedItem = _app.LanguagesManager.CurrentLanguage;

            tbxText.Focus();
            UpdateStatus();
        }

        void Clear()
        {
            tbxText.Text = string.Empty;
            pbProgress.Maximum = 1; // Reset progress bar.
            pbProgress.Value = 0;
            UpdateStatus();
        }

        new void Close()
        {
            Hide();
        }

        /*void Save()
		{
			Dictionary dictionary = (Dictionary)cboLanguage.SelectedItem;
			string text = tbxText.Text;

			// If there is only one word, just create it.
			MatchCollection words = regex.Matches(text);
			if (words.Count == 1)
			{
				_app.LanguagesManager.CurrentLanguage.Create(new Word(dictionary.Number, words[0].Value));
				return;
			}

			pbProgress.Maximum = pairs.Count;

			foreach (var pair in pairs)
			{
                Word first = dictionary.Create(new Word(dictionary.Number, pair.Item1));
				Word second = dictionary.Create(new Word(dictionary.Number, pair.Item2));

                _app.PredictionsService.UpdatePair(dictionary, first.Text, second.Text);

				pbProgress.Dispatcher.Invoke(() => pbProgress.Value++, DispatcherPriority.Background); // I didn't want to use a BackgroundWorker.
			}
		}*/

        void Save()
        {
            Language language = (Language)cboLanguage.SelectedItem;
            string text = tbxText.Text;

            // If there is only one word, just create it.
            if (_words == 1)
            {
                _app.LanguagesManager.CurrentLanguage.Create(new Word(language.Id, text.Trim(), Algorithms.Current(text.Trim(), (LanguagesEnum)language.Id)));
                return;
            }

            pbProgress.Maximum = _relationships.Count;

            foreach (var relationship in _relationships)
            {
                string[] couple = relationship.Key.Split(' ');

                Word first = language.Create(new Word(language.Id, couple[0], Algorithms.Current(couple[0], (LanguagesEnum)language.Id)));
                Word second = language.Create(new Word(language.Id, couple[1], Algorithms.Current(couple[1], (LanguagesEnum)language.Id)));

                _app.PredictionsService.UpdatePair(language, first.Text, second.Text, relationship.Value);

                pbProgress.Dispatcher.Invoke(() => pbProgress.Value++, DispatcherPriority.Background);
            }
        }

        void UpdateStatus()
        {
            int mytoken = _token;

            string text = tbxText.Text;

            string currentWord = "";
            string lastWord = "";

            bool isNewWord = true;
            int words = 0;


            Dictionary<string, int> relationships = new Dictionary<string, int>();

            Task.Run(() =>
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (_token != mytoken)
                        return;

                    char c = text[i];

                    switch (c)
                    {
                        // Word separation
                        case ' ':
                        case '-':
                        case '\'':
                            if (lastWord != "" && currentWord != "")
                                Store(relationships, lastWord, currentWord);

                            isNewWord = true;

                            lastWord = currentWord;
                            currentWord = "";

                            break;

                        // Sentence separation
                        case '\n':
                        case '\r':
                        case '.':
                        case ',':
                        case '!':
                        case '?':
                        case ';':
                            if (lastWord != "" && currentWord != "")
                                Store(relationships, lastWord, currentWord);

                            lastWord = "";
                            currentWord = "";

                            isNewWord = true;

                            break;

                        default:
                            currentWord += c;

                            if (isNewWord)
                            {
                                words++;
                                isNewWord = false;
                            }
                            break;
                    }
                }

                if (lastWord != "" && currentWord != "")
                    Store(relationships, lastWord, currentWord);

                _token = 0;
                _words = words;
                _relationships = relationships;

                string status = string.Format(Client.Resources.Strings.RelationshipsStatus, words, relationships.Count);

                Dispatcher.BeginInvoke((Action)(() => { tbxStatus.Text = status; }));
            });
        }

        #region Events

        //private void btnClose_Click(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}

        //private void btnSave_Click(object sender, RoutedEventArgs e)
        //{
        //    btnSave.IsEnabled = false;
        //    Save();
        //    btnSave.IsEnabled = true;
        //    Clear();
        //    Close();
        //}

        private void tbxText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _token++;
            UpdateStatus();
        }

        //private void Window_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //        Close();
        //}

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    e.Cancel = true; // Avoid window's destruction.
        //    Close();
        //}

        #endregion
    }
}
