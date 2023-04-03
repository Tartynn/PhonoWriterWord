using System;
using System.Collections.Generic;
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
using System.Linq;

namespace PhonoWriterWord



{
    public partial class ThisAddIn
    { 

        //user control
        public PWUserControl usr;
        // Custom task pane
        private Microsoft.Office.Tools.CustomTaskPane myCustomTaskPane;
        private ElementHost eh;
        private PWwpf wpf;

        #region Variables

        // Instances
        private Log _log;

        // Controllers
        public DatabaseController DatabaseController { get; private set; }


        // Variables
        private bool _demo = true;
        private bool _predictionSelected;     // Flag to know if a prediction has been selected in the predictions window.
        private string _previousInput;
        private string _context;
        private string _currentInput;
        private string _selectedPrediction;     // Return selected prediction.
        private DateTime _lastLog;              // Date of last log created. Avoid sending multiple logs
        private List<string> _predictions;

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

        //public WordTextProvider WordTextProvider { get; private set; }


        // Services
        public LogService LogService { get; protected set; }
        public DatabaseService DatabaseService { get; set; }
        public PredictionsService PredictionsService { get; protected set; }
        public object Configuration { get; internal set; }

        //public SpyService SpyService { get; protected set; }
        //public DesktopUpdateService UpdateService { get; protected set; }

        //public Prediction pc = new PredictionClassic();
        //public Prediction pf = new PredictionFuzzy();
        public Prediction pr = new PredictionRelationship();
        #endregion

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {

            usr = new PWUserControl();
            wpf = new PWwpf();
            eh = new ElementHost { Child = wpf };
            usr.Controls.Add(eh);
            eh.Dock = DockStyle.Fill;
            //this.Application.DocumentBeforeSave += new Word.ApplicationEvents4_DocumentBeforeSaveEventHandler(Application_DocumentBeforeSave);
            System.Diagnostics.Debug.WriteLine("Hello World");
            myCustomTaskPane = this.CustomTaskPanes.Add(usr, "My Task Pane");
            myCustomTaskPane.Visible = true;
            myCustomTaskPane.Width = 400;
            Globals.ThisAddIn.Application.WindowSelectionChange += new Word.ApplicationEvents4_WindowSelectionChangeEventHandler(Application_WindowSelectionChange);

            //App Contructor
            _predictions = new List<string>();
            Current = this;

            CheckDatabase();
            CheckFolders();
            InitializeServices();

            // TESTS ========================================
            RegisterTextProviders();
            //PredictionsManager.PredictionsFound += PredictionsManager_PredictionFound;
            TextProvidersManager.TextFound += TextProvidersManager_TextFound;
            // ==============================================

            wpf.dbc = DatabaseController;
        }

        private void InitializeServices()
        {
            // Load managers and services.
            DatabaseService = new DatabaseService(Constants.DATABASE_FILE);
            DatabaseController = new DatabaseController(DatabaseService);
            LanguagesManager = new LanguagesManager();
            //EnginesManager = new EnginesManager();
            TextProvidersManager = new TextProvidersManager();
            //PredictionsProvidersManager = new PredictionsProvidersManager();
            PredictionsManager = new PredictionsManager();
            
            PredictionsService = new PredictionsService();
            //SpyService = new SpyService();
            //if (IsWindows11())
            //    SpyService = new SpyServiceWin11();

            // Start managers and services.
            //LanguagesManager.Initialize();
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
            //TextProvidersManager.RegisterProvider(WordTextProvider);
            //TextProvidersManager.RegisterProvider((ITextProvider)SpyService);
        }

        //private void PredictionsManager_PredictionFound(object sender, PredictionsFoundArgs e)
        //{
        //    _predictionSelected = false;

        //    if (e.Input != _currentInput)
        //        return;


        //    //
        //    // Prepare predictions.
        //    //


        //    // Filter predictions.
        //    _predictions = PredictionsManager.Filter(e.Predictions, e.Input);

        //    // Code below commented until Configuration.PictographicHidePictureless is created
        //    //// Hide pictureless words if necessary.
        //    //if (Configuration.PictographicHidePictureless)
        //    //{
        //    //    var retained = new List<string>();
        //    //    var words = LanguagesManager.GetLanguage((LanguagesEnum)Configuration.InterfaceLanguage).Words;

        //    //    for (int i = 0; i < _predictions.Count; i++)
        //    //    {
        //    //        // Bypass if word exist and has no image or doesn't exist.
        //    //        var results = words.Where(w => w.Text == _predictions[i]);
        //    //        if (results.Any() && results.First().Image > 0)
        //    //            retained.Add(_predictions[i]);
        //    //    }

        //    //    _predictions = retained;
        //    //}

        //    // Add predictive following word if possible.
        //    int max = 7; // Configuration.ClassicPredictionsNumber + Configuration.FuzzyPredictionNumber + Configuration.PhoneticPredictionNumber;
        //    int nexts = max - _predictions.Count; // (INSTEAD OF 'max') : Configuration.ClassicPredictionsNumber + Configuration.FuzzyPredictionNumber + Configuration.PhoneticPredictionNumber 

        //    if (!_demo)
        //    {
        //        string previousWord = TextProvidersManager.CurrentProvider.GetPreviousWord().ToLower();
        //        var nextWords = PredictionsManager.RequestRelationships(previousWord).OrderByDescending(o => o.Value).Where(w => w.Prediction.StartsWith(e.Input)).Take(nexts);
        //        _predictions.InsertRange(0, nextWords.Select(s => s.Prediction).Take(nexts));

        //        if (e.Input.Length > 0 && Configuration.Language == (int)LanguagesEnum.Francais)
        //        {
        //            var suggestions = trie.GetValuesByPrefix(previousWord + " " + e.Input).Take(9).Select(s => s.ToString().Substring(previousWord.Length + 1));
        //            //var suggestions = trie.GetValuesByPrefix(previousWord + " " + e.Input).OrderBy(o => o.ToString().Length).Take(nexts - nextWords.Count()).Select(s => s.ToString().Substring(previousWord.Length + 1));
        //            _predictions.InsertRange(0, suggestions); //_couples.Where(w => w.StartsWith(previousWord + " " + e.Input)).OrderBy(o => o.Length).Take(nexts - nextWords.Count()).Select(s => s.Substring(previousWord.Length + 1)));
        //        }

        //        _predictions.RemoveAll(m => m.Length == 0);
        //        Console.WriteLine("PREVIOUS : '{0}'", previousWord);
        //        // Restore the uppercase from input if necessary.
        //        //if (!string.IsNullOrWhiteSpace(e.Input) && char.IsUpper(e.Input[0]) || previousWord == string.Empty) <= error with previousWord empty, causing too much uppercase on wrong conditions
        //        if (!string.IsNullOrWhiteSpace(e.Input) && char.IsUpper(e.Input[0]))
        //            _predictions = _predictions.Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)).ToList();
        //    }

        //    //// Remove eszatts for swiss german if needed.
        //    //if (Configuration.RemoveEszetts && Configuration.Language == (int)LanguagesEnum.Deutsch)
        //    //    _predictions = _predictions.Select(s => s.Replace("ß", "ss")).ToList();

        //    // Sort and regroup.
        //    _predictions = _predictions.GroupBy(s => s).OrderByDescending(g => g.Count()).Select(g => g.Key).Take(max).ToList();


        //    //
        //    // Show predictions
        //    //

        //}

        //====================================================================

        private void Application_WindowSelectionChange(Word.Selection sel)
        {
            // the
            //
            //
            //
            // will contain the list of proposed words
            System.Windows.Controls.ListView lw = (System.Windows.Controls.ListView)wpf.FindName("myList");
            lw.Items.Clear();

            // the PictureBox will contain the picture related to the selected word
            System.Windows.Controls.ContentControl pb = (System.Windows.Controls.ContentControl)wpf.FindName("PictureBox");

            Word.Document document = this.Application.ActiveDocument;
            //select the range of the word when the cursor is on it
            Range selectionRange = document.ActiveWindow.Selection.Range;
            selectionRange.Expand(WdUnits.wdWord);
            string word = selectionRange.Text.Trim();
            //assigning the value to our label
            System.Windows.Controls.Label label = (System.Windows.Controls.Label)wpf.FindName("mySelection");
            label.Content = word;
            ((System.Windows.Controls.Image)wpf.FindName("pictureBox")).Source = null;
            PredictionsManager.Request(word);

            //=================================================================================
            // Update pair.
            //string previousInput = TextProvidersManager.CurrentProvider.GetPreviousWord(document);
            //var language = new Database.Models.Language(1, "fr");
            //if (!string.IsNullOrWhiteSpace(previousInput))
            //    PredictionsService.UpdatePair(language, previousInput, "Relationship"); // prediction instead of "Relationship"

            //// Increment word's balancy if exists.
            //var w1 = language.Words.Find(w => w.Text == "Relationship"); // prediction instead of "Relationship"
            //if (w1 != null)
            //{
            //    w1.Occurrence++;
            //    w1.IsUpdated = true;
            //    language.Update(w1);
            //}

            //TextProvidersManager.Apply(_currentInput, "Relationship"); // prediction instead of "Relationship"

            //=================================================================================

            var defaultParallelOptions = new ParallelOptions();
            //var words = pc.Work(word, defaultParallelOptions);
            //var words1 = pf.Work(word, defaultParallelOptions);
            var words2 = pr.Work(word, defaultParallelOptions);



            /*var ic = new ImagesController(DatabaseController);
            var wc = new WordsController(DatabaseController);
            var fr = new Database.Models.Language(1, "fr");
            var wordObj = wc.ResearchByText(fr, word);

            if (wordObj != null)
            {
                var img = ic.ResearchByWord(wordObj);
                System.Diagnostics.Debug.WriteLine(img.FileName);
            }*/

     //       foreach (var w in words)
     //       {
     //           lw.Items.Add(w.Prediction);
     //       }

            foreach (var w in words2)
            {
                lw.Items.Add(w.Prediction);
            }
        }

            
        //void Application_DocumentBeforeSave(Word.Document Doc, ref bool SaveAsUI, ref bool Cancel)
        //{
        //    Doc.Paragraphs[1].Range.InsertParagraphBefore();
        //    Doc.Paragraphs[1].Range.Text = "Saving code.";
        //}
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
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
            System.Diagnostics.Debug.WriteLine("TextProvidersManager_TextFound [text : '{0}', fetch : {1}]", e.Text, e.FetchType);

            // Read found text.
            //!!!!!!!!!!!!!!CHANGE CONDITION
            if (true)
            {
                Task.Run(() =>
                {
                    // Assume we have to wait 500ms so the phonetic prediction
                    // can run before we play the selected text.
                    // TODO : Increase if users complains.
                    //Thread.Sleep(500); //UNCOMMENT for Phonetic Prediction -- GAETAN

                    // Prevent playing the text too fast (can be annoying).
                    if (_currentInput != e.Text)
                        return;

                    //EnginesManager.CurrentEngine.Speak(e.Text, LanguageUtils.ConvertLanguageToString(Configuration.Language, true), Configuration.VoiceVolume, Configuration.VoiceSpeed);
                });
            }

            // Start a predictions request.
            if (_currentInput != e.Text)
                PredictionsManager.Request(e.Text);

            // Store current input for futur usage.
            _currentInput = e.Text;
        }
    }
}
