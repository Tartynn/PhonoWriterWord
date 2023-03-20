using System;
using System.Collections.Generic;
using Word = Microsoft.Office.Interop.Word;
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using PhonoWriterWord.Database;
using PhonoWriterWord.Managers;
using PhonoWriterWord.Predictions;
using PhonoWriterWord.Services.Log;
using PhonoWriterWord.Services;
using System.IO;
using PhonoWriterWord.Values;

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
        public LanguagesManager LanguagessManager { get; private set; }
        //public SpeechEnginesManager SpeechEnginesManager { get; private set; }
        public PredictionsProvidersManager PredictionsProvidersManager { get; private set; }
        //public TextProvidersManager TextProvidersManager { get; private set; }

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
    }
}
