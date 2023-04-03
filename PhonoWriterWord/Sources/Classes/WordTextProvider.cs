using Word = Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using PhonoWriterWord.Sources.Classes;

namespace PhonoWriterWord.Sources.Classes
{
    public partial class WordTextProvider : ITextProvider
    {

        ThisAddIn _current = ThisAddIn.Current;

        #region Events
        // Events
        public event EventHandler<TextFoundArgs> TextFound;
        public event EventHandler<SeparatorFoundArgs> SeparatorFound;
        public event EventHandler PunctuationFound;
        #endregion

        public void Apply(string input, string prediction)
        {
            //document.Range
        }

        public string GetPreviousWord()
        {
            throw new NotImplementedException();
        }

    }
}
