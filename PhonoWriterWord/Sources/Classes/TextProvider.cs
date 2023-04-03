using Microsoft.Office.Interop.Word;
using System;

namespace PhonoWriterWord.Sources.Classes
{
	// Enums
	public enum FetchTypeEnum
	{
		Typing,
        Click,
		Selection,
		Separator
	};

	public interface ITextProvider
	{
        event EventHandler<TextFoundArgs> TextFound;                // Occures when a text has been found.
        event EventHandler<SeparatorFoundArgs> SeparatorFound;		// Occures when a separator has been found.
        event EventHandler PunctuationFound;                        // Occures when a punctuation has been found.

        //Adaptation to work with Word's Add-Ins
        //void Apply(Document document, string input, string prediction);
        //string GetPreviousWord(Document document);

        // OLD CODE
        void Apply(string input, string prediction);
        string GetPreviousWord();
    }

	public class TextFoundArgs : EventArgs
	{
		string _text;
		FetchTypeEnum _fetchType;

		public string Text { get { return _text; } }
		public FetchTypeEnum FetchType { get { return _fetchType; } }

		public TextFoundArgs(string text, FetchTypeEnum fetchType)
		{
			_text = text;
			_fetchType = fetchType;
		}
	}

	public class SeparatorFoundArgs : EventArgs
	{
		string _previousWord;

		public string PreviousWord { get { return _previousWord; } }

		public SeparatorFoundArgs()
		{
			_previousWord = string.Empty;
		}

		public SeparatorFoundArgs(string previousWord)
		{
			_previousWord = previousWord.Trim();
		}
	}
}
