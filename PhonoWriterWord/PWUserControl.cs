using PhonoWriterWord.Database;
using PhonoWriterWord.Database.Controllers;
using PhonoWriterWord.Predictions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhonoWriterWord
{
    public partial class PWUserControl : UserControl
    {
        private Microsoft.Office.Interop.Word.Document myDoc;
        public PWUserControl()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            myDoc = Globals.ThisAddIn.Application.ActiveDocument;
            myDoc.Paragraphs[1].Range.InsertParagraphBefore();
            myDoc.Paragraphs[1].Range.Text = "Button Tap.";
        }
    }
}
