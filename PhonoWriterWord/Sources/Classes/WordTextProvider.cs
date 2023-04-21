using Word = Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using PhonoWriterWord.Sources.Classes;
using PhonoWriterWord.Utils;
using System.Windows.Documents;
using System.Windows;

namespace PhonoWriterWord.Sources.Classes
{
    public partial class WordTextProvider : ITextProvider
    {
        // Instance
        ThisAddIn _current = ThisAddIn.Current;

        // Variables
        private string _lastText; // Memorize last text capture. Avoid sending the same word many times.


        #region Events
        // Events
        public event EventHandler<TextFoundArgs> TextFound;
        public event EventHandler<SeparatorFoundArgs> SeparatorFound;
        public event EventHandler PunctuationFound;
        #endregion

        public WordTextProvider()
        {
            _current = ThisAddIn.Current;
        }

        public void Apply(string input, string prediction)
        {

            //// Replace word and focus the editor.
            UpdateWord(prediction);

            // Add a space char if necessary or just move the caret forward.
            Word.Application app = Globals.ThisAddIn.Application;
            Word.Selection selection = app.Selection;
            string textInRun = selection.Range.Text;
            
            if(textInRun == null)
            {
                selection.Range.Text = " ";
                return;
            }

            if (textInRun.Length > 0 && textInRun[0] == ' ')
            {
                selection.MoveRight(Word.WdUnits.wdCharacter);
            }
            else
            {
                selection.Range.Text = " ";
            }
            //string textInRun = rtbText.Selection.Start.GetTextInRun(LogicalDirection.Forward);
            //if (textInRun.Length > 0 && textInRun[0] == ' ')
            //    rtbText.CaretPosition = rtbText.CaretPosition.GetPositionAtOffset(1);
            //else
            //    InputUtil.Keyboard.TextEntry(" ");
        }

        /// <summary>
        /// Update the current word with new words.
        /// </summary>
        /// <param name="words"></param>
        public void UpdateWord(string words)
        {
            // Test, if the selection is a click location or a selection (more than one character)
            var app = _current.Application;
            //Document activeDoc = app.ActiveDocument;
            Word.Selection selection = app.Selection;

            //============================
            if (selection.Type == Word.WdSelectionType.wdSelectionIP)
            {
                Word.Range range = selection.Range;
                range.Text = words;
                selection.MoveRight(Word.WdUnits.wdCharacter, words.Length, Word.WdMovementType.wdMove);
            }
            else
            {
                Word.Range range = selection.Range;
                range.Text = words;
            }
            //============================
        }

        public string GetPreviousWord()
        {
            string text = "";

            text = TextBoxUtil.GetPreviousWord(_current);

            return text;
        }

        public void SelectionChanged() //object sender, RoutedEventArgs e
        {
            //if (_capture) // Avoid text capture til selection is finished.
            //    return;

            // Get focused text.
            string text = TextBoxUtil.GetCurrentWord(_current.Application.ActiveDocument);
            if (text == _lastText || text.Length == 0)
                return;

            // If buffer not empty and has a text...
            if (!string.IsNullOrWhiteSpace(text))
            {
                TextFound?.Invoke(this, new TextFoundArgs(text, FetchTypeEnum.Typing));
            }
            else
            {
                string previousWord = TextBoxUtil.GetPreviousWord(_current);
                if (!string.IsNullOrWhiteSpace(previousWord))
                    SeparatorFound?.Invoke(this, new SeparatorFoundArgs(previousWord));
                else
                    PunctuationFound?.Invoke(this, new EventArgs());
            }

            _lastText = text;
        }

    }
}
