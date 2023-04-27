using PhonoWriterWord.Enumerations;
using PhonoWriterWord.Services.Log;
using PhonoWriterWord.Sources.Classes;
using System;
using System.Collections.Generic;
using Language = PhonoWriterWord.Database.Models.Language;

namespace PhonoWriterWord.Managers
{
    public sealed class LanguagesManager
    {
        #region Variables

        // Attributs
        private ThisAddIn _app;
        private List<Language> _languages;
        private Language _language;
        private Log _log;

        public Dictionary<string, int> words;
        public Dictionary<int, int> images;

        // Events/delegates
        public event EventHandler DictionaryChanged;

        #endregion

        #region Properties

        public List<Language> Languages
        {
            get { return _languages; }
        }

        public Language CurrentLanguage
        {
            get { return _language; }
            set
            {
                if (value == _language)
                    return;
                _language = value;
                //_log.Info("Dictionary : {0} [{1} words]", _language.Label, _language.Words.Count);

                words.Clear();
                images.Clear();
                int i = 0;
                foreach (var w in _language.Words)
                {
                    // try-catch needed to prevent crash
                    try
                    {
                        words.Add(w.Text.ToLower(), i);
                        images.Add(i, w.Image);
                        i++;
                    } catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception in get CurrentLanguage " + ex.Message);
                    }
                }
                DictionaryChanged?.Invoke(this, null);
            }
        }

        #endregion

        #region Constructor

        public LanguagesManager()
        {
            _app = (ThisAddIn)ThisAddIn.Current; //(App)App.Current;
            //_log = new Log(_app.LogService, GetType().Name);
            words = new Dictionary<string, int>();
            images = new Dictionary<int, int>();
        }

        #endregion

        #region Methods

        public Language GetLanguage(LanguagesEnum language)
        {
            return _languages[(int)language - 1];
        }

        public void Initialize()
        {
            // Check if dictionaries exist, otherwise, shutdown and ask for reinstall.
            //System.Diagnostics.Debug.WriteLine("_app : " + _app.Name);
            //System.Diagnostics.Debug.WriteLine("_app : " + _app.DatabaseController);
            if (_app == null)
            {
                System.Diagnostics.Debug.WriteLine("app is null in LanguagesManager.Initialize");
                return;
            }
            _languages = _app.DatabaseController.LanguagesController.ResearchAllLanguages();
            if (_languages == null)
            {
                System.Diagnostics.Debug.WriteLine("Error, languages null - LanguageManager.cs, line 90++");
                //MessageBox.Show(Resources.Strings.DictionaryError, Resources.Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //_app.Shutdown();
                return;
            }

            CurrentLanguage = _languages[0]; //_app.Configuration.Language - 1];
            System.Diagnostics.Debug.WriteLine("LanguagesManager initialized, currentLanguage = " + CurrentLanguage.Label);
        }

        #endregion
    }
}
