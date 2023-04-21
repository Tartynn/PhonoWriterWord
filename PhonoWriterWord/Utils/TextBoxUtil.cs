using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Office.Interop.Word;

namespace PhonoWriterWord.Utils
{
    class TextBoxUtil
    {
        public const int POSITION_START = 0;
        public const int POSITION_LENGTH = 1;

        private static string[] spaceCharacters = { " ", "\n", "\r", "\t", ".", ",", ";", ":", "!", "?", "(", ")", "/" };
        private static char[] separators = { '\'', '’', '-', ',' };

        private static readonly IList<String> liste = new List<String>(spaceCharacters);

        public string[] SpaceCharacters
        {
            get { return spaceCharacters; }
            set { spaceCharacters = value; }
        }

        /// <summary>
        /// Get the current word posititon (start, length) the given document
        /// </summary>
        /// <param name="activeDocument">TextBox from which the positition is retrieved</param>
        /// <returns></returns>
        public static int[] GetCurrentWordPosition(Document activeDocument)
        {
            Application app = activeDocument.Application;
            Selection selection = app.Selection;
            Range range = selection.Range;
            int[] position = new int[2];

            if (range.Start == range.End)
            {
                range.MoveStart(WdUnits.wdWord, -1);
                range.MoveEnd(WdUnits.wdWord, 1);
            }

            int docStart = activeDocument.Range(0, 0).Start;
            int start = range.Start - docStart;
            position[0] = start;
            position[1] = range.Text.Length;

            return position;
        }

        /// <summary>
        /// Get the current word according to the given document
        /// </summary>
        /// <param name="activeDocument">TextBox from which the current word has to be retrieved</param>
        /// <returns></returns>
        public static string GetCurrentWord(Document activeDocument)
        {
            Application app = activeDocument.Application;
            Selection selection = app.Selection;
            string wordBeforePointer = "";
            string wordAfterPointer = "";

            int originalStart = selection.Start;
            int originalEnd = selection.End;

            Microsoft.Office.Interop.Word.Range range = app.ActiveDocument.Range(originalStart, originalEnd);

            object unit = Microsoft.Office.Interop.Word.WdUnits.wdWord;
            object count = 1;

            range.MoveStart(ref unit, -1);
            range.MoveEnd(ref unit, 1);

            // Restore the original selection
            selection.SetRange(originalStart, originalEnd);

            // Trim spaces and return the current word
            string currentWord = range.Text?.Trim() ?? string.Empty;

            // System.Diagnostics.Debug.WriteLine(currentWord);

            return currentWord;

            //if (selection.Type == WdSelectionType.wdSelectionIP)
            //{
            //    Range range = selection.Range;
            //    range.MoveStart(WdUnits.wdWord, -1);
            //    wordBeforePointer = range.Text.TrimEnd();
            //    range.MoveStart(WdUnits.wdWord, 1);
            //    range.MoveEnd(WdUnits.wdWord, 1);
            //    wordAfterPointer = range.Text.TrimStart();
            //}
            //else
            //{
            //    Range range = selection.Range;
            //    Range wordRange = range.Words.First;
            //    wordBeforePointer = wordRange.Previous().Text.TrimEnd();
            //    wordAfterPointer = wordRange.Next().Text.TrimStart();
            //}

            //return new string[2] { wordBeforePointer, wordAfterPointer };
           
        }

        public static string GetPreviousWord(ThisAddIn _app)
        {
            Application wordApp = _app.Application;
            Selection selection = wordApp.Selection;
            Range range = selection.Range;

            if (range.Text == null)
                return "";

            range.MoveStart(WdUnits.wdWord, -1);
            string textBeforeSelection = range.Text.Trim();

            string[] wordsBeforeSelection = textBeforeSelection.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (wordsBeforeSelection.Length > 0)
            {
                return wordsBeforeSelection.Last();
            }
            else
            {
                return "";
            }
        }
    }
}
