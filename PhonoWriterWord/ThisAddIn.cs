using System;
using System.Collections.Generic;
using IEnumerableTrie.Simple;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Word;
using PhonoWriterWord.Database;
using PhonoWriterWord.Managers;
using PhonoWriterWord.Predictions; //used for tests purpose (when we call public Prediction instead of Manager/Controller/Service
using PhonoWriterWord.Services.Log;
using PhonoWriterWord.Services;
using System.IO;
using PhonoWriterWord.Values;
using PhonoWriterWord.Sources.Classes;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Word.Application;
using PhonoWriterWord.Predictions.Predictors;
using PhonoWriterWord.Database.Controllers;
using PhonoWriterWord.Enumerations;
using PhonoWriterWord.Database.Models;
using Timer = System.Windows.Forms.Timer;
//using PredictionsFoundArgs = PhonoWriterWord.Managers.PredictionsFoundArgs;
using System.Linq;
using System.Reflection;
using PhonoWriterWord.Utils;

namespace PhonoWriterWord



{
    public partial class ThisAddIn
    {
        #region Variables
        //user control
        public PWUserControl usr;
        // Custom task pane
        private Microsoft.Office.Tools.CustomTaskPane myCustomTaskPane;
        private ElementHost eh;
        private PWwpf wpf;

        private Timer _timer;
        private Word.Application _application;

        // Instances
        private Log _log;

        // Controllers
        public DatabaseController DatabaseController { get; private set; }


        // Variables
        private bool _demo ;
        private bool _predictionSelected;     // Flag to know if a prediction has been selected in the predictions window.
        private string _previousInput = "";
        private string _context;
        private string _currentInput;
        private string _selectedPrediction;     // Return selected prediction.
        private DateTime _lastLog;              // Date of last log created. Avoid sending multiple logs
        private List<string> _predictions;
        private StringTrie trie;

        #endregion

        #region Properties

        public static ThisAddIn Current { get; protected set; }
        public string Name => "PhonoWriter";

        // Managers
        public LanguagesManager LanguagesManager { get; private set; }
        //public SpeechEnginesManager SpeechEnginesManager { get; private set; }
        //public PredictionsProvidersManager PredictionsProvidersManager { get; private set; }
        public TextProvidersManager TextProvidersManager { get; private set; }
        public PredictionsManager PredictionsManager { get; private set; }

        public WordTextProvider WordTextProvider { get; private set; }


        // Services
        public LogService LogService { get; protected set; }
        public DatabaseService DatabaseService { get; set; }
        public PredictionsService PredictionsService { get; protected set; }
        public object Configuration { get; internal set; }
        public static object config { get; private set; }

        //public SpyService SpyService { get; protected set; }
        //public DesktopUpdateService UpdateService { get; protected set; }

        public Prediction pc = new PredictionClassic();
        public Prediction pf = new PredictionFuzzy();
        public Prediction pr = new PredictionRelationship();
        #endregion

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            usr = new PWUserControl();
            wpf = new PWwpf();
            eh = new ElementHost { Child = wpf };
            usr.Controls.Add(eh);
            eh.Dock = DockStyle.Fill;
            System.Diagnostics.Debug.WriteLine("Hello World");
            myCustomTaskPane = this.CustomTaskPanes.Add(usr, "My Task Pane");
            myCustomTaskPane.Visible = true;
            myCustomTaskPane.Width = 400;
            //Globals.ThisAddIn.Application.WindowSelectionChange += new Word.ApplicationEvents4_WindowSelectionChangeEventHandler(Application_WindowSelectionChange);
            // Globals.ThisAddIn.Application.DocumentOpen += new Word.ApplicationEvents4_DocumentOpenEventHandler(Application_DocumentOpen);


            this.Application.DocumentChange += Application_DocumentChange;

            //App Contructor
            _predictions = new List<string>();
            Current = this;

            CheckDatabase();
            CheckFolders();
            InitializeServices();

            // Test ================
            RegisterTextProviders();

            // TEMPORARY ======================================================

            Task.Run(() =>
            {
                trie = new StringTrie();
                if (File.Exists("couples.txt")) 
                { 
                    foreach (var c in File.ReadAllLines("couples.txt"))
                        trie.Add(new StringTrieNode(c));
                }
                //Console.WriteLine("ADDED TRIE " + trie.Count());
            });

            // TEMPORARY ======================================================

            // Register events.
            //Configuration.ConfigurationChanged += Configuration_Changed;
            PredictionsManager.PredictionsFound += PredictionsManager_PredictionsFound;
            //PredictionsManager.PredictionSelected += PredictionsManager_PredictionSelected;
            TextProvidersManager.PunctuationFound += TextProvidersManager_PunctuationFound;
            TextProvidersManager.SeparatorFound += TextProvidersManager_SeparatorFound;
            TextProvidersManager.TextFound += TextProvidersManager_TextFound;

            // =====================

            wpf.dbc = DatabaseController;

            _application = Globals.ThisAddIn.Application;

            System.Diagnostics.Debug.WriteLine("Doc open");

        }

        //private static System.Windows.Application wpfApplication = null;
        //public void DispatchToUI(Action action)
        //{
        //    Microsoft.Office.Interop.Word.Application wordApplication = Globals.ThisAddIn.Application;
        //    if (wordApplication != null && wordApplication.Windows.Count > 0)
        //    {
        //        object win = wordApplication.Windows[1];
        //        if (win != null)
        //        {
        //            object obj = win.GetType().InvokeMember("Application", BindingFlags.GetProperty, null, win, null);
        //            if (obj != null)
        //            {
        //                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
        //                {
        //                    if (wpfApplication == null)
        //                    {
        //                        wpfApplication = new System.Windows.Application();
        //                        wpfApplication.Run();
        //                    }
        //                    wpfApplication.Dispatcher.Invoke(action);
        //                });
        //            }
        //        }
        //    }
        //}

        //public void Apply(string prediction)
        //{
        //    DispatchToUI(() =>
        //    {

        //        string previousInput = TextProvidersManager.CurrentProvider.GetPreviousWord();

        //    System.Diagnostics.Debug.WriteLine("Apply [_currentInput : '{0}', _previousInput : '{1}', prediction : '{2}']", _currentInput, previousInput, prediction);

        //        var lm = Globals.ThisAddIn.LanguagesManager;

        //        var language = lm.CurrentLanguage; //LanguagesManager.Languages[Configuration.Language - 1];

        //        // Update pair.
        //        if (!string.IsNullOrWhiteSpace(previousInput))
        //            PredictionsService.UpdatePair(language, previousInput, prediction);

        //        // Increment word's balancy if exists.
        //        var word = language.Words.Find(w => w.Text == prediction);
        //        if (word != null)
        //        {
        //            word.Occurrence++;
        //            word.IsUpdated = true;
        //            language.Update(word);
        //        }

        //        TextProvidersManager.Apply(_currentInput, prediction);

        //    });
        //}

        private string GetPreviousWord()
        {
            Word.Selection selection = Globals.ThisAddIn.Application.Selection;
            Word.Range currentRange = selection.Range;

            currentRange.MoveStart(Word.WdUnits.wdWord, -1);
            string previousWord = currentRange.Text;

            return previousWord;
        }

        private void Application_DocumentChange()
        {
            _timer = new Timer();
            _timer.Interval = 1000; // Adjust the interval based on your requirements
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Word.Selection selection = _application.Selection;

            if (selection == null)
            {
                return;
            }
            System.Diagnostics.Debug.WriteLine("Timer");

            if (selection.StoryLength <1)
            {
                return;
            }
            //string currentWord = GetCurrentWord(selection);
            string currentWord = TextBoxUtil.GetCurrentWord(_application.ActiveDocument);
            // ======================
            if (!selection.Equals(_currentInput))
                WordTextProvider.SelectionChanged();
            // ======================

            System.Diagnostics.Debug.WriteLine("--------------------Current------------------------------");

            System.Diagnostics.Debug.WriteLine(currentWord);


            if (!string.IsNullOrWhiteSpace(currentWord) && !_previousInput.Equals(currentWord))
            {
                GetSuggestions(currentWord);
                //Apply(currentWord); // need to apply A PREDICTOR !!! NOT A WORD
                System.Diagnostics.Debug.WriteLine("Current word: " +currentWord);
                System.Diagnostics.Debug.WriteLine("Suggestions:");

                //foreach (string suggestion in )
                //{
                //    System.Diagnostics.Debug.WriteLine(suggestion);
                //}
            }
            System.Diagnostics.Debug.WriteLine("-----------------------Previous---------------------------");

            System.Diagnostics.Debug.WriteLine(_previousInput);
            _previousInput = currentWord;
        }

        private string GetCurrentWord(Word.Selection selection)
        
        {

            int originalStart = selection.Start;
            int originalEnd = selection.End;
            System.Diagnostics.Debug.WriteLine("From ThisAddIn.cs - GetCurrentWord - range from " +originalStart +" To " +originalEnd);
            Word.Range range = _application.ActiveDocument.Range(originalStart, originalEnd);

            object unit = Word.WdUnits.wdWord;
            object count = 1;

            range.MoveStart(ref unit, -1);
            range.MoveEnd(ref unit, 1);

            // Restore the original selection
            selection.SetRange(originalStart, originalEnd);

            // Trim spaces and return the current word
            string currentWord = range.Text?.Trim() ?? string.Empty;

            System.Diagnostics.Debug.WriteLine("From ThisAddIn.cs - GetCurrentWord - line 281 : " +currentWord);

            return currentWord;
        }

        private void GetSuggestions(string input)
        {
            System.Diagnostics.Debug.WriteLine("From GetSuggestions - ThisAddin.cs line 287 - INPUT = "+input);

            System.Windows.Controls.ListView lw = (System.Windows.Controls.ListView)wpf.FindName("myList");
            lw.Items.Clear();

            System.Windows.Controls.ContentControl pb = (System.Windows.Controls.ContentControl)wpf.FindName("PictureBox");

            System.Windows.Controls.Label label = (System.Windows.Controls.Label)wpf.FindName("mySelection");
            label.Content = input;
            ((System.Windows.Controls.Image) wpf.FindName("pictureBox")).Source = null;
            PredictionsManager.Request(input);

            //var defaultParallelOptions = new ParallelOptions();
            //var words = pc.Work(input, defaultParallelOptions);
            //var words1 = pf.Work(input, defaultParallelOptions);
            //var words2 = pr.Work(input, defaultParallelOptions);

            //foreach (var w in words)
            //{
            //    lw.Items.Add(w.Prediction);
            //}

            //foreach (var w in words1)
            //{
            //    lw.Items.Add(w.Prediction);
            //}

            //if (lw.Items.Count > 0)
            //{
            //    lw.SelectedIndex = 0;
            //}

            //Thread.Sleep(100);

            System.Diagnostics.Debug.WriteLine("ThisAddIn.cs - End of GetSuggestion(string input) - Start of displaying predictions - size : " +_predictions.Count());

            foreach (var w in _predictions)
            {
                lw.Items.Add(w);
            }

        }
        


        private void InitializeServices()
        {
            // Load managers and services.
            DatabaseService = new DatabaseService(Constants.DATABASE_FILE);
            DatabaseController = new DatabaseController(DatabaseService);
            LanguagesManager = new LanguagesManager();
            //EnginesManager = new EnginesManager();
            TextProvidersManager = new TextProvidersManager();
            //=====================================
            WordTextProvider = new WordTextProvider();
            //=====================================

            //PredictionsProvidersManager = new PredictionsProvidersManager();
            PredictionsManager = new PredictionsManager();
           
            PredictionsService = new PredictionsService();
            //SpyService = new SpyService();
            //if (IsWindows11())
            //    SpyService = new SpyServiceWin11();

            // Start managers and services.
            System.Diagnostics.Debug.WriteLine("Initializing services");
            LanguagesManager.Initialize();
            PredictionsManager.Initialize();
            TextProvidersManager.Initialize();



        }

        private void CheckDatabase()
        {
            System.Diagnostics.Debug.WriteLine("Checking DB");

            // If shared database doesn't exist...
            if (!File.Exists(Constants.DATABASE_BASE_FILE))
            {
                //Write error message
                System.Diagnostics.Debug.WriteLine("File not existing: " + Constants.DATABASE_BASE_FILE);

                //Display error message
                //System.Windows.MessageBox.Show("The required database file is missing. Please contact your system administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // If database doesn't exist, copy the original.
            if (!File.Exists(Constants.DATABASE_FILE))
            {
                File.Copy(Constants.DATABASE_BASE_FILE, Constants.DATABASE_FILE);
                System.Diagnostics.Debug.WriteLine("File copied");

                return;
            }
        }

        //====================================================================
        //      Concerning 'setup' - needs improvement !
        //====================================================================
        public void CheckFolders()
        {
            if (!Directory.Exists(Constants.LOCAL_APP_PATH))
                //Directory.CreateDirectory(Constants.LOCAL_APP_PATH);
                System.Diagnostics.Debug.WriteLine("Error : Constants.LOCAL_APP_PATH");

            if (!Directory.Exists(Constants.COMMON_APP_PATH))
                //Directory.CreateDirectory(Constants.COMMON_APP_PATH);
                System.Diagnostics.Debug.WriteLine("Error : Constants.COMMON_APP_PATH");

            if (!Directory.Exists(Constants.IMAGES_CUSTOM_PATH))
                //Directory.CreateDirectory(Constants.IMAGES_CUSTOM_PATH);
                System.Diagnostics.Debug.WriteLine("Error : Constants.IMAGES_CUSTOM_PATH");

            System.Diagnostics.Debug.WriteLine("Folders checked");

        }

        private void RegisterTextProviders()
        {
            System.Diagnostics.Debug.WriteLine("THIS ADDIN.cs - line 428 - RegisterTextProviders()");
            TextProvidersManager.RegisterProvider(WordTextProvider);
            //TextProvidersManager.RegisterProvider((ITextProvider)SpyService);
            System.Diagnostics.Debug.WriteLine("THIS ADDIN.cs - line 431 - TextProviders = " +this.WordTextProvider);
        }

        private void PredictionsManager_PredictionsFound(object sender, PredictionsFoundArgs e)
        {
            _predictionSelected = false;

            System.Diagnostics.Debug.WriteLine("THIS ADDIN.cs - line 438 - PredictionsFound ! e.Input = " +e.Input.ToString() + " _currentInput = " +_currentInput);

            // TEST ==========
            if (e.Input != _currentInput)
                return;


            //
            // Prepare predictions.
            //

            // Filter predictions.
            System.Diagnostics.Debug.WriteLine("Trying to fill out _predictions - from ThisAddIn.cs - PredictionsManager_PredictionsFound");
            _predictions = PredictionsManager.Filter(e.Predictions, e.Input); //_currentInput

            // Code below commented until Configuration.PictographicHidePictureless is created
            //// Hide pictureless words if necessary.
            //if (Configuration.PictographicHidePictureless)
            //{
            //    var retained = new List<string>();
            //    var words = LanguagesManager.GetLanguage((LanguagesEnum)Configuration.InterfaceLanguage).Words;

            //    for (int i = 0; i < _predictions.Count; i++)
            //    {
            //        // Bypass if word exist and has no image or doesn't exist.
            //        var results = words.Where(w => w.Text == _predictions[i]);
            //        if (results.Any() && results.First().Image > 0)
            //            retained.Add(_predictions[i]);
            //    }

            //    _predictions = retained;
            //}

            // Add predictive following word if possible.
            int max = 7; // Configuration.ClassicPredictionsNumber + Configuration.FuzzyPredictionNumber + Configuration.PhoneticPredictionNumber;
            int nexts = 4;//max - _predictions.Count; // (INSTEAD OF 'max') : Configuration.ClassicPredictionsNumber + Configuration.FuzzyPredictionNumber + Configuration.PhoneticPredictionNumber 

            System.Diagnostics.Debug.WriteLine("ThisAddIn.cs - Nexts ?? nb of predictions ?? " + _predictions.Count());

            foreach(var p in _predictions)
            {
                System.Diagnostics.Debug.WriteLine("ThisAddIn.cs - Predictions, PredictionsManager_PredictionsFound : " + p);
            }

            if (_demo)
            {
                var lm = Globals.ThisAddIn.LanguagesManager;
                string previousWord = TextProvidersManager.CurrentProvider.GetPreviousWord().ToLower();
                var nextWords = PredictionsManager.RequestRelationships(previousWord).OrderByDescending(o => o.Value).Where(w => w.Prediction.StartsWith(e.Input)).Take(nexts);
                _predictions.InsertRange(0, nextWords.Select(s => s.Prediction).Take(nexts));

                if (e.Input.Length > 0 && lm.CurrentLanguage == lm.GetLanguage(LanguagesEnum.Francais))
                {
                    var suggestions = trie.GetValuesByPrefix(previousWord + " " + e.Input).Take(9).Select(s => s.ToString().Substring(previousWord.Length + 1));
                    //var suggestions = trie.GetValuesByPrefix(previousWord + " " + e.Input).OrderBy(o => o.ToString().Length).Take(nexts - nextWords.Count()).Select(s => s.ToString().Substring(previousWord.Length + 1));
                    _predictions.InsertRange(0, suggestions); //_couples.Where(w => w.StartsWith(previousWord + " " + e.Input)).OrderBy(o => o.Length).Take(nexts - nextWords.Count()).Select(s => s.Substring(previousWord.Length + 1)));
                }

                _predictions.RemoveAll(m => m.Length == 0);
                System.Diagnostics.Debug.WriteLine("PREVIOUS : '{0}'", previousWord);
                // Restore the uppercase from input if necessary.
                //if (!string.IsNullOrWhiteSpace(e.Input) && char.IsUpper(e.Input[0]) || previousWord == string.Empty) <= error with previousWord empty, causing too much uppercase on wrong conditions
                if (!string.IsNullOrWhiteSpace(e.Input) && char.IsUpper(e.Input[0]))
                    _predictions = _predictions.Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)).ToList();
            }

            //// Remove eszatts for swiss german if needed.
            //if (Configuration.RemoveEszetts && Configuration.Language == (int)LanguagesEnum.Deutsch)
            //    _predictions = _predictions.Select(s => s.Replace("ß", "ss")).ToList();

            // Sort and regroup.
            _predictions = _predictions.GroupBy(s => s).OrderByDescending(g => g.Count()).Select(g => g.Key).Take(max).ToList();
            foreach (var p in _predictions)
            {
                System.Diagnostics.Debug.WriteLine("ThisAddIn.cs - Predictions AFTER SORTING, PredictionsManager_PredictionsFound : " + p);
            }
            //
            // Show predictions
            //

        }

        //private void PredictionsManager_PredictionSelected(object sender, string prediction)
        //{
        //    Apply(prediction);
        //}

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion

        public static void KeyReturnPressed(System.Windows.Controls.ListViewItem item)
        {
            Application wordApp = null;
            try
            {
                wordApp = (Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // Word is not running; handle this case if necessary
            }
            var str = "";
            if (wordApp != null)
            {
                Word.Document document = wordApp.ActiveDocument;
                Range selectionRange = document.ActiveWindow.Selection.Range;
                selectionRange.Expand(WdUnits.wdWord);
                str = item.Content.ToString() + " ";
                selectionRange.Text = str;
                selectionRange.Start = selectionRange.End;
                selectionRange.Select();
            }
            Word.Window window = wordApp.ActiveWindow;
            window.SetFocus();
            window.Activate();
        }

        private void TextProvidersManager_TextFound(object sender, TextFoundArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ThisAddIns.cs - TextProvidersManager_TextFound [text : '{0}', fetch : {1}]", e.Text, e.FetchType);

            // Read found text.
            //!!!!!!!!!!!!!!CHANGE CONDITION
            //if (true)
            //{
            //    Task.Run(() =>
            //    {
            //        // Assume we have to wait 500ms so the phonetic prediction
            //        // can run before we play the selected text.
            //        // TODO : Increase if users complains.
            //        //Thread.Sleep(500); //UNCOMMENT for Phonetic Prediction -- GAETAN

            //        // Prevent playing the text too fast (can be annoying).
            //        if (_currentInput != e.Text)
            //            return;

            //        //EnginesManager.CurrentEngine.Speak(e.Text, LanguageUtils.ConvertLanguageToString(Configuration.Language, true), Configuration.VoiceVolume, Configuration.VoiceSpeed);
            //    });
            //}

            // Start a predictions request.
            if (_currentInput != e.Text)
                PredictionsManager.Request(e.Text);

            // Store current input for futur usage.
            _currentInput = e.Text;
        }

        private void TextProvidersManager_PunctuationFound(object sender, EventArgs e)
        {
            ClearPredictions();
        }

        private void TextProvidersManager_SeparatorFound(object sender, SeparatorFoundArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TextProvidersManager_SeparatorFound [previous : '{0}']", e.PreviousWord);

            _currentInput = string.Empty;

            ClearPredictions();

            _previousInput = e.PreviousWord;
            //_currentInput = e.PreviousWord;

            // Start a predictions request.
            if (!_demo)
                PredictionsManager_PredictionsFound(this, new PredictionsFoundArgs(string.Empty, new List<Predictions.PredictionValue>()));
        }

        private void ClearPredictions()
        {
            _currentInput = string.Empty;
            _predictionSelected = false;
        }

        public static void LanguageChanged(string selectedLanguage)
        {
            var lm = Globals.ThisAddIn.LanguagesManager;
            Database.Models.Language language = new Database.Models.Language();

            if (selectedLanguage == "Francais")
            {
                language = lm.GetLanguage(LanguagesEnum.Francais);
                lm.CurrentLanguage = language;
            }
            else if (selectedLanguage == "English")
            {
                language = lm.GetLanguage(LanguagesEnum.English);
                lm.CurrentLanguage = language;
            }
            else if (selectedLanguage == "Deutsch")
            {
                language = lm.GetLanguage(LanguagesEnum.Deutsch);
                lm.CurrentLanguage = language;
            }
            else if (selectedLanguage == "Italiano")
            {
                language = lm.GetLanguage(LanguagesEnum.Italiano);
                lm.CurrentLanguage = language;
            }
            else if (selectedLanguage == "Spanish")
            {
                language = lm.GetLanguage(LanguagesEnum.Spanish);
                lm.CurrentLanguage = language;
                
            }

            // LanguagesManager is null on startup, because for some reason this method gets called before InitializeServices is finished
            if (lm != null)
            {
                System.Diagnostics.Debug.WriteLine("CURRENT LANGUAGE " + lm.CurrentLanguage.Label);
            }
        }
    }
}
