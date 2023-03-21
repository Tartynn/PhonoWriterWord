using System;
using System.Collections.Generic;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Word;
using PhonoWriterWord.Database;
using PhonoWriterWord.Managers;
using PhonoWriterWord.Predictions;
using PhonoWriterWord.Services.Log;
using PhonoWriterWord.Services;
using System.IO;
using PhonoWriterWord.Values;
using Icare.PhonoWriter.Client.Classes;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

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

        // Variables
        private bool _predictionIsSelected;     // Flag to know if a prediction has been selected in the predictions window.
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

        public DatabaseController DatabaseController { get; }

        // Managers
        public LanguagesManager LanguagesManager { get; private set; }
        //public SpeechEnginesManager SpeechEnginesManager { get; private set; }
        public PredictionsProvidersManager PredictionsProvidersManager { get; private set; }
        public TextProvidersManager TextProvidersManager { get; private set; }
        public PredictionsManager PredictionsManager { get; private set; }


        // Services
        public LogService LogService { get; protected set; }
        public DatabaseService DatabaseService { get; set; }
        public PredictionsService PredictionsService { get; protected set; }
        //public SpyService SpyService { get; protected set; }
        //public DesktopUpdateService UpdateService { get; protected set; }

        #endregion

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            usr = new PWUserControl();
            wpf = new PWwpf();
            eh = new ElementHost { Child = wpf };
            usr.Controls.Add(eh);
            eh.Dock = DockStyle.Fill;
            this.Application.DocumentBeforeSave += new Word.ApplicationEvents4_DocumentBeforeSaveEventHandler(Application_DocumentBeforeSave);
            System.Diagnostics.Debug.WriteLine("Hello World");
            myCustomTaskPane = this.CustomTaskPanes.Add(usr, "My Task Pane");
            myCustomTaskPane.Visible = true;
            myCustomTaskPane.Width = 400;
            Globals.ThisAddIn.Application.WindowSelectionChange += new Word.ApplicationEvents4_WindowSelectionChangeEventHandler(Application_WindowSelectionChange);

            //App Contructor
            _predictions = new List<string>();
            Current = this;

            CheckDatabase();
            InitializeServices();
           

        }

        private void InitializeServices()
        {
            // Load managers and services.
            DatabaseService = new DatabaseService(Constants.DATABASE_FILE);
            LanguagesManager = new LanguagesManager();
            //EnginesManager = new EnginesManager();
            TextProvidersManager = new TextProvidersManager();
            PredictionsManager = new PredictionsManager();
            //DatabaseController = new DatabaseController(DatabaseService);
            //PredictionsService = new PredictionsService();
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
                MessageBox.Show("The required database file is missing. Please contact your system administrator.", "Error", (MessageBoxButton)MessageBoxButtons.OK, (MessageBoxImage)MessageBoxIcon.Error);
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
        }

        //====================================================================

        private void Application_WindowSelectionChange(Word.Selection sel)
        {
            Word.Document document = this.Application.ActiveDocument;
            //select the range of the word when the cursor is on it
            Range selectionRange = document.ActiveWindow.Selection.Range;
            selectionRange.Expand(WdUnits.wdWord);
            string word = selectionRange.Text.Trim();
            //assigning the value to our label
            System.Windows.Controls.Label label = (System.Windows.Controls.Label)wpf.FindName("mySelection");
            label.Content = word;
        }

        void Application_DocumentBeforeSave(Word.Document Doc, ref bool SaveAsUI, ref bool Cancel)
        {
            Doc.Paragraphs[1].Range.InsertParagraphBefore();
            Doc.Paragraphs[1].Range.Text = "Saving code.";
        }
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


        private void TextProvidersManager_TextFound(object sender, TextFoundArgs e)
        {
            _log.Debug("TextProvidersManager_TextFound [text : '{0}', fetch : {1}]", e.Text, e.FetchType);

            // Read found text.
            //!!!!!!!!!!!!!!CHANGE CONDITION
            if (true)
            {
                Task.Run(() =>
                {
                    // Assume we have to wait 500ms so the phonetic prediction
                    // can run before we play the selected text.
                    // TODO : Increase if users complains.
                    Thread.Sleep(500);

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
