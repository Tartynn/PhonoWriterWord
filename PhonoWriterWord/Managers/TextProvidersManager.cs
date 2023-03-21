using Icare.PhonoWriter.Client.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonoWriterWord.Managers
{
    public class TextProvidersManager
	{
		#region Variables

		// Instances
		private ThisAddIn _app;

		// Variables
		private string _currentInput;
		private FetchTypeEnum _currentFetchType;
		private ITextProvider _currentProvider;

		// Events
		public event EventHandler<TextFoundArgs> TextFound;
		public event EventHandler<SeparatorFoundArgs> SeparatorFound;
		public event EventHandler PunctuationFound;

		#endregion

		#region Properties

		public string CurrentInput => _currentInput;
		public FetchTypeEnum CurrentFetchType => _currentFetchType;
		public ITextProvider CurrentProvider => _currentProvider;

		#endregion

		#region Constructors

		public TextProvidersManager()
		{
			_app = ThisAddIn.Current;
		}

		#endregion

		#region Methods

		public void Apply(string input, string prediction)
		{
			_currentProvider.Apply(input, prediction);
		}

		public void Initialize()
		{
		}

		public void RegisterProvider(ITextProvider provider)
		{
			if (provider == null)
				return;

			provider.PunctuationFound += delegate (object sender, EventArgs e)
			{
				_currentProvider = (ITextProvider)sender;
				_currentInput = string.Empty;
				PunctuationFound?.Invoke(sender, e);
			};
			provider.SeparatorFound += delegate (object sender, SeparatorFoundArgs e)
			{
				_currentProvider = (ITextProvider)sender;
				_currentInput = string.Empty;
				SeparatorFound?.Invoke(sender, e);
			};
			provider.TextFound += delegate (object sender, TextFoundArgs e)
			{
				_currentProvider = (ITextProvider)sender;
				_currentFetchType = e.FetchType;
				//if (e.Text == _lastTextFound) return; // Cancel if same text as before.
				_currentInput = e.Text;
				TextFound?.Invoke(sender, e);
			};
		}

		#endregion
	}
}
