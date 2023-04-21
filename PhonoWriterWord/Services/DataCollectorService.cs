//using PhonoWriterWord.Database.Models;
//using PhonoWriterWord.Predictions;
//using PhonoWriterWord.Values;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PhonoWriterWord.Services
//{
//	public class DataCollectorService : IDisposable
//	{
//		#region Variables

//		private ThisAddIn _app;
//		//private Log _log;

//		private bool _started;
//		private string _input;
//		private List<PredictionValue> _predictions;
//		private StreamWriter _writer;

//		// Constants
//		readonly string PATH = Path.Combine(Constants.LOCAL_APP_PATH, "userlog.csv");

//		#endregion

//		#region Constructor

//		public DataCollectorService()
//		{
//			_app = (ThisAddIn)ThisAddIn.Current;
//			_predictions = new List<PredictionValue>();

//			//_app.Configuration.ConfigurationChanged += Configuration_Changed;
//		}

//		#endregion

//		#region Methods

//		public void Dispose()
//		{
//			if (!_started)
//				return;

//			Stop();

//			// Wait 3 seconds for user log to be sent.
//			// TODO : avoid thread sleep.
//			System.Threading.Thread.Sleep(3000);
//		}

//		public void Log(string input, string selected, List<PredictionValue> predictions)
//		{
//			if (predictions == null)
//				return;

//			StringBuilder sb = new StringBuilder();
//			char separator = ';';

//			int weighting = 0;
//			Word selectedWord = _app.LanguagesManager.CurrentLanguage.Words.Find(m => m.Text == selected);
//			if (selectedWord != null)
//				weighting = selectedWord.Occurrence;

//			sb.Append(input).Append(separator);
//			sb.Append(selected).Append(separator);
//			sb.Append(weighting);

//			foreach (PredictionValue prediction in predictions)
//			{
//				sb.Append(separator).Append(prediction.Prediction);
//				sb.Append(separator).Append(prediction.Value);
//				sb.Append(separator).Append(prediction.Type);
//			}

//			_writer.WriteLine(sb.ToString());
//		}

//		public void Start()
//		{
//			//_log.Info("Start");

//			// Open file to write.
//			try
//			{
//				_writer = new StreamWriter(PATH, false);
//			}
//			catch (Exception e)
//			{
//				System.Diagnostics.Debug.WriteLine("Couldn't start DataCollector. Error : " + e.Message);
//				return;
//			}

//			// Catch predictions events.
//			(ThisAddIn)_app.PredictionsManager.PredictionsFound += PredictionsManager_PredictionsFound;

//			_started = true;
//		}

//		public void Stop()
//		{
//			_log.Info("Stop");

//			// Close file.
//			if (_writer != null)
//			{
//				_writer.Close();
//				_writer.Dispose();
//			}

//			// Stop catching predictions events.
//			_app.PredictionsManager.PredictionsFound -= PredictionsManager_PredictionsFound;

//			_started = false;
//		}

//		#endregion

//		#region Events

//		private void Configuration_Changed(object sender, EventArgs e)
//		{
//			if (_app.Configuration.ActivateLogs == _started)
//				return;

//			if (_app.Configuration.ActivateLogs)
//				Start();
//			else
//				Stop();
//		}

//		private void PredictionsManager_PredictionSelected(object sender, PredictionSelectedArg e)
//		{
//			Log(_input, e.Prediction, _predictions);
//		}

//		private void PredictionsManager_PredictionsFound(object sender, PredictionsFoundArgs e)
//		{
//			_input = e.Input;
//			_predictions = e.Predictions;
//		}

//		#endregion
//	}
//}
